using System;
using System.Collections.Generic;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Entities;

public sealed class Tenant
{
    private readonly List<Membership> memberships = new();
    private readonly List<TenantId> childTenantIds = new();

    public Tenant(
        TenantId id,
        string name,
        TenantType type,
        DateTimeOffset createdAt,
        bool isActive = true,
        TenantId? parentTenantId = null,
        string? externalId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name cannot be null or whitespace.", nameof(name));
        }

        if (name.Length > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(name), name.Length, "Tenant name cannot exceed 200 characters.");
        }

        if (!string.IsNullOrWhiteSpace(externalId) && externalId!.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(externalId), externalId.Length, "External id cannot exceed 100 characters.");
        }

        if (type == TenantType.District && parentTenantId is not null)
        {
            throw new InvalidOperationException("District tenants cannot have a parent tenant.");
        }

        if (type == TenantType.School && parentTenantId is null)
        {
            throw new InvalidOperationException("School tenants must specify a parent tenant id.");
        }

        Id = id;
        Name = name;
        Type = type;
        CreatedAt = createdAt;
        IsActive = isActive;
        ParentTenantId = parentTenantId;
        ExternalId = externalId;
    }

    public TenantId Id { get; }

    public string Name { get; private set; }

    public TenantType Type { get; }

    public TenantId? ParentTenantId { get; }

    public string? ExternalId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<Membership> Memberships => memberships;

    public IReadOnlyCollection<TenantId> ChildTenantIds => childTenantIds;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name cannot be null or whitespace.", nameof(name));
        }

        Name = name;
    }

    public void UpsertExternalId(string? externalId)
    {
        if (!string.IsNullOrWhiteSpace(externalId) && externalId!.Length > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(externalId), externalId.Length, "External id cannot exceed 100 characters.");
        }

        ExternalId = externalId;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void AddMembership(Membership membership)
    {
        if (membership is null)
        {
            throw new ArgumentNullException(nameof(membership));
        }

        if (membership.TenantId != Id)
        {
            throw new InvalidOperationException("Cannot add membership for a different tenant.");
        }

        memberships.Add(membership);
    }

    public void RemoveMembership(Guid membershipId)
    {
        memberships.RemoveAll(m => m.Id == membershipId);
    }

    public void AddChildTenant(TenantId childTenantId)
    {
        if (!childTenantIds.Contains(childTenantId))
        {
            childTenantIds.Add(childTenantId);
        }
    }

    public void RemoveChildTenant(TenantId childTenantId)
    {
        childTenantIds.Remove(childTenantId);
    }
}

public enum TenantType
{
    District = 1,
    School = 2
}
