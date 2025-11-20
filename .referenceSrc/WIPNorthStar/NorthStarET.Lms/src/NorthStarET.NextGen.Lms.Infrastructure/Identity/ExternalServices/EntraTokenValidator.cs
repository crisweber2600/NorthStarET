using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.ExternalServices;

internal sealed class EntraTokenValidator : IEntraTokenValidator
{
    private readonly IConfigurationManager<OpenIdConnectConfiguration> configurationManager;
    private readonly EntraIdOptions options;
    private readonly ILogger<EntraTokenValidator> logger;

    public EntraTokenValidator(IOptions<EntraIdOptions> options, ILogger<EntraTokenValidator> logger, IDocumentRetriever documentRetriever)
    {
        this.options = options.Value;
        this.logger = logger;

        var authority = BuildAuthority(this.options.Instance, this.options.TenantId);
        var metadataAddress = $"{authority}/v2.0/.well-known/openid-configuration";
        configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever);
    }

    public async Task<ClaimsPrincipal> ValidateAsync(string entraToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entraToken))
        {
            throw new ArgumentException("Entra token cannot be null or whitespace.", nameof(entraToken));
        }

        var configuration = await configurationManager.GetConfigurationAsync(cancellationToken);

        // Build a list of acceptable issuers. Some tokens use the v2.0 issuer (contains /v2.0)
        // while configuration.Issuer may be null in some metadata documents, so include both
        // the authority with and without /v2.0 as fallbacks.
        var authorityBase = BuildAuthority(options.Instance, options.TenantId).TrimEnd('/');
        var v2Issuer = authorityBase + "/v2.0";

        var validIssuers = new List<string>();
        if (!string.IsNullOrWhiteSpace(configuration?.Issuer))
        {
            validIssuers.Add(configuration.Issuer);
        }

        // Add both v2.0 and non-v2 forms as fallbacks
        validIssuers.Add(v2Issuer);
        validIssuers.Add(authorityBase);

        validIssuers = validIssuers.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var validationParameters = new TokenValidationParameters
        {
            ValidIssuers = validIssuers,
            ValidateIssuer = true,
            ValidAudiences = new[] { options.ClientId },
            ValidateAudience = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(entraToken, validationParameters, out _);
            return principal;
        }
        catch (SecurityTokenException exception)
        {
            logger.LogWarning(exception, "Failed to validate Entra token");
            throw;
        }
    }

    private static string BuildAuthority(string instance, string tenantId)
    {
        var trimmedInstance = instance.TrimEnd('/');
        return string.IsNullOrWhiteSpace(tenantId)
            ? trimmedInstance
            : $"{trimmedInstance}/{tenantId}";
    }
}
