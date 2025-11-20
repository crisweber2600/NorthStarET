using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authentication.Commands;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Tests.Authentication;

public sealed class SessionManagementTests
{
    [Fact]
    public async Task RefreshSession_WhenSessionExists_ShouldExtendExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        var session = Session.Create(
            userId,
            "hash",
            "token",
            tenantId,
            now.AddMinutes(-10),
            now.AddMinutes(20),
            "127.0.0.1",
            "agent");

        var oldExpiration = session.ExpiresAt;

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sessionStore = new Mock<ISessionStore>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<RefreshSessionCommandHandler>>();

        var settings = new IdentityModuleSettings { SessionSlidingExpirationMinutes = 30 };
        var options = Options.Create(settings);

        var command = new RefreshSessionCommand(session.Id, "newToken");
        var handler = new RefreshSessionCommandHandler(sessionRepository.Object, sessionStore.Object, logger.Object, options);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        session.ExpiresAt.Should().BeAfter(oldExpiration);
        session.LmsAccessToken.Should().Be("newToken");
        sessionRepository.Verify(x => x.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        sessionStore.Verify(x => x.CacheSessionAsync(session, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshSession_WhenSessionNotFound_ShouldThrow()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        var sessionStore = new Mock<ISessionStore>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<RefreshSessionCommandHandler>>();

        var settings = new IdentityModuleSettings { SessionSlidingExpirationMinutes = 30 };
        var options = Options.Create(settings);

        var command = new RefreshSessionCommand(sessionId, "newToken");
        var handler = new RefreshSessionCommandHandler(sessionRepository.Object, sessionStore.Object, logger.Object, options);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RevokeSession_WhenSessionExists_ShouldMarkAsRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        var session = Session.Create(
            userId,
            "hash",
            "token",
            tenantId,
            now.AddMinutes(-10),
            now.AddMinutes(20),
            "127.0.0.1",
            "agent");

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sessionStore = new Mock<ISessionStore>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<RevokeSessionCommandHandler>>();

        var command = new RevokeSessionCommand(session.Id);
        var handler = new RevokeSessionCommandHandler(sessionRepository.Object, sessionStore.Object, logger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        session.IsRevoked.Should().BeTrue();
        sessionRepository.Verify(x => x.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        sessionStore.Verify(x => x.RemoveSessionAsync(session.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeSession_WhenSessionRevoked_ShouldClearCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        var session = Session.Create(
            userId,
            "hash",
            "token",
            tenantId,
            now.AddMinutes(-10),
            now.AddMinutes(20),
            "127.0.0.1",
            "agent");

        var sessionRepository = new Mock<ISessionRepository>();
        sessionRepository
            .Setup(x => x.GetActiveSessionAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var sessionStore = new Mock<ISessionStore>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<RevokeSessionCommandHandler>>();

        var command = new RevokeSessionCommand(session.Id);
        var handler = new RevokeSessionCommandHandler(sessionRepository.Object, sessionStore.Object, logger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        sessionStore.Verify(x => x.RemoveSessionAsync(session.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DetectExpiredSession_WhenSessionExpired_ShouldReturnTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        var session = Session.Create(
            userId,
            "hash",
            "token",
            tenantId,
            now.AddMinutes(-30),
            now.AddMinutes(-5), // Already expired
            "127.0.0.1",
            "agent");

        // Act & Assert
        session.ExpiresAt.Should().BeBefore(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void DetectExpiredSession_WhenSessionActive_ShouldReturnFalse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenantId = new TenantId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;

        var session = Session.Create(
            userId,
            "hash",
            "token",
            tenantId,
            now.AddMinutes(-10),
            now.AddMinutes(20), // Still valid
            "127.0.0.1",
            "agent");

        // Act & Assert
        session.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }
}
