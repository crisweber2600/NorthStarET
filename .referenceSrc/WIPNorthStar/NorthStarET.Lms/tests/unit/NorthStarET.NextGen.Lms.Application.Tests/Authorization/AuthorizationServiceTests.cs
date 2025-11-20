using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Authorization.Services;
using NorthStarET.NextGen.Lms.Application.Common.Caching;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.Repositories;
using Xunit;

namespace NorthStarET.NextGen.Lms.Application.Tests.Authorization;

public sealed class AuthorizationServiceTests
{
    [Fact]
    public async Task CheckPermissionAsync_WhenDecisionExistsInCache_ShouldReturnCachedDecision()
    {
        var fixture = new AuthorizationServiceFixture();
        var cachedDecision = fixture.CreateDecision(allowed: true);

        fixture.Cache
            .Setup(x => x.GetAsync(fixture.UserId, fixture.TenantId, fixture.Resource, fixture.Action, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDecision);

        var result = await fixture.Service.CheckPermissionAsync(
            fixture.UserId,
            fixture.TenantId,
            fixture.Resource,
            fixture.Action,
            fixture.Context,
            CancellationToken.None);

        result.Should().BeSameAs(cachedDecision);
        fixture.DataService.Verify(
            x => x.FetchDecisionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>?>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.Cache.Verify(
            x => x.SetAsync(It.IsAny<AuthorizationDecision>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()),
            Times.Never);
        fixture.AuditRepository.Verify(
            x => x.AddAsync(It.IsAny<AuthorizationAuditLog>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CheckPermissionAsync_WhenCacheMissAndDecisionAllowed_ShouldFetchAndCacheDecision()
    {
        var fixture = new AuthorizationServiceFixture();
        var fetchedDecision = fixture.CreateDecision(allowed: true);
        var expectedTtl = TimeSpan.FromMinutes(fixture.Settings.AuthorizationCacheTtlMinutes);

        fixture.Cache
            .Setup(x => x.GetAsync(fixture.UserId, fixture.TenantId, fixture.Resource, fixture.Action, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorizationDecision?)null);

        fixture.DataService
            .Setup(x => x.FetchDecisionAsync(fixture.UserId, fixture.TenantId, fixture.Resource, fixture.Action, fixture.Context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fetchedDecision);

        AuthorizationDecision? cachedDecision = null;
        TimeSpan? cachedTtl = null;

        fixture.Cache
            .Setup(x => x.SetAsync(fetchedDecision, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<AuthorizationDecision, TimeSpan, CancellationToken>((decision, ttl, _) =>
            {
                cachedDecision = decision;
                cachedTtl = ttl;
            })
            .Returns(Task.CompletedTask);

        var result = await fixture.Service.CheckPermissionAsync(
            fixture.UserId,
            fixture.TenantId,
            fixture.Resource,
            fixture.Action,
            fixture.Context,
            CancellationToken.None);

        result.Should().BeSameAs(fetchedDecision);
        cachedDecision.Should().BeSameAs(fetchedDecision);
        cachedTtl.Should().Be(expectedTtl);
        fixture.AuditRepository.Verify(
            x => x.AddAsync(
                It.Is<AuthorizationAuditLog>(log => log.UserId == fixture.UserId && log.Allowed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckPermissionAsync_WhenDecisionDenied_ShouldCacheDeniedDecision()
    {
        var fixture = new AuthorizationServiceFixture();
        var deniedDecision = fixture.CreateDecision(allowed: false, reason: "Missing permission");
        var expectedTtl = TimeSpan.FromMinutes(fixture.Settings.AuthorizationCacheTtlMinutes);

        fixture.Cache
            .Setup(x => x.GetAsync(fixture.UserId, fixture.TenantId, fixture.Resource, fixture.Action, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthorizationDecision?)null);

        fixture.DataService
            .Setup(x => x.FetchDecisionAsync(fixture.UserId, fixture.TenantId, fixture.Resource, fixture.Action, fixture.Context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deniedDecision);

        AuthorizationDecision? cachedDecision = null;
        TimeSpan? cachedTtl = null;

        fixture.Cache
            .Setup(x => x.SetAsync(deniedDecision, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<AuthorizationDecision, TimeSpan, CancellationToken>((decision, ttl, _) =>
            {
                cachedDecision = decision;
                cachedTtl = ttl;
            })
            .Returns(Task.CompletedTask);

        var result = await fixture.Service.CheckPermissionAsync(
            fixture.UserId,
            fixture.TenantId,
            fixture.Resource,
            fixture.Action,
            fixture.Context,
            CancellationToken.None);

        result.Should().BeSameAs(deniedDecision);
        result.Allowed.Should().BeFalse();
        result.Reason.Should().Be("Missing permission");
        cachedDecision.Should().BeSameAs(deniedDecision);
        cachedTtl.Should().Be(expectedTtl);
        fixture.AuditRepository.Verify(
            x => x.AddAsync(
                It.Is<AuthorizationAuditLog>(log => log.UserId == fixture.UserId && !log.Allowed && log.Reason == "Missing permission"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class AuthorizationServiceFixture
    {
        public AuthorizationServiceFixture()
        {
            UserId = Guid.NewGuid();
            TenantId = Guid.NewGuid();
            Resource = "Grades";
            Action = "Write";
            Context = new Dictionary<string, string> { ["studentId"] = Guid.NewGuid().ToString() };

            Settings = new IdentityModuleSettings
            {
                AuthorizationCacheTtlMinutes = 10
            };

            Logger = Mock.Of<ILogger<AuthorizationService>>();
            DataService = new Mock<IIdentityAuthorizationDataService>(MockBehavior.Strict);
            Cache = new Mock<IAuthorizationCache>(MockBehavior.Strict);
            AuditRepository = new Mock<IAuthorizationAuditRepository>(MockBehavior.Strict);

            AuditRepository
                .Setup(x => x.AddAsync(It.IsAny<AuthorizationAuditLog>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var options = Options.Create(Settings);

            Service = new AuthorizationService(Logger, DataService.Object, Cache.Object, AuditRepository.Object, options);
        }

        public Guid UserId { get; }

        public Guid TenantId { get; }

        public string Resource { get; }

        public string Action { get; }

        public IReadOnlyDictionary<string, string> Context { get; }

        public Mock<IIdentityAuthorizationDataService> DataService { get; }

        public Mock<IAuthorizationCache> Cache { get; }

        public Mock<IAuthorizationAuditRepository> AuditRepository { get; }

        public ILogger<AuthorizationService> Logger { get; }

        public IdentityModuleSettings Settings { get; }

        public AuthorizationService Service { get; }

        public AuthorizationDecision CreateDecision(bool allowed, string? reason = null)
        {
            return new AuthorizationDecision(
                UserId,
                TenantId,
                Resource,
                Action,
                allowed,
                allowed ? Guid.NewGuid() : null,
                allowed ? "Teacher" : null,
                reason,
                DateTimeOffset.UtcNow);
        }
    }
}
