using Microsoft.EntityFrameworkCore;
using NorthStarET.Foundation.Identity.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;
using NorthStarET.Foundation.Identity.Infrastructure.Data;

namespace NorthStarET.Foundation.Identity.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Session entity
/// </summary>
public class SessionRepository : ISessionRepository
{
    private readonly IdentityDbContext _context;
    
    public SessionRepository(IdentityDbContext context)
    {
        _context = context;
    }
    
    public async Task<Session?> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && !s.IsRevoked && s.DeletedAt == null, cancellationToken);
    }
    
    public async Task<IEnumerable<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Session> AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        await _context.Sessions.AddAsync(session, cancellationToken);
        return session;
    }
    
    public async Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        _context.Sessions.Update(session);
        await Task.CompletedTask;
    }
    
    public async Task RevokeAsync(SessionId sessionId, CancellationToken cancellationToken = default)
    {
        var session = await GetBySessionIdAsync(sessionId, cancellationToken);
        if (session != null)
        {
            session.Revoke();
            _context.Sessions.Update(session);
        }
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
