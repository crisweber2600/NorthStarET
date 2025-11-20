using MediatR;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

namespace NorthStarET.NextGen.Lms.Infrastructure.Common;

/// <summary>
/// MediatR-based domain event publisher using INotification.
/// Publishes domain events to all registered handlers.
/// </summary>
internal sealed class MediatRDomainEventPublisher : IDomainEventPublisher
{
    private readonly IPublisher _publisher;

    public MediatRDomainEventPublisher(IPublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Wrap domain event in notification envelope
        var notification = new DomainEventNotification(domainEvent);
        await _publisher.Publish(notification, cancellationToken);
    }
}

/// <summary>
/// Notification wrapper for domain events to integrate with MediatR INotification.
/// </summary>
internal sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
