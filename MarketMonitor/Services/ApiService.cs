using Discord;
using Discord.WebSocket;
using Flurl.Http;
using MarketMonitor.Entities;
using MarketMonitor.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace MarketMonitor.Services;

public class ApiService(DatabaseContext db, DiscordSocketClient client)
{
    public async Task UpdateItems()
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var existingItems = await db.GameItems.ToListAsync();

        Log.Information("[XIVAPI] Downloading items");
        var initial = await "https://xivapi.com/item?limit=3000".GetJsonAsync<PaginatedRequest>();
        var rawItems = new List<ItemResult>();

        rawItems.AddRange(initial.Results);

        for (int i = initial.Pagination.PageNext!.Value; i <= initial.Pagination.PageTotal; i++)
        {
            Log.Information($"[XIVAPI] Downloading items - page {i:N0}/{initial.Pagination.PageTotal:N0} - loaded {rawItems.Count:N0}/{initial.Pagination.ResultsTotal:N0}");
            var request = await $"https://xivapi.com/item?limit=3000&page={i}".GetJsonAsync<PaginatedRequest>();
            rawItems.AddRange(request.Results);
        }

        Log.Information($"[XIVAPI] Downloaded {rawItems.Count:N0} items");

        var marketableItems = await "https://universalis.app/api/v2/marketable".WithHeader("User-Agent", "MarketMonitor").GetJsonAsync<int[]>();

        Log.Information($"[Universalis] Downloaded {marketableItems.Length:N0} marketable items");

        int updated = 0;
        int created = 0;

        foreach (var item in rawItems)
        {
            var exists = existingItems.FirstOrDefault(i => i.Id == item.ID);
            if (exists != null)
            {
                exists.Name = item.Name;
                exists.Icon = item.Icon;
                exists.Marketable = marketableItems.Contains(item.ID);
                db.Update(exists);
                updated++;
            }
            else
            {
                await db.AddAsync(new GameItemEntity
                {
                    Id = item.ID,
                    Name = item.Name,
                    Icon = item.Icon,
                    Marketable = marketableItems.Contains(item.ID)
                });
                created++;
            }
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        Log.Information($"[DB] Created {created:N0} and updated {updated:N0} items");
    }

    public async Task UpdateWorlds()
    {
        var worlds = await "https://universalis.app/api/v2/worlds".WithHeader("User-Agent", "MarketMonitor").GetJsonAsync<List<WorldsRequest>>();
        var existing = await db.Worlds.ToListAsync();
        foreach (var world in worlds)
        {
            var exists = existing.FirstOrDefault(w => w.Id == world.id);
            if (exists != null)
            {
                exists.Name = world.name;
                db.Update(exists);
            }
            else
            {
                await db.AddAsync(new WorldEntity
                {
                    Id = world.id,
                    Name = world.name
                });
            }
        }

        await db.SaveChangesAsync();
        Log.Information("[Universalis] Updated worlds");
    }

    public async Task UpdateDC()
    {
        var dcs = await "https://universalis.app/api/v2/data-centers".WithHeader("User-Agent", "MarketMonitor").GetJsonAsync<List<DCRequest>>();
        var existing = await db.Datacenters.ToListAsync();
        foreach (var dc in dcs)
        {
            if (dc.name == "NA Cloud DC (Beta)") continue;
            var exists = existing.FirstOrDefault(d => d.Name == dc.name);
            if (exists == null)
            {
                await db.AddAsync(new DatacenterEntity()
                {
                    Name = dc.name
                });
            }
        }

        await db.SaveChangesAsync();
        Log.Information("[Universalis] Updated DCs");
    }

