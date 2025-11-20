using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", IdentityDbContext.SchemaName);

        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).ValueGeneratedNever();

        builder.Property(role => role.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(role => role.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasMaxLength(500);

        builder.Property(role => role.IsSystemRole)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(role => role.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        var permissionsProperty = builder.Property(role => role.Permissions)
            .HasField("permissions")
            .HasColumnName("permissions")
            .HasColumnType("jsonb")
            .HasConversion(
                permissions => JsonSerializer.Serialize(permissions, JsonOptions),
                json => string.IsNullOrWhiteSpace(json)
                    ? new List<Permission>()
                    : JsonSerializer.Deserialize<List<Permission>>(json, JsonOptions) ?? new List<Permission>())
            .IsRequired();

        var permissionsComparer = new ValueComparer<IReadOnlyCollection<Permission>>(
            (left, right) =>
                (left ?? Array.Empty<Permission>()).SequenceEqual(right ?? Array.Empty<Permission>()),
            collection =>
                (collection ?? Array.Empty<Permission>()).Aggregate(0, (hash, permission) => HashCode.Combine(hash, permission.GetHashCode())),
            collection => (collection ?? Array.Empty<Permission>()).ToList());

        permissionsProperty.Metadata.SetValueComparer(permissionsComparer);

        builder.HasIndex(role => role.Name).IsUnique();
        builder.HasIndex(role => role.IsSystemRole);

        builder.Metadata.FindProperty(nameof(Role.Permissions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
