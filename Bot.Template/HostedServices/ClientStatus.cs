using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace Bot.Template.HostedServices;

internal sealed class ClientStatus(DiscordSocketClient client) : IHostedService, IDisposable
{
    private int lastStatus;
    private Timer? timer;

    public void Dispose()
    {
        timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(SetStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void SetStatus(object? state)
    {
        var status = "/help";
        switch (lastStatus)
        {
            case 0:
                lastStatus++;
                break;
            case 1:
                status = "eris.gg";
                lastStatus = 0;
                break;
        }

        await client.SetCustomStatusAsync(status);
    }
}