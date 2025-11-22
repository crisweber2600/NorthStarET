using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.Foundation.Identity.Application.Commands;

namespace NorthStarET.Foundation.Identity.API.Controllers;

/// <summary>
/// Authentication endpoints for token exchange, login, and logout
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthenticationController> _logger;
    
    public AuthenticationController(IMediator mediator, ILogger<AuthenticationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    /// <summary>
    /// Exchanges an Entra ID access token for a NorthStar session
    /// </summary>
    /// <param name="request">Token exchange request</param>
    /// <returns>Session information with cookie</returns>
    [HttpPost("exchange-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ExchangeTokenResult>> ExchangeToken([FromBody] ExchangeTokenRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var command = new ExchangeTokenCommand(
            request.AccessToken,
            ipAddress,
            userAgent
        );
        
        var result = await _mediator.Send(command);
        
        // Set session cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = result.ExpiresAt
        };
        
        Response.Cookies.Append("session_id", result.SessionId.ToString(), cookieOptions);
        
        _logger.LogInformation("User {UserPrincipalName} authenticated successfully", result.UserPrincipalName);
        
        return Ok(result);
    }
    
    /// <summary>
    /// Refreshes an existing session
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> RefreshSession()
    {
        // TODO: Implement session refresh
        return Ok(new { Message = "Session refresh endpoint - to be implemented" });
    }
    
    /// <summary>
    /// Logs out the current user by revoking the session
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // TODO: Implement logout
        Response.Cookies.Delete("session_id");
        return NoContent();
    }
}

/// <summary>
/// Request model for token exchange
/// </summary>
public record ExchangeTokenRequest(string AccessToken);
