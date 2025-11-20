namespace NorthStarET.NextGen.Lms.Application.Common.Configuration;

public sealed class IdentityModuleSettings
{
    public int AuthorizationCacheTtlMinutes { get; set; } = 10;

    public int TenantFactsCacheTtlMinutes { get; set; } = 10;

    public int SessionSlidingExpirationMinutes { get; set; } = 60;

    public int TokenRefreshLeadTimeMinutes { get; set; } = 5;

    /// <summary>
    /// Signing key for JWT LMS access tokens. Should be at least 32 characters for HS256.
    /// </summary>
    public string JwtSigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Issuer claim for JWT LMS access tokens.
    /// </summary>
    public string JwtIssuer { get; set; } = "NorthStarET.Lms";

    /// <summary>
    /// Audience claim for JWT LMS access tokens.
    /// </summary>
    public string JwtAudience { get; set; } = "NorthStarET.Lms.Api";

    public bool EnableTestBootstrap { get; set; } = true; // default on for dev/test; turn off in prod
}
