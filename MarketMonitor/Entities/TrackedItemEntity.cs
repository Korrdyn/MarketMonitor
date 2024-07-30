using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMonitor.Entities;

public class TrackedItemEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(Seller))]
    public ulong SellerId { get; set; }

    [ForeignKey(nameof(Item))]
    public int ItemId { get; set; }

    [ForeignKey(nameof(World))]
    public int? WorldId { get; set; }

    [ForeignKey(nameof(Datacenter))]
    [MaxLength(64)]
    public string? DatacenterId { get; set; }
    
    public DateTime? LastNotify { get; set; }

    public virtual UserEntity Seller { get; set; } = null!;
    public virtual GameItemEntity Item { get; set; } = null!;
    public virtual WorldEntity? World { get; set; }
    public virtual DatacenterEntity? Datacenter { get; set; }
}