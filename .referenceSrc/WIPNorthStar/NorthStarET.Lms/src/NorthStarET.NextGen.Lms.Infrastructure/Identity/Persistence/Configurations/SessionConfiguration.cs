using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions", IdentityDbContext.SchemaName);

        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).ValueGeneratedNever();

        builder.Property(session => session.UserId)
            .IsRequired();

        builder.Property(session => session.EntraTokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(session => session.LmsAccessToken)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(session => session.ActiveTenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(session => session.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(session => session.ExpiresAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(session => session.LastActivityAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(session => session.IsRevoked)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(session => session.IpAddress)
            .HasMaxLength(45);

        builder.Property(session => session.UserAgent)
            .HasMaxLength(500);

        builder.HasIndex(session => session.UserId);
        builder.HasIndex(session => session.ActiveTenantId);
        builder.HasIndex(session => session.EntraTokenHash).IsUnique();
        builder.HasIndex(session => session.ExpiresAt);
        builder.HasIndex(session => session.IsRevoked);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(session => session.ActiveTenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
