using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Application.Interfaces;

public interface ISessionManager
{
    Task<Session> CreateSessionAsync(
        Guid tenantId,
        Guid userId,
        string userPrincipalName,
        bool isAdmin,
        string? ipAddress = null,
        string? userAgent = null,
        string? claimsJson = null,
        CancellationToken cancellationToken = default);
    
    Task<Session?> GetSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default);
    Task<Session> RefreshSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(SessionId sessionId, CancellationToken cancellationToken = default);
}
