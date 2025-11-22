using NorthStarET.Foundation.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Domain.Entities;

/// <summary>
/// Represents an active user session
/// </summary>
public class Session : EntityBase, ITenantEntity
{
    public Guid TenantId { get; set; }
    
    public SessionId SessionId { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public User? User { get; private set; }
    
    public string? UserPrincipalName { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public DateTime LastActivityAt { get; private set; }
    
    public bool IsRevoked { get; private set; }
    
    public DateTime? RevokedAt { get; private set; }
    
    public string? IpAddress { get; private set; }
    
    public string? UserAgent { get; private set; }
    
    /// <summary>
    /// JSON-serialized claims from the session
    /// </summary>
    public string? ClaimsJson { get; private set; }
    
    protected Session() { } // For EF Core
    
    public Session(
        Guid tenantId,
        Guid userId,
        string? userPrincipalName,
        TimeSpan duration,
        string? ipAddress = null,
        string? userAgent = null,
        string? claimsJson = null)
    {
        TenantId = tenantId;
        SessionId = SessionId.New();
        UserId = userId;
        UserPrincipalName = userPrincipalName;
        ExpiresAt = DateTime.UtcNow.Add(duration);
        LastActivityAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ClaimsJson = claimsJson;
    }
    
    public void Refresh(TimeSpan slidingDuration)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Cannot refresh a revoked session");
            
        LastActivityAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(slidingDuration);
        MarkAsUpdated();
    }
    
    public void UpdateClaims(string claimsJson)
    {
        ClaimsJson = claimsJson;
        MarkAsUpdated();
    }
    
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
    
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    
    public bool IsValid() => !IsRevoked && !IsExpired();
}
