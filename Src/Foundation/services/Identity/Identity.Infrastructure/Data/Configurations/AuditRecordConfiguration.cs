using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.Foundation.Identity.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Infrastructure.Data.Configurations;

public class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("audit_records");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.EventType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.EventData)
            .HasColumnType("jsonb");
        
        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);
        
        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(1000);
        
        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.EventType);
        builder.HasIndex(a => a.CreatedAt);
        
        // Ignore domain events collection
        builder.Ignore(a => a.DomainEvents);
    }
}
