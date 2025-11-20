using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NorthStarET.NextGen.Lms.Application.Authentication.Commands;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Contracts.Authentication;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ILogger<AuthenticationController> logger;

    public AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "BearerOnly")]
    [HttpPost("exchange-token")]
    public async Task<ActionResult<TokenExchangeResponse>> ExchangeToken([FromBody] TokenExchangeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(
                new ExchangeEntraTokenCommand(
                    request.EntraToken,
                    request.ActiveTenantId,
                    request.IpAddress,
                    request.UserAgent),
                cancellationToken);

            return Ok(MapTokenExchange(result));
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Token exchange failed due to invalid operation");
            return StatusCode(403, new { error = exception.Message });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected failure during token exchange");
            return StatusCode(500, new { error = "Token exchange failed." });
        }
    }

    [Authorize]
    [HttpGet("current-user")]
    public async Task<ActionResult<UserContextDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        if (!TryResolveSessionId(out var sessionId, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            var result = await mediator.Send(new GetCurrentUserContextQuery(sessionId), cancellationToken);
            Response.Headers["X-Session-ExpiresAt"] = result.ExpiresAt.UtcDateTime.ToString("O");
            return Ok(MapUserContext(result.User, result.AvailableTenants));
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Failed to resolve current user context for session {SessionId}", sessionId);
            return StatusCode(403, new { error = exception.Message });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error resolving current user context for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Unable to resolve user context." });
        }
    }

    [Authorize]
    [HttpPost("validate")]
    public async Task<ActionResult<ValidateSessionResponse>> ValidateSession(CancellationToken cancellationToken)
    {
        if (!TryResolveSessionId(out var sessionId, out var errorResult))
        {
            return errorResult!;
        }

        try
        {
            var result = await mediator.Send(new ValidateSessionQuery(sessionId), cancellationToken);

            var response = new ValidateSessionResponse
            {
                IsValid = true,
                UserId = result.UserId,
                ActiveTenantId = result.ActiveTenantId,
                ExpiresAt = result.ExpiresAt
            };

            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Session validation failed for session {SessionId}", sessionId);
            return StatusCode(403, new { error = exception.Message });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error validating session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Session validation failed." });
        }
    }

    private static TokenExchangeResponse MapTokenExchange(TokenExchangeResult result)
    {
        return new TokenExchangeResponse
        {
            LmsAccessToken = result.LmsAccessToken,
            SessionId = result.SessionId,
            ExpiresAt = result.ExpiresAt,
            User = MapUserContext(result.User, result.AvailableTenants)
        };
    }

    private static UserContextDto MapUserContext(UserContextModel user, IReadOnlyCollection<TenantSummaryModel> tenants)
    {
        return new UserContextDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            ActiveTenantId = user.ActiveTenantId,
            ActiveTenantName = user.ActiveTenantName,
            ActiveTenantType = user.ActiveTenantType,
            Role = user.Role,
            AvailableTenants = tenants
                .Select(tenant => new TenantDto
                {
                    TenantId = tenant.TenantId,
                    Name = tenant.Name,
                    Type = tenant.Type
                })
                .ToArray()
        };
    }

    private bool TryResolveSessionId(out Guid sessionId, out ActionResult? errorResult)
    {
        sessionId = Guid.Empty;

        if (!Request.Headers.TryGetValue(AuthenticationHeaders.SessionId, out var headerValues))
        {
            errorResult = BadRequest(new { error = $"Missing {AuthenticationHeaders.SessionId} header." });
            return false;
        }

        if (!Guid.TryParse(headerValues.ToString(), out sessionId))
        {
            errorResult = BadRequest(new { error = "Invalid session identifier." });
            return false;
        }

        errorResult = null;
        return true;
    }
}
