using Discord;
using Discord.Interactions;
using MarketMonitor.Entities;
using MarketMonitor.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketMonitor.Modules;

[Group("retainers", "Retainers"), CommandContextType([InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel]),
 IntegrationType(ApplicationIntegrationType.UserInstall)]
public class RetainerModule(DatabaseContext db, ApiService api) : BaseModule(db)
{
    [SlashCommand("add", "Add a retainer for tracking")]
    public async Task Add(string name)
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        if (!await CheckDC(user)) return;

        var retainer = new RetainerEntity
        {
            Name = name,
            OwnerId = Context.User.Id
        };
        await db.AddAsync(retainer);
        await db.SaveChangesAsync();

        await FollowupAsync(embed: Embeds.Success("Retainer added"));
    }

    [SlashCommand("remove", "Remove a retainer from being tracked")]
    public async Task Remove(string name)
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        if (!await CheckDC(user)) return;

        var retainer = await db.Retainers.FirstOrDefaultAsync(r => r.OwnerId == user.Id && r.Name == name);
        if (retainer == null)
        {
            await FollowupAsync(embed: Embeds.Error("Unknown retainer"));
            return;
        }

        db.Remove(retainer);
        await db.SaveChangesAsync();

        await FollowupAsync(embed: Embeds.Success("Retainer removed"));
    }
}