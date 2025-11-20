using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace NorthStarET.NextGen.Lms.Playwright.Tests;

/// <summary>
/// Playwright coverage shell for the Entra single sign-on journey.
/// Pending the delivery of design artifacts captured in
/// <c>specs/001-unified-sso-auth/figma-prompts/sign-in-flow.md</c>.
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
[Ignore("Blocked: awaiting sign-in flow Figma frames to finalize selectors and assertions.")]
public sealed class SignInFlowTests : PageTest
{
    private const string DefaultWebBaseUrl = "https://localhost:7100";

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.BaseURL = Environment.GetEnvironmentVariable("NORTHSTARET_LMS_WEB_BASE_URL") ?? DefaultWebBaseUrl;
        options.IgnoreHTTPSErrors = true;
        return options;
    }

    [SetUp]
    public Task ResetAsync()
    {
        // Ensure Aspire orchestration is available before running tests
        if (!AspirePlaywrightFixture.AspireAvailable)
        {
            Assert.Ignore(AspirePlaywrightFixture.SkipReason ?? "Aspire orchestration not available");
        }

        // TODO: Spin up Aspire AppHost and seed Entra stub contexts once UI artifacts exist.
        return Task.CompletedTask;
    }

    [Test]
    public async Task SignInJourney_DisplaysUserContextAcrossPortals()
    {
        // TODO: Implement navigation, Entra redirect handling, and cross-portal assertions once UI elements are defined.
        await Task.CompletedTask;
    }
}
