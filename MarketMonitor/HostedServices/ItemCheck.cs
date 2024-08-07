using Discord.WebSocket;
using MarketMonitor.Services;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MarketMonitor.HostedServices;

internal sealed class ItemCheck(ApiService api) : IHostedService, IDisposable
{
    private Timer? timer;

    public void Dispose()
    {
        timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(SetStatus, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void SetStatus(object? state)
    {
        await api.CheckItems();
    }
}