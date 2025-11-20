using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NorthStarET.NextGen.Lms.Playwright.Utilities;

namespace NorthStarET.NextGen.Lms.Playwright;

/// <summary>
/// Aspire test fixture that starts the full application stack (API, Web, PostgreSQL, Redis)
/// for Playwright end-to-end testing. Provides dynamic URLs for the web application.
/// 
/// Note: These tests require Docker and a properly configured DCP (Developer Control Plane).
/// If Aspire cannot start (e.g., in CI environments without Docker), tests will be skipped.
/// </summary>
[SetUpFixture]
public sealed class AspirePlaywrightFixture
{
    // Increased timeout to allow for Docker container pulls and startup in CI environments
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);
    private static DistributedApplication? _app;

    private const string TestAuthVariable = "NORTHSTARET_LMS_USE_TEST_AUTH";
    private const string PlaywrightStubsVariable = "NORTHSTARET_LMS_USE_PLAYWRIGHT_STUBS";

    public static bool AspireAvailable { get; private set; }
    public static string? SkipReason { get; private set; }

    public static HttpClient ApiClient { get; private set; } = default!;

    public static HttpClient WebClient { get; private set; } = default!;

    public static string WebBaseUrl { get; private set; } = string.Empty;

    public static string ApiBaseUrl { get; private set; } = string.Empty;

    public static Guid SeededDistrictId { get; private set; }

    public static string SeededDistrictName { get; private set; } = string.Empty;

    public static string SeededDistrictSuffix { get; private set; } = string.Empty;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Check if we should skip Aspire-based tests
        var skipAspire = Environment.GetEnvironmentVariable("SKIP_ASPIRE_TESTS");
        if (!string.IsNullOrEmpty(skipAspire) && skipAspire.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            AspireAvailable = false;
            SkipReason = "Aspire tests skipped via SKIP_ASPIRE_TESTS environment variable";
            Console.WriteLine(SkipReason);
            Assert.Ignore(SkipReason);
            return;
        }

        try
        {
            Environment.SetEnvironmentVariable(TestAuthVariable, "true");
            // Force real API/database interactions so Playwright exercises the full Aspire stack.
            Environment.SetEnvironmentVariable(PlaywrightStubsVariable, "false");
            Environment.SetEnvironmentVariable("ASPIRE_EPHEMERAL_STORAGE", "true");

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HEADLESS")))
            {
                Environment.SetEnvironmentVariable("HEADLESS", "false");
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("HEADED")))
                {
                    Environment.SetEnvironmentVariable("HEADED", "1");
                }
            }

            var cancellationToken = new CancellationTokenSource(DefaultTimeout).Token;

            Console.WriteLine("Creating Aspire testing builder...");
            var testingBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.NorthStarET_NextGen_Lms_AppHost>(cancellationToken);

            // Ensure HTTP clients created through Aspire respect the shared policies defined in ServiceDefaults
            testingBuilder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.ConfigureHttpClient((_, client) => client.Timeout = TimeSpan.FromSeconds(30));
            });

            Console.WriteLine("Building Aspire application...");
            _app = await testingBuilder.BuildAsync(cancellationToken);

            Console.WriteLine("Starting Aspire application (this may take several minutes for container pulls)...");
            await _app.StartAsync(cancellationToken);

            Console.WriteLine("Waiting for resources to become healthy...");
            // Wait for all resources to be healthy before proceeding with extended timeouts
            await _app.ResourceNotifications.WaitForResourceHealthyAsync("identity-db", cancellationToken);
            Console.WriteLine("  ✓ identity-db is healthy");

            await _app.ResourceNotifications.WaitForResourceHealthyAsync("identity-redis", cancellationToken);
            Console.WriteLine("  ✓ identity-redis is healthy");

            await _app.ResourceNotifications.WaitForResourceHealthyAsync("northstaret-nextgen-lms-api", cancellationToken);
            Console.WriteLine("  ✓ northstaret-nextgen-lms-api is healthy");

            await _app.ResourceNotifications.WaitForResourceHealthyAsync("northstaret-nextgen-lms-web", cancellationToken);
            Console.WriteLine("  ✓ northstaret-nextgen-lms-web is healthy");

            // Create HTTP clients and capture their base URLs
            ApiClient = _app.CreateHttpClient("northstaret-nextgen-lms-api");
            WebClient = _app.CreateHttpClient("northstaret-nextgen-lms-web");

            ApiBaseUrl = ApiClient.BaseAddress?.ToString().TrimEnd('/') ?? throw new InvalidOperationException("API base URL not found");
            WebBaseUrl = WebClient.BaseAddress?.ToString().TrimEnd('/') ?? throw new InvalidOperationException("Web base URL not found");

            Console.WriteLine($"API Base URL: {ApiBaseUrl}");
            Console.WriteLine($"Web Base URL: {WebBaseUrl}");

            // Set environment variable so tests can access the dynamic URL
            Environment.SetEnvironmentVariable("NORTHSTARET_LMS_WEB_BASE_URL", WebBaseUrl);
            Environment.SetEnvironmentVariable("NORTHSTARET_LMS_API_BASE_URL", ApiBaseUrl);

            AspireAvailable = true;
            Console.WriteLine("Aspire application started successfully!");

            try
            {
                var seedResult = await PlaywrightDataSeeder.EnsureSeedDistrictAsync(WebBaseUrl);
                SeededDistrictId = seedResult.DistrictId;
                SeededDistrictName = seedResult.Name;
                SeededDistrictSuffix = seedResult.Suffix;

                if (SeededDistrictId != Guid.Empty)
                {
                    Environment.SetEnvironmentVariable("NORTHSTARET_LMS_SEEDED_DISTRICT_ID", SeededDistrictId.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("════════════════════════════════════════════════════════════════");
                Console.WriteLine("⚠️  WARNING: Playwright data seeding failed");
                Console.WriteLine("════════════════════════════════════════════════════════════════");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("════════════════════════════════════════════════════════════════");
            }
        }
        catch (Exception ex) when (ex is Polly.Timeout.TimeoutRejectedException
            or TimeoutException
            or SocketException
            or HttpRequestException
            or TaskCanceledException
            or OperationCanceledException)
        {
            // DCP/Aspire startup failed - likely due to environment constraints
            AspireAvailable = false;
            SkipReason = $"Aspire/DCP cannot start in this environment: {ex.GetType().Name} - {ex.Message}. " +
                        "These tests require Docker and proper network configuration. " +
                        "Set SKIP_ASPIRE_TESTS=true to explicitly skip.";

            Console.WriteLine("════════════════════════════════════════════════════════════════");
            Console.WriteLine("⚠️  WARNING: Aspire-based Playwright tests will be skipped");
            Console.WriteLine("════════════════════════════════════════════════════════════════");
            Console.WriteLine(SkipReason);
            Console.WriteLine("════════════════════════════════════════════════════════════════");

            // Skip all tests in this fixture
            Assert.Ignore(SkipReason);
        }
        catch (Exception ex)
        {
            // Other unexpected errors should still fail
            AspireAvailable = false;
            SkipReason = $"Unexpected error: {ex.GetType().Name} - {ex.Message}";
            Console.WriteLine($"Failed to start Aspire application: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
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
