using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Services;

public interface IAuthorizationService
{
    Task<AuthorizationDecision> CheckPermissionAsync(
        Guid userId,
        Guid tenantId,
        string resource,
        string action,
        IReadOnlyDictionary<string, string>? context,
        CancellationToken cancellationToken);
}
