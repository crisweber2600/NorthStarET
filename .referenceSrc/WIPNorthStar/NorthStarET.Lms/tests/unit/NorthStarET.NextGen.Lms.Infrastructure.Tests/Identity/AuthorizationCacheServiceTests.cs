using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NorthStarET.NextGen.Lms.Application.Authorization.Models;
using NorthStarET.NextGen.Lms.Application.Common.Configuration;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Caching;
using StackExchange.Redis;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Identity;

public sealed class AuthorizationCacheServiceTests
{
    [Fact]
    public async Task GetAsync_WhenDecisionPresent_ShouldReturnDeserializedDecision()
    {
        var fixture = new AuthorizationCacheServiceFixture();
        var decision = fixture.CreateDecision();
        var payload = JsonSerializer.Serialize(decision);

        fixture.Database
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)payload);

        var result = await fixture.Service.GetAsync(
            decision.UserId,
            decision.TenantId,
            decision.Resource,
            decision.Action,
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Allowed.Should().BeTrue();
        result.CheckedAt.Should().BeCloseTo(decision.CheckedAt, TimeSpan.FromSeconds(1));
        fixture.Database.Verify(x => x.StringGetAsync(
            It.Is<RedisKey>(key => key == fixture.ExpectedKey(decision)),
            CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenPayloadInvalid_ShouldRemoveCacheEntry()
    {
        var fixture = new AuthorizationCacheServiceFixture();
        var decision = fixture.CreateDecision();

        fixture.Database
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)"{ invalid json }");

        var result = await fixture.Service.GetAsync(
            decision.UserId,
            decision.TenantId,
            decision.Resource,
            decision.Action,
            CancellationToken.None);

        result.Should().BeNull();
        fixture.Database.Verify(x => x.KeyDeleteAsync(
            It.Is<RedisKey>(key => key == fixture.ExpectedKey(decision)),
            CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeDecision_WithConfiguredExpiry()
    {
        var fixture = new AuthorizationCacheServiceFixture();
        var decision = fixture.CreateDecision();
        var expectedTtl = TimeSpan.FromMinutes(fixture.Settings.AuthorizationCacheTtlMinutes);
        RedisValue? storedValue = null;
        TimeSpan? storedExpiry = null;

        fixture.Database
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan?, When, CommandFlags>((key, value, expiry, _, _) =>
            {
                storedValue = value;
                storedExpiry = expiry;
            })
            .ReturnsAsync(true);

        fixture.Database
            .Setup(x => x.SetAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        fixture.Database
            .Setup(x => x.KeyExpireAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<ExpireWhen>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await fixture.Service.SetAsync(decision, expectedTtl, CancellationToken.None);

        fixture.Database.Verify(x => x.StringSetAsync(
            It.Is<RedisKey>(key => key == fixture.ExpectedKey(decision)),
            It.IsAny<RedisValue>(),
            expectedTtl,
            When.Always,
            CommandFlags.None), Times.Once);

        fixture.Database.Verify(x => x.SetAddAsync(
            It.Is<RedisKey>(key => key == fixture.ExpectedIndexKey(decision)),
            It.Is<RedisValue>(v => v.ToString() == fixture.ExpectedKey(decision).ToString()),
            CommandFlags.None), Times.Once);

        fixture.Database.Verify(x => x.KeyExpireAsync(
            It.Is<RedisKey>(key => key == fixture.ExpectedIndexKey(decision)),
            It.Is<TimeSpan?>(ts => ts == expectedTtl.Add(expectedTtl)),
            ExpireWhen.Always,
            CommandFlags.FireAndForget), Times.Once);

        storedValue.HasValue.Should().BeTrue();
        var roundTripped = JsonSerializer.Deserialize<AuthorizationDecision>(storedValue!.Value!.ToString());
        roundTripped.Should().NotBeNull();
        roundTripped!.Allowed.Should().BeTrue();
        storedExpiry.Should().Be(expectedTtl);
    }

    [Fact]
    public async Task ClearForUserAndTenantAsync_ShouldDeleteAllCacheEntriesUsingSet()
    {
        var fixture = new AuthorizationCacheServiceFixture();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var indexKey = fixture.ExpectedIndexKey(userId, tenantId);

        // Simulate 3 cached authorization entries
        var key1 = (RedisValue)$"authz:{userId}:{tenantId}:Grades:Read";
        var key2 = (RedisValue)$"authz:{userId}:{tenantId}:Grades:Write";
        var key3 = (RedisValue)$"authz:{userId}:{tenantId}:Students:Read";
        var keys = new[] { key1, key2, key3 };

        fixture.Database
            .Setup(x => x.SetMembersAsync(indexKey, CommandFlags.None))
            .ReturnsAsync(keys);

        fixture.Database
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey[]>(), CommandFlags.None))
            .ReturnsAsync(3);

        fixture.Database
            .Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue[]>(), CommandFlags.None))
            .ReturnsAsync(3);

        await fixture.Service.ClearForUserAndTenantAsync(userId, tenantId, CancellationToken.None);

        // Verify that SetMembers was called to retrieve tracked keys
        fixture.Database.Verify(x => x.SetMembersAsync(indexKey, CommandFlags.None), Times.Once);

        // Verify batch delete was called with the correct keys
        fixture.Database.Verify(x => x.KeyDeleteAsync(
            It.Is<RedisKey[]>(k => k.Length == 3),
            CommandFlags.None), Times.Once);

        // Verify that members were removed from the index set (not the entire set deleted)
        fixture.Database.Verify(x => x.SetRemoveAsync(
            indexKey,
            It.Is<RedisValue[]>(v => v.Length == 3),
            CommandFlags.None), Times.Once);
    }

    [Fact]
    public async Task ClearForUserAndTenantAsync_WhenNoKeys_ShouldNotModifyIndexSet()
    {
        var fixture = new AuthorizationCacheServiceFixture();
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var indexKey = fixture.ExpectedIndexKey(userId, tenantId);

        fixture.Database
            .Setup(x => x.SetMembersAsync(indexKey, CommandFlags.None))
            .ReturnsAsync(Array.Empty<RedisValue>());

        await fixture.Service.ClearForUserAndTenantAsync(userId, tenantId, CancellationToken.None);

        // Verify that SetMembers was called
        fixture.Database.Verify(x => x.SetMembersAsync(indexKey, CommandFlags.None), Times.Once);

        // Verify batch delete was NOT called (no keys to delete)
        fixture.Database.Verify(x => x.KeyDeleteAsync(
            It.IsAny<RedisKey[]>(),
            CommandFlags.None), Times.Never);

        // Verify the index set was NOT modified (no members to remove)
        fixture.Database.Verify(x => x.SetRemoveAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue[]>(),
            CommandFlags.None), Times.Never);
    }

    private sealed class AuthorizationCacheServiceFixture
    {
        public AuthorizationCacheServiceFixture()
        {
            Connection = new Mock<IConnectionMultiplexer>(MockBehavior.Strict);
            Database = new Mock<IDatabase>(MockBehavior.Strict);

            Connection
                .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(Database.Object);

            Database
                .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            Settings = new IdentityModuleSettings
            {
                AuthorizationCacheTtlMinutes = 10
            };

            var options = Options.Create(Settings);

            Service = new AuthorizationCacheService(Connection.Object, options, NullLogger<AuthorizationCacheService>.Instance);
        }

        public Mock<IConnectionMultiplexer> Connection { get; }

        public Mock<IDatabase> Database { get; }

        public IdentityModuleSettings Settings { get; }

        public AuthorizationCacheService Service { get; }

        public AuthorizationDecision CreateDecision()
        {
            return new AuthorizationDecision(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Grades",
                "Write",
                allowed: true,
                Guid.NewGuid(),
                "Teacher",
                reason: null,
                DateTimeOffset.UtcNow);
        }

        public RedisKey ExpectedKey(AuthorizationDecision decision)
        {
            return (RedisKey)$"authz:{decision.UserId}:{decision.TenantId}:{decision.Resource}:{decision.Action}";
        }

        public RedisKey ExpectedIndexKey(AuthorizationDecision decision)
        {
            return (RedisKey)$"authz:index:{decision.UserId}:{decision.TenantId}";
        }

        public RedisKey ExpectedIndexKey(Guid userId, Guid tenantId)
        {
            return (RedisKey)$"authz:index:{userId}:{tenantId}";
        }
    }
}