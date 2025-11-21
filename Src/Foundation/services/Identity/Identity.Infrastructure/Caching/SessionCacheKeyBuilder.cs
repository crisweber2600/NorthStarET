using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Infrastructure.Caching;

/// <summary>
/// Builds cache keys for session-related data
/// </summary>
public static class SessionCacheKeyBuilder
{
    private const string SessionPrefix = "session";
    private const string UserSessionsPrefix = "user-sessions";
    private const string ClaimsPrefix = "claims";
    
    /// <summary>
    /// Builds cache key for a session by SessionId
    /// </summary>
    public static string ForSession(SessionId sessionId)
        => $"{SessionPrefix}:{sessionId}";
    
    /// <summary>
    /// Builds cache key for a session by SessionId (Guid)
    /// </summary>
    public static string ForSession(Guid sessionId)
        => $"{SessionPrefix}:{sessionId}";
    
    /// <summary>
    /// Builds cache key for all sessions of a user
    /// </summary>
    public static string ForUserSessions(Guid userId)
        => $"{UserSessionsPrefix}:{userId}";
    
    /// <summary>
    /// Builds cache key for user claims
    /// </summary>
    public static string ForUserClaims(Guid userId, Guid tenantId)
        => $"{ClaimsPrefix}:{tenantId}:{userId}";
}
