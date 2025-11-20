using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authentication.Queries;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;

namespace NorthStarET.NextGen.Lms.Application.Tests.Authentication;

public sealed class GetCurrentUserContextQueryTests
{
    [Fact]
    public async Task Handle_WhenSessionFoundInCache_ShouldReturnUserContext()
    {
        var fixture = new Fixture();
        var result = await fixture.Handler.Handle(new GetCurrentUserContextQuery(fixture.SessionId), CancellationToken.None);

        result.SessionId.Should().Be(fixture.SessionId);
        fixture.CachedSession.Should().NotBeNull();
        result.ExpiresAt.Should().Be(fixture.CachedSession!.ExpiresAt);
        result.User.ActiveTenantId.Should().Be(fixture.ActiveTenantId);
        result.User.Email.Should().Be(fixture.User.Email);
        result.AvailableTenants.Should().ContainSingle();

        fixture.SessionRepository.Verify(x => x.GetActiveSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSessionNotInCache_ShouldUseRepositoryAndCache()
    {
        var fixture = new Fixture(includeCacheEntry: false);

        var result = await fixture.Handler.Handle(new GetCurrentUserContextQuery(fixture.SessionId), CancellationToken.None);

        result.SessionId.Should().Be(fixture.SessionId);
        fixture.SessionRepository.Verify(x => x.GetActiveSessionAsync(fixture.SessionId, It.IsAny<CancellationToken>()), Times.Once);
        fixture.SessionStore.Verify(
            x => x.CacheSessionAsync(fixture.SessionFromRepository!, It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSessionExpired_ShouldThrow()
    {
        var fixture = new Fixture();
        fixture.CachedSession.Should().NotBeNull();
        fixture.CachedSession = fixture.CachedSession! with
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-5)
        };

        fixture.SessionStore
            .Setup(x => x.GetSessionAsync(fixture.SessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fixture.CachedSession);

        await FluentActions.Invoking(() => fixture.Handler.Handle(new GetCurrentUserContextQuery(fixture.SessionId), CancellationToken.None))
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
            RoleId = Guid.NewGuid();

            SessionStore = new Mock<ISessionStore>();
            SessionRepository = new Mock<ISessionRepository>();

            SessionFromRepository = Session.Create(
                UserId,
                "hash",
                "token",
                new TenantId(ActiveTenantId),
                now.AddMinutes(-10),
                now.AddMinutes(35),
                "127.0.0.1",
                "agent");

            SessionStore
                .Setup(x => x.CacheSessionAsync(SessionFromRepository, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            if (includeCacheEntry)
            {
                SessionId = Guid.NewGuid();

                CachedSession = new SessionCacheModel(
                    SessionId,
                    UserId,
                    ActiveTenantId,
                    now.AddMinutes(30),
                    now,
                    false);

                SessionStore
                    .Setup(x => x.GetSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CachedSession);

                SessionRepository
                    .Setup(x => x.GetActiveSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(SessionFromRepository);
            }
            else
            {
                SessionId = SessionFromRepository.Id;

                SessionStore
                    .Setup(x => x.GetSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((SessionCacheModel?)null);

                SessionRepository
                    .Setup(x => x.GetActiveSessionAsync(SessionId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(SessionFromRepository);
            }

            User = new User(UserId, new EntraSubjectId("subject"), "user@example.com", "Ada", "Lovelace", now.AddMonths(-3));

            UserRepository = new Mock<IUserRepository>();
            UserRepository
                .Setup(x => x.GetAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(User);

            ActiveMembership = new Membership(Guid.NewGuid(), UserId, new TenantId(ActiveTenantId), RoleId, now.AddMonths(-2), null, now.AddMonths(2));

            MembershipRepository = new Mock<IMembershipRepository>();
            MembershipRepository
                .Setup(x => x.GetActiveMembershipsForUserAsync(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Membership> { ActiveMembership });

            Tenant = new Tenant(new TenantId(ActiveTenantId), "Test District", TenantType.District, now.AddYears(-1));

            TenantRepository = new Mock<ITenantRepository>();
            TenantRepository
                .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<TenantId>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Tenant> { Tenant });

            Role = new Role(RoleId, "administrator", "Administrator", now.AddYears(-5), true);

            RoleRepository = new Mock<IRoleRepository>();
            RoleRepository
                .Setup(x => x.GetAsync(RoleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Role);

            Handler = new GetCurrentUserContextQueryHandler(
                Mock.Of<ILogger<GetCurrentUserContextQueryHandler>>(),
                SessionStore.Object,
                SessionRepository.Object,
                UserRepository.Object,
                MembershipRepository.Object,
                TenantRepository.Object,
                RoleRepository.Object);
        }

        public Guid SessionId { get; }

        public Guid UserId { get; }

        public Guid ActiveTenantId { get; }

        public Guid RoleId { get; }

        public SessionCacheModel? CachedSession { get; set; }

        public Session SessionFromRepository { get; }

        public User User { get; }

        public Membership ActiveMembership { get; }

        public Tenant Tenant { get; }

        public Role Role { get; }

        public Mock<ISessionStore> SessionStore { get; }

        public Mock<ISessionRepository> SessionRepository { get; }

        public Mock<IUserRepository> UserRepository { get; }

        public Mock<IMembershipRepository> MembershipRepository { get; }

        public Mock<ITenantRepository> TenantRepository { get; }

        public Mock<IRoleRepository> RoleRepository { get; }

        public GetCurrentUserContextQueryHandler Handler { get; }
    }
}
