using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<API.Models.Timer> Timers { get; set; }
    public DbSet<BlockedUser> BlockedUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<API.Models.Timer>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UserId).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Username).IsRequired().HasMaxLength(100);
            entity.Property(t => t.ChannelId).IsRequired();
            entity.Property(t => t.DurationMinutes).IsRequired();
            entity.Property(t => t.CreatedAt).IsRequired();
            entity.Property(t => t.ExpiresAt).IsRequired();
            entity.Property(t => t.IsCompleted).HasDefaultValue(false);
            entity.Property(t => t.Message).HasMaxLength(500);

            // Index for faster queries on expired timers
            entity.HasIndex(t => new { t.ExpiresAt, t.IsCompleted });
        });

        modelBuilder.Entity<BlockedUser>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.UserId).IsRequired().HasMaxLength(50);
            entity.Property(b => b.Username).IsRequired().HasMaxLength(100);
            entity.Property(b => b.BlockedAt).IsRequired();
            entity.Property(b => b.BlockedBy).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Reason).HasMaxLength(500);

            // Index for faster queries on blocked users
            entity.HasIndex(b => b.UserId).IsUnique();
        });
    }
}
