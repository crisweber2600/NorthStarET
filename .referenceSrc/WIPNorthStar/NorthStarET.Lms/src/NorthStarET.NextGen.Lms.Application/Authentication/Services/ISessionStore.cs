using System;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;

namespace NorthStarET.NextGen.Lms.Application.Authentication.Services;

public interface ISessionStore
{
    Task CacheSessionAsync(Session session, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task<SessionCacheModel?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
