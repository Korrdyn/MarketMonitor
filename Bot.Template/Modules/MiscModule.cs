﻿using System.Reflection;
using Bot.Template.HostedServices;
using Discord;
using Discord.Interactions;

namespace Bot.Template.Modules;

public class MiscModule() : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("about", "Information about the bot")]
    public async Task AboutCommand()
    {
        var library = Assembly.GetAssembly(typeof(InteractionModuleBase))!.GetName();
        var self = Context.Client.GetUser(160168328520794112);
        var embed = new EmbedBuilder()
            .WithAuthor(Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl())
            .AddField("Guilds", Context.Client.Guilds.Count.ToString("N0"), true)
            .AddField("Users", Context.Client.Guilds.Select(guild => guild.MemberCount).Sum().ToString("N0"), true)
            .AddField("Library", $"Discord.Net {library.Version!.ToString()}", true)
            .AddField("Developer", $"{DiscordClientHost.DisplayName(self)}", true)
            .AddField("Links",
                $"[GitHub](https://github.com/Korrdyn)\n[Support](https://discord.gg/{Environment.GetEnvironmentVariable("DISCORD_INVITE")})\n[Patreon](https://patreon.com/Korrdyn)",
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
            $"https://discord.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot%20applications.commands");

    [SlashCommand("help", "Commands")]
    public async Task HelpCommand()
    {
        var commands = await (DiscordClientHost.IsDebug() ? Context.Guild.GetApplicationCommandsAsync() : Context.Client.GetGlobalApplicationCommandsAsync());

        var embed = new EmbedBuilder()
            .WithTitle("Commands")
            .WithColor(Color.Blue)
            .WithDescription(string.Join("\n", commands.Select(c => $"</{c.Name}:{c.Id}>")))
            .WithFooter(DiscordClientHost.DisplayName(Context.User), Context.User.GetAvatarUrl())
            .Build();

        await RespondAsync(embed: embed);
    }
}