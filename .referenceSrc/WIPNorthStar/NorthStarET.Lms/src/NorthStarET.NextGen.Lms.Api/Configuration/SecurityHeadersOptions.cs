namespace NorthStarET.NextGen.Lms.Api.Configuration;

public sealed class SecurityHeadersOptions
{
    public const string SectionName = "SecurityHeaders";

    public string ContentSecurityPolicy { get; set; } = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none'";
}
