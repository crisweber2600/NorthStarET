namespace NorthStarET.Foundation.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique session identifier
/// </summary>
public readonly record struct SessionId(Guid Value)
{
    public static SessionId New() => new(Guid.NewGuid());
    
    public static SessionId Empty => new(Guid.Empty);
    
    public override string ToString() => Value.ToString();
    
    public static implicit operator Guid(SessionId sessionId) => sessionId.Value;
    
    public static implicit operator SessionId(Guid value) => new(value);
}
