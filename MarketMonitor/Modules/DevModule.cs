using Discord;
using Discord.Interactions;
using Flurl.Http;
using MarketMonitor.Models;
using MarketMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MarketMonitor.Modules;

[Group("dev", "Dev commands")]
[CommandContextType([InteractionContextType.Guild, InteractionContextType.BotDm, InteractionContextType.PrivateChannel]), IntegrationType(ApplicationIntegrationType.GuildInstall)]
public class DevModule(DatabaseContext db, ApiService api) : BaseModule(db)
{
    [Group("update", "Updates")]
    public class Updates(DatabaseContext db, ApiService api) : BaseModule(db)
    {
        [SlashCommand("items", "Update items")]
        public async Task UpdateItems()
        {
            await DeferAsync();
            if (!await CheckDev()) return;
            await api.UpdateItems();
            await FollowupAsync("Finished");
        }

        [SlashCommand("worlds", "Update worlds")]
        public async Task UpdateWorlds()
        {
            await DeferAsync();
            if (!await CheckDev()) return;
            await api.UpdateWorlds();
            await FollowupAsync("Finished");
        }

        [SlashCommand("dcs", "Update datacenters")]
        public async Task UpdateDCs()
        {
            await DeferAsync();
            if (!await CheckDev()) return;
            await api.UpdateDC();
            await FollowupAsync("Finished");
        }
    }
}