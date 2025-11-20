using NorthStarET.NextGen.Lms.Contracts.Authentication;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;

namespace NorthStarET.NextGen.Lms.Web.Services;

public interface ITokenExchangeService
{
    Task<string?> ExchangeTokenAsync(string accessToken, HttpContext httpContext);
}

public class TokenExchangeService : ITokenExchangeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenExchangeService> _logger;

    public TokenExchangeService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TokenExchangeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> ExchangeTokenAsync(string accessToken, HttpContext httpContext)
    {
        var enableVerboseLogging = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var clientIpAddress = GetClientIpAddress(httpContext);
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            if (enableVerboseLogging)
            {
                _logger.LogInformation("[TOKEN-EXCHANGE] Starting token exchange at {Timestamp}", DateTimeOffset.UtcNow);
                _logger.LogInformation("[TOKEN-EXCHANGE] Client IP: {IpAddress}, User-Agent: {UserAgent}", 
                    clientIpAddress ?? "<unknown>", 
                    userAgent ?? "<unknown>");
            }

            var request = new TokenExchangeRequest 
            { 
                EntraToken = accessToken,
                ActiveTenantId = Guid.Empty, // Backend will auto-select first available tenant
                IpAddress = clientIpAddress,
                UserAgent = userAgent
            };

            if (enableVerboseLogging)
            {
                _logger.LogInformation("[TOKEN-EXCHANGE] Request payload - ActiveTenantId: {TenantId}, IpAddress: {IpAddress}", 
                    request.ActiveTenantId, 
                    request.IpAddress ?? "<null>");
            }

            // Create HttpClient and manually add the Authorization header with the access token
            var httpClient = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["DownstreamApi:BaseUrl"] ?? "https://localhost:7001";
            httpClient.BaseAddress = new Uri(baseUrl);
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            if (enableVerboseLogging)
            {
                _logger.LogInformation("[TOKEN-EXCHANGE] Calling API at {BaseUrl}/api/auth/exchange-token", baseUrl);
                _logger.LogInformation("[TOKEN-EXCHANGE] Authorization header set with Bearer token (length: {TokenLength})", 
                    accessToken.Length);
            }

            var apiCallStart = Stopwatch.GetTimestamp();
            var httpResponse = await httpClient.PostAsJsonAsync("/api/auth/exchange-token", request);
            var apiCallElapsed = Stopwatch.GetElapsedTime(apiCallStart);

            if (enableVerboseLogging)
            {
                _logger.LogInformation("[TOKEN-EXCHANGE] API call completed in {Elapsed}ms with status code: {StatusCode}", 
                    apiCallElapsed.TotalMilliseconds,
                    (int)httpResponse.StatusCode);
            }
            
            if (httpResponse.IsSuccessStatusCode)
            {
                var deserializeStart = Stopwatch.GetTimestamp();
                var response = await httpResponse.Content.ReadFromJsonAsync<TokenExchangeResponse>();
                var deserializeElapsed = Stopwatch.GetElapsedTime(deserializeStart);
                
                if (enableVerboseLogging)
                {
                    _logger.LogInformation("[TOKEN-EXCHANGE] Response deserialized in {Elapsed}ms. Response is null: {IsNull}", 
                        deserializeElapsed.TotalMilliseconds,
                        response == null);
                }
                
                if (response != null)
                {
                    stopwatch.Stop();
                    _logger.LogInformation("Successfully exchanged Entra token for LMS session: {SessionId}", response.SessionId);
                    
                    if (enableVerboseLogging)
                    {
                        _logger.LogInformation("[TOKEN-EXCHANGE] Total exchange time: {Elapsed}ms", stopwatch.ElapsedMilliseconds);
                        _logger.LogInformation("[TOKEN-EXCHANGE] Response details - SessionId: {SessionId}, LmsAccessToken length: {AccessTokenLength}", 
                            response.SessionId, 
                            response.LmsAccessToken?.Length ?? 0);
                    }
                    
                    return response.SessionId.ToString();
                }
                else
                {
                    if (enableVerboseLogging)
                    {
                        var responseBody = await httpResponse.Content.ReadAsStringAsync();
                        _logger.LogWarning("[TOKEN-EXCHANGE] Null response after deserialization. Raw response body: {ResponseBody}", 
                            responseBody);
                    }
                }
            }
            else
            {
                var responseBody = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Token exchange failed with status code: {StatusCode}", httpResponse.StatusCode);
                
                if (enableVerboseLogging)
                {
                    _logger.LogWarning("[TOKEN-EXCHANGE] API error response - StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}, Body: {ResponseBody}", 
                        (int)httpResponse.StatusCode,
                        httpResponse.ReasonPhrase ?? "<null>",
                        responseBody);
                }
            }

            _logger.LogWarning("Token exchange returned null response");
            return null;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP request failed during token exchange");
            
            if (enableVerboseLogging)
            {
                _logger.LogError("[TOKEN-EXCHANGE] HttpRequestException after {Elapsed}ms - StatusCode: {StatusCode}, Message: {Message}, InnerException: {InnerException}", 
                    stopwatch.ElapsedMilliseconds,
                    ex.StatusCode,
                    ex.Message,
                    ex.InnerException?.Message ?? "<none>");
            }
            
            return null;
        }
        catch (TaskCanceledException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Token exchange request timed out");
            
            if (enableVerboseLogging)
            {
                _logger.LogError("[TOKEN-EXCHANGE] Request timeout after {Elapsed}ms - Message: {Message}", 
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
            }
            
            return null;
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to deserialize token exchange response");
            
            if (enableVerboseLogging)
            {
                _logger.LogError("[TOKEN-EXCHANGE] JSON deserialization error after {Elapsed}ms - Path: {Path}, LineNumber: {LineNumber}, Message: {Message}", 
                    stopwatch.ElapsedMilliseconds,
                    ex.Path ?? "<unknown>",
                    ex.LineNumber,
                    ex.Message);
            }
            
            return null;
        }
    }

    private static string? GetClientIpAddress(HttpContext httpContext)
    {
        // Check for X-Forwarded-For header (for proxies/load balancers)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the last one (IP seen by the last trusted proxy)
            return forwardedFor.Split(',').LastOrDefault()?.Trim();
        }

        // Fall back to RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}