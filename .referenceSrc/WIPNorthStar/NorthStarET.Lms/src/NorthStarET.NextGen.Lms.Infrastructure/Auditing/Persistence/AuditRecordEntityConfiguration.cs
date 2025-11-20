using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Auditing;

namespace NorthStarET.NextGen.Lms.Infrastructure.Auditing.Persistence;

/// <summary>
/// Entity Framework Core configuration for AuditRecord entity.
/// Includes indexes for tenant-scoped queries, correlation tracking, and entity lookups.
/// </summary>
internal sealed class AuditRecordEntityConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("AuditRecords", "districts");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.Id)
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(ar => ar.DistrictId)
            .HasColumnName("DistrictId")
            .IsRequired();

        builder.Property(ar => ar.ActorId)
            .HasColumnName("ActorId")
            .IsRequired();

        builder.Property(ar => ar.ActorRole)
            .HasColumnName("ActorRole")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ar => ar.Action)
            .HasColumnName("Action")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ar => ar.EntityType)
            .HasColumnName("EntityType")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ar => ar.EntityId)
            .HasColumnName("EntityId");

        builder.Property(ar => ar.BeforePayload)
            .HasColumnName("BeforePayload")
            .HasColumnType("jsonb");

        builder.Property(ar => ar.AfterPayload)
            .HasColumnName("AfterPayload")
            .HasColumnType("jsonb");

        builder.Property(ar => ar.TimestampUtc)
            .HasColumnName("TimestampUtc")
            .IsRequired();

        builder.Property(ar => ar.CorrelationId)
            .HasColumnName("CorrelationId")
            .IsRequired();

        // Index on DistrictId for tenant-scoped queries
        builder.HasIndex(ar => ar.DistrictId)
            .HasDatabaseName("IX_AuditRecords_DistrictId");

        // Index on CorrelationId for grouping related records
        builder.HasIndex(ar => ar.CorrelationId)
            .HasDatabaseName("IX_AuditRecords_CorrelationId");

        // Composite index on (EntityType, EntityId) for entity history queries
        builder.HasIndex(ar => new { ar.EntityType, ar.EntityId })
            .HasDatabaseName("IX_AuditRecords_EntityType_EntityId");

        // Composite index on (DistrictId, ActorId) for actor activity queries
        builder.HasIndex(ar => new { ar.DistrictId, ar.ActorId })
            .HasDatabaseName("IX_AuditRecords_DistrictId_ActorId");

        // Index on TimestampUtc for chronological queries
        builder.HasIndex(ar => ar.TimestampUtc)
            .HasDatabaseName("IX_AuditRecords_TimestampUtc");
    }
}
