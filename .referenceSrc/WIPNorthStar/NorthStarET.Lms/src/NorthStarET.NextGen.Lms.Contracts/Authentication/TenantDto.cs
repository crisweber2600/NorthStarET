using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class TenantDto
{
    public Guid TenantId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;
}
