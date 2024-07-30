namespace MarketMonitor.Models;

public class PaginatedRequest
{
    public Pagination Pagination { get; set; }
    public List<ItemResult> Results { get; set; }
}

public class Pagination
{
    public int Page { get; set; }
    public int? PageNext { get; set; }
    public int? PagePrev { get; set; }
    public int PageTotal { get; set; }
    public int ResultsTotal { get; set; }
}

public class ItemResult
{
    public int ID { get; set; }
    public string Icon { get; set; }
    public string Name { get; set; }
}