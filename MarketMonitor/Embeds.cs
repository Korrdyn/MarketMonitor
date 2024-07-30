using Discord;

namespace MarketMonitor;

public static class Embeds
{
    public static Embed Success(string desc) => new EmbedBuilder().WithTitle("Success").WithDescription(desc).WithColor(Color.Green).Build();
    public static Embed Error(string desc) => new EmbedBuilder().WithTitle("Error").WithDescription(desc).WithColor(Color.Gold).Build();
}