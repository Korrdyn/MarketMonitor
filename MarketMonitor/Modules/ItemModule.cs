using System.Text;
using Discord;
using Discord.Interactions;
using MarketMonitor.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketMonitor.Modules;

[Group("items", "Items")]
[CommandContextType([InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel]), IntegrationType(ApplicationIntegrationType.UserInstall)]
public class ItemModule(DatabaseContext db) : BaseModule(db)
{
    [SlashCommand("track", "Add an item to tracking")]
    public async Task Track([Summary(description: "Item to track"), Autocomplete(typeof(TrackItemAutocompleteHandler))] int id,
        [Summary(description: "World to track item in (leave empty to track in your dc)"), Autocomplete(typeof(TrackWorldAutocompleteHandler))]
        int? world = null)
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        if (!await CheckDC(user)) return;
        var (hasRetainers, retainers) = await CheckRetainer(user);
        if (!hasRetainers) return;

        var item = await db.GameItems.FirstOrDefaultAsync(i => i.Id == id);
        if (item == null)
        {
            await FollowupAsync(embed: Embeds.Error("Unknown item"));
            return;
        }

        if (!item.Marketable)
        {
            await FollowupAsync(embed: Embeds.Error("That item can't be sold on the market"));
            return;
        }

        var exists = await db.TrackedItems.FirstOrDefaultAsync(i => i.SellerId == user.Id && i.ItemId == item.Id);
        if (exists != null)
        {
            await FollowupAsync(embed: Embeds.Error("Already tracking that item"));
            return;
        }

        var worldEntity = world is null ? null : await db.Worlds.FirstOrDefaultAsync(w => w.Id == world);

        var entity = new TrackedItemEntity
        {
            SellerId = user.Id,
            ItemId = item.Id,
            WorldId = world,
            DatacenterId = world != null ? null : user.Datacenter
        };
        await db.AddAsync(entity);
        await db.SaveChangesAsync();

        await FollowupAsync(embed: Embeds.Success($"Now tracking **{item.Name}**{(worldEntity != null ? $" on **{worldEntity.Name}**" : "")}"));
    }

    [SlashCommand("remove", "Stop tracking an item")]
    public async Task Remove([Summary(description: "Item to remove"), Autocomplete(typeof(RemoveAutocompleteHandler))] string id)
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        if (!await CheckDC(user)) return;
        var (hasRetainers, retainers) = await CheckRetainer(user);
        if (!hasRetainers) return;

        var item = await db.TrackedItems.Include(i => i.Item).Include(i => i.World).FirstOrDefaultAsync(i => i.Id == Guid.Parse(id) && i.SellerId == user.Id);

        if (item == null)
        {
            await FollowupAsync(embed: Embeds.Error("Unknown item"));
            return;
        }

        db.Remove(item);
        await db.SaveChangesAsync();

        await FollowupAsync(embed: Embeds.Success($"Stopped tracking **{item.Item.Name}**{(item.World != null ? $" on **{item.World.Name}**" : "")}"));
    }

    [SlashCommand("list", "List tracked items")]
    public async Task List()
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        if (!await CheckDC(user)) return;
        var (hasRetainers, retainers) = await CheckRetainer(user);
        if (!hasRetainers) return;

        var items = await db.TrackedItems.Include(i => i.Item).Include(i => i.World).Include(i => i.Datacenter).Where(i => i.SellerId == user.Id).OrderBy(i => i.WorldId)
            .ThenBy(i => i.DatacenterId).ThenBy(i => i.ItemId).Take(25)
            .ToListAsync();
        if (items.Count == 0)
        {
            await FollowupAsync(embed: Embeds.Error($"No items tracked.\nTrack an item with {await GetCommand("items", ["track"])}"));
            return;
        }

        var count = await db.TrackedItems.CountAsync(i => i.SellerId == user.Id);
        var (embed, components) = BuildItemList(user.Id, items, 0, Math.Ceiling((decimal)count / 25));

        await FollowupAsync(embed: embed, components: components);
    }

    // 0 based page
    public (Embed, MessageComponent) BuildItemList(ulong id, List<TrackedItemEntity> items, int page, decimal totalPages)
    {
        var embed = new EmbedBuilder()
            .WithTitle("Tracked Items")
            .WithDescription(string.Join("\n", items.Select(i => $"[{(i.World != null ? i.World.Name : i.Datacenter!.Name)}] {i.Item.Name}")))
            .WithColor(Color.Magenta)
            .WithFooter($"Page {page + 1:N0} of {totalPages:N0}").Build();
        var components = new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton("Previous page", $"item_list_{id}_{page - 1}", disabled: page == 0)
                .WithButton("Next page", $"item_list_{id}_{page + 1}", disabled: page + 1 == totalPages))
            .Build();

        return (embed, components);
    }

    [ComponentInteraction("item_list_*_*", true)]
    public async Task ItemListButtons(ulong id, int page)
    {
        if (Context.User.Id != id) return;
        await DeferAsync();

        var items = await db.TrackedItems.Include(i => i.Item).Include(i => i.World).Include(i => i.Datacenter).Where(i => i.SellerId == Context.User.Id).OrderBy(i => i.WorldId)
            .ThenBy(i => i.DatacenterId).ThenBy(i => i.ItemId)
            .Skip(page * 25).Take(25)
            .ToListAsync();
        if (items.Count == 0)
        {
            await FollowupAsync(embed: Embeds.Error($"No items tracked.\nTrack an item with {await GetCommand("items", ["track"])}"));
            return;
        }

        var count = await db.TrackedItems.CountAsync(i => i.SellerId == Context.User.Id);
        var (embed, components) = BuildItemList(Context.User.Id, items, page, Math.Ceiling((decimal)count / 25));

        await ModifyOriginalResponseAsync(m =>
        {
            m.Embed = embed;
            m.Components = components;
        });
    }

    public class TrackItemAutocompleteHandler(DatabaseContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            var items = await db.GameItems.Where(i => i.Name.ToLower().Contains(((string)autocompleteInteraction.Data.Current.Value).ToLower()) && i.Marketable)
                .OrderBy(k => k.Name).Take(25)
                .ToListAsync();
            IEnumerable<AutocompleteResult> results = items.Select(i => new AutocompleteResult(i.Name, i.Id));
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class TrackWorldAutocompleteHandler(DatabaseContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            var items = await db.Worlds.Where(i => i.Name.ToLower().Contains(((string)autocompleteInteraction.Data.Current.Value).ToLower()))
                .OrderBy(k => k.Name).Take(25)
                .ToListAsync();
            IEnumerable<AutocompleteResult> results = items.Select(i => new AutocompleteResult(i.Name, i.Id));
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }

    public class RemoveAutocompleteHandler(DatabaseContext db) : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            var items = await db.TrackedItems.Include(i => i.Item).Include(i => i.World).Include(i => i.Datacenter).Where(i =>
                    i.Item.Name.ToLower().Contains(((string)autocompleteInteraction.Data.Current.Value).ToLower()) && i.SellerId == context.User.Id)
                .OrderBy(k => k.Item.Name).Take(25)
                .ToListAsync();
            IEnumerable<AutocompleteResult> results = items.Select(i =>
                new AutocompleteResult($"[{(i.World != null ? i.World!.Name : i.Datacenter!.Name)}] {i.Item.Name}", i.Id.ToString()));
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}