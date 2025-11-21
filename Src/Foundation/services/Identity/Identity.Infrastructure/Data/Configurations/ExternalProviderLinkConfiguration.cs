using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.Foundation.Identity.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Infrastructure.Data.Configurations;

public class ExternalProviderLinkConfiguration : IEntityTypeConfiguration<ExternalProviderLink>
{
    public void Configure(EntityTypeBuilder<ExternalProviderLink> builder)
    {
        builder.ToTable("external_provider_links");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ProviderName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.SubjectId)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => new Domain.ValueObjects.EntraSubjectId(v))
            .HasMaxLength(100);
        
        builder.Property(e => e.UserPrincipalName)
            .HasMaxLength(256);
        
        builder.HasIndex(e => new { e.ProviderName, e.SubjectId }).IsUnique();
        builder.HasIndex(e => e.UserId);
        
        // Ignore domain events collection
        builder.Ignore(e => e.DomainEvents);
    }
}
