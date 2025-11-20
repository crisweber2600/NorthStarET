using System;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services;

/// <summary>
/// Service for generating JWT-based LMS access tokens with session metadata.
/// </summary>
public interface ILmsTokenGenerator
{
    /// <summary>
    /// Generates a JWT access token with session claims.
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="sessionId">The session identifier</param>
    /// <param name="expiresAt">Token expiration time</param>
    /// <returns>A signed JWT token string</returns>
    string GenerateAccessToken(Guid userId, Guid sessionId, DateTimeOffset expiresAt);
}
