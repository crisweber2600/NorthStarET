using System;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;

namespace NorthStarET.NextGen.Lms.Application.Common.Caching;

public interface IAuthorizationCache
{
    Task<AuthorizationDecision?> GetAsync(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        CancellationToken cancellationToken);

    Task SetAsync(
        AuthorizationDecision decision,
        TimeSpan timeToLive,
        CancellationToken cancellationToken);

    Task ClearForUserAndTenantAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken);
}
