using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Queries;

public sealed record CheckPermissionQuery(
    Guid UserId,
    Guid TenantId,
    string Resource,
    string Action,
    IReadOnlyDictionary<string, string>? Context) : IRequest<AuthorizationDecision>;

internal sealed class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, AuthorizationDecision>
{
    private readonly IAuthorizationService authorizationService;
    private readonly ILogger<CheckPermissionQueryHandler> logger;

    public CheckPermissionQueryHandler(
        IAuthorizationService authorizationService,
        ILogger<CheckPermissionQueryHandler> logger)
    {
        this.authorizationService = authorizationService;
        this.logger = logger;
    }

    public async Task<AuthorizationDecision> Handle(CheckPermissionQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Handling authorization request for user {UserId} tenant {TenantId} resource {Resource} action {Action}.",
            request.UserId,
            request.TenantId,
            request.Resource,
            request.Action);

        return await authorizationService.CheckPermissionAsync(
            request.UserId,
            request.TenantId,
            request.Resource,
            request.Action,
            request.Context,
            cancellationToken);
    }
}
