using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Authorization.Queries;
using NorthStarET.NextGen.Lms.Contracts.Authorization;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

[ApiController]
[Route("v1/identity")]
public sealed class AuthorizationController : ControllerBase
{
    private const string AuthorizationCacheProfile = "AuthorizationNoStore";
    private readonly IMediator mediator;
    private readonly ILogger<AuthorizationController> logger;

    public AuthorizationController(IMediator mediator, ILogger<AuthorizationController> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    [Authorize]
    [HttpPost("authorize/check")]
    [ResponseCache(CacheProfileName = AuthorizationCacheProfile)]
    public async Task<ActionResult<CheckPermissionResponse>> CheckPermission([FromBody] CheckPermissionRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request payload is required." });
        }

        try
        {
            var decision = await mediator.Send(
                new CheckPermissionQuery(
                    request.UserId,
                    request.TenantId,
                    request.Resource,
                    request.Action,
                    request.Context),
                cancellationToken);

            return Ok(MapDecision(decision));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to evaluate authorization decision for user {UserId} tenant {TenantId} resource {Resource} action {Action}.", request.UserId, request.TenantId, request.Resource, request.Action);
            return StatusCode(500, new { error = "Authorization check failed." });
        }
    }

    [Authorize]
    [HttpPost("authorize/batch")]
    [ResponseCache(CacheProfileName = AuthorizationCacheProfile)]
    public async Task<ActionResult<BatchCheckPermissionResponse>> BatchCheckPermissions([FromBody] BatchCheckPermissionRequest request, CancellationToken cancellationToken)
    {
        if (request?.Checks is null || request.Checks.Count == 0)
        {
            return BadRequest(new { error = "At least one authorization check must be provided." });
        }

        try
        {
            var results = new List<CheckPermissionResponse>(request.Checks.Count);

            foreach (var check in request.Checks)
            {
                var decision = await mediator.Send(
                    new CheckPermissionQuery(
                        check.UserId,
                        check.TenantId,
                        check.Resource,
                        check.Action,
                        check.Context),
                    cancellationToken);

                results.Add(MapDecision(decision));
            }

            return Ok(new BatchCheckPermissionResponse
            {
                Results = results
            });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to evaluate batch authorization checks.");
            return StatusCode(500, new { error = "Batch authorization check failed." });
        }
    }

    [Authorize]
    [HttpGet("tenants/{userId:guid}")]
    [ResponseCache(CacheProfileName = AuthorizationCacheProfile)]
    public async Task<ActionResult<UserTenantsResponse>> GetUserTenants(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new GetUserTenantsQuery(userId), cancellationToken);

            if (!result.Tenants.Any())
            {
                return NotFound(new { error = "No tenant memberships found for the specified user." });
            }

            return Ok(MapTenants(result));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve tenant memberships for user {UserId}.", userId);
            return StatusCode(500, new { error = "Failed to retrieve tenant memberships." });
        }
    }

    private static CheckPermissionResponse MapDecision(AuthorizationDecision decision)
    {
        return new CheckPermissionResponse
        {
            Allowed = decision.Allowed,
            UserId = decision.UserId,
            TenantId = decision.TenantId,
            Resource = decision.Resource,
            Action = decision.Action,
            RoleId = decision.RoleId,
            RoleName = decision.RoleName,
            Reason = decision.Reason,
            CheckedAt = decision.CheckedAt
        };
    }

    private static UserTenantsResponse MapTenants(GetUserTenantsResult result)
    {
        var memberships = result.Tenants
            .Select(membership => new TenantMembershipDto
            {
                TenantId = membership.TenantId,
                Name = membership.TenantName,
                Type = membership.TenantType.ToString(),
                ParentTenantId = membership.ParentTenantId,
                RoleId = membership.RoleId,
                RoleName = membership.RoleName,
                GrantedAt = membership.GrantedAt,
                ExpiresAt = membership.ExpiresAt
            })
            .ToArray();

        return new UserTenantsResponse
        {
            UserId = result.UserId,
            Tenants = memberships
        };
    }
}
