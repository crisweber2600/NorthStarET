using FluentAssertions;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Infrastructure.Common.Behaviors;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Common.Behaviors;

public sealed class TenantIsolationBehaviorTests
{
    private readonly Guid _userDistrictId = Guid.NewGuid();
    private readonly Guid _differentDistrictId = Guid.NewGuid();

    [Fact]
    public async Task Handle_WhenRequestIsNotTenantScoped_ShouldAllowExecution()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = _userDistrictId,
            Role = ActorRole.DistrictAdmin
        };
        var behavior = new TenantIsolationBehavior<NonTenantScopedCommand, string>(currentUserService);
        var command = new NonTenantScopedCommand();
        var expectedResponse = "success";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(command, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCommandIsTenantScopedAndDistrictMatches_ShouldAllowExecution()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = _userDistrictId,
            Role = ActorRole.DistrictAdmin
        };
        var behavior = new TenantIsolationBehavior<TestTenantScopedCommand, string>(currentUserService);
        var command = new TestTenantScopedCommand(_userDistrictId);
        var expectedResponse = "success";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(command, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCommandIsTenantScopedAndDistrictDoesNotMatch_ShouldThrowUnauthorized()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = _userDistrictId,
            Role = ActorRole.DistrictAdmin
        };
        var behavior = new TenantIsolationBehavior<TestTenantScopedCommand, string>(currentUserService);
        var command = new TestTenantScopedCommand(_differentDistrictId);

        // Act
        var act = async () => await behavior.Handle(command, () => Task.FromResult("success"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"Access denied: User's district ({_userDistrictId}) does not match requested district ({_differentDistrictId}).");
    }

    [Fact]
    public async Task Handle_WhenQueryIsTenantScopedAndDistrictMatches_ShouldAllowExecution()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = _userDistrictId,
            Role = ActorRole.DistrictAdmin
        };
        var behavior = new TenantIsolationBehavior<TestTenantScopedQuery, string>(currentUserService);
        var query = new TestTenantScopedQuery(_userDistrictId);
        var expectedResponse = "query result";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(query, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenQueryIsTenantScopedAndDistrictDoesNotMatch_ShouldThrowUnauthorized()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = _userDistrictId,
            Role = ActorRole.DistrictAdmin
        };
        var behavior = new TenantIsolationBehavior<TestTenantScopedQuery, string>(currentUserService);
        var query = new TestTenantScopedQuery(_differentDistrictId);

        // Act
        var act = async () => await behavior.Handle(query, () => Task.FromResult("query result"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"Access denied: User's district ({_userDistrictId}) does not match requested district ({_differentDistrictId}).");
    }

    [Fact]
    public async Task Handle_WhenPlatformAdminAccessesDifferentDistrict_ShouldAllowExecution()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = _userDistrictId,
            Role = ActorRole.PlatformAdmin
        };
        var behavior = new TenantIsolationBehavior<TestTenantScopedCommand, string>(currentUserService);
        var command = new TestTenantScopedCommand(_differentDistrictId);
        var expectedResponse = "success";
        var nextCalled = false;

        // Act
        var result = await behavior.Handle(command, () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserHasNoDistrictAssignment_ShouldThrowUnauthorized()
    {
        // Arrange
        var currentUserService = new TestCurrentUserService
        {
            DistrictId = null, // No district assignment
            Role = ActorRole.DistrictAdmin
        };
        var behavior = new TenantIsolationBehavior<TestTenantScopedCommand, string>(currentUserService);
        var command = new TestTenantScopedCommand(_differentDistrictId);

        // Act
        var act = async () => await behavior.Handle(command, () => Task.FromResult("success"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User does not have a district assignment.");
    }

    // Test request types
    private sealed record NonTenantScopedCommand : IRequest<string>;

    private sealed record TestTenantScopedCommand(Guid DistrictId) : IRequest<string>, ITenantScoped;

    private sealed record TestTenantScopedQuery(Guid DistrictId) : IRequest<string>, ITenantScoped;

    // Test implementation of ICurrentUserService
    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; init; }
        public ActorRole Role { get; init; }
        public Guid? DistrictId { get; init; }
        public Guid? CorrelationId { get; init; }
    }
}
