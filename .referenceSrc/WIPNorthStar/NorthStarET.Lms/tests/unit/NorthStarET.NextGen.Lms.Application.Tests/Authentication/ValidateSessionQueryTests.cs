using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Tests.Authentication;

public sealed class ValidateSessionQueryTests
{
    [Fact]
    public async Task Handle_WhenSessionCached_ShouldReturnSnapshot()
    {
        var fixture = new Fixture();

        var result = await fixture.Handler.Handle(new ValidateSessionQuery(fixture.SessionId), CancellationToken.None);

        result.SessionId.Should().Be(fixture.SessionId);
        result.UserId.Should().Be(fixture.UserId);
        result.ActiveTenantId.Should().Be(fixture.ActiveTenantId);
        fixture.SessionRepository.Verify(x => x.GetActiveSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSessionLoadedFromRepository_ShouldCacheResult()
    {
        var fixture = new Fixture(includeCacheEntry: false);

        var result = await fixture.Handler.Handle(new ValidateSessionQuery(fixture.SessionId), CancellationToken.None);

        result.SessionId.Should().Be(fixture.SessionId);
        fixture.SessionRepository.Verify(x => x.GetActiveSessionAsync(fixture.SessionId, It.IsAny<CancellationToken>()), Times.Once);
        fixture.SessionStore.Verify(
            x => x.CacheSessionAsync(fixture.SessionFromRepository!, It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSessionRevoked_ShouldThrow()
    {
        var fixture = new Fixture();
        fixture.CachedSession = fixture.CachedSession with { IsRevoked = true };

        fixture.SessionStore
            .Setup(x => x.GetSessionAsync(fixture.SessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.CachedSession);

        await FluentActions.Invoking(() => fixture.Handler.Handle(new ValidateSessionQuery(fixture.SessionId), CancellationToken.None))
            .Should()
            .ThrowAsync<InvalidOperationException>();
    }

    private sealed class Fixture
    {
        private readonly DateTimeOffset now = DateTimeOffset.UtcNow;

        public Fixture(bool includeCacheEntry = true)
        {
            UserId = Guid.NewGuid();
            ActiveTenantId = Guid.NewGuid();

            SessionRepository = new Mock<ISessionRepository>();

            SessionFromRepository = Domain.Identity.Entities.Session.Create(
                UserId,
                "hash",
                "token",
                new TenantId(ActiveTenantId),
                now.AddMinutes(-5),
                now.AddMinutes(25),
                "127.0.0.1",
                "agent");

            SessionStore = new Mock<ISessionStore>();

            if (includeCacheEntry)
            {
                SessionId = Guid.NewGuid();

                CachedSession = new SessionCacheModel(
                    SessionId,
                    UserId,
                    ActiveTenantId,
                    now.AddMinutes(20),
                    now,
                    false);

                SessionStore
                    .Setup(x => x.GetSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CachedSession);
            }
            else
            {
                SessionId = SessionFromRepository.Id;
                CachedSession = new SessionCacheModel(
                    SessionId,
                    UserId,
                    ActiveTenantId,
                    SessionFromRepository.ExpiresAt,
                    SessionFromRepository.LastActivityAt,
                    SessionFromRepository.IsRevoked);

                SessionStore
                    .Setup(x => x.GetSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((SessionCacheModel?)null);
            }

            SessionRepository
                .Setup(x => x.GetActiveSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(SessionFromRepository);

            SessionStore
                .Setup(x => x.CacheSessionAsync(SessionFromRepository, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Handler = new ValidateSessionQueryHandler(
                Mock.Of<ILogger<ValidateSessionQueryHandler>>(),
                SessionStore.Object,
                SessionRepository.Object);
        }

        public Guid SessionId { get; }

        public Guid UserId { get; }

        public Guid ActiveTenantId { get; }

        public SessionCacheModel CachedSession { get; set; }

        public Domain.Identity.Entities.Session SessionFromRepository { get; }

        public Mock<ISessionStore> SessionStore { get; }

        public Mock<ISessionRepository> SessionRepository { get; }

        public ValidateSessionQueryHandler Handler { get; }
    }
}
