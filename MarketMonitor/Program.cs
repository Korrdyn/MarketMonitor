using MarketMonitor.HostedServices;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MarketMonitor;
using MarketMonitor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .MinimumLevel.Verbose()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
    var builder = new HostApplicationBuilder();

    builder.Services
        .AddDbContext<DatabaseContext>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE");
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        })
        .AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
        })
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton<InteractionService>()
        .AddTransient<ApiService>()
        .AddHostedService<DiscordClientHost>()
        .AddHostedService<ClientStatus>()
        .AddHostedService<ItemCheck>()
        .AddSerilog();

    builder.Build().Run();
}
catch (HostAbortedException)
{ }
catch (Exception error)
{
    Log.Error(error, "Error in main");
}
finally
{
    Log.CloseAndFlush();
}