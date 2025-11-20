using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Domain.Identity.Repositories;

public interface ISessionRepository : IRepository<Session>
{
    Task<Session?> GetActiveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Session>> GetSessionsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Session?> GetByTokenHashAsync(string entraTokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Session>> GetByTenantAsync(TenantId tenantId, CancellationToken cancellationToken = default);
}
