using NorthStarET.Foundation.Domain.Entities;
using NorthStarET.Foundation.Identity.Domain.ValueObjects;

namespace NorthStarET.Foundation.Identity.Domain.Entities;

/// <summary>
/// Represents a user in the NorthStar LMS system
/// </summary>
public class User : EntityBase, ITenantEntity
{
    public Guid TenantId { get; set; }
    
    public string Email { get; private set; } = string.Empty;
    
    public string FirstName { get; private set; } = string.Empty;
    
    public string LastName { get; private set; } = string.Empty;
    
    public string? DisplayName { get; private set; }
    
    public bool IsActive { get; private set; } = true;
    
    public DateTime? LastLoginAt { get; private set; }
    
    private readonly List<ExternalProviderLink> _externalProviders = new();
    public IReadOnlyCollection<ExternalProviderLink> ExternalProviders => _externalProviders.AsReadOnly();
    
    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    
    protected User() { } // For EF Core
    
    public User(Guid tenantId, string email, string firstName, string lastName, string? displayName = null)
    {
        TenantId = tenantId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName ?? $"{firstName} {lastName}";
    }
    
    public void UpdateProfile(string firstName, string lastName, string? displayName = null)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = displayName ?? $"{firstName} {lastName}";
        MarkAsUpdated();
    }
    
    public void LinkExternalProvider(string provider, EntraSubjectId subjectId, string? userPrincipalName = null)
    {
        var link = new ExternalProviderLink(Id, provider, subjectId, userPrincipalName);
        _externalProviders.Add(link);
        MarkAsUpdated();
    }
    
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }
    
    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
    
    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
    
    public void AddRole(Role role, Guid? assignedByUserId = null)
    {
        if (_roles.Any(r => r.RoleId == role.Id))
            return;
            
        var userRole = new UserRole(Id, role.Id, assignedByUserId);
        _roles.Add(userRole);
        MarkAsUpdated();
    }
    
    public void RemoveRole(Guid roleId)
    {
        var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (role != null)
        {
            _roles.Remove(role);
            MarkAsUpdated();
        }
    }
}
