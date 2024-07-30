using System.ComponentModel.DataAnnotations;

namespace MarketMonitor.Entities;

public class WorldEntity
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(64)]
    public required string Name { get; set; }
}