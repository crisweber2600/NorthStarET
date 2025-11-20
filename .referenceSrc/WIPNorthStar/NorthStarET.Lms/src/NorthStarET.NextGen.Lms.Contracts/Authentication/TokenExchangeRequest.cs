using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class TokenExchangeRequest
{
    public string EntraToken { get; init; } = string.Empty;

    public Guid ActiveTenantId { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }
}
