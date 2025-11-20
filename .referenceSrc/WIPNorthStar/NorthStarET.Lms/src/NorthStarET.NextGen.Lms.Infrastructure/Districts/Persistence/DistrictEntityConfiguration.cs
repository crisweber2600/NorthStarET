using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// Entity Framework Core configuration for District aggregate.
/// Enforces unique suffix constraint and soft-delete global query filter.
/// </summary>
internal sealed class DistrictEntityConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts", "districts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(d => d.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Suffix)
            .HasColumnName("Suffix")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.CreatedAtUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(d => d.UpdatedAtUtc)
            .HasColumnName("UpdatedAtUtc");

        builder.Property(d => d.DeletedAt)
            .HasColumnName("DeletedAt");

        // Unique index on Suffix (case-insensitive) for active districts only
        builder.HasIndex(d => d.Suffix)
            .HasDatabaseName("IX_Districts_Suffix_Unique")
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        // Index on DeletedAt for soft-delete queries
        builder.HasIndex(d => d.DeletedAt)
            .HasDatabaseName("IX_Districts_DeletedAt");

        // Global query filter to exclude soft-deleted districts by default
        builder.HasQueryFilter(d => d.DeletedAt == null);

        // Ignore domain events collection (not persisted)
        builder.Ignore("DomainEvents");
    }
}
