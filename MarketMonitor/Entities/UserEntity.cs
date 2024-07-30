using System.ComponentModel.DataAnnotations;

namespace MarketMonitor.Entities;

// TODO: Link users with their lodestone
public class UserEntity
{
    [Key]
    public ulong Id { get; set; }

    [MaxLength(64)]
    public string? Datacenter { get; set; }
    
    public TimeSpan NotifyFreq { get; set; } = TimeSpan.FromHours(8);

    public virtual IEnumerable<RetainerEntity> Retainers { get; set; } = null!;
}