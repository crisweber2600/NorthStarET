using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class RevokeSessionRequest
{
    public Guid SessionId { get; init; }
}
