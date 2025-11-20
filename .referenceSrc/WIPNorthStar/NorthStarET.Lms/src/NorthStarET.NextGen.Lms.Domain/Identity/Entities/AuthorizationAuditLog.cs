using System;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Entities;

public sealed class AuthorizationAuditLog
{
    public AuthorizationAuditLog(
        Guid id,
        Guid userId,
        TenantId tenantId,
        string resource,
        string action,
        bool allowed,
        DateTimeOffset evaluatedAt,
        Guid? roleId,
        string? roleName,
        string? reason,
        string? traceId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Audit id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException("Resource cannot be null or whitespace.", nameof(resource));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));
        }

        Id = id;
        UserId = userId;
        TenantId = tenantId;
        Resource = resource;
        Action = action;
        Allowed = allowed;
        EvaluatedAt = evaluatedAt;
        RoleId = roleId;
        RoleName = roleName;
        Reason = reason;
        TraceId = traceId;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public TenantId TenantId { get; }

    public string Resource { get; }

    public string Action { get; }

    public bool Allowed { get; }

    public DateTimeOffset EvaluatedAt { get; }

    public Guid? RoleId { get; }

    public string? RoleName { get; }

    public string? Reason { get; }

    public string? TraceId { get; }
}
