using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", IdentityDbContext.SchemaName);

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.EntraSubjectId)
            .HasConversion(subjectId => subjectId.Value, value => new EntraSubjectId(value))
            .HasColumnName("entra_subject_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property<string?>("displayNameOverride")
            .HasColumnName("display_name_override")
            .HasMaxLength(200);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(u => u.EntraSubjectId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.IsActive);

        builder.HasMany(user => user.Memberships)
            .WithOne()
            .HasForeignKey(membership => membership.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Sessions)
            .WithOne()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.Memberships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(user => user.Sessions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
