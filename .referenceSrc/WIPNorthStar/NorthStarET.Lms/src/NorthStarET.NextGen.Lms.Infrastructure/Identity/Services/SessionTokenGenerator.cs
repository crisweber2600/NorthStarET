using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Services;

/// <summary>
/// Session-based token generator for Entra ID federated authentication.
/// Since Entra ID handles authentication, we don't need to generate JWTs.
/// Instead, we return a simple session reference token.
/// </summary>
internal sealed class SessionTokenGenerator : ILmsTokenGenerator
{
    private readonly ILogger<SessionTokenGenerator> logger;
    private readonly IdentityModuleSettings settings;

    public SessionTokenGenerator(
        ILogger<SessionTokenGenerator> logger,
        IOptions<IdentityModuleSettings> settings)
    {
        this.logger = logger;
        this.settings = settings.Value;
    }

    public string GenerateAccessToken(Guid userId, Guid sessionId, DateTimeOffset expiresAt)
    {
        // With Entra ID federated auth, we don't need to generate JWTs.
        // The session ID itself serves as the access token.
        // Frontend will send this in Authorization header, and SessionAuthenticationHandler
        // will validate it by looking up the session in Redis/database.

        var token = $"lms_session_{sessionId:N}";

        logger.LogInformation(
            "Generated session token for user {UserId} session {SessionId} expiring at {ExpiresAt}",
            userId,
            sessionId,
            expiresAt);

        return token;
    }
}
