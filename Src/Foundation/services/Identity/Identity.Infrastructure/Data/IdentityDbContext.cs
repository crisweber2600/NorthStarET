using Microsoft.EntityFrameworkCore;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Infrastructure.Persistence;

namespace NorthStarET.Foundation.Identity.Infrastructure.Data;

/// <summary>
/// Database context for the Identity service
/// </summary>
public class IdentityDbContext : ApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ExternalProviderLink> ExternalProviderLinks => Set<ExternalProviderLink>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
    
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("identity");
        
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
