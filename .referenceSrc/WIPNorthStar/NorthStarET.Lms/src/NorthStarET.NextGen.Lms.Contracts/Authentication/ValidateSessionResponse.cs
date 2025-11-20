using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class ValidateSessionResponse
{
    public bool IsValid { get; init; }

    public Guid UserId { get; init; }

    public Guid ActiveTenantId { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
}
