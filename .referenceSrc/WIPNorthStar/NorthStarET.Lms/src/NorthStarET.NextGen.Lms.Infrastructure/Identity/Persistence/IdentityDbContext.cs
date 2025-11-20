using System;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public const string SchemaName = "identity";
    private static readonly string ConfigurationsNamespace = typeof(IdentityDbContext).Namespace + ".Configurations";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Membership> Memberships => Set<Membership>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<AuthorizationAuditLog> AuthorizationAuditLogs => Set<AuthorizationAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDbContext).Assembly,
            type => type.Namespace is not null && type.Namespace.StartsWith(ConfigurationsNamespace, StringComparison.Ordinal));
    }
}
