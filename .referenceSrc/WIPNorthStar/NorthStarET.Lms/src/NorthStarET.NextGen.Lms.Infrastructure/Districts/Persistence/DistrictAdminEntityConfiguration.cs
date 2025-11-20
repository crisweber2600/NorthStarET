using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// Entity Framework Core configuration for DistrictAdmin aggregate.
/// Enforces composite unique constraint (DistrictId + Email) and foreign key to Districts.
/// </summary>
internal sealed class DistrictAdminEntityConfiguration : IEntityTypeConfiguration<DistrictAdmin>
{
    public void Configure(EntityTypeBuilder<DistrictAdmin> builder)
    {
        builder.ToTable("DistrictAdmins", "districts");

        builder.HasKey(da => da.Id);

        builder.Property(da => da.Id)
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(da => da.DistrictId)
            .HasColumnName("DistrictId")
            .IsRequired();

        builder.Property(da => da.Email)
            .HasColumnName("Email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(da => da.Status)
            .HasColumnName("Status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(da => da.InvitedAtUtc)
            .HasColumnName("InvitedAtUtc")
            .IsRequired();

        builder.Property(da => da.VerifiedAtUtc)
            .HasColumnName("VerifiedAtUtc");

        builder.Property(da => da.RevokedAtUtc)
            .HasColumnName("RevokedAtUtc");

        // Foreign key relationship to District (cascade delete will revoke admins)
        builder.HasOne<District>()
            .WithMany()
            .HasForeignKey(da => da.DistrictId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index on (DistrictId, Email) - case-insensitive
        builder.HasIndex(da => new { da.DistrictId, da.Email })
            .HasDatabaseName("IX_DistrictAdmins_DistrictId_Email_Unique")
            .IsUnique();

        // Index on DistrictId for tenant-scoped queries
        builder.HasIndex(da => da.DistrictId)
            .HasDatabaseName("IX_DistrictAdmins_DistrictId");

        // Index on Status for filtering active/pending admins
        builder.HasIndex(da => da.Status)
            .HasDatabaseName("IX_DistrictAdmins_Status");

        // Index on InvitedAtUtc for expiry checks
        builder.HasIndex(da => da.InvitedAtUtc)
            .HasDatabaseName("IX_DistrictAdmins_InvitedAtUtc");

        // Ignore domain events collection (not persisted)
        builder.Ignore("DomainEvents");
    }
}
