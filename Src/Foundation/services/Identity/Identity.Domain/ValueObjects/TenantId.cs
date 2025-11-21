namespace NorthStarET.Foundation.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a tenant (district) identifier
/// </summary>
public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());
    
    public static TenantId Empty => new(Guid.Empty);
    
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
    
    public static implicit operator TenantId(Guid value) => new(value);
}
