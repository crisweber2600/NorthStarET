using System;
using System.Collections.Generic;
using System.Linq;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Entities;

public sealed class Role
{
    private readonly List<Permission> permissions = new();

    private Role()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        DisplayName = string.Empty;
        CreatedAt = DateTimeOffset.MinValue;
        Description = null;
    }

    public Role(
        Guid id,
        string name,
        string displayName,
        DateTimeOffset createdAt,
        bool isSystemRole,
        IEnumerable<Permission>? initialPermissions = null,
        string? description = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name cannot be null or whitespace.", nameof(name));
        }

        if (name.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(name), name.Length, "Role name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be null or whitespace.", nameof(displayName));
        }

        if (displayName.Length > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(displayName), displayName.Length, "Display name cannot exceed 200 characters.");
        }

        if (!string.IsNullOrWhiteSpace(description) && description!.Length > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(description), description.Length, "Description cannot exceed 500 characters.");
        }

        Id = id;
        Name = name;
        DisplayName = displayName;
        CreatedAt = createdAt;
        IsSystemRole = isSystemRole;
        Description = description;

        if (initialPermissions is not null)
        {
            permissions.AddRange(initialPermissions);
        }
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string DisplayName { get; private set; }

    public string? Description { get; private set; }

    public bool IsSystemRole { get; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<Permission> Permissions => permissions;

    public void Rename(string name, string displayName, string? description)
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be renamed.");
        }

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Role name and display name cannot be null or whitespace.");
        }

        Name = name;
        DisplayName = displayName;
        Description = description;
    }

    public void SetPermissions(IEnumerable<Permission> updatedPermissions)
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot have their permissions changed locally.");
        }

        if (updatedPermissions is null)
        {
            throw new ArgumentNullException(nameof(updatedPermissions));
        }

        permissions.Clear();
        permissions.AddRange(updatedPermissions);
    }

    public bool HasPermission(string resource, string action)
    {
        return permissions.Any(permission =>
            string.Equals(permission.Resource, resource, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(permission.Action, action, StringComparison.OrdinalIgnoreCase));
    }
}
