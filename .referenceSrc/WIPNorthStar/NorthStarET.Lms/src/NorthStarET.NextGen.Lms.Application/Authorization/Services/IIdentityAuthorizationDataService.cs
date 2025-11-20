using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Services;

public interface IIdentityAuthorizationDataService
{
    Task<AuthorizationDecision> FetchDecisionAsync(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken);

    Task<GetUserTenantsResult> GetUserTenantsAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
