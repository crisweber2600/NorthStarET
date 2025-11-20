using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace NorthStarET.NextGen.Lms.Playwright.Utilities;

internal static class PlaywrightDataSeeder
{
    private const string SeedDistrictName = "Playwright Auth Seed District";
    private const string SeedDistrictSuffix = "playwright-auth-seed.edu";

    public static async Task<PlaywrightSeedResult> EnsureSeedDistrictAsync(string baseUrl)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);

        try
        {
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = DetermineHeadless()
            }).ConfigureAwait(false);

            try
            {
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    BaseURL = baseUrl,
                    IgnoreHTTPSErrors = true
                }).ConfigureAwait(false);

                try
                {
                    var page = await context.NewPageAsync().ConfigureAwait(false);
                    await page.GotoAsync("/Districts", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }).ConfigureAwait(false);

                    var seedRow = page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = SeedDistrictSuffix });

                    if (await seedRow.CountAsync().ConfigureAwait(false) == 0)
                    {
                        Console.WriteLine("Playwright seeding: creating default district via UI");
                        await EnsureDistrictCreatedAsync(page).ConfigureAwait(false);
                        seedRow = page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = SeedDistrictSuffix });
                    }
                    else
                    {
                        Console.WriteLine("Playwright seeding: district already exists, skipping creation");
                    }

                    await seedRow.First.WaitForAsync().ConfigureAwait(false);

                    var editHref = await seedRow.First
                        .GetByRole(AriaRole.Link, new() { Name = "Edit" })
                        .GetAttributeAsync("href")
                        .ConfigureAwait(false);

                    var districtId = ExtractDistrictId(editHref);
                    if (districtId == Guid.Empty)
                    {
                        throw new InvalidOperationException("Unable to extract seeded district identifier from edit link.");
                    }

                    Console.WriteLine($"Playwright seeding: ensured district '{SeedDistrictName}' ({districtId}) is available.");

                    return new PlaywrightSeedResult(districtId, SeedDistrictName, SeedDistrictSuffix);
                }
                finally
                {
                    await context.CloseAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await browser.CloseAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            playwright?.Dispose();
        }
    }

    private static async Task EnsureDistrictCreatedAsync(IPage page)
    {
        var createLink = page.GetByTestId("create-district-button");
        await createLink.ClickAsync().ConfigureAwait(false);
        await page.WaitForURLAsync("**/Districts/Create").ConfigureAwait(false);

        await page.GetByLabel("District Name").FillAsync(SeedDistrictName).ConfigureAwait(false);
        await page.GetByLabel("District Suffix").FillAsync(SeedDistrictSuffix).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create District" }).ClickAsync().ConfigureAwait(false);

        await page.WaitForURLAsync("**/Districts").ConfigureAwait(false);
        await page.WaitForSelectorAsync(".alert-success").ConfigureAwait(false);
        await page.WaitForSelectorAsync("table tbody tr").ConfigureAwait(false);
        await page.WaitForSelectorAsync($"text={SeedDistrictSuffix}").ConfigureAwait(false);
    }

    private static Guid ExtractDistrictId(string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
        {
            return Guid.Empty;
        }

        var segments = href.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return Guid.Empty;
        }

        var lastSegment = segments[^1];
        return Guid.TryParse(lastSegment, out var id) ? id : Guid.Empty;
    }

    private static bool DetermineHeadless()
    {
        var headlessValue = Environment.GetEnvironmentVariable("HEADLESS");
        if (string.IsNullOrWhiteSpace(headlessValue))
        {
            return false;
        }

        if (bool.TryParse(headlessValue, out var parsed))
        {
            return parsed;
        }

        return headlessValue.Trim() == "1";
    }
}

internal sealed record PlaywrightSeedResult(Guid DistrictId, string Name, string Suffix);
