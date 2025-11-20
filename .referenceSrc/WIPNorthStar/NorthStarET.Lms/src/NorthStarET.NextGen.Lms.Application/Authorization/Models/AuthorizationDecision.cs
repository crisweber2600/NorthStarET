using System;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Models;

public sealed class AuthorizationDecision
{
    public AuthorizationDecision(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        bool allowed,
        Guid? roleId,
        string? roleName,
        string? reason,
        DateTimeOffset checkedAt)
    {
        UserId = userId;
        TenantId = tenantId;
        Resource = resource;
        Action = action;
        Allowed = allowed;
        RoleId = roleId;
        RoleName = roleName;
        Reason = reason;
        CheckedAt = checkedAt;
    }

    public Guid UserId { get; }

    public Guid TenantId { get; }

    public string Resource { get; }

    public string Action { get; }

    public bool Allowed { get; }

    public Guid? RoleId { get; }

    public string? RoleName { get; }

    public string? Reason { get; }

    public DateTimeOffset CheckedAt { get; }
}
