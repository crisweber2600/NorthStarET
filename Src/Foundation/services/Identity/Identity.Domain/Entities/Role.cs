using NorthStarET.Foundation.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Domain.Entities;

/// <summary>
/// Represents a role that can be assigned to users
/// </summary>
public class Role : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    
    public string NormalizedName { get; private set; } = string.Empty;
    
    public string? Description { get; private set; }
    
    /// <summary>
    /// Maps to Entra ID App Role if this role is sourced from Entra
    /// </summary>
    public string? EntraAppRoleId { get; private set; }
    
    public bool IsSystemRole { get; private set; }
    
    private readonly List<string> _permissions = new();
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();
    
    protected Role() { } // For EF Core
    
    public Role(string name, string? description = null, string? entraAppRoleId = null, bool isSystemRole = false)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
        Description = description;
        EntraAppRoleId = entraAppRoleId;
        IsSystemRole = isSystemRole;
    }
    
    public void AddPermission(string permission)
    {
        if (!_permissions.Contains(permission))
        {
            _permissions.Add(permission);
            MarkAsUpdated();
        }
    }
    
    public void RemovePermission(string permission)
    {
        if (_permissions.Remove(permission))
        {
            MarkAsUpdated();
        }
    }
    
    public void UpdateDescription(string? description)
    {
        Description = description;
        MarkAsUpdated();
    }
}
