using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.Foundation.Identity.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Infrastructure.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        
        builder.HasKey(ur => ur.Id);
        
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        
        // Configure relationships
        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Ignore domain events collection
        builder.Ignore(ur => ur.DomainEvents);
    }
}
