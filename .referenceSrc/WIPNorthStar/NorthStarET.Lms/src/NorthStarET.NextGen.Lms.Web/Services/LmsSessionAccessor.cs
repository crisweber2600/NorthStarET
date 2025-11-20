using Microsoft.AspNetCore.Http;

namespace NorthStarET.NextGen.Lms.Web.Services;

public interface ILmsSessionAccessor
{
    Task<string?> GetSessionIdAsync();
    Task SetSessionIdAsync(string sessionId);
    Task ClearSessionAsync();
}

public class LmsSessionAccessor : ILmsSessionAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LmsSessionAccessor> _logger;
    private const string SessionIdCookieName = "LmsSessionId";
    private const string SessionIdItemKey = "__LmsSessionId";

    public LmsSessionAccessor(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<LmsSessionAccessor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<string?> GetSessionIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            var enableVerboseLogging = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
            if (enableVerboseLogging)
            {
                _logger.LogWarning("[SESSION-ACCESSOR] HttpContext is null when retrieving session ID");
            }
            return Task.FromResult<string?>(null);
        }

        var sessionId = httpContext.Request.Cookies[SessionIdCookieName];

        if (httpContext.Items.TryGetValue(SessionIdItemKey, out var sessionItem) &&
            sessionItem is string sessionFromItems &&
            !string.IsNullOrEmpty(sessionFromItems))
        {
            sessionId = sessionFromItems;
        }

        var enableVerbose = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
        if (enableVerbose)
        {
            _logger.LogInformation("[SESSION-ACCESSOR] Retrieved session ID from cookie - HasValue: {HasValue}, CookieName: {CookieName}",
                !string.IsNullOrEmpty(sessionId),
                SessionIdCookieName);
        }

        return Task.FromResult<string?>(sessionId);
    }

    public Task SetSessionIdAsync(string sessionId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            var enableVerboseLogging = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
            if (enableVerboseLogging)
            {
                _logger.LogWarning("[SESSION-ACCESSOR] HttpContext is null when setting session ID");
            }
            return Task.CompletedTask;
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax, // Lax allows cookie to be sent on top-level navigation (OAuth redirects)
            Expires = DateTimeOffset.UtcNow.AddHours(8), // Match session expiration
            IsEssential = true // Mark as essential for authentication
        };

        httpContext.Response.Cookies.Append(SessionIdCookieName, sessionId, cookieOptions);
        httpContext.Items[SessionIdItemKey] = sessionId;

        var enableVerbose = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
        if (enableVerbose)
        {
            _logger.LogInformation("[SESSION-ACCESSOR] Set session cookie - SessionId: {SessionId}, HttpOnly: {HttpOnly}, Secure: {Secure}, SameSite: {SameSite}, Expires: {Expires}",
                sessionId,
                cookieOptions.HttpOnly,
                cookieOptions.Secure,
                cookieOptions.SameSite,
                cookieOptions.Expires);
        }

        return Task.CompletedTask;
    }

    public Task ClearSessionAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            var enableVerboseLogging = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
            if (enableVerboseLogging)
            {
                _logger.LogWarning("[SESSION-ACCESSOR] HttpContext is null when clearing session");
            }
            return Task.CompletedTask;
        }

        httpContext.Response.Cookies.Delete(SessionIdCookieName);
        httpContext.Items.Remove(SessionIdItemKey);

        var enableVerbose = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
        if (enableVerbose)
        {
            _logger.LogInformation("[SESSION-ACCESSOR] Cleared session cookie - CookieName: {CookieName}", SessionIdCookieName);
        }

        return Task.CompletedTask;
    }
}