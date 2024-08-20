namespace MarketMonitor.Models;

public class PaginatedRequest
{
    public string schema { get; set; }
    public List<ItemResult> rows { get; set; }
}

public class ItemResult
{
    public int row_id { get; set; }
    public ItemFields fields { get; set; }
}

public class ItemFields
{
    public string Name { get; set; }
    public ItemIcon Icon { get; set; }
}

public class ItemIcon
{
    public string path_hr1 { get; set; }
}
