using System;

namespace NorthStarET.NextGen.Lms.Contracts.Authentication;

public sealed class RefreshSessionRequest
{
    public Guid SessionId { get; init; }

    /// <summary>
    /// The current Entra (Azure AD) access token to validate before refreshing the session.
    /// </summary>
    public string EntraToken { get; init; } = string.Empty;
}
