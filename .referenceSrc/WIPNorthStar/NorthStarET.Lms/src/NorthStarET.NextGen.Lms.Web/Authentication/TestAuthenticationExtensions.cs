using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Contracts.Authentication;
using NorthStarET.NextGen.Lms.Web.Services;

namespace NorthStarET.NextGen.Lms.Web.Authentication;

internal static class TestAuthenticationDefaults
{
    public const string AuthenticationScheme = "TestAuth";
    public const string DisplayName = "Playwright Test Authentication";
}

/// <summary>
/// Authentication handler that issues a deterministic System Admin identity for UI automation.
/// Also bootstraps an LMS session by calling the API test endpoint once per browser context.
/// </summary>
internal sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILmsSessionAccessor _sessionAccessor;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string ApiBaseUrlEnvironmentVariable = "NORTHSTARET_LMS_API_BASE_URL";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ILmsSessionAccessor sessionAccessor,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
        : base(options, logger, encoder)
    {
        _sessionAccessor = sessionAccessor;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(TestAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "playwright-system-admin"));
        identity.AddClaim(new Claim(ClaimTypes.Name, "Playwright System Admin"));
        identity.AddClaim(new Claim(ClaimTypes.Email, "playwright-admin@example.edu"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "SystemAdmin"));

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthenticationDefaults.AuthenticationScheme);

        // Ensure LMS session exists for API calls
        var sessionId = await _sessionAccessor.GetSessionIdAsync();
        if (string.IsNullOrEmpty(sessionId))
        {
            try
            {
                // Call API test bootstrap to create session
                var baseUrl = Environment.GetEnvironmentVariable(ApiBaseUrlEnvironmentVariable)
                    ?? _configuration["DownstreamApi:BaseUrl"]
                    ?? "https://localhost:7001";

                var client = _httpClientFactory.CreateClient();

                if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                {
                    client.BaseAddress = baseUri;
                }
                else
                {
                    Logger.LogWarning("[TEST-AUTH] Skipping bootstrap because API base URL '{BaseUrl}' is invalid", baseUrl);
                    return AuthenticateResult.Success(ticket);
                }

                var payload = new TestBootstrapRequest
                {
                    Email = "playwright-admin@example.edu",
                    DisplayName = "Playwright System Admin",
                    ActiveTenantId = Guid.Empty
                };

                var response = await client.PostAsJsonAsync("/api/auth/test-bootstrap", payload);
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadFromJsonAsync<TokenExchangeResponse>();
                    if (body != null)
                    {
                        await _sessionAccessor.SetSessionIdAsync(body.SessionId.ToString());
                        Logger.LogInformation("[TEST-AUTH] Created session via bootstrap at {BaseUrl}", client.BaseAddress);
                    }
                    else
                    {
                        Logger.LogWarning("[TEST-AUTH] Bootstrap response body was empty (status {StatusCode})", (int)response.StatusCode);
                    }
                }
                else
                {
                    Logger.LogWarning(
                        "[TEST-AUTH] Bootstrap request failed with status {StatusCode} (BaseUrl={BaseUrl})",
                        (int)response.StatusCode,
                        client.BaseAddress);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "[TEST-AUTH] Exception thrown while attempting to bootstrap session");
            }
        }

        return AuthenticateResult.Success(ticket);
    }
}

internal static class TestAuthenticationExtensions
{
    public static AuthenticationBuilder AddTestAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
            TestAuthenticationDefaults.AuthenticationScheme,
            TestAuthenticationDefaults.DisplayName,
            _ => { });
    }
}