using Aspire.Hosting;

namespace NorthStarET.NextGen.Lms.AspireTests.Tests;

[Collection(nameof(AspireTestCollection))]
public sealed class DistrictAdminTests
{
    private readonly AspireTestFixture _fixture;

    public DistrictAdminTests(AspireTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DistrictAdminInfrastructureIsConfigured()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert - District admin features require API, Redis for email queue, and DB
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasApi = model.Resources.Any(r => r.Name.Equals("northstaret-nextgen-lms-api", StringComparison.OrdinalIgnoreCase));
        var hasRedis = model.Resources.Any(r => r.Name.Equals("identity-redis", StringComparison.OrdinalIgnoreCase));
        var hasDb = model.Resources.Any(r => r.Name.Equals("identity-db", StringComparison.OrdinalIgnoreCase));
        
        Assert.True(hasApi, "District admin requires API");
        Assert.True(hasRedis, "District admin requires Redis for email queue");
        Assert.True(hasDb, "District admin requires database");
    }

    [Fact]
    public async Task DistrictAdminWebUIIsConfigured()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert - Web project needed for district admin UI
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasWeb = model.Resources.Any(r => r.Name.Equals("northstaret-nextgen-lms-web", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasWeb, "District admin UI requires Web project");
    }
}
