using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using NorthStarET.NextGen.Lms.Web.Services;
using System.Diagnostics;

namespace NorthStarET.NextGen.Lms.Web.Authentication;

public static class LmsAuthenticationExtensions
{
    public static void ConfigureLmsTokenExchange(this OpenIdConnectOptions options)
    {
        var originalTokenValidated = options.Events.OnTokenValidated;

        options.Events.OnTokenValidated = async context =>
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Call the original event first if it exists
            if (originalTokenValidated != null)
            {
                await originalTokenValidated(context);
            }

            // Retrieve logger and configuration first so they're available throughout
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OpenIdConnectOptions>>();
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var enableVerboseLogging = configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);

            if (enableVerboseLogging)
            {
                logger.LogInformation("[AUTH-FLOW] Token validation event triggered at {Timestamp}", DateTimeOffset.UtcNow);
                logger.LogInformation("[AUTH-FLOW] User principal present: {HasPrincipal}, Identity: {Identity}", 
                    context.Principal != null, 
                    context.Principal?.Identity?.Name ?? "<null>");
            }

            try
            {
                if (enableVerboseLogging)
                {
                    logger.LogInformation("[AUTH-FLOW] Retrieving required services for token exchange");
                }

                var tokenAcquisition = context.HttpContext.RequestServices.GetRequiredService<ITokenAcquisition>();
                var tokenExchangeService = context.HttpContext.RequestServices.GetRequiredService<ITokenExchangeService>();
                var sessionAccessor = context.HttpContext.RequestServices.GetRequiredService<ILmsSessionAccessor>();

                if (enableVerboseLogging)
                {
                    logger.LogInformation("[AUTH-FLOW] Services retrieved successfully");
                }

                // Get access token for the LMS API
                var scopeString = configuration["LmsApi:Scope"];
                var scopes = scopeString?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                
                if (enableVerboseLogging)
                {
                    logger.LogInformation("[AUTH-FLOW] Requesting access token for scopes: {Scopes}", string.Join(", ", scopes));
                }

                var tokenAcquisitionStart = Stopwatch.GetTimestamp();
                var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes, user: context.Principal);
                var tokenAcquisitionElapsed = Stopwatch.GetElapsedTime(tokenAcquisitionStart);

