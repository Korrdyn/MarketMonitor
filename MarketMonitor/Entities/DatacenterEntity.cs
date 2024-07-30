using System.ComponentModel.DataAnnotations;

namespace MarketMonitor.Entities;

public class DatacenterEntity
{
    [Key]
    [MaxLength(64)]
    public required string Name { get; set; }
}