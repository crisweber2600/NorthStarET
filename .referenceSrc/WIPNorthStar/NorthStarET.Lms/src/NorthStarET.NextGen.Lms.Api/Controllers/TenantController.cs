using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authorization.Commands;
using NorthStarET.NextGen.Lms.Application.Authorization.Queries;
using NorthStarET.NextGen.Lms.Contracts.Authorization;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

[ApiController]
[Route("api/tenant")]
[Authorize]
public sealed class TenantController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TenantController> _logger;

    public TenantController(IMediator mediator, ILogger<TenantController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all tenants where the current user has membership.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's tenant memberships</returns>
    [HttpGet("list")]
    public async Task<ActionResult<UserTenantsResponse>> GetUserTenants(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetUserTenantsQuery(userId), cancellationToken);

            var response = new UserTenantsResponse
            {
                UserId = result.UserId,
                Tenants = result.Tenants.Select(t => new TenantMembershipDto
                {
                    TenantId = t.TenantId,
                    Name = t.TenantName,
                    Type = t.TenantType.ToString(),
                    ParentTenantId = t.ParentTenantId,
                    RoleId = t.RoleId,
                    RoleName = t.RoleName,
                    GrantedAt = t.GrantedAt,
                    ExpiresAt = t.ExpiresAt
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve tenants for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve user tenants" });
        }
    }

    /// <summary>
    /// Switch the active tenant context for a user's session.
    /// </summary>
    /// <param name="request">Tenant switch request containing session ID, user ID, and target tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPost("switch")]
    public async Task<IActionResult> SwitchTenant(
        [FromBody] SwitchTenantRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request payload is required." });
        }

        try
        {
            await _mediator.Send(
                new SwitchTenantContextCommand(request.SessionId, request.UserId, request.TargetTenantId),
                cancellationToken);

            _logger.LogInformation(
                "User {UserId} successfully switched to tenant {TargetTenantId}",
                request.UserId,
                request.TargetTenantId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tenant switch failed for user {UserId}", request.UserId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tenant switch failed for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "Failed to switch tenant context" });
        }
    }
}
