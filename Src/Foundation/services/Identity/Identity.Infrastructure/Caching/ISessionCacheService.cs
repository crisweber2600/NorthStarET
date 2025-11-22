using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Infrastructure.Caching;

/// <summary>
/// Service for caching session data in Redis
/// </summary>
public interface ISessionCacheService
{
    /// <summary>
    /// Gets a session from cache by SessionId
    /// </summary>
    Task<Session?> GetSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stores a session in cache with appropriate TTL
    /// </summary>
    Task SetSessionAsync(Session session, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a session from cache
    /// </summary>
    Task RemoveSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets cached claims for a user in a tenant
    /// </summary>
    Task<string?> GetUserClaimsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Caches user claims
    /// </summary>
    Task SetUserClaimsAsync(Guid userId, Guid tenantId, string claimsJson, TimeSpan expiration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes cached user claims
    /// </summary>
    Task RemoveUserClaimsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
