using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Configurations;

internal sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships", IdentityDbContext.SchemaName);

        builder.HasKey(membership => membership.Id);

        builder.Property(membership => membership.Id)
            .ValueGeneratedNever();

        builder.Property(membership => membership.UserId)
            .IsRequired();

        builder.Property(membership => membership.RoleId)
            .IsRequired();

        builder.Property(membership => membership.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(membership => membership.GrantedBy);

        builder.Property(membership => membership.GrantedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(membership => membership.ExpiresAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(membership => membership.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasIndex(membership => membership.UserId);
        builder.HasIndex(membership => membership.TenantId);
        builder.HasIndex(membership => membership.RoleId);
        builder.HasIndex(membership => membership.ExpiresAt);
        builder.HasIndex(membership => membership.IsActive);
        builder.HasIndex(membership => new { membership.UserId, membership.TenantId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(membership => membership.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(membership => membership.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
