using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Domain.Auditing;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common.Services;

/// <summary>
/// HTTP context-based implementation of ICurrentUserService.
/// Extracts user context from ClaimsPrincipal populated by SessionAuthenticationHandler.
/// </summary>
internal sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : null;
        }
    }

    public ActorRole Role
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return ActorRole.SystemService;
            }

            var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value
                ?? principal.FindFirst("role")?.Value
                ?? principal.FindFirst("lms:role")?.Value;

            if (!string.IsNullOrWhiteSpace(roleClaim))
            {
                if (roleClaim.Equals("SystemAdmin", StringComparison.OrdinalIgnoreCase)
                    || roleClaim.Equals("PlatformAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return ActorRole.PlatformAdmin;
                }

                if (roleClaim.Equals("DistrictAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    return ActorRole.DistrictAdmin;
                }

                if (roleClaim.Equals("SystemService", StringComparison.OrdinalIgnoreCase))
                {
                    return ActorRole.SystemService;
                }
            }

            // Fallback: a missing tenant assignment indicates platform-wide scope
            return DistrictId.HasValue ? ActorRole.DistrictAdmin : ActorRole.PlatformAdmin;
        }
    }

    public Guid? DistrictId
    {
        get
        {
            var districtIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("lms:tenant_id");
            return districtIdClaim != null && Guid.TryParse(districtIdClaim.Value, out var districtId)
                ? districtId
                : null;
        }
    }

    public Guid? CorrelationId
    {
        get
        {
            // Use TraceIdentifier from HttpContext as correlation ID
            var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier;

            // Generate a deterministic GUID from the trace identifier using SHA256
            if (!string.IsNullOrEmpty(traceId))
            {
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(traceId));

                // Take first 16 bytes of the hash to create a GUID
                var guidBytes = new byte[16];
                Array.Copy(hash, 0, guidBytes, 0, 16);
                return new Guid(guidBytes);
            }

            return null;
        }
    }
}
