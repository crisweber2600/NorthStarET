using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authorization;

public sealed class TenantMembershipDto
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public Guid? ParentTenantId { get; set; }

    public Guid RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
}
