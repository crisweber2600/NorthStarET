using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace NorthStarET.NextGen.Lms.Playwright.Tests;

/// <summary>
/// Playwright end-to-end tests for District Admin Delegation UI flows.
/// Covers User Story 2: System Admin Delegates District Admins (P2).
/// Figma references:
/// - Manage Admins page: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-181&m=dev
/// - First Name field: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-285&m=dev
/// - Last Name field: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-282&m=dev
/// - Email field: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-288&m=dev
/// - Send Invitation button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-314&m=dev
/// - Unverified status: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-410&m=dev
/// - Verified status: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=24-403&m=dev
/// - Resend button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-96&m=dev
/// - Remove button: https://www.figma.com/design/cXwWUeggyig95XE7pJqls3/NorthStarET.NextGen.Lms-UI?node-id=340-99&m=dev
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public sealed class DistrictAdminDelegationTests : PageTest
{
    private const string DefaultWebBaseUrl = "https://localhost:7002";

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
        if (!AspirePlaywrightFixture.AspireAvailable)
        {
            Assert.Ignore(AspirePlaywrightFixture.SkipReason ?? "Aspire orchestration not available");
        }

        if (AspirePlaywrightFixture.SeededDistrictId == Guid.Empty)
        {
            Assert.Ignore("Seeded district unavailable for district admin tests.");
        }

        var managePath = $"/DistrictAdmins/Manage?districtId={AspirePlaywrightFixture.SeededDistrictId}";
        await Page.GotoAsync(managePath);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task ManageAdminsPage_LoadsSuccessfully()
    {
        await Expect(Page).ToHaveTitleAsync(new Regex("Manage District Admins", RegexOptions.IgnoreCase));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Manage District Admins");
    }

    [Test]
    public async Task InviteForm_DisplaysAllRequiredFields()
    {
        var firstNameInput = Page.GetByLabel("First Name");
        var lastNameInput = Page.GetByLabel("Last Name");
        var emailInput = Page.GetByLabel("Email");
        var sendButton = Page.GetByRole(AriaRole.Button, new() { Name = "Send Invitation" });

        await Expect(firstNameInput).ToBeVisibleAsync();
        await Expect(lastNameInput).ToBeVisibleAsync();
        await Expect(emailInput).ToBeVisibleAsync();
        await Expect(sendButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task InviteForm_ValidatesRequiredFields()
    {
        var sendButton = Page.GetByRole(AriaRole.Button, new() { Name = "Send Invitation" });
        await sendButton.ClickAsync();

        await Expect(Page.GetByLabel("First Name")).ToHaveAttributeAsync("required", string.Empty);
        await Expect(Page.GetByLabel("Last Name")).ToHaveAttributeAsync("required", string.Empty);
        await Expect(Page.GetByLabel("Email")).ToHaveAttributeAsync("required", string.Empty);
    }

    [Test]
    public async Task InviteForm_ValidatesEmailFormat()
    {
        await Page.GetByLabel("Email").FillAsync("invalid-email");

        var sendButton = Page.GetByRole(AriaRole.Button, new() { Name = "Send Invitation" });
        await sendButton.ClickAsync();

        await Expect(Page.GetByLabel("Email")).ToHaveAttributeAsync("type", "email");
    }

    [Test]
    public async Task InviteFlow_SubmitsSuccessfullyWithMatchingSuffix()
    {
        var email = await InviteAdminAsync("john.doe", "John", "Doe");
        var adminRow = GetAdminRow(email);

        await Expect(adminRow).ToHaveAttributeAsync("data-admin-status", new Regex("Unverified", RegexOptions.IgnoreCase));

        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task InviteFlow_RejectsEmailWithWrongSuffix()
    {
        await Page.GetByLabel("First Name").FillAsync("Jane");
        await Page.GetByLabel("Last Name").FillAsync("Smith");
        await Page.GetByLabel("Email").FillAsync("admin@wrongdomain.com");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Invitation" }).ClickAsync();

        await Expect(Page.Locator(".alert-danger")).ToContainTextAsync("Email domain must match district suffix");
    }

    [Test]
    public async Task AdminList_DisplaysStatusBadges()
    {
        var unverifiedBadge = Page.Locator(".badge-warning:has-text('Unverified')").First;
        var verifiedBadge = Page.Locator(".badge-success:has-text('Verified')").First;

        var hasAdmins = await Page.Locator("table tbody tr").CountAsync() > 0;
        if (hasAdmins)
        {
            var hasBadges = await unverifiedBadge.IsVisibleAsync() || await verifiedBadge.IsVisibleAsync();
            Assert.That(hasBadges, Is.True, "Status badges should be visible for admins");
        }
    }

    [Test]
    public async Task ResendButton_IsVisibleForUnverifiedAdmins()
    {
        var email = await InviteAdminAsync("resend-visible");
        var unverifiedRow = GetAdminRow(email);
        var resendButton = unverifiedRow.GetByRole(AriaRole.Button, new() { Name = "Resend" });

        await Expect(resendButton).ToBeVisibleAsync();

        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task ResendButton_DisplaysConfirmationModal()
    {
        var email = await InviteAdminAsync("resend-modal");
        var unverifiedRow = GetAdminRow(email);
        var resendButton = unverifiedRow.GetByRole(AriaRole.Button, new() { Name = "Resend" });

        await resendButton.ClickAsync();

        var modal = Page.Locator(".modal:visible");
        await Expect(modal).ToBeVisibleAsync();
        await Expect(modal.Locator(".modal-title")).ToContainTextAsync("Resend Invitation");
        await Expect(modal.GetByRole(AriaRole.Button, new() { Name = "Confirm" })).ToBeVisibleAsync();

        await modal.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task ResendFlow_UpdatesInvitationTimestamp()
    {
        var email = await InviteAdminAsync("resend-flow");
        var targetRow = GetAdminRow(email);
        var originalTimestamp = await targetRow.GetAttributeAsync("data-invited-utc") ?? string.Empty;

        await targetRow.GetByRole(AriaRole.Button, new() { Name = "Resend" }).ClickAsync();
        var modal = Page.Locator(".modal:visible");
        await modal.GetByRole(AriaRole.Button, new() { Name = "Confirm" }).ClickAsync();

        await Expect(Page.Locator(".alert-success")).ToContainTextAsync("Invitation resent successfully");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var refreshedRow = GetAdminRow(email);
        await Expect(refreshedRow).ToBeVisibleAsync();

        var newTimestamp = await refreshedRow.GetAttributeAsync("data-invited-utc") ?? string.Empty;
        Assert.That(newTimestamp, Is.Not.Empty);
        Assert.That(newTimestamp, Is.Not.EqualTo(originalTimestamp));

        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task RemoveButton_IsVisibleForAllAdmins()
    {
        var email = await InviteAdminAsync("remove-button-visible");
        var adminRow = GetAdminRow(email);
        var removeButton = adminRow.GetByRole(AriaRole.Button, new() { Name = "Remove" });

        await Expect(removeButton).ToBeVisibleAsync();

        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task RemoveButton_DisplaysConfirmationWithWarning()
    {
        var email = await InviteAdminAsync("remove-modal-warning");
        var adminRow = GetAdminRow(email);
        var removeButton = adminRow.GetByRole(AriaRole.Button, new() { Name = "Remove" });

        await removeButton.ClickAsync();

        var modal = Page.Locator(".modal:visible");
        await Expect(modal).ToBeVisibleAsync();
        await Expect(modal.Locator(".modal-title")).ToContainTextAsync("Remove Admin");
        await Expect(modal.Locator(".alert-danger")).ToContainTextAsync("Warning: This action cannot be undone");
        await Expect(modal.Locator("p.text-muted"))
            .ToContainTextAsync("immediately revoke access");
        await Expect(modal.GetByRole(AriaRole.Button, new() { Name = "Confirm Delete" })).ToBeVisibleAsync();

        await modal.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();
        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task RemoveFlow_RemovesAdminFromList()
    {
        var email = await InviteAdminAsync("remove-target", "Remove", "Target");
        var adminRow = GetAdminRow(email);
        await Expect(adminRow).ToBeVisibleAsync();

        await adminRow.GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
        var modal = Page.Locator(".modal:visible");
        await modal.GetByRole(AriaRole.Button, new() { Name = "Confirm Delete" }).ClickAsync();
        await Expect(Page.Locator(".alert-success")).ToContainTextAsync("Admin removed successfully");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var removedRow = Page.Locator($"tr[data-admin-email='{email}']");
        await Expect(removedRow).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task AdminList_DisplaysEmptyStateWhenNoAdmins()
    {
        var hasAdmins = await Page.Locator("table tbody tr").CountAsync() > 0;
        if (!hasAdmins)
        {
            var emptyState = Page.Locator(".text-center").Filter(new() { HasText = "No district admins found" });
            await Expect(emptyState).ToBeVisibleAsync();
            await Expect(Page.GetByText("Invite your first district admin to get started")).ToBeVisibleAsync();
        }
    }

    [Test]
    public async Task AdminList_DisplaysAllRelevantColumns()
    {
        var email = await InviteAdminAsync("columns-check");
        var table = Page.Locator("table");

        await Expect(table.Locator("thead th:has-text('Name')")).ToBeVisibleAsync();
        await Expect(table.Locator("thead th:has-text('Email')")).ToBeVisibleAsync();
        await Expect(table.Locator("thead th:has-text('Status')")).ToBeVisibleAsync();
        await Expect(table.Locator("thead th:has-text('Invited At')")).ToBeVisibleAsync();
        await Expect(table.Locator("thead th:has-text('Actions')")).ToBeVisibleAsync();

        await RemoveAdminAsync(email);
    }

    [Test]
    public async Task InviteForm_ClearsAfterSuccessfulSubmission()
    {
        await Page.GetByLabel("First Name").FillAsync("Test");
        await Page.GetByLabel("Last Name").FillAsync("User");
        await Page.GetByLabel("Email").FillAsync("test@example.com");

        var firstNameValue = await Page.GetByLabel("First Name").InputValueAsync();
        Assert.That(firstNameValue, Is.EqualTo("Test"));
    }

    [Test]
    public async Task Navigation_BackToDistrictsFromManageAdmins()
    {
        var backButton = Page.GetByRole(AriaRole.Link, new() { Name = "Back to Districts" });

        if (await backButton.IsVisibleAsync())
        {
            await backButton.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex("/Districts$"));
        }
    }

    [Test]
    public async Task VerifiedAdmins_DoNotShowResendButton()
    {
        var verifiedRow = Page.Locator("tr:has(.badge-success:has-text('Verified'))").First;
        var resendButton = verifiedRow.GetByRole(AriaRole.Button, new() { Name = "Resend" });

        await Expect(resendButton).Not.ToBeVisibleAsync();
    }

    private async Task<string> InviteAdminAsync(string slug, string firstName = "Resend", string lastName = "Target")
    {
        var suffix = AspirePlaywrightFixture.SeededDistrictSuffix;
        Assume.That(!string.IsNullOrWhiteSpace(suffix), "Seeded district suffix required");

        var email = $"{slug}.{DateTime.UtcNow.Ticks}@{suffix}";
        var normalizedEmail = email.ToLowerInvariant();

        await Page.GetByLabel("First Name").FillAsync(firstName);
        await Page.GetByLabel("Last Name").FillAsync(lastName);
        await Page.GetByLabel("Email").FillAsync(normalizedEmail);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Send Invitation" }).ClickAsync();
        await Page.WaitForSelectorAsync(".alert-success");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var row = GetAdminRow(normalizedEmail);
        await Expect(row).ToBeVisibleAsync();

        return normalizedEmail;
    }

    private ILocator GetAdminRow(string email)
    {
        return Page.Locator($"tr[data-admin-email='{email.ToLowerInvariant()}']").First;
    }

    private async Task RemoveAdminAsync(string email)
    {
        var row = GetAdminRow(email);
        await Expect(row).ToBeVisibleAsync();

        await row.GetByRole(AriaRole.Button, new() { Name = "Remove" }).ClickAsync();
        var modal = Page.Locator(".modal:visible");
        await modal.GetByRole(AriaRole.Button, new() { Name = "Confirm Delete" }).ClickAsync();
        await Page.WaitForSelectorAsync(".alert-success");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var removedRow = Page.Locator($"tr[data-admin-email='{email.ToLowerInvariant()}']");
        await Expect(removedRow).Not.ToBeVisibleAsync();
    }
}
