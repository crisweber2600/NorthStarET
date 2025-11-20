using System;
using System.Collections.Generic;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class TokenExchangeResponse
{
    public string LmsAccessToken { get; init; } = string.Empty;

    public Guid SessionId { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public UserContextDto User { get; init; } = new();
}
