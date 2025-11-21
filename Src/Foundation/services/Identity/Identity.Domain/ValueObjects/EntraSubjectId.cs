namespace NorthStarET.Foundation.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a Microsoft Entra ID subject identifier (oid claim)
/// </summary>
public readonly record struct EntraSubjectId(string Value)
{
    public override string ToString() => Value;
    
    public static implicit operator string(EntraSubjectId subjectId) => subjectId.Value;
    
    public static implicit operator EntraSubjectId(string value) => new(value);
}
