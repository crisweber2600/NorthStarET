using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Web.Models.Districts;
using NorthStarET.NextGen.Lms.Web.Services;
using NorthStarET.NextGen.Lms.Web.Testing;

namespace NorthStarET.NextGen.Lms.Web.Controllers;

[Authorize]
public sealed class DistrictsController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly IPlaywrightStubStore _stubStore;
    private readonly ILmsSessionAccessor _sessionAccessor;

    public DistrictsController(IApiClient apiClient, IPlaywrightStubStore stubStore, ILmsSessionAccessor sessionAccessor)
    {
        _apiClient = apiClient;
        _stubStore = stubStore;
        _sessionAccessor = sessionAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var sessionId = await _sessionAccessor.GetSessionIdAsync();
        var useStubs = TestEnvironment.UsePlaywrightStubs() || string.IsNullOrEmpty(sessionId);

        if (useStubs)
        {
            var districts = _stubStore.GetDistricts();

            var stubModel = new DistrictListViewModel
            {
                Districts = districts,
                PageNumber = 1,
                PageSize = districts.Count,
                TotalCount = districts.Count
            };

            return View(stubModel);
        }

        var response = await _apiClient.GetAsync<PagedResponse<DistrictResponse>>(
            $"/api/districts?pageNumber={page}&pageSize={pageSize}",
            cancellationToken);

        if (response == null)
        {
            return View(new DistrictListViewModel { PageNumber = page, PageSize = pageSize });
        }

        var model = new DistrictListViewModel
        {
            Districts = response.Items,
            PageNumber = response.PageNumber,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateDistrictViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDistrictViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var sessionId = await _sessionAccessor.GetSessionIdAsync();
        if (TestEnvironment.UsePlaywrightStubs() || string.IsNullOrEmpty(sessionId))
        {
            try
            {
                _stubStore.CreateDistrict(model.Name, model.Suffix);
                TempData["SuccessMessage"] = $"District '{model.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        var request = new CreateDistrictRequest
        {
            Name = model.Name,
            Suffix = model.Suffix
        };

        var response = await _apiClient.PostAsync<CreateDistrictRequest, CreateDistrictResponse>(
            "/api/districts",
            request,
            cancellationToken);

        if (response != null)
        {
            TempData["SuccessMessage"] = $"District '{model.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Failed to create district. Please try again.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        if (TestEnvironment.UsePlaywrightStubs() || string.IsNullOrEmpty(await _sessionAccessor.GetSessionIdAsync()))
        {
            var stubDistrict = _stubStore.GetDistrictDetail(id);
            if (stubDistrict == null)
            {
                return NotFound();
            }

            var stubModel = new EditDistrictViewModel
            {
                Id = stubDistrict.Id,
                Name = stubDistrict.Name,
                Suffix = stubDistrict.Suffix
            };

            return View(stubModel);
        }

        var district = await _apiClient.GetAsync<DistrictDetailResponse>(
            $"/api/districts/{id}",
            cancellationToken);

        if (district == null)
        {
            return NotFound();
        }

        var model = new EditDistrictViewModel
        {
            Id = district.Id,
            Name = district.Name,
            Suffix = district.Suffix
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditDistrictViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (TestEnvironment.UsePlaywrightStubs() || string.IsNullOrEmpty(await _sessionAccessor.GetSessionIdAsync()))
        {
            var updated = _stubStore.UpdateDistrict(id, model.Name);
            if (updated)
            {
                TempData["SuccessMessage"] = $"District '{model.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Failed to update district. Please try again.");
            return View(model);
        }

        var request = new UpdateDistrictRequest
        {
            Name = model.Name,
            Suffix = model.Suffix
        };

        var success = await _apiClient.PutAsync(
            $"/api/districts/{id}",
            request,
            cancellationToken);

        if (success)
        {
            TempData["SuccessMessage"] = $"District '{model.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, "Failed to update district. Please try again.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (TestEnvironment.UsePlaywrightStubs() || string.IsNullOrEmpty(await _sessionAccessor.GetSessionIdAsync()))
        {
            var stubDistrict = _stubStore.GetDistrictDetail(id);
            if (stubDistrict == null)
            {
                return NotFound();
            }

            var stubModel = new DeleteDistrictViewModel
            {
                Id = stubDistrict.Id,
                Name = stubDistrict.Name,
                Suffix = stubDistrict.Suffix,
                AdminCount = stubDistrict.ActiveAdminCount + stubDistrict.PendingAdminCount
            };

            return View(stubModel);
        }

        var district = await _apiClient.GetAsync<DistrictDetailResponse>(
            $"/api/districts/{id}",
            cancellationToken);

        if (district == null)
        {
            return NotFound();
        }

        var model = new DeleteDistrictViewModel
        {
            Id = district.Id,
            Name = district.Name,
            Suffix = district.Suffix,
            AdminCount = district.ActiveAdminCount + district.PendingAdminCount
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        if (TestEnvironment.UsePlaywrightStubs() || string.IsNullOrEmpty(await _sessionAccessor.GetSessionIdAsync()))
        {
            var stubSuccess = _stubStore.DeleteDistrict(id);
            if (stubSuccess)
            {
                TempData["SuccessMessage"] = "District deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete district. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        var success = await _apiClient.DeleteAsync(
            $"/api/districts/{id}",
            cancellationToken);

        if (success)
        {
            TempData["SuccessMessage"] = "District deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Failed to delete district. Please try again.";
        return RedirectToAction(nameof(Delete), new { id });
    }
}

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
