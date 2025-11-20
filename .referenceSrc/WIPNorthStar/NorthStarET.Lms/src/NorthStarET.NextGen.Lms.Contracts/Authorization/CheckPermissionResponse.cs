using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class CheckPermissionResponse
{
    public bool Allowed { get; set; }

    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string Resource { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public Guid? RoleId { get; set; }

    public string? RoleName { get; set; }

    public string? Reason { get; set; }

    public DateTimeOffset CheckedAt { get; set; }
}
