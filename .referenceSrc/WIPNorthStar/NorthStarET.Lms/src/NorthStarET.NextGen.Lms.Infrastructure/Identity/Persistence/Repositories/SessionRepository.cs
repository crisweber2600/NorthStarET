using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;

internal sealed class SessionRepository : RepositoryBase<Session>, ISessionRepository
{
    public SessionRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Session?> GetActiveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await DbSet
            .Where(session => session.Id == sessionId && !session.IsRevoked && session.ExpiresAt > now)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Session>> GetSessionsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(session => session.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetByTokenHashAsync(string entraTokenHash, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(session => session.EntraTokenHash == entraTokenHash)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Session>> GetByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(session => session.ActiveTenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}
