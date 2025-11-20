using Aspire.Hosting;

namespace NorthStarET.NextGen.Lms.AspireTests.Tests;

[Collection(nameof(AspireTestCollection))]
public sealed class DistrictManagementTests
{
    private readonly AspireTestFixture _fixture;

    public DistrictManagementTests(AspireTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DistrictManagementResourcesAreConfigured()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert - Verify district management requires proper infrastructure
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasDb = model.Resources.Any(r => r.Name.Equals("identity-db", StringComparison.OrdinalIgnoreCase));
        var hasRedis = model.Resources.Any(r => r.Name.Equals("identity-redis", StringComparison.OrdinalIgnoreCase));
        var hasApi = model.Resources.Any(r => r.Name.Equals("northstaret-nextgen-lms-api", StringComparison.OrdinalIgnoreCase));
        
        Assert.True(hasDb, "District management requires database");
        Assert.True(hasRedis, "District management requires Redis");
        Assert.True(hasApi, "District management requires API");
    }

    [Fact]
    public async Task AppHostIncludesWebResourceForDistrictUI()
    {
        // Arrange
        var builder = await _fixture.CreateBuilderAsync();

        // Act
        await using var app = await builder.BuildAsync();

        // Assert
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var hasWeb = model.Resources.Any(r => r.Name.Equals("northstaret-nextgen-lms-web", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasWeb, "District UI requires Web project");
    }
}
