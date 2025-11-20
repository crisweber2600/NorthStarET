using Aspire.Hosting.Testing;
using Microsoft.Extensions.Logging;

namespace NorthStarET.NextGen.Lms.AspireTests;

/// <summary>
/// Shared fixture that provides the Aspire test builder configuration.
/// Note: This fixture is lightweight and doesn't start containers, making it suitable for CI/CD.
/// Each test builds the app independently to verify configuration without orchestration overhead.
/// </summary>
public sealed class AspireTestFixture : IAsyncLifetime
{
    public CancellationToken CancellationToken { get; private set; }

    public async Task InitializeAsync()
    {
        CancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token;
        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<IDistributedApplicationTestingBuilder> CreateBuilderAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.NorthStarET_NextGen_Lms_AppHost>(CancellationToken);

        // Configure logging for better diagnostics
        builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter("Aspire", LogLevel.Debug);
        });

        // Configure HTTP clients with resilience
        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
            clientBuilder.ConfigureHttpClient((_, client) => client.Timeout = TimeSpan.FromSeconds(30));
        });

        return builder;
    }
}

/// <summary>
/// Collection definition to share the AspireTestFixture across all test classes.
/// </summary>
[CollectionDefinition(nameof(AspireTestCollection))]
public class AspireTestCollection : ICollectionFixture<AspireTestFixture>
{
}
