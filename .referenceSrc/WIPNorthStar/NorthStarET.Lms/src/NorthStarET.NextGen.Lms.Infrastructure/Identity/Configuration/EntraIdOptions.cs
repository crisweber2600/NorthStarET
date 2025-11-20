namespace NorthStarET.NextGen.Lms.Infrastructure.Identity.Configuration;

public sealed class EntraIdOptions
{
    public const string SectionName = "EntraId";

    public string Instance { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/signin-oidc";

    public string ClientSecret { get; set; } = string.Empty;

    public string[] Scopes { get; set; } = [];
}
