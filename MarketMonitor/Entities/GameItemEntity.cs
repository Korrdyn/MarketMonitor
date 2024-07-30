using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MarketMonitor.Entities;

[Index(nameof(Name))]
public class GameItemEntity
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(128)]
    public required string Name { get; set; }
    
    [MaxLength(128)]
    public required string Icon { get; set; }
    
    public bool Marketable { get; set; }
}