namespace NorthStarET.NextGen.Lms.Domain.Common;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Aggregate roots manage domain events and ensure consistency boundaries.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Unique identifier for the aggregate root.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Read-only collection of domain events raised by this aggregate.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event that will be published after the aggregate is persisted.
    /// </summary>
    /// <param name="domainEvent">Domain event to raise</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the aggregate (called after publishing).
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
