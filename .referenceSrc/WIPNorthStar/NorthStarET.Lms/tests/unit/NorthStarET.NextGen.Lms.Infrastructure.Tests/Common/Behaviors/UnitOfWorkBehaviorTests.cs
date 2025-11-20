using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Infrastructure.Common.Behaviors;
using NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Common.Behaviors;

public sealed class UnitOfWorkBehaviorTests
{
    private DistrictsDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DistrictsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new DistrictsDbContext(options, domainEventPublisher: null);
    }

    [Fact]
    public async Task Handle_WhenRequestIsCommand_ShouldCommitChanges()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(context);
        var command = new TestCommand();
        var expectedResponse = "command result";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(command, () =>
        {
            nextCalled = true;
            // Add a district to track changes
            context.Districts.Add(Domain.Districts.District.Create(
                Guid.NewGuid(),
                "Test District",
                "test",
                new TestDateTimeProvider()));
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
        // Verify that SaveChanges was called by checking if entity was persisted
        context.Districts.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenRequestIsQuery_ShouldNotCommitChanges()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var behavior = new UnitOfWorkBehavior<TestQuery, string>(context);
        var query = new TestQuery();
        var expectedResponse = "query result";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(query, () =>
        {
            nextCalled = true;
            // Add a district but it should NOT be saved for queries
            context.Districts.Add(Domain.Districts.District.Create(
                Guid.NewGuid(),
                "Test District",
                "test",
                new TestDateTimeProvider()));
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
        // Verify that SaveChanges was NOT called - entity should not be persisted
        context.Districts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRequestIsCommandAndNextThrows_ShouldNotCommitChanges()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(context);
        var command = new TestCommand();

        // Act
        var act = async () => await behavior.Handle(command, () =>
        {
            // Add a district to track changes
            context.Districts.Add(Domain.Districts.District.Create(
                Guid.NewGuid(),
                "Test District",
                "test",
                new TestDateTimeProvider()));
            throw new InvalidOperationException("Command handler failed");
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Command handler failed");
        // Verify that SaveChanges was NOT called due to exception - entity should not be persisted
        context.Districts.Should().BeEmpty();
    }

    // Test command that implements ICommand
    private sealed record TestCommand : IRequest<string>, ICommand;

    // Test query that does NOT implement ICommand
    private sealed record TestQuery : IRequest<string>;

    // Test helper for DateTimeProvider
    private sealed class TestDateTimeProvider : Domain.Common.Interfaces.IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
