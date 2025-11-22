using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.Foundation.Identity.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Infrastructure.Data.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.SessionId)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.SessionId(v));
        
        builder.Property(s => s.UserPrincipalName)
            .HasMaxLength(256);
        
        builder.Property(s => s.IpAddress)
            .HasMaxLength(45); // IPv6 max length
        
        builder.Property(s => s.UserAgent)
            .HasMaxLength(500);
        
        builder.Property(s => s.ClaimsJson)
            .HasColumnType("jsonb");
        
        builder.HasIndex(s => s.SessionId).IsUnique();
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => s.ExpiresAt);
        builder.HasIndex(s => s.IsRevoked);
        
        // Configure relationships
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore domain events collection
        builder.Ignore(s => s.DomainEvents);
    }
}
