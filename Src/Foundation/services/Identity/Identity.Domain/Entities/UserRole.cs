using NorthStarET.Foundation.Domain.Entities;

namespace NorthStarET.Foundation.Identity.Domain.Entities;

/// <summary>
/// Join entity for User-Role many-to-many relationship
/// </summary>
public class UserRole : EntityBase
{
    public Guid UserId { get; private set; }
    
    public User? User { get; private set; }
    
    public Guid RoleId { get; private set; }
    
    public Role? Role { get; private set; }
    
    public Guid? AssignedByUserId { get; private set; }
    
    public DateTime AssignedAt { get; private set; }
    
    protected UserRole() { } // For EF Core
    
    public UserRole(Guid userId, Guid roleId, Guid? assignedByUserId = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedByUserId = assignedByUserId;
        AssignedAt = DateTime.UtcNow;
    }
}
