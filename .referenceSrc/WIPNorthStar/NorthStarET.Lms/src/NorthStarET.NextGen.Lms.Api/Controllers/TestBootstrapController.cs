using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Application.Common.Configuration; // Added using for IdentityModuleSettings
using NorthStarET.NextGen.Lms.Contracts.Authentication;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

[ApiController]
[Route("api/auth/test-bootstrap")]
public sealed class TestBootstrapController : ControllerBase
{
    private readonly ITokenExchangeService _tokenExchangeService;
    private readonly IdentityModuleSettings _settings;

    public TestBootstrapController(
        ITokenExchangeService tokenExchangeService,
        IOptions<IdentityModuleSettings> settings)
    {
        _tokenExchangeService = tokenExchangeService;
        _settings = settings.Value;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<TokenExchangeResponse>> BootstrapAsync([FromBody] TestBootstrapRequest request, CancellationToken cancellationToken)
    {
        // Only enable when explicitly allowed via configuration (dev/test only)
        if (!_settings.EnableTestBootstrap)
        {
            return Forbid();
        }

        var entraLikeToken = GenerateDeterministicTestToken(request.Email);
        var context = new TokenExchangeCommandContext(
            entraLikeToken,
            request.ActiveTenantId,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"].ToString());

        var result = await _tokenExchangeService.ExchangeAsync(context, cancellationToken);

        var response = new TokenExchangeResponse
        {
            LmsAccessToken = result.LmsAccessToken,
            SessionId = result.SessionId,
            ExpiresAt = result.ExpiresAt,
            User = new UserContextDto
            {
                Id = result.User.Id,
                Email = result.User.Email,
                DisplayName = result.User.DisplayName,
                ActiveTenantId = result.User.ActiveTenantId,
                ActiveTenantName = result.User.ActiveTenantName,
                ActiveTenantType = result.User.ActiveTenantType,
                Role = result.User.Role,
                AvailableTenants = result.AvailableTenants.Select(t => new TenantDto
                {
                    TenantId = t.TenantId,
                    Name = t.Name,
                    Type = t.Type
                }).ToArray()
            }
        };

        return Ok(response);
    }

    private static string GenerateDeterministicTestToken(string email)
    {
        // A stable string unique enough to compute the same token hash across requests for idempotency
        return $"TEST-AUTH::{email.ToLowerInvariant()}";
    }
}
