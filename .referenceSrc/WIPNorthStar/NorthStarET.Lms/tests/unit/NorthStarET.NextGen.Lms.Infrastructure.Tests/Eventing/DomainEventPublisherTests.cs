using FluentAssertions;
using MediatR;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Infrastructure.Common;
using NSubstitute;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Eventing;

/// <summary>
/// Tests for MediatRDomainEventPublisher which wraps domain events in MediatR notifications.
/// Future enhancements (Event Grid, outbox pattern, retries) will require additional tests.
/// </summary>
public sealed class DomainEventPublisherTests
{
    private readonly IPublisher _publisherMock;
    private readonly MediatRDomainEventPublisher _sut;

    public DomainEventPublisherTests()
    {
        _publisherMock = Substitute.For<IPublisher>();
        _sut = new MediatRDomainEventPublisher(_publisherMock);
    }

    [Fact]
    public async Task Should_PublishDomainEventAsNotification_When_EventProvided()
    {
        // Arrange
        var districtCreatedEvent = new DistrictCreatedEvent(
            Guid.NewGuid(),
            "Test District",
            "test",
            DateTime.UtcNow
        );

        // Act
        await _sut.PublishAsync(districtCreatedEvent, CancellationToken.None);

        // Assert
        await _publisherMock.Received(1).Publish(
            Arg.Is<DomainEventNotification>(n => n.DomainEvent == districtCreatedEvent),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_PassCancellationToken_When_Publishing()
    {
        // Arrange
        var districtCreatedEvent = new DistrictCreatedEvent(
            Guid.NewGuid(),
            "Test District",
            "test",
            DateTime.UtcNow
        );
        var cancellationToken = new CancellationToken();

        // Act
        await _sut.PublishAsync(districtCreatedEvent, cancellationToken);

        // Assert
        await _publisherMock.Received(1).Publish(
            Arg.Any<DomainEventNotification>(),
            Arg.Is<CancellationToken>(ct => ct == cancellationToken));
    }

    [Fact]
    public async Task Should_WrapMultipleDomainEvents_When_PublishedSequentially()
    {
        // Arrange
        var event1 = new DistrictCreatedEvent(Guid.NewGuid(), "District 1", "d1", DateTime.UtcNow);
        var event2 = new DistrictCreatedEvent(Guid.NewGuid(), "District 2", "d2", DateTime.UtcNow);

        // Act
        await _sut.PublishAsync(event1, CancellationToken.None);
        await _sut.PublishAsync(event2, CancellationToken.None);

        // Assert
        await _publisherMock.Received(2).Publish(
            Arg.Any<DomainEventNotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Should_ThrowArgumentNullException_When_PublisherIsNull()
    {
        // Act
        var act = () => new MediatRDomainEventPublisher(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("publisher");
    }
}
