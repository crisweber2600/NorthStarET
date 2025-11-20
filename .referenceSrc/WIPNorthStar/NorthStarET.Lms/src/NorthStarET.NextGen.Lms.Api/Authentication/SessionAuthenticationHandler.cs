using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;

namespace NorthStarET.NextGen.Lms.Api.Authentication;

public sealed class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IMediator mediator;
    private readonly IConfiguration configuration;

    public SessionAuthenticationHandler(
        IMediator mediator,
        IConfiguration configuration,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        this.mediator = mediator;
        this.configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var enableVerboseLogging = configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);
        var stopwatch = Stopwatch.StartNew();

        if (enableVerboseLogging)
        {
            Logger.LogInformation("[SESSION-AUTH] Authentication attempt at {Timestamp} for {Path}", 
                DateTimeOffset.UtcNow,
                Request.Path);
        }

        if (!Request.Headers.TryGetValue(SessionAuthenticationDefaults.SessionHeaderName, out var headerValues))
        {
            if (enableVerboseLogging)
            {
                Logger.LogWarning("[SESSION-AUTH] Session header '{HeaderName}' not found in request. Available headers: {Headers}", 
                    SessionAuthenticationDefaults.SessionHeaderName,
                    string.Join(", ", Request.Headers.Keys));
            }
            
            return AuthenticateResult.NoResult();
        }

        var headerValue = headerValues.ToString();
        
        if (enableVerboseLogging)
        {
            Logger.LogInformation("[SESSION-AUTH] Session header found with value: {HeaderValue}", headerValue);
        }

        if (!Guid.TryParse(headerValue, out var sessionId))
        {
            if (enableVerboseLogging)
            {
                Logger.LogWarning("[SESSION-AUTH] Failed to parse session ID as GUID: {HeaderValue}", headerValue);
            }
            
            return AuthenticateResult.Fail("Invalid session identifier header.");
        }

        if (enableVerboseLogging)
        {
            Logger.LogInformation("[SESSION-AUTH] Validating session ID: {SessionId}", sessionId);
        }

        ValidateSessionResult validation;
        try
        {
            var validationStart = Stopwatch.GetTimestamp();
            validation = await mediator.Send(new ValidateSessionQuery(sessionId), Context.RequestAborted);
            var validationElapsed = Stopwatch.GetElapsedTime(validationStart);
            
            if (enableVerboseLogging)
            {
                Logger.LogInformation("[SESSION-AUTH] Session validation completed in {Elapsed}ms - UserId: {UserId}, ActiveTenantId: {TenantId}", 
                    validationElapsed.TotalMilliseconds,
                    validation.UserId,
                    validation.ActiveTenantId);
            }
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            Logger.LogWarning(ex, "Session validation failed for {SessionId}", sessionId);
            
            if (enableVerboseLogging)
            {
                Logger.LogWarning("[SESSION-AUTH] InvalidOperationException after {Elapsed}ms - Message: {Message}", 
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
            }
            
            return AuthenticateResult.Fail("Session is not valid.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Unexpected error during session validation for {SessionId}", sessionId);
            
            if (enableVerboseLogging)
            {
                Logger.LogError("[SESSION-AUTH] Exception after {Elapsed}ms - Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    stopwatch.ElapsedMilliseconds,
                    ex.GetType().FullName,
                    ex.Message,
                    ex.StackTrace);
            }
            
            return AuthenticateResult.Fail("Session validation error.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, validation.UserId.ToString()),
            new Claim(SessionClaimTypes.SessionId, validation.SessionId.ToString()),
            new Claim(SessionClaimTypes.ActiveTenantId, validation.ActiveTenantId.ToString())
        };

        var identity = new ClaimsIdentity(claims, SessionAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SessionAuthenticationDefaults.AuthenticationScheme);

        stopwatch.Stop();
        
        if (enableVerboseLogging)
        {
            Logger.LogInformation("[SESSION-AUTH] Authentication successful in {Elapsed}ms - SessionId: {SessionId}, UserId: {UserId}, TenantId: {TenantId}, Claims: {ClaimsCount}", 
                stopwatch.ElapsedMilliseconds,
                validation.SessionId,
                validation.UserId,
                validation.ActiveTenantId,
                claims.Length);
        }

        return AuthenticateResult.Success(ticket);
    }
}