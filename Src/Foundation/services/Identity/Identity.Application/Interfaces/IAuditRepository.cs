namespace NorthStarET.Foundation.Identity.Application.Interfaces;

public interface IAuditRepository
{
    Task LogAuthenticationAsync(
        Guid tenantId,
        Guid? userId,
        Guid? sessionId,
        string eventType,
        bool isSuccess,
        string? eventData = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);
}
