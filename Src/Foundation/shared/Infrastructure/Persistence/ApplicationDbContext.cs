using Microsoft.EntityFrameworkCore;
using NorthStarET.Foundation.Infrastructure.Persistence.Entities;

namespace NorthStarET.Foundation.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for all services
/// </summary>
public abstract class ApplicationDbContext : DbContext
{
    protected ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        });
    }
}
