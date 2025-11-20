using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// EF Core entity configuration for School aggregate.
/// Defines table mapping, indexes, and relationships for the Schools table.
/// </summary>
public sealed class SchoolEntityConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("Schools");

        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.DistrictId)
            .IsRequired();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Code)
            .HasMaxLength(50);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.ConcurrencyStamp)
            .IsRequired()
            .IsConcurrencyToken()
            .HasMaxLength(50);

        builder.Property(s => s.CreatedBy)
            .IsRequired();

        builder.Property(s => s.UpdatedBy);

        builder.Property(s => s.CreatedAtUtc)
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc);

        builder.Property(s => s.DeletedAt);

        // Indexes for queries
        builder.HasIndex(s => s.DistrictId)
            .HasDatabaseName("IX_Schools_DistrictId");

        builder.HasIndex(s => new { s.DistrictId, s.Name })
            .IsUnique()
            .HasDatabaseName("IX_Schools_DistrictId_Name_Unique")
            .HasFilter("[DeletedAt] IS NULL"); // Only enforce uniqueness for non-deleted schools

        builder.HasIndex(s => new { s.DistrictId, s.Code })
            .IsUnique()
            .HasDatabaseName("IX_Schools_DistrictId_Code_Unique")
            .HasFilter("[Code] IS NOT NULL AND [DeletedAt] IS NULL"); // Only when code is provided and not deleted

        builder.HasIndex(s => s.DeletedAt)
            .HasDatabaseName("IX_Schools_DeletedAt");

        // Owned collection: GradeOfferings
        builder.OwnsMany(s => s.GradeOfferings, gradeBuilder =>
        {
            gradeBuilder.ToTable("GradeOfferings");

            gradeBuilder.HasKey(g => g.Id);

            gradeBuilder.Property(g => g.SchoolId)
                .IsRequired();

            gradeBuilder.Property(g => g.GradeLevel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            gradeBuilder.Property(g => g.SchoolType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            gradeBuilder.Property(g => g.EffectiveFrom)
                .IsRequired();

            gradeBuilder.Property(g => g.EffectiveTo);

            gradeBuilder.Property(g => g.ConcurrencyStamp)
                .IsRequired()
                .IsConcurrencyToken()
                .HasMaxLength(50);

            gradeBuilder.Property(g => g.CreatedBy)
                .IsRequired();

            gradeBuilder.Property(g => g.UpdatedBy);

            gradeBuilder.Property(g => g.CreatedAtUtc)
                .IsRequired();

            gradeBuilder.Property(g => g.UpdatedAtUtc);

            // Index for querying active offerings
            gradeBuilder.HasIndex(g => new { g.SchoolId, g.EffectiveTo })
                .HasDatabaseName("IX_GradeOfferings_SchoolId_EffectiveTo");

            // Foreign key to School
            gradeBuilder.WithOwner()
                .HasForeignKey(g => g.SchoolId);
        });

        // Ignore domain events collection (handled separately)
        builder.Ignore(s => s.DomainEvents);

        // Ignore computed properties
        builder.Ignore(s => s.IsDeleted);
        builder.Ignore(s => s.GradeRangeMin);
        builder.Ignore(s => s.GradeRangeMax);
    }
}
