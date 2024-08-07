using System.Reflection;
using Discord;
using Discord.Interactions;
using MarketMonitor.HostedServices;

namespace MarketMonitor.Modules;

[CommandContextType([InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel]),
 IntegrationType([ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])]
public class MiscModule() : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("about", "Information about the bot")]
    public async Task AboutCommand()
    {
        var library = Assembly.GetAssembly(typeof(InteractionModuleBase))!.GetName();
        var self = Context.Client.GetUser(160168328520794112);
        var embed = new EmbedBuilder()
            .WithAuthor(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
            .AddField("Library", $"Discord.Net {library.Version!.ToString()}", true)
            .AddField("Developer", $"{DiscordClientHost.DisplayName(self)}", true)
            .AddField("Links",
                $"[GitHub](https://github.com/Azorant/MarketMonitor)\n[Support](https://discord.gg/{Environment.GetEnvironmentVariable("DISCORD_INVITE")})\n[Ko-fi](https://ko-fi.com/azorant)",
                true)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .WithFooter(DiscordClientHost.DisplayName(Context.User), Context.User.GetAvatarUrl())
            .Build();

        await RespondAsync(embed: embed);
    }

    [SlashCommand("invite", "Invite the bot")]
    public async Task InviteCommand()
        => await RespondAsync(
            $"https://discord.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}");
}