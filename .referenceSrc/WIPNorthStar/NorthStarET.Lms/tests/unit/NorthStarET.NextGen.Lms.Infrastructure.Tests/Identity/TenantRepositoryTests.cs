using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Identity.Entities;
using NorthStarET.NextGen.Lms.Domain.Identity.ValueObjects;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence;
using NorthStarET.NextGen.Lms.Infrastructure.Identity.Persistence.Repositories;
using Xunit;

namespace NorthStarET.NextGen.Lms.Infrastructure.Tests.Identity;

public sealed class TenantRepositoryTests
{
    [Fact]
    public async Task GetByIdsAsync_WithValidTenantIds_ShouldReturnMatchingTenants()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IdentityDbContext(options);
        var repository = new TenantRepository(context);

        var tenant1Id = new TenantId(Guid.NewGuid());
        var tenant2Id = new TenantId(Guid.NewGuid());
        var tenant3Id = new TenantId(Guid.NewGuid());

        var tenant1 = new Tenant(tenant1Id, "Tenant1", TenantType.District, DateTimeOffset.UtcNow);
        var tenant2 = new Tenant(tenant2Id, "Tenant2", TenantType.District, DateTimeOffset.UtcNow);
        var tenant3 = new Tenant(tenant3Id, "Tenant3", TenantType.District, DateTimeOffset.UtcNow);

        await context.Tenants.AddRangeAsync(tenant1, tenant2, tenant3);
        await context.SaveChangesAsync();

        var idsToFind = new[] { tenant1Id, tenant3Id };

        // Act
        var foundTenants = await repository.GetByIdsAsync(idsToFind);

        // Assert
        foundTenants.Should().HaveCount(2);
        foundTenants.Should().Contain(t => t.Id == tenant1Id);
        foundTenants.Should().Contain(t => t.Id == tenant3Id);
        foundTenants.Should().NotContain(t => t.Id == tenant2Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IdentityDbContext(options);
        var repository = new TenantRepository(context);

        // Act
        var foundTenants = await repository.GetByIdsAsync(Array.Empty<TenantId>());

        // Assert
        foundTenants.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdsAsync_WithNonExistentIds_ShouldReturnEmptyCollection()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IdentityDbContext(options);
        var repository = new TenantRepository(context);

        var nonExistentIds = new[] { new TenantId(Guid.NewGuid()), new TenantId(Guid.NewGuid()) };

        // Act
        var foundTenants = await repository.GetByIdsAsync(nonExistentIds);

        // Assert
        foundTenants.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByExternalIdAsync_WithValidExternalId_ShouldReturnTenant()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IdentityDbContext(options);
        var repository = new TenantRepository(context);

        var tenantId = new TenantId(Guid.NewGuid());
        var externalId = "external-123";
        var tenant = new Tenant(tenantId, "TestTenant", TenantType.District, DateTimeOffset.UtcNow, externalId: externalId);

        await context.Tenants.AddAsync(tenant);
        await context.SaveChangesAsync();

        // Act
        var foundTenant = await repository.GetByExternalIdAsync(externalId);

        // Assert
        foundTenant.Should().NotBeNull();
        foundTenant!.Id.Should().Be(tenantId);
        foundTenant.ExternalId.Should().Be(externalId);
    }

    [Fact]
    public async Task GetByExternalIdAsync_WithNonExistentExternalId_ShouldReturnNull()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new IdentityDbContext(options);
        var repository = new TenantRepository(context);

        // Act
        var foundTenant = await repository.GetByExternalIdAsync("non-existent");

        // Assert
        foundTenant.Should().BeNull();
    }
}
