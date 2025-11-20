using System;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Entities;

public sealed class Membership
{
    public Membership(
        Guid id,
        Guid userId,
        TenantId tenantId,
        Guid roleId,
        DateTimeOffset grantedAt,
        Guid? grantedBy,
        DateTimeOffset? expiresAt,
        bool isActive = true)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Membership id cannot be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        }

        if (expiresAt.HasValue && expiresAt <= grantedAt)
        {
            throw new ArgumentException("ExpiresAt must be greater than GrantedAt.", nameof(expiresAt));
        }

        Id = id;
        UserId = userId;
        TenantId = tenantId;
        RoleId = roleId;
        GrantedAt = grantedAt;
        GrantedBy = grantedBy;
        ExpiresAt = expiresAt;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public TenantId TenantId { get; }

    public Guid RoleId { get; }

    public DateTimeOffset GrantedAt { get; }

    public Guid? GrantedBy { get; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void SetExpiration(DateTimeOffset? expiresAt)
    {
        if (expiresAt.HasValue && expiresAt <= GrantedAt)
        {
            throw new ArgumentException("ExpiresAt must be greater than GrantedAt.", nameof(expiresAt));
        }

        ExpiresAt = expiresAt;
    }
}
