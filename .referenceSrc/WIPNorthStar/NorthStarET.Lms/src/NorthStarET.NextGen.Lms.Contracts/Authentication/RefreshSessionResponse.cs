using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class RefreshSessionResponse
{
    public DateTimeOffset ExpiresAt { get; init; }

    public DateTimeOffset LastActivityAt { get; init; }
}
