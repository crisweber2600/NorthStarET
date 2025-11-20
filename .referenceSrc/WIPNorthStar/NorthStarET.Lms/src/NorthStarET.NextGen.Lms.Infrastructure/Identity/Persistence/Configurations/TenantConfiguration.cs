using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", IdentityDbContext.SchemaName);

        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Id)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .ValueGeneratedNever();

        builder.Property(tenant => tenant.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tenant => tenant.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tenant => tenant.ParentTenantId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new TenantId(value.Value) : null)
            .HasColumnName("parent_tenant_id");

        builder.Property(tenant => tenant.ExternalId)
            .HasMaxLength(100);

        builder.Property(tenant => tenant.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(tenant => tenant.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(tenant => tenant.ParentTenantId);
        builder.HasIndex(tenant => tenant.Type);
        builder.HasIndex(tenant => tenant.IsActive);
        builder.HasIndex(tenant => tenant.ExternalId)
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL");

        builder.HasMany(tenant => tenant.Memberships)
            .WithOne()
            .HasForeignKey(membership => membership.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(tenant => tenant.ChildTenantIds);

        builder.Navigation(tenant => tenant.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
