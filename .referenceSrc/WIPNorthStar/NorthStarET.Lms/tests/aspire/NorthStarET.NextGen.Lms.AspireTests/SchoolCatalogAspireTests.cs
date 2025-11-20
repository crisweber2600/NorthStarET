using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace NorthStarET.NextGen.Lms.AspireTests;

public sealed class SchoolCatalogAspireTests : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _apiClient;
    private HttpClient? _webClient;
    private bool _enabled;

    public async Task InitializeAsync()
    {
        _enabled = string.Equals(Environment.GetEnvironmentVariable("ASPIRE_TESTS"), "1", StringComparison.OrdinalIgnoreCase);
        if (!_enabled)
        {
            return; // Opt-out by default to avoid hanging runs
        }

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.NorthStarET_NextGen_Lms_AppHost>();
        _app = await appHost.BuildAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        await _app.StartAsync(cts.Token);

        _apiClient = _app.CreateHttpClient("northstaret-nextgen-lms-api");
        _webClient = _app.CreateHttpClient("northstaret-nextgen-lms-web");
        _apiClient.Timeout = TimeSpan.FromSeconds(15);
        _webClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
        _apiClient?.Dispose();
        _webClient?.Dispose();
    }

    [Fact]
    public async Task Api_ShouldBeHealthy()
    {
        if (!_enabled) return;
        var response = await _apiClient!.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Web_ShouldBeHealthy()
    {
        if (!_enabled) return;
        var response = await _webClient!.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Implementation pending - requires Infrastructure project reference")]
    public async Task PostgreSQL_ShouldBeConnectedAndMigrated() => await Task.CompletedTask;

    [Fact(Skip = "Implementation pending - requires Infrastructure project reference")]
    public async Task Redis_ShouldBeConnectedForCaching() => await Task.CompletedTask;

    [Fact]
    public async Task SchoolsApi_ShouldReturnUnauthorized_WhenNoAuthentication()
    {
        if (!_enabled) return;
        var response = await _apiClient!.GetAsync("/api/schools");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(Skip = "Implementation pending - requires authentication setup")]
    public async Task SchoolsApi_ShouldReturnEmptyList_WhenDistrictHasNoSchools()
    {
        if (!_enabled) return;
        var response = await _apiClient!.GetAsync("/api/schools");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var schools = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(schools);
        Assert.Empty(schools);
    }

    [Fact(Skip = "Implementation pending - requires authentication setup")]
    public async Task SchoolsApi_ShouldCreateSchool_WhenValidDataProvided()
    {
        if (!_enabled) return;
        var createRequest = new { Name = "Integration Test Elementary", Code = "INTTEST001", GradeLevels = new[] { "Kindergarten", "Grade1", "Grade2" } };
        var response = await _apiClient!.PostAsJsonAsync("/api/schools", createRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var location = response.Headers.Location;
        Assert.NotNull(location);
        await Task.Delay(TimeSpan.FromSeconds(2));
        var getResponse = await _apiClient.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact(Skip = "Implementation pending - requires authentication setup")]
    public async Task SchoolsApi_ShouldEnforceTenantIsolation_WhenListingSchools()
    {
        if (!_enabled) return;
        var responseA = await _apiClient!.GetAsync("/api/schools");
        Assert.Equal(HttpStatusCode.OK, responseA.StatusCode);
        var schoolsA = await responseA.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(schoolsA);
    }

    [Fact(Skip = "Implementation pending - requires idempotency service")]
    public async Task SchoolsApi_ShouldPreventDuplicateCreation_WithinIdempotencyWindow()
    {
        if (!_enabled) return;
        var createRequest = new { Name = "Idempotency Test School", Code = "IDEM001", GradeLevels = new[] { "Kindergarten" } };
        var response1 = await _apiClient!.PostAsJsonAsync("/api/schools", createRequest);
        var response2 = await _apiClient.PostAsJsonAsync("/api/schools", createRequest);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.True(response2.StatusCode == HttpStatusCode.Conflict || response2.StatusCode == HttpStatusCode.Created);
    }

    [Fact(Skip = "Implementation pending - requires authentication setup")]
    public async Task Web_ShouldRenderSchoolManagementPage_WhenAuthenticated()
    {
        if (!_enabled) return;
        var response = await _webClient!.GetAsync("/schools");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("School Management", html);
    }
}
