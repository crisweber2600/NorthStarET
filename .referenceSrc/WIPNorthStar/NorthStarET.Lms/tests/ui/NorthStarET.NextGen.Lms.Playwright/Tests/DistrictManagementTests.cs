using System;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace NorthStarET.NextGen.Lms.Playwright.Tests;

/// <summary>
/// Playwright end-to-end tests for District Management UI flows.
/// Covers User Story 1: System Admin Manages Districts (P1 - MVP).
/// Figma references:
/// - District Management page: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-2&amp;m=dev
/// - Create District button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-43&amp;m=dev
/// - Create New District modal: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-67&amp;m=dev
/// - Edit District modal: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=22-278&amp;m=dev
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public sealed class DistrictManagementTests : PageTest
{
    private const string DefaultWebBaseUrl = "https://localhost:7002";
    private const string DistrictsPath = "/Districts";

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions() ?? new BrowserNewContextOptions();
        options.BaseURL = Environment.GetEnvironmentVariable("NORTHSTARET_LMS_WEB_BASE_URL") ?? DefaultWebBaseUrl;
        options.IgnoreHTTPSErrors = true;
        return options;
    }

    [SetUp]
    public async Task SetupAsync()
    {
        // Ensure Aspire orchestration is available before running tests
        if (!AspirePlaywrightFixture.AspireAvailable)
        {
            Assert.Ignore(AspirePlaywrightFixture.SkipReason ?? "Aspire orchestration not available");
        }

        // Navigate to districts page before each test
        await Page.GotoAsync(DistrictsPath);
    }

    [Test]
    public async Task DistrictManagementPage_LoadsSuccessfully()
    {
        // Arrange & Act - Page already navigated in SetUp

        // Assert
        await Expect(Page).ToHaveTitleAsync(new Regex("District Management", RegexOptions.IgnoreCase));
        await Expect(Page.Locator("h1")).ToContainTextAsync("District Management");
    }

    [Test]
    public async Task CreateDistrictButton_IsVisible()
    {
        // Act
        var createButton = Page.GetByTestId("create-district-button");

        // Assert
        await Expect(createButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateDistrictFlow_DisplaysFormWithRequiredFields()
    {
        // Arrange
        var createButton = Page.GetByTestId("create-district-button");

        // Act
        await createButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex("/Districts/Create"));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Create New District");

        // Verify form fields exist
        await Expect(Page.GetByLabel("District Name")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("District Suffix")).ToBeVisibleAsync();

        var submitButton = Page.GetByRole(AriaRole.Button, new() { Name = "Create District" });
        await Expect(submitButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateDistrictFlow_ValidatesRequiredFields()
    {
        // Arrange
        await Page.GotoAsync("/Districts/Create");

        // Act - Submit empty form
        var submitButton = Page.GetByRole(AriaRole.Button, new() { Name = "Create District" });
        await submitButton.ClickAsync();

        // Assert - HTML5 validation should prevent submission
        var nameInput = Page.GetByLabel("District Name");
        await Expect(nameInput).ToHaveAttributeAsync("required", "");

        var suffixInput = Page.GetByLabel("District Suffix");
        await Expect(suffixInput).ToHaveAttributeAsync("required", "");
    }

    [Test]
    public async Task CreateDistrictFlow_SubmitsSuccessfully()
    {
        // Arrange
        await Page.GotoAsync("/Districts/Create");
        var timestamp = DateTime.UtcNow.Ticks;
        var testDistrictName = $"Playwright District {timestamp}";
        var testSuffix = $"playwright-{timestamp}.edu";

        // Act
        await Page.GetByLabel("District Name").FillAsync(testDistrictName);
        await Page.GetByLabel("District Suffix").FillAsync(testSuffix);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create District" }).ClickAsync();
        await Page.WaitForURLAsync(new Regex("/Districts$"));

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex("/Districts$"));

        var successAlert = Page.Locator(".alert-success");
        await Expect(successAlert).ToContainTextAsync("created successfully", new LocatorAssertionsToContainTextOptions { IgnoreCase = true });

        // .First is used here after filtering by unique suffix to get the single matching row
        var newRow = Page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = testSuffix }).First;
        await Expect(newRow).ToBeVisibleAsync();
        // .First is used here to get the first cell (district name column) of the row
        await Expect(newRow.Locator("td").First).ToContainTextAsync(testDistrictName);
    }

    [Test]
    public async Task EditDistrictFlow_LoadsExistingDistrictData()
    {
        // Arrange
        var seededSuffix = AspirePlaywrightFixture.SeededDistrictSuffix;
        Assume.That(!string.IsNullOrWhiteSpace(seededSuffix), "Seeded district suffix required");

        // .First is used here after filtering by unique suffix to get the single matching row
        var seededRow = Page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = seededSuffix }).First;
        await Expect(seededRow).ToBeVisibleAsync();

        var editButton = seededRow.GetByRole(AriaRole.Link, new() { Name = "Edit" });

        // Act
        await editButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex("/Districts/Edit/"));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Edit District");

        var expectedName = AspirePlaywrightFixture.SeededDistrictName;
        if (!string.IsNullOrWhiteSpace(expectedName))
        {
            await Expect(Page.GetByLabel("District Name")).ToHaveValueAsync(expectedName);
        }

        await Expect(Page.GetByLabel("District Suffix")).ToHaveValueAsync(seededSuffix);

        // Verify suffix is readonly
        var suffixInput = Page.GetByLabel("District Suffix");
        await Expect(suffixInput).ToHaveAttributeAsync("readonly", "");
    }

    [Test]
    public async Task DeleteDistrictFlow_DisplaysConfirmationWithWarning()
    {
        // Arrange
        var seededSuffix = AspirePlaywrightFixture.SeededDistrictSuffix;
        Assume.That(!string.IsNullOrWhiteSpace(seededSuffix), "Seeded district suffix required");

        // .First is used here after filtering by unique suffix to get the single matching row
        var seededRow = Page.Locator("table tbody tr").Filter(new LocatorFilterOptions { HasText = seededSuffix }).First;
        await Expect(seededRow).ToBeVisibleAsync();

        var deleteButton = seededRow.GetByRole(AriaRole.Link, new() { Name = "Delete" });

        // Act
        await deleteButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex("/Districts/Delete/"));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Delete District");

        // Verify warning is displayed
        await Expect(Page.Locator(".alert-danger")).ToContainTextAsync("cannot be undone");
        await Expect(Page.Locator(".alert-danger")).ToContainTextAsync("Archive all associated district admins");

        // Verify confirm button exists
        var confirmButton = Page.GetByRole(AriaRole.Button, new() { Name = "Confirm Delete" });
        await Expect(confirmButton).ToBeVisibleAsync();
    }

    [Test]
    [Ignore("Requires authentication setup and seeded test data")]
    public async Task DistrictList_DisplaysPaginationWhenNeeded()
    {
        // Act - Assuming more than 20 districts exist

        // Assert
        var pagination = Page.Locator(".pagination");
        await Expect(pagination).ToBeVisibleAsync();

        // Verify pagination controls
        await Expect(Page.GetByText("Previous")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Next")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DistrictList_DisplaysEmptyStateWhenNoDistricts()
    {
        // Act - Assuming no districts in clean state

        // Assert
        var emptyState = Page.Locator(".text-center").Filter(new() { HasText = "No districts found" });

        // Empty state should show either districts table OR empty message
        var hasDistricts = await Page.Locator("table").IsVisibleAsync();
        if (!hasDistricts)
        {
            await Expect(emptyState).ToBeVisibleAsync();
            await Expect(Page.GetByText("Get started by creating your first district")).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task NavigationFlow_BackToListFromCreate()
    {
        // Arrange
        await Page.GotoAsync("/Districts/Create");

        // Act
        var backButton = Page.GetByRole(AriaRole.Link, new() { Name = "Back to List" });
        await backButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex("/Districts$"));
    }

    [Test]
    public async Task CancelButton_NavigatesBackToList()
    {
        // Arrange
        await Page.GotoAsync("/Districts/Create");

        // Act
        var cancelButton = Page.GetByRole(AriaRole.Link, new() { Name = "Cancel" });
        await cancelButton.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex("/Districts$"));
    }
}
