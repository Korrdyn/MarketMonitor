using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMonitor.Entities;

public class RetainerEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(64)]
    public required string Name { get; set; }

    [ForeignKey(nameof(Owner))]
    public ulong OwnerId { get; set; }

    public virtual UserEntity Owner { get; set; } = null!;
}