namespace NorthStarET.NextGen.Lms.Infrastructure.Notifications.Configuration;

/// <summary>
/// Configuration settings for email notifications.
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>
    /// Service name for the Web application used to build verification links.
    /// In Aspire, this is resolved via service discovery (e.g., "http://northstaret-nextgen-lms-web").
    /// For non-Aspire deployments, provide the full URL (e.g., "https://lms.northstaret.org").
    /// </summary>
    public string WebServiceName { get; set; } = "http://northstaret-nextgen-lms-web";
}
