using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.Foundation.Identity.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Infrastructure.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(r => r.Description)
            .HasMaxLength(500);
        
        builder.Property(r => r.EntraAppRoleId)
            .HasMaxLength(100);
        
        // Store permissions as JSON array
        builder.Property(r => r.Permissions)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb");
        
        builder.HasIndex(r => r.NormalizedName).IsUnique();
        builder.HasIndex(r => r.EntraAppRoleId);
        
        // Ignore domain events collection
        builder.Ignore(r => r.DomainEvents);
    }
}
