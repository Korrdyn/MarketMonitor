namespace MarketMonitor.Models;

public class MultiItemPriceResult
{
    public SortedList<string, SingleItemPriceResult> items { get; set; }
}