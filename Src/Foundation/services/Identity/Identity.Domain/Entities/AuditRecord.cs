using NorthStarET.Foundation.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Domain.Entities;

/// <summary>
/// Audit record for authentication and authorization events
/// </summary>
public class AuditRecord : EntityBase, ITenantEntity
{
    public Guid TenantId { get; set; }
    
    public Guid? UserId { get; private set; }
    
    public Guid? SessionId { get; private set; }
    
    public string EventType { get; private set; } = string.Empty;
    
    public string? EventData { get; private set; }
    
    public string? IpAddress { get; private set; }
    
    public string? UserAgent { get; private set; }
    
    public bool IsSuccess { get; private set; }
    
    public string? ErrorMessage { get; private set; }
    
    protected AuditRecord() { } // For EF Core
    
    public AuditRecord(
        Guid tenantId,
        string eventType,
        bool isSuccess,
        Guid? userId = null,
        Guid? sessionId = null,
        string? eventData = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? errorMessage = null)
    {
        TenantId = tenantId;
        UserId = userId;
        SessionId = sessionId;
        EventType = eventType;
        EventData = eventData;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}
