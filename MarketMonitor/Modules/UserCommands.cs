using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace MarketMonitor.Modules;

[CommandContextType([InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel]), IntegrationType(ApplicationIntegrationType.UserInstall)]
public class UserCommands(DatabaseContext db) : BaseModule(db)
{
    [SlashCommand("datacenter", "Set the datacenter you're playing on")]
    public async Task Datacenter(
        [Choice("Aether", "aether"), Choice("Chaos", "chaos"), Choice("Crystal", "crystal"), Choice("Dynamis", "dynamis"), Choice("Elemental", "elemental"), Choice("Gaia", "gaia"),
         Choice("Korea", "korea"), Choice("Light", "light"), Choice("Mana", "mana"), Choice("Materia", "materia"), Choice("Meteor", "meteor"), Choice("Primal", "primal"),
         Choice("猫小胖", "猫小胖"), Choice("莫古力", "莫古力"), Choice("豆豆柴", "豆豆柴"), Choice("陆行鸟", "陆行鸟")]
        string dc)
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        var datacenter = await db.Datacenters.FirstOrDefaultAsync(d => d.Name == dc);
        if (datacenter == null)
        {
            await FollowupAsync(embed: Embeds.Error("Unknown DC"));
            return;
        }

        user.Datacenter = datacenter.Name;
        db.Update(user);
        await db.SaveChangesAsync();
        await FollowupAsync(embed: Embeds.Success($"Datacenter set to `{datacenter.Name}`"));
    }

    [SlashCommand("profile", "Show your profile")]
    public async Task Profile()
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        var retainers = await db.Retainers.Where(r => r.OwnerId == user.Id).ToListAsync();
        var items = await db.TrackedItems.CountAsync(i => i.SellerId == user.Id);
        await FollowupAsync(embed: new EmbedBuilder()
            .WithTitle("Profile")
            .AddField("Datacenter", user.Datacenter ?? "Not set", true)
            .AddField("Notification Frequency", user.NotifyFreq.ToReadableString(), true)
            .AddField("Tracked Items", $"{items:N0}", true)
            .AddField("Retainers", retainers.Any() ? string.Join("\n", retainers.Select(r => r.Name)) : "None linked")
            .WithColor(Color.Blue).Build());
    }

    [SlashCommand("notify", "Set your notification interval")]
    public async Task Notify(
        [Choice("5 minutes", 5), Choice("15 minutes", 15), Choice("30 minutes", 30), Choice("45 minutes", 45), Choice("1 hour", 60), Choice("2 hours", 120), Choice("3 hours", 180),
         Choice("4 hours", 240), Choice("5 hours", 300), Choice("6 hours", 360), Choice("7 hours", 420), Choice("8 hours", 480), Choice("12 hours", 720), Choice("1 day", 1440)]
        int frequency)
    {
        await DeferAsync();
        var user = await db.GetUser(Context.User.Id);
        user.NotifyFreq = TimeSpan.FromMinutes(frequency);
        db.Update(user);
        await db.SaveChangesAsync();
        await FollowupAsync(embed: Embeds.Success("Notification frequency set"));
    }
}