using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Configurations;

internal sealed class AuthorizationAuditLogConfiguration : IEntityTypeConfiguration<AuthorizationAuditLog>
{
    public void Configure(EntityTypeBuilder<AuthorizationAuditLog> builder)
    {
        builder.ToTable("AuthorizationAuditLogs", IdentityDbContext.SchemaName);

        builder.HasKey(log => log.Id);
        builder.Property(log => log.Id).ValueGeneratedNever();

        builder.Property(log => log.UserId)
            .IsRequired();

        builder.Property(log => log.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(log => log.Resource)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(log => log.Allowed)
            .IsRequired();

        builder.Property(log => log.EvaluatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(log => log.RoleId);

        builder.Property(log => log.RoleName)
            .HasMaxLength(200);

        builder.Property(log => log.Reason)
            .HasMaxLength(500);

        builder.Property(log => log.TraceId)
            .HasMaxLength(100);

        builder.HasIndex(log => log.UserId);
        builder.HasIndex(log => log.TenantId);
        builder.HasIndex(log => log.EvaluatedAt);
        builder.HasIndex(log => log.Resource);
    }
}
