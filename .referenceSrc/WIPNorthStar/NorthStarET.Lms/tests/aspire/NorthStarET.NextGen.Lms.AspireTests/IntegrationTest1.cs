using Aspire.Hosting;

namespace NorthStarET.NextGen.Lms.AspireTests.Tests;

[Collection(nameof(AspireTestCollection))]
public class AppHostTests
{
    private readonly AspireTestFixture _fixture;

    public AppHostTests(AspireTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AppHostCanBeBuilt()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert
        Assert.NotNull(app);
    }

    [Fact]
    public async Task AppHostConfiguresExpectedResourceCount()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert - Verify we have configured the expected minimum number of resources
        // (PostgreSQL server, DB, Redis, API, Web = 5 resources minimum)
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        Assert.True(model.Resources.Count >= 5, $"Expected at least 5 resources, found {model.Resources.Count}");
    }

    [Fact]
    public async Task AppHostIncludesPostgreSQLDatabase()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasPostgres = model.Resources.Any(r => r.Name.Equals("identity-db", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasPostgres, "AppHost should include PostgreSQL database configuration");
    }

    [Fact]
    public async Task AppHostIncludesRedisCache()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasRedis = model.Resources.Any(r => r.Name.Equals("identity-redis", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasRedis, "AppHost should include Redis cache configuration");
    }

    [Fact]
    public async Task AppHostIncludesApiProject()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasApi = model.Resources.Any(r => r.Name.Equals("northstaret-nextgen-lms-api", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasApi, "AppHost should include API project");
    }

    [Fact]
    public async Task AppHostIncludesWebProject()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasWeb = model.Resources.Any(r => r.Name.Equals("northstaret-nextgen-lms-web", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasWeb, "AppHost should include Web project");
    }
}