                if (enableVerboseLogging)
                {
                    logger.LogInformation("[AUTH-FLOW] Token acquisition completed in {Elapsed}ms. Token received: {HasToken}, Length: {TokenLength}", 
                        tokenAcquisitionElapsed.TotalMilliseconds,
                        !string.IsNullOrEmpty(accessToken),
                        accessToken?.Length ?? 0);
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    if (enableVerboseLogging)
                    {
                        // Log token claims (without exposing full token)
                        logger.LogInformation("[AUTH-FLOW] Access token first 10 chars: {TokenPrefix}...", accessToken.Substring(0, Math.Min(10, accessToken.Length)));
                    }

                    // Exchange the Entra token for an LMS session
                    var exchangeStart = Stopwatch.GetTimestamp();
                    var sessionId = await tokenExchangeService.ExchangeTokenAsync(accessToken, context.HttpContext);
                    var exchangeElapsed = Stopwatch.GetElapsedTime(exchangeStart);

                    if (enableVerboseLogging)
                    {
                        logger.LogInformation("[AUTH-FLOW] Token exchange completed in {Elapsed}ms. Session ID received: {HasSessionId}", 
                            exchangeElapsed.TotalMilliseconds,
                            !string.IsNullOrEmpty(sessionId));
                    }

                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        // Store the session ID in a cookie
                        var cookieSetStart = Stopwatch.GetTimestamp();
                        await sessionAccessor.SetSessionIdAsync(sessionId);
                        var cookieSetElapsed = Stopwatch.GetElapsedTime(cookieSetStart);

                        if (enableVerboseLogging)
                        {
                            logger.LogInformation("[AUTH-FLOW] Session cookie set in {Elapsed}ms. Session ID: {SessionId}", 
                                cookieSetElapsed.TotalMilliseconds,
                                sessionId);
                        }

                        logger.LogInformation("Successfully created LMS session for user");
                        
                        if (enableVerboseLogging)
                        {
                            stopwatch.Stop();
                            logger.LogInformation("[AUTH-FLOW] Total authentication flow completed in {Elapsed}ms", stopwatch.ElapsedMilliseconds);
                        }
                    }
                    else
                    {
                        logger.LogWarning("Failed to create LMS session - token exchange returned null");
                        
                        if (enableVerboseLogging)
                        {
                            logger.LogWarning("[AUTH-FLOW] Token exchange failure. Check TokenExchangeService logs for details.");
                        }
                    }
                }
                else
                {
                    logger.LogWarning("Failed to acquire access token for LMS API");
                    
                    if (enableVerboseLogging)
                    {
                        logger.LogWarning("[AUTH-FLOW] Token acquisition returned null/empty. Scopes requested: {Scopes}", string.Join(", ", scopes));
                        logger.LogWarning("[AUTH-FLOW] User principal claims: {Claims}", 
                            string.Join(", ", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                    }
                }
            }
            // Exception handling strategy:
            // - MicrosoftIdentityWebChallengeUserException: Re-throw to allow OAuth middleware to handle user consent/interaction
            // - HttpRequestException/InvalidOperationException: Swallow to prevent authentication failure on transient/config errors
            // - Exception: Catch-all for unexpected errors
            // Note: These exception types are independent with no inheritance relationship
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                // RE-THROW: User interaction required (consent, MFA, etc.)
                // Must propagate to OAuth middleware to trigger proper challenge flow
                logger.LogWarning(ex, "User challenge required during token acquisition");
                
                if (enableVerboseLogging)
                {
                    logger.LogWarning("[AUTH-FLOW] MicrosoftIdentityWebChallengeUserException - rethrowing to OAuth middleware. Message: {Message}", ex.Message);
                }
                
                throw;
            }
            catch (HttpRequestException ex)
            {
                // SWALLOW: Network/HTTP failures should not block authentication
                // User can still access the application even if LMS session creation fails
                logger.LogError(ex, "HTTP request failed during LMS token exchange");
                
                if (enableVerboseLogging)
                {
                    logger.LogError("[AUTH-FLOW] HttpRequestException details - StatusCode: {StatusCode}, Message: {Message}", 
                        ex.StatusCode, 
                        ex.Message);
                }
            }
            catch (InvalidOperationException ex)
            {
                // SWALLOW: Service misconfiguration should not block authentication
                // Allows graceful degradation when services are unavailable
                logger.LogError(ex, "Invalid operation during LMS token exchange - check service configuration");
                
                if (enableVerboseLogging)
                {
                    logger.LogError("[AUTH-FLOW] InvalidOperationException details - Message: {Message}, StackTrace: {StackTrace}", 
                        ex.Message, 
                        ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                // SWALLOW: Unexpected errors logged but don't block authentication
                logger.LogError(ex, "Unexpected error during LMS token exchange");
                
                if (enableVerboseLogging)
                {
                    logger.LogError("[AUTH-FLOW] Unexpected exception - Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                        ex.GetType().FullName, 
                        ex.Message, 
                        ex.StackTrace);
                }
            }
        };

        var originalSignedOut = options.Events.OnSignedOutCallbackRedirect;

        options.Events.OnSignedOutCallbackRedirect = async context =>
        {
            // Call the original event first if it exists
            if (originalSignedOut != null)
            {
                await originalSignedOut(context);
            }

            // Retrieve logger and configuration first so they're available in catch blocks
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OpenIdConnectOptions>>();
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var enableVerboseLogging = configuration.GetValue<bool>("AuthenticationDiagnostics:EnableVerboseLogging", false);

            if (enableVerboseLogging)
            {
                logger.LogInformation("[AUTH-FLOW] Sign out callback redirect event triggered at {Timestamp}", DateTimeOffset.UtcNow);
            }

            try
            {
                var sessionAccessor = context.HttpContext.RequestServices.GetRequiredService<ILmsSessionAccessor>();
                await sessionAccessor.ClearSessionAsync();
                
                if (enableVerboseLogging)
                {
                    logger.LogInformation("[AUTH-FLOW] Session cleared successfully during sign out");
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Invalid operation during session cleanup - check service configuration");
                
                if (enableVerboseLogging)
                {
                    logger.LogError("[AUTH-FLOW] InvalidOperationException during sign out - Message: {Message}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error clearing LMS session on sign out");
                
                if (enableVerboseLogging)
                {
                    logger.LogError("[AUTH-FLOW] Unexpected exception during sign out - Type: {ExceptionType}, Message: {Message}", 
                        ex.GetType().FullName, 
                        ex.Message);
                }
            }
        };
    }
}