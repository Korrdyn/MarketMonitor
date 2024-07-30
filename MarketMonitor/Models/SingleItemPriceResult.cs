namespace MarketMonitor.Models;

public class SingleItemPriceResult
{
    public List<Listing> listings { get; set; }
    public long lastUploadTime { get; set; }
}

public class Listing
{
    public int pricePerUnit { get; set; }
    public string retainerName { get; set; }
    public bool hq { get; set; }
}