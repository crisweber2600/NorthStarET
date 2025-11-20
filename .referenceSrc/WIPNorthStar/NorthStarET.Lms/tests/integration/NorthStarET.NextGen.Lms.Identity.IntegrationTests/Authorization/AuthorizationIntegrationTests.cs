using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;
using StackExchange.Redis;
using Xunit;

namespace NorthStarET.NextGen.Lms.Identity.IntegrationTests.Authorization;

public class AuthorizationIntegrationTests : IClassFixture<AspireIdentityFixture>, IAsyncLifetime
{
    private readonly AspireIdentityFixture fixture;
    private readonly Guid userId = Guid.NewGuid();
    private readonly Guid tenantId = Guid.NewGuid();
    private readonly Guid roleId = Guid.NewGuid();
    private readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

    public AuthorizationIntegrationTests(AspireIdentityFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task CheckPermission_WhenCacheMiss_ShouldPersistDecisionToCache()
    {
        await ResetStateAsync();
        await SeedIdentityDataAsync();

        var request = new
        {
            userId,
            tenantId,
            resource = "Grades",
            action = "Write"
        };

        var response = await fixture.ApiClient.PostAsJsonAsync("/v1/identity/authorize/check", request);

        response.IsSuccessStatusCode.Should().BeTrue();

        var payload = await response.Content.ReadFromJsonAsync<AuthorizationDecisionResponse>(serializerOptions);
        payload.Should().NotBeNull();
        payload!.Allowed.Should().BeTrue();
        payload.RoleId.Should().Be(roleId);

    await using var redis = await ConnectionMultiplexer.ConnectAsync(fixture.IdentityRedisConnectionString);
    var db = redis.GetDatabase();
        var redisKey = new RedisKey($"authz:{userId}:{tenantId}:Grades:Write");
        var cachedValue = await db.StringGetAsync(redisKey);

        cachedValue.HasValue.Should().BeTrue("authorization decision should be cached after cache miss");
    }

    [Fact]
    public async Task CheckPermission_WhenDecisionCached_ShouldReturnCachedResult()
    {
        await ResetStateAsync();
        await SeedIdentityDataAsync();

    await using var redis = await ConnectionMultiplexer.ConnectAsync(fixture.IdentityRedisConnectionString);
    var db = redis.GetDatabase();
        var cachedDecision = new AuthorizationDecisionResponse
        {
            Allowed = false,
            UserId = userId,
            TenantId = tenantId,
            Resource = "Grades",
            Action = "Write",
            RoleId = null,
            RoleName = null,
            Reason = "cached-deny",
            CheckedAt = DateTimeOffset.UtcNow
        };

        var serialized = JsonSerializer.Serialize(cachedDecision, serializerOptions);
        await db.StringSetAsync(new RedisKey($"authz:{userId}:{tenantId}:Grades:Write"), serialized, TimeSpan.FromMinutes(5));

        var request = new
        {
            userId,
            tenantId,
            resource = "Grades",
            action = "Write"
        };

        var response = await fixture.ApiClient.PostAsJsonAsync("/v1/identity/authorize/check", request);
        response.IsSuccessStatusCode.Should().BeTrue();

        var payload = await response.Content.ReadFromJsonAsync<AuthorizationDecisionResponse>(serializerOptions);
        payload.Should().NotBeNull();
        payload!.Allowed.Should().BeFalse();
        payload.Reason.Should().Be("cached-deny");
    }

    public async Task InitializeAsync()
    {
        await ResetStateAsync();
    }

    public async Task DisposeAsync()
    {
        await ResetStateAsync();
    }

    private async Task SeedIdentityDataAsync()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(fixture.IdentityPostgresConnectionString)
            .Options;

        await using var context = new IdentityDbContext(options);

        var user = new User(
            userId,
            new EntraSubjectId("integration-test-subject"),
            "integration.user@nse.edu",
            "Integration",
            "User",
            DateTimeOffset.UtcNow);

        var tenant = new Tenant(
            new TenantId(tenantId),
            "Integration District",
            TenantType.District,
            DateTimeOffset.UtcNow,
            isActive: true,
            parentTenantId: null,
            externalId: null);

        var role = new Role(
            roleId,
            "Teacher",
            "Teacher",
            DateTimeOffset.UtcNow,
            true,
            new[]
            {
                new Permission("Grades", "Write")
            });

        var membership = new Membership(
            Guid.NewGuid(),
            userId,
            new TenantId(tenantId),
            roleId,
            DateTimeOffset.UtcNow,
            null,
            null);

        await context.AuthorizationAuditLogs.ExecuteDeleteAsync();
        await context.Memberships.ExecuteDeleteAsync();
        await context.Sessions.ExecuteDeleteAsync();
        await context.Users.ExecuteDeleteAsync();
        await context.Roles.ExecuteDeleteAsync();
        await context.Tenants.ExecuteDeleteAsync();

        await context.Users.AddAsync(user);
        await context.Tenants.AddAsync(tenant);
        await context.Roles.AddAsync(role);
        await context.Memberships.AddAsync(membership);

        await context.SaveChangesAsync();
    }

    private async Task ResetStateAsync()
    {
    await using var redis = await ConnectionMultiplexer.ConnectAsync(fixture.IdentityRedisConnectionString);
    var db = redis.GetDatabase();
    await db.ExecuteAsync("FLUSHDB");
    }

    private sealed class AuthorizationDecisionResponse
    {
        public bool Allowed { get; set; }

        public Guid UserId { get; set; }

        public Guid TenantId { get; set; }

        public string Resource { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public Guid? RoleId { get; set; }

        public string? RoleName { get; set; }

        public string? Reason { get; set; }

        public DateTimeOffset CheckedAt { get; set; }
    }
}
