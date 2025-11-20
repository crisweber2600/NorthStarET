using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authentication.Services;
using NorthStarET.NextGen.Lms.Application.Authentication.Services.Models;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;
using Xunit;

namespace NorthStarET.NextGen.Lms.Application.Tests.Authentication;

public class TokenExchangeServiceTests
{
    [Fact]
    public async Task ExchangeToken_WhenEntraTokenIsValid_ShouldPersistSession()
    {
        var fixture = new TokenExchangeServiceFixture(sessionExists: false);

        var result = await fixture.Service.ExchangeAsync(fixture.Context, CancellationToken.None);

        fixture.CreatedSession.Should().NotBeNull();
        fixture.SessionRepository.Verify(x => x.AddAsync(fixture.CreatedSession!, It.IsAny<CancellationToken>()), Times.Once);
        fixture.SessionRepository.Verify(x => x.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.SessionStore.Verify(x => x.CacheSessionAsync(fixture.CreatedSession!, It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero), It.IsAny<CancellationToken>()), Times.Once);

        result.SessionId.Should().Be(fixture.CreatedSession!.Id);
        result.LmsAccessToken.Should().Be(fixture.CreatedSession!.LmsAccessToken);
        result.ExpiresAt.Should().Be(fixture.CreatedSession!.ExpiresAt);
        result.User.Id.Should().Be(fixture.User.Id);
        result.User.ActiveTenantId.Should().Be(fixture.Context.ActiveTenantId);
        result.AvailableTenants.Should().ContainSingle(tenant => tenant.TenantId == fixture.Context.ActiveTenantId);

        fixture.CachedSession.Should().BeSameAs(fixture.CreatedSession);
        fixture.CachedTtl.Should().NotBeNull();
        fixture.CachedTtl!.Value.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ExchangeToken_WhenSessionExists_ShouldExtendExpiration()
    {
        var fixture = new TokenExchangeServiceFixture(sessionExists: true);
        var existingSession = fixture.ExistingSession ?? throw new InvalidOperationException("Fixture did not configure an existing session.");
        var previousExpiration = existingSession.ExpiresAt;
        var previousActivity = existingSession.LastActivityAt;
        var previousToken = existingSession.LmsAccessToken;

        var result = await fixture.Service.ExchangeAsync(fixture.Context, CancellationToken.None);

        existingSession.ExpiresAt.Should().BeAfter(previousExpiration);
        existingSession.LastActivityAt.Should().BeAfter(previousActivity);
        existingSession.LmsAccessToken.Should().NotBe(previousToken);

        fixture.SessionRepository.Verify(x => x.UpdateAsync(existingSession, It.IsAny<CancellationToken>()), Times.Once);
        fixture.SessionRepository.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        fixture.SessionStore.Verify(x => x.CacheSessionAsync(existingSession, It.Is<TimeSpan>(ttl => ttl > TimeSpan.Zero), It.IsAny<CancellationToken>()), Times.Once);

        result.SessionId.Should().Be(existingSession.Id);
        result.ExpiresAt.Should().Be(existingSession.ExpiresAt);
        result.LmsAccessToken.Should().Be(existingSession.LmsAccessToken);

        fixture.CachedSession.Should().BeSameAs(existingSession);
        fixture.CachedTtl.Should().NotBeNull();
        fixture.CachedTtl!.Value.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ExchangeToken_WhenNoTenantSpecified_ShouldAutoSelectFirstAvailableTenant()
    {
        var fixture = new TokenExchangeServiceFixture(sessionExists: false, useEmptyTenantId: true);

        var result = await fixture.Service.ExchangeAsync(fixture.Context, CancellationToken.None);

        fixture.CreatedSession.Should().NotBeNull();
        result.SessionId.Should().Be(fixture.CreatedSession!.Id);
        result.User.ActiveTenantId.Should().NotBe(Guid.Empty);
        result.User.ActiveTenantId.Should().Be(fixture.ActiveMembership.TenantId.Value);
        result.AvailableTenants.Should().ContainSingle(tenant => tenant.TenantId == fixture.ActiveMembership.TenantId.Value);
    }

    [Fact]
    public async Task ExchangeToken_WhenNoActiveMemberships_ShouldThrow()
    {
        var fixture = new TokenExchangeServiceFixture(sessionExists: false, hasNoMemberships: true);

        var act = () => fixture.Service.ExchangeAsync(fixture.Context, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Active membership required for token exchange.");
    }

    [Fact]
    public async Task ExchangeToken_WhenFirstUserLogin_ShouldCreateSystemAdminAccess()
    {
        var fixture = new TokenExchangeServiceFixture(sessionExists: false, useEmptyTenantId: true, isFirstUser: true);

        var result = await fixture.Service.ExchangeAsync(fixture.Context, CancellationToken.None);

        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        fixture.UserRepository.Verify(x => x.GetUserCountAsync(It.IsAny<CancellationToken>()), Times.Once);
        fixture.TenantRepository.Verify(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        fixture.RoleRepository.Verify(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        fixture.MembershipRepository.Verify(x => x.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class TokenExchangeServiceFixture
    {
        private static readonly string EntraTokenValue = "entra-token";
        private static readonly string IpAddress = "127.0.0.1";
        private static readonly string UserAgent = "unit-test-agent";

        public TokenExchangeServiceFixture(bool sessionExists, bool useEmptyTenantId = false, bool hasNoMemberships = false, bool isFirstUser = false)
        {
            var now = DateTimeOffset.UtcNow;
            var activeTenantGuid = useEmptyTenantId ? Guid.Empty : Guid.NewGuid();
            var tenantId = useEmptyTenantId ? default : new TenantId(activeTenantGuid);
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var subject = "subject-123";
            var email = "user@example.com";
            var firstName = "Ada";
            var lastName = "Lovelace";

            var principal = BuildPrincipal(subject, email, firstName, lastName);
            ExpectedTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(EntraTokenValue)));

            TokenValidator = new Mock<IEntraTokenValidator>();
            TokenValidator
                .Setup(x => x.ValidateAsync(EntraTokenValue, It.IsAny<CancellationToken>()))
                .ReturnsAsync(principal);

            // For first user scenario, user doesn't exist yet
            if (isFirstUser)
            {
                User = null!;
                var capturedUser = (User?)null;

                UserRepository = new Mock<IUserRepository>();
                UserRepository
                    .Setup(x => x.GetByEntraSubjectIdAsync(It.Is<EntraSubjectId>(id => id.Value == subject), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
                UserRepository
                    .Setup(x => x.GetUserCountAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(0);
                UserRepository
                    .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .Callback<User, CancellationToken>((user, _) => capturedUser = user)
                    .Returns(Task.CompletedTask);
            }
            else
            {
                User = new User(userId, new EntraSubjectId(subject), email, firstName, lastName, now.AddDays(-30));

                UserRepository = new Mock<IUserRepository>();
                UserRepository
                    .Setup(x => x.GetByEntraSubjectIdAsync(It.Is<EntraSubjectId>(id => id.Value == subject), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(User);
                UserRepository
                    .Setup(x => x.UpdateAsync(User, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                UserRepository
                    .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            // Create actual tenant for membership even if context uses empty Guid
            var membershipTenantId = useEmptyTenantId ? new TenantId(Guid.NewGuid()) : tenantId;

            if (hasNoMemberships && !isFirstUser)
            {
                ActiveMembership = null!;
                MembershipRepository = new Mock<IMembershipRepository>();
                MembershipRepository
                    .Setup(x => x.GetActiveMembershipsForUserAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Array.Empty<Membership>());
            }
            else if (isFirstUser)
            {
                // First user scenario: no memberships initially, but they will be created
                ActiveMembership = null!;
                var capturedMembership = (Membership?)null;

                MembershipRepository = new Mock<IMembershipRepository>();
                MembershipRepository
                    .Setup(x => x.GetActiveMembershipsForUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => capturedMembership != null ? new[] { capturedMembership } : Array.Empty<Membership>());
                MembershipRepository
                    .Setup(x => x.AddAsync(It.IsAny<Membership>(), It.IsAny<CancellationToken>()))
                    .Callback<Membership, CancellationToken>((membership, _) => capturedMembership = membership)
                    .Returns(Task.CompletedTask);
            }
            else
            {
                ActiveMembership = new Membership(Guid.NewGuid(), userId, membershipTenantId, roleId, now.AddMonths(-1), null, now.AddMonths(1));

                MembershipRepository = new Mock<IMembershipRepository>();
                if (!useEmptyTenantId)
                {
                    MembershipRepository
                        .Setup(x => x.GetByUserAndTenantAsync(userId, tenantId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(ActiveMembership);
                }
                MembershipRepository
                    .Setup(x => x.GetActiveMembershipsForUserAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new[] { ActiveMembership });
            }

            ActiveTenant = (hasNoMemberships && !isFirstUser) ? null! : new Tenant(membershipTenantId, "Test District", TenantType.District, now.AddYears(-5));

            TenantRepository = new Mock<ITenantRepository>();
            if (isFirstUser)
            {
                var capturedTenant = (Tenant?)null;
                TenantRepository
                    .Setup(x => x.GetByExternalIdAsync("Platform", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Tenant?)null);
                TenantRepository
                    .Setup(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                    .Callback<Tenant, CancellationToken>((tenant, _) => capturedTenant = tenant)
                    .Returns(Task.CompletedTask);
                TenantRepository
                    .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<TenantId>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => capturedTenant != null ? new List<Tenant> { capturedTenant }.AsReadOnly() : new List<Tenant>().AsReadOnly());
            }
            else if (!hasNoMemberships)
            {
                TenantRepository
                    .Setup(x => x.GetByIdsAsync(It.Is<IEnumerable<TenantId>>(ids => ids.Contains(membershipTenantId)), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Tenant> { ActiveTenant }.AsReadOnly());
            }

            Role = new Role(roleId, "administrator", "Administrator", now.AddYears(-10), true);

            RoleRepository = new Mock<IRoleRepository>();
            if (isFirstUser)
            {
                var capturedRole = (Role?)null;
                RoleRepository
                    .Setup(x => x.GetByNameAsync("PlatformAdmin", It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Role?)null);
                RoleRepository
                    .Setup(x => x.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                    .Callback<Role, CancellationToken>((role, _) => capturedRole = role)
                    .Returns(Task.CompletedTask);
                RoleRepository
                    .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => capturedRole);
            }
            else
            {
                RoleRepository
                    .Setup(x => x.GetAsync(roleId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Role);
            }

            SessionRepository = new Mock<ISessionRepository>();

            if (sessionExists && !hasNoMemberships)
            {
                var createdAt = now.AddMinutes(-30);
                var expiresAt = now.AddMinutes(10);
                var sessionTenantId = useEmptyTenantId ? membershipTenantId : tenantId;
                ExistingSession = Session.Create(userId, ExpectedTokenHash, "existing-token", sessionTenantId, createdAt, expiresAt, IpAddress, UserAgent);

                SessionRepository
                    .Setup(x => x.GetByTokenHashAsync(ExpectedTokenHash, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(ExistingSession);
                SessionRepository
                    .Setup(x => x.UpdateAsync(ExistingSession, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                SessionRepository
                    .Setup(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
                    .Throws(new InvalidOperationException("Should not add a session when one already exists."));
            }
            else
            {
                SessionRepository
                    .Setup(x => x.GetByTokenHashAsync(ExpectedTokenHash, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Session?)null);
                SessionRepository
                    .Setup(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
                    .Callback<Session, CancellationToken>((session, _) => CreatedSession = session)
                    .Returns(Task.CompletedTask);
                SessionRepository
                    .Setup(x => x.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }

            SessionRepository
                .Setup(x => x.DeleteAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            SessionRepository
                .Setup(x => x.GetActiveSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Session?)null);
            SessionRepository
                .Setup(x => x.GetSessionsForUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Session>());
            SessionRepository
                .Setup(x => x.GetByTenantAsync(It.IsAny<TenantId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Session>());

            SessionStore = new Mock<ISessionStore>();
            SessionStore
                .Setup(x => x.CacheSessionAsync(It.IsAny<Session>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Callback<Session, TimeSpan, CancellationToken>((session, ttl, _) =>
                {
                    CachedSession = session;
                    CachedTtl = ttl;
                })
                .Returns(Task.CompletedTask);
            SessionStore
                .Setup(x => x.GetSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SessionCacheModel?)null);
            SessionStore
                .Setup(x => x.RemoveSessionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Settings = new IdentityModuleSettings
            {
                SessionSlidingExpirationMinutes = 45,
                JwtSigningKey = "ThisIsAVerySecureSigningKeyForTestingPurposesOnly12345",
                JwtIssuer = "TestIssuer",
                JwtAudience = "TestAudience"
            };

            var options = Options.Create(Settings);
            var logger = Mock.Of<ILogger<TokenExchangeService>>();

            var lmsTokenGeneratorMock = new Mock<ILmsTokenGenerator>();
            lmsTokenGeneratorMock
                .Setup(g => g.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTimeOffset>()))
                .Returns((Guid userId, Guid sessionId, DateTimeOffset expiresAt) =>
                    $"jwt.token.{userId}.{sessionId}");

            Service = new TokenExchangeService(
                logger,
                TokenValidator.Object,
                UserRepository.Object,
                TenantRepository.Object,
                MembershipRepository.Object,
                RoleRepository.Object,
                SessionRepository.Object,
                SessionStore.Object,
                lmsTokenGeneratorMock.Object,
                options);

            Context = new TokenExchangeCommandContext(EntraTokenValue, activeTenantGuid, IpAddress, UserAgent);
        }

        public TokenExchangeService Service { get; }

        public TokenExchangeCommandContext Context { get; }

        public Mock<IEntraTokenValidator> TokenValidator { get; }

        public Mock<IUserRepository> UserRepository { get; }

        public Mock<ITenantRepository> TenantRepository { get; }

        public Mock<IMembershipRepository> MembershipRepository { get; }

        public Mock<IRoleRepository> RoleRepository { get; }

        public Mock<ISessionRepository> SessionRepository { get; }

        public Mock<ISessionStore> SessionStore { get; }

        public User User { get; }

        public Membership ActiveMembership { get; }

        public Tenant ActiveTenant { get; }

        public Role Role { get; }

        public string ExpectedTokenHash { get; }

        public Session? ExistingSession { get; }

        public Session? CreatedSession { get; private set; }

        public Session? CachedSession { get; private set; }

        public TimeSpan? CachedTtl { get; private set; }

        public IdentityModuleSettings Settings { get; }

        private static ClaimsPrincipal BuildPrincipal(string subject, string email, string firstName, string lastName)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, subject),
                new(ClaimTypes.Email, email),
                new(ClaimTypes.GivenName, firstName),
                new(ClaimTypes.Surname, lastName)
            };

            var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
            return new ClaimsPrincipal(identity);
        }
    }
}
