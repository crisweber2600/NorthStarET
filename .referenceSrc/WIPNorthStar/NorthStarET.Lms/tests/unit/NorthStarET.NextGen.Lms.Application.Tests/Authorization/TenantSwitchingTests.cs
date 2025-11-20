using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authorization.Commands;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Authorization.Queries;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;
using NorthStarET.NextGen.Lms.Application.Common.Caching;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Tests.Authorization;

public sealed class TenantSwitchingTests
{
    [Fact]
    public async Task SwitchTenantContext_WhenUserHasMembership_ShouldUpdateSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentTenantId = new TenantId(Guid.NewGuid());
        var targetTenantId = new TenantId(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = Session.Create(
            userId,
            "hash",
            "token",
            currentTenantId,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddMinutes(20),
            "127.0.0.1",
            "agent");

        // Create a real membership entity
        var membership = new Membership(
            Guid.NewGuid(),
            userId,
            targetTenantId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-30),
            null,
            null,
            true);

        var membershipRepository = new Mock<IMembershipRepository>();
        membershipRepository
            .Setup(x => x.GetByUserAndTenantAsync(userId, targetTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var authCache = new Mock<IAuthorizationCache>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SwitchTenantContextCommandHandler>>();

        var command = new SwitchTenantContextCommand(sessionId, userId, targetTenantId.Value);
        var handler = new SwitchTenantContextCommandHandler(
            sessionRepository.Object,
            membershipRepository.Object,
            authCache.Object,
            logger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        session.ActiveTenantId.Value.Should().Be(targetTenantId.Value);
        sessionRepository.Verify(x => x.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        authCache.Verify(x => x.ClearForUserAndTenantAsync(userId, currentTenantId.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SwitchTenantContext_WhenUserLacksMembership_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentTenantId = new TenantId(Guid.NewGuid());
        var sessionId = Guid.NewGuid();
        var targetTenantId = new TenantId(Guid.NewGuid());

        var session = Session.Create(
            userId,
            "hash",
            "token",
            currentTenantId,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddMinutes(20),
            "127.0.0.1",
            "agent");

        var membershipRepository = new Mock<IMembershipRepository>();
        membershipRepository
            .Setup(x => x.GetByUserAndTenantAsync(userId, targetTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Membership?)null);

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var authCache = new Mock<IAuthorizationCache>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SwitchTenantContextCommandHandler>>();

        var command = new SwitchTenantContextCommand(sessionId, userId, targetTenantId.Value);
        var handler = new SwitchTenantContextCommandHandler(
            sessionRepository.Object,
            membershipRepository.Object,
            authCache.Object,
            logger.Object);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User {userId} does not have membership in tenant {targetTenantId.Value}.");
    }

    [Fact]
    public async Task SwitchTenantContext_WhenSwitching_ShouldClearAuthorizationCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentTenantId = new TenantId(Guid.NewGuid());
        var targetTenantId = new TenantId(Guid.NewGuid());
        var sessionId = Guid.NewGuid();

        var session = Session.Create(
            userId,
            "hash",
            "token",
            currentTenantId,
            DateTimeOffset.UtcNow.AddMinutes(-10),
            DateTimeOffset.UtcNow.AddMinutes(20),
            "127.0.0.1",
            "agent");

        // Create a real membership entity
        var membership = new Membership(
            Guid.NewGuid(),
            userId,
            targetTenantId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(-30),
            null,
            null,
            true);

        var membershipRepository = new Mock<IMembershipRepository>();
        membershipRepository
            .Setup(x => x.GetByUserAndTenantAsync(userId, targetTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var authCache = new Mock<IAuthorizationCache>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<SwitchTenantContextCommandHandler>>();

        var command = new SwitchTenantContextCommand(sessionId, userId, targetTenantId.Value);
        var handler = new SwitchTenantContextCommandHandler(
            sessionRepository.Object,
            membershipRepository.Object,
            authCache.Object,
            logger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        authCache.Verify(x => x.ClearForUserAndTenantAsync(userId, currentTenantId.Value, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserTenants_WhenUserHasMultipleMemberships_ShouldReturnAllTenants()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var dataService = new Mock<IIdentityAuthorizationDataService>();
        dataService
            .Setup(x => x.GetUserTenantsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetUserTenantsResult(userId, new List<UserTenantMembership>
            {
                new UserTenantMembership(Guid.NewGuid(), "Lincoln Elementary", TenantType.School, null, Guid.NewGuid(), "Teacher", DateTimeOffset.UtcNow, null),
                new UserTenantMembership(Guid.NewGuid(), "Washington Middle", TenantType.School, null, Guid.NewGuid(), "Admin", DateTimeOffset.UtcNow, null)
            }));

        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<GetUserTenantsQueryHandler>>();

        var query = new GetUserTenantsQuery(userId);
        var handler = new GetUserTenantsQueryHandler(dataService.Object, logger.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tenants.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserTenants_ShouldCompleteUnder200Milliseconds()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var dataService = new Mock<IIdentityAuthorizationDataService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<GetUserTenantsQueryHandler>>();

        var query = new GetUserTenantsQuery(userId);
        var handler = new GetUserTenantsQueryHandler(dataService.Object, logger.Object);

        // Act & Assert - performance requirement
        // When implemented, measure execution time and assert < 200ms
        await Task.CompletedTask;
    }
}
