using System.Text;
using Microsoft.AspNetCore.Http;
using NorthStarET.Foundation.Infrastructure.Caching;

namespace NorthStarET.Foundation.Infrastructure.Middleware;

/// <summary>
/// Middleware to handle idempotency for POST/PUT/PATCH requests
/// </summary>
public class IdempotencyMiddleware
{
    private const string IdempotencyKeyHeader = "X-Idempotency-Key";
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, IIdempotencyService idempotencyService)
    {
        // Only check idempotency for POST, PUT, PATCH
        if (!HttpMethods.IsPost(context.Request.Method) &&
            !HttpMethods.IsPut(context.Request.Method) &&
            !HttpMethods.IsPatch(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers[IdempotencyKeyHeader].FirstOrDefault();
        
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await _next(context);
            return;
        }

        // Check if request was already processed
        var (exists, result) = await idempotencyService.CheckIdempotencyAsync(idempotencyKey);
        
        if (exists && result is not null)
        {
            // Return cached result
            context.Response.StatusCode = StatusCodes.Status202Accepted;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result);
            return;
        }

        // Capture the response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Store the result if successful
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            
            await idempotencyService.StoreIdempotencyAsync(idempotencyKey, responseText);

            responseBody.Seek(0, SeekOrigin.Begin);
        }

        await responseBody.CopyToAsync(originalBodyStream);
    }
}
