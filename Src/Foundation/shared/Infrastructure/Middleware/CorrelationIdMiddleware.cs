using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace NorthStarET.Foundation.Infrastructure.Middleware;

/// <summary>
/// Middleware to extract or generate correlation IDs for distributed tracing
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // Add to response headers
        context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

        // Add to activity baggage for OpenTelemetry
        Activity.Current?.SetBaggage("correlation.id", correlationId);

        await _next(context);
    }
}
