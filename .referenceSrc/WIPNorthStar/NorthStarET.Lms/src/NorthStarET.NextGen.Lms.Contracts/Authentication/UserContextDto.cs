using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class UserContextDto
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public Guid ActiveTenantId { get; init; }

    public string ActiveTenantName { get; init; } = string.Empty;

    public string ActiveTenantType { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public IReadOnlyCollection<TenantDto> AvailableTenants { get; init; } = Array.Empty<TenantDto>();
}
