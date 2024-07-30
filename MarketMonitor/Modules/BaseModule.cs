using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MarketMonitor.Entities;
using MarketMonitor.HostedServices;
using Microsoft.EntityFrameworkCore;

namespace MarketMonitor.Modules;

public class BaseModule(DatabaseContext db) : InteractionModuleBase<SocketInteractionContext>
{
    internal async Task<bool> CheckDev()
    {
        var app = await Context.Client.GetApplicationInfoAsync();
        var owner = false;
        if (app.Team == null)
        {
            owner = app.Owner.Id == Context.User.Id;
        }
        else
        {
            var member = app.Team.TeamMembers.FirstOrDefault(m => m.User.Id == Context.User.Id);
            owner = member != null;
        }

        if (owner)
            return true;
        await FollowupAsync(embed: new EmbedBuilder()
            .WithTitle("Whoa")
            .WithDescription($"You've got to be a developer to run this command")
            .WithColor(Color.Red)
            .Build());
        return false;
    }

    // ReSharper disable once InconsistentNaming
    internal async Task<bool> CheckDC(UserEntity user)
    {
        if (user.Datacenter == null)
        {
            await FollowupAsync(embed: new EmbedBuilder()
                .WithTitle("Missing DC")
                .WithDescription($"Please run {await GetCommand("datacenter")} and try again.")
                .WithColor(Color.Gold)
                .Build());
            return false;
        }

        return true;
    }
    internal async Task<(bool, List<RetainerEntity>)> CheckRetainer(UserEntity user)
    {
        var retainers = await db.Retainers.Where(r => r.OwnerId == user.Id).ToListAsync();
        if (retainers.Count == 0)
        {
            await FollowupAsync(embed: new EmbedBuilder()
                .WithTitle("Missing Retainer")
                .WithDescription($"Please run {await GetCommand("retainers", ["add"])} and try again.")
                .WithColor(Color.Gold)
                .Build());
            return (false, []);
        }

        return (true, retainers);
    }

    private Task<IReadOnlyCollection<SocketApplicationCommand>> GetCommands()
    {
        return DiscordClientHost.IsDebug() ? Context.Guild.GetApplicationCommandsAsync() : Context.Client.GetGlobalApplicationCommandsAsync();
    }

    internal async Task<string> GetCommand(string name, List<string>? subcommands = null)
    {
        var commands = await GetCommands();
        var command = commands.FirstOrDefault(c => c.Name == name);
        return command is null
            ? $"/{name}{(subcommands is null ? "" : $" {string.Join(' ', subcommands)}")}"
            : $"</{name}{(subcommands is null ? "" : $" {string.Join(' ', subcommands)}")}:{command.Id}>";
    }
}