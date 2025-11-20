namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Configuration;

public sealed class IdentityModuleOptions
{
    public const string SectionName = "IdentityModule";

    public int AuthorizationCacheTtlMinutes { get; set; } = 10;

    public int TenantFactsCacheTtlMinutes { get; set; } = 10;

    public int SessionSlidingExpirationMinutes { get; set; } = 30;

    public int TokenRefreshLeadTimeMinutes { get; set; } = 5;
}
