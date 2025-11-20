using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Web.Models.DistrictAdmins;
using NorthStarET.NextGen.Lms.Web.Services;
using NorthStarET.NextGen.Lms.Web.Testing;

namespace NorthStarET.NextGen.Lms.Web.Controllers;

[Authorize]
public sealed class DistrictAdminsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly IPlaywrightStubStore _stubStore;

    public DistrictAdminsController(IApiClient apiClient, IPlaywrightStubStore stubStore)
    {
        _apiClient = apiClient;
        _stubStore = stubStore;
    }

    [HttpGet]
    public async Task<IActionResult> Manage(Guid? districtId, CancellationToken cancellationToken = default)
    {
        var useStubs = TestEnvironment.UsePlaywrightStubs();

        var resolvedDistrictId = districtId ?? (useStubs ? TestEnvironment.DefaultDistrictId : null);

        if (resolvedDistrictId == null)
        {
            TempData["ErrorMessage"] = "District ID is required.";
            return RedirectToAction("Index", "Districts");
        }

        DistrictDetailResponse? district;

        if (useStubs)
        {
            district = _stubStore.GetDistrictDetail(resolvedDistrictId.Value);
        }
        else
        {
            district = await _apiClient.GetAsync<DistrictDetailResponse>(
                $"/api/districts/{resolvedDistrictId}",
                cancellationToken);
        }

        if (district == null)
        {
            return NotFound();
        }

        List<DistrictAdminResponse> admins;

        if (useStubs)
        {
            admins = _stubStore.GetAdmins(district.Id).ToList();
        }
        else
        {
            admins = await _apiClient.GetAsync<List<DistrictAdminResponse>>(
                $"/api/districts/{district.Id}/admins",
                cancellationToken) ?? new List<DistrictAdminResponse>();
        }

        var model = new ManageDistrictAdminsViewModel
        {
            DistrictId = district.Id,
            DistrictName = district.Name,
            Admins = admins
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(InviteDistrictAdminViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide all required fields.";
            return RedirectToAction(nameof(Manage), new { districtId = model.DistrictId });
        }

        var request = new InviteDistrictAdminRequest
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email
        };

        if (TestEnvironment.UsePlaywrightStubs())
        {
            try
            {
                _stubStore.InviteAdmin(model.DistrictId, model.FirstName, model.LastName, model.Email);
                TempData["SuccessMessage"] = $"Invitation sent successfully to {model.Email}.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Manage), new { districtId = model.DistrictId });
        }

        var response = await _apiClient.PostAsync<InviteDistrictAdminRequest, InviteDistrictAdminResponse>(
            $"/api/districts/{model.DistrictId}/admins",
            request,
            cancellationToken);

        if (response != null)
        {
            TempData["SuccessMessage"] = $"Invitation sent successfully to {model.Email}.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to send invitation. Please verify the email matches the district suffix and try again.";
        }

        return RedirectToAction(nameof(Manage), new { districtId = model.DistrictId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend(Guid districtId, Guid adminId, CancellationToken cancellationToken)
    {
        if (TestEnvironment.UsePlaywrightStubs())
        {
            var stubSuccess = _stubStore.ResendAdminInvitation(districtId, adminId);
            if (stubSuccess)
            {
                TempData["SuccessMessage"] = "Invitation resent successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to resend invitation. Please try again.";
            }

            return RedirectToAction(nameof(Manage), new { districtId });
        }

        var success = await _apiClient.PostAsync<object>(
            $"/api/districts/{districtId}/admins/{adminId}/resend",
            null,
            cancellationToken);

        if (success)
        {
            TempData["SuccessMessage"] = "Invitation resent successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to resend invitation. Please try again.";
        }

        return RedirectToAction(nameof(Manage), new { districtId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid districtId, Guid adminId, CancellationToken cancellationToken)
    {
        if (TestEnvironment.UsePlaywrightStubs())
        {
            var stubSuccess = _stubStore.RemoveAdmin(districtId, adminId);
            if (stubSuccess)
            {
                TempData["SuccessMessage"] = "Admin removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove admin. Please try again.";
            }

            return RedirectToAction(nameof(Manage), new { districtId });
        }

        var success = await _apiClient.DeleteAsync(
            $"/api/districts/{districtId}/admins/{adminId}",
            cancellationToken);

        if (success)
        {
            TempData["SuccessMessage"] = "Admin removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to remove admin. Please try again.";
        }

        return RedirectToAction(nameof(Manage), new { districtId });
    }
}
