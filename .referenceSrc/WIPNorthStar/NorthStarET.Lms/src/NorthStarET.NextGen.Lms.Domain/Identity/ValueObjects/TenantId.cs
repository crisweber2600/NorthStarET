using System;

namespace NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

public readonly record struct TenantId
{
    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be an empty GUID.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;

    public static implicit operator TenantId(Guid value) => new(value);
}
