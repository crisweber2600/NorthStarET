using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace NorthStarET.NextGen.Lms.IntegrationTests;

/// <summary>
/// Example integration tests demonstrating the use of AspireIntegrationFixture.
/// This fixture starts the full Aspire stack (API, Web, PostgreSQL, Redis) and provides
/// HTTP clients for making real requests to the application.
/// </summary>
public sealed class AspireFixtureExampleTests : IClassFixture<AspireIntegrationFixture>
{
    private readonly AspireIntegrationFixture _fixture;

    public AspireFixtureExampleTests(AspireIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ApiClient_CanReachHealthEndpoint()
    {
        // Act
        var response = await _fixture.ApiClient.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WebClient_CanReachRootPage()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                    response.StatusCode == HttpStatusCode.Redirect || 
                    response.StatusCode == HttpStatusCode.Found,
                    $"Expected success or redirect, got {response.StatusCode}");
    }

    [Fact]
    public async Task ApiClient_RequiresAuthenticationForProtectedEndpoints()
    {
        // Act
        var response = await _fixture.ApiClient.GetAsync("/api/districts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
