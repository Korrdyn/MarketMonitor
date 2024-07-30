using MarketMonitor.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketMonitor;

public class DatabaseContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RetainerEntity> Retainers { get; set; }
    public DbSet<GameItemEntity> GameItems { get; set; }
    public DbSet<TrackedItemEntity> TrackedItems { get; set; }
    public DbSet<WorldEntity> Worlds { get; set; }
    public DbSet<DatacenterEntity> Datacenters { get; set; }

    public async Task<UserEntity> GetUser(ulong id)
    {
        var user = await Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null) return user;
        user = new UserEntity
        {
            Id = id
        };
        await AddAsync(user);
        await SaveChangesAsync();
        return user;
    }
}