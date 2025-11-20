using NorthStarET.NextGen.Lms.Contracts.Authentication;

namespace NorthStarET.NextGen.Lms.Web.Services;

public class LmsSessionHandler : DelegatingHandler
{
    private readonly ILmsSessionAccessor _sessionAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LmsSessionHandler> _logger;

    public LmsSessionHandler(
        ILmsSessionAccessor sessionAccessor,
        IConfiguration configuration,
        ILogger<LmsSessionHandler> logger)
    {
        _sessionAccessor = sessionAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var enableVerboseLogging = _configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
        
        var sessionId = await _sessionAccessor.GetSessionIdAsync();

        if (!string.IsNullOrEmpty(sessionId))
        {
            request.Headers.Add(AuthenticationHeaders.SessionId, sessionId);
            
            if (enableVerboseLogging)
            {
                _logger.LogInformation("[SESSION-HANDLER] Added session header to request - Method: {Method}, URI: {Uri}, SessionId: {SessionId}", 
                    request.Method,
                    request.RequestUri,
                    sessionId);
            }
        }
        else
        {
            if (enableVerboseLogging)
            {
                _logger.LogWarning("[SESSION-HANDLER] No session ID available for request - Method: {Method}, URI: {Uri}", 
                    request.Method,
                    request.RequestUri);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        if (enableVerboseLogging)
        {
            _logger.LogInformation("[SESSION-HANDLER] Response received - StatusCode: {StatusCode}, HasSessionHeader: {HasSessionHeader}", 
                (int)response.StatusCode,
                !string.IsNullOrEmpty(sessionId));
        }

        return response;
    }
}