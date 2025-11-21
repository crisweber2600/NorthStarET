using NorthStarET.Foundation.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Domain.Entities;

/// <summary>
/// Links a user to an external identity provider (e.g., Microsoft Entra ID)
/// </summary>
public class ExternalProviderLink : EntityBase
{
    public Guid UserId { get; private set; }
    
    public User? User { get; private set; }
    
    public string ProviderName { get; private set; } = string.Empty;
    
    public EntraSubjectId SubjectId { get; private set; }
    
    public string? UserPrincipalName { get; private set; }
    
    public DateTime? LastAuthenticatedAt { get; private set; }
    
    protected ExternalProviderLink() { } // For EF Core
    
    public ExternalProviderLink(Guid userId, string providerName, EntraSubjectId subjectId, string? userPrincipalName = null)
    {
        UserId = userId;
        ProviderName = providerName;
        SubjectId = subjectId;
        UserPrincipalName = userPrincipalName;
    }
    
    public void RecordAuthentication()
    {
        LastAuthenticatedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
}
