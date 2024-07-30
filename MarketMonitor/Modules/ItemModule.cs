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
            var items = await db.TrackedItems.Include(i => i.Item).Include(i => i.World).Where(i =>
                    i.Item.Name.ToLower().Contains(((string)autocompleteInteraction.Data.Current.Value).ToLower()) && i.SellerId == context.User.Id)
                .OrderBy(k => k.Item.Name).Take(25)
                .ToListAsync();
            IEnumerable<AutocompleteResult> results = items.Select(i => new AutocompleteResult($"{i.Item.Name}{(i.World != null ? $" - {i.World!.Name}" : "")}", i.Id.ToString()));
            return AutocompletionResult.FromSuccess(results.Take(25));
        }
    }
}