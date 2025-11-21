using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Infrastructure.Repositories;

/// <summary>
/// Repository interface for Session entity
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Gets a session by SessionId
    /// </summary>
    Task<Session?> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active sessions for a user
    /// </summary>
    Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new session
    /// </summary>
    Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing session
    /// </summary>
    Task UpdateAsync(Session session, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revokes a session
    /// </summary>
    Task RevokeAsync(SessionId sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