    public async Task CheckItems()
    {
        var trackedItems = await db.TrackedItems.Include(i => i.Seller).ThenInclude(s => s.Retainers).Include(i => i.Item).Include(i => i.World).Include(i => i.Datacenter)
            .ToListAsync();

        var groups = trackedItems.Where(i => i.LastNotify == null || DateTime.UtcNow - i.LastNotify > i.Seller.NotifyFreq)
            .GroupBy(i => new { i.WorldId, i.DatacenterId }).ToList();

        Log.Information($"[Universalis] Checking {trackedItems.Count:N0} items ({groups.Count:N0} groups)");

        foreach (var group in groups)
        {
            var region = group.Key.WorldId != null ? group.Key.WorldId.ToString() : group.Key.DatacenterId;
            var idChunks = group.Select(i => i.ItemId).Distinct().Chunk(100);
            foreach (var ids in idChunks)
            {
                MultiItemPriceResult priceResponse;
                if (ids.Length == 1)
                {
                    var item = await $"https://universalis.app/api/v2/{region}/{ids[0]}?fields=listings.pricePerUnit%2Clistings.retainerName%2ClastUploadTime"
                        .WithHeader("User-Agent", "MarketMonitor")
                        .GetJsonAsync<SingleItemPriceResult>();
                    var list = new SortedList<string, SingleItemPriceResult>();
                    list.Add(ids[0].ToString(), item);
                    priceResponse = new MultiItemPriceResult
                    {
                        items = list
                    };
                }
                else
                {
                    priceResponse = await $"https://universalis.app/api/v2/{region}/{string.Join(',', ids)}".WithHeader("User-Agent", "MarketMonitor")
                        .GetJsonAsync<MultiItemPriceResult>();
                }

                // Loop over items from uni
                foreach (var el in priceResponse.items)
                {
                    var id = int.Parse(el.Key);
                    var prices = el.Value;

                    // Loop over tracked items with same id and check if retainer is lowest price
                    foreach (var item in group.Where(i => i.ItemId == id))
                    {
                        var retainers = item.Seller.Retainers.Select(r => r.Name);
                        var selfPrice = prices.listings.OrderBy(i => i.pricePerUnit).FirstOrDefault(i => retainers.Contains(i.retainerName));
                        if (selfPrice == null) continue;
                        var nextPrice = prices.listings.Where(i => !retainers.Contains(i.retainerName) && i.hq == selfPrice.hq).MinBy(i => i.pricePerUnit);
                        if (nextPrice == null || selfPrice.pricePerUnit <= nextPrice.pricePerUnit) continue;

                        var cutAmount = selfPrice.pricePerUnit - nextPrice.pricePerUnit;
                        // Send user notification of undercut
                        try
                        {
                            var user = await client.GetUserAsync(item.SellerId);
                            await user.SendMessageAsync(embed: new EmbedBuilder()
                                .WithAuthor(item.World is null ? item.Datacenter!.Name : item.World.Name, "https://cdn.carbon.pics/cross-world-flower.png")
                                .WithFooter("You've been under cut!")
                                .WithTitle(item.Item.Name)
                                .WithDescription(
                                    $"[Universalis | Updated {new TimestampTag(DateTimeOffset.FromUnixTimeMilliseconds(prices.lastUploadTime), TimestampTagStyles.Relative)}](https://universalis.app/market/{item.ItemId})")
                                .WithThumbnailUrl($"https://xivapi.com{item.Item.Icon}")
                                .AddField("Your Price", $"<:gil:1270605677651558433> {selfPrice.pricePerUnit:N0}", true)
                                .AddField("Lowest Price", $"<:gil:1270605677651558433> {nextPrice.pricePerUnit:N0}", true)
                                .AddField("Amount Cut",
                                    $"<:gil:1267715732704198797> {cutAmount:N0} ({Math.Round((decimal)(selfPrice.pricePerUnit - nextPrice.pricePerUnit) / selfPrice.pricePerUnit * 100, 1):N1}%)",
                                    true)
                                .WithColor(Color.Orange)
                                .Build());
                            item.LastNotify = DateTime.UtcNow;
                            db.Update(item);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception)
                        { }


                        Log.Information($"{region} {item.ItemId} self {selfPrice?.pricePerUnit} ({selfPrice?.retainerName}) {nextPrice?.pricePerUnit} ({nextPrice?.retainerName})");
                    }
                }
            }
        }
    }
}