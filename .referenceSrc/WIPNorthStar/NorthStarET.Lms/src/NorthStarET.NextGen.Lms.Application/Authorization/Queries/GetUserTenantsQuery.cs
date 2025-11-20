using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;

namespace NorthStarET.NextGen.Lms.Application.Authorization.Queries;

public sealed record GetUserTenantsQuery(Guid UserId) : IRequest<GetUserTenantsResult>;

internal sealed class GetUserTenantsQueryHandler : IRequestHandler<GetUserTenantsQuery, GetUserTenantsResult>
{
    private readonly IIdentityAuthorizationDataService dataService;
    private readonly ILogger<GetUserTenantsQueryHandler> logger;

    public GetUserTenantsQueryHandler(
        IIdentityAuthorizationDataService dataService,
        ILogger<GetUserTenantsQueryHandler> logger)
    {
        this.dataService = dataService;
        this.logger = logger;
    }

    public async Task<GetUserTenantsResult> Handle(GetUserTenantsQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving tenant memberships for user {UserId}.", request.UserId);
        return await dataService.GetUserTenantsAsync(request.UserId, cancellationToken);
    }
}
