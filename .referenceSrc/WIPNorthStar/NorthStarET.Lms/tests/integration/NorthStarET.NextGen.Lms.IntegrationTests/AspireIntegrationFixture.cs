using System;
using System.Net.Http;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace NorthStarET.NextGen.Lms.IntegrationTests;

/// <summary>
/// Aspire test fixture that starts the full application stack (API, Web, PostgreSQL, Redis)
/// for integration testing. Provides access to HTTP clients.
/// </summary>
public sealed class AspireIntegrationFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);
    private DistributedApplication? _app;

    public HttpClient ApiClient { get; private set; } = default!;

    public HttpClient WebClient { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;
        var testingBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.NorthStarET_NextGen_Lms_AppHost>(cancellationToken);

        // Ensure HTTP clients created through Aspire respect the shared policies defined in ServiceDefaults
        testingBuilder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.ConfigureHttpClient((_, client) => client.Timeout = TimeSpan.FromSeconds(30));
        });

        _app = await testingBuilder.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Wait for all resources to be healthy before proceeding
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("identity-db", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("identity-redis", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("northstaret-nextgen-lms-api", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("northstaret-nextgen-lms-web", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Create HTTP clients for API and Web
        ApiClient = _app.CreateHttpClient("northstaret-nextgen-lms-api");
        WebClient = _app.CreateHttpClient("northstaret-nextgen-lms-web");
    }

    public async Task DisposeAsync()
    {
        ApiClient?.Dispose();
        WebClient?.Dispose();

        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
