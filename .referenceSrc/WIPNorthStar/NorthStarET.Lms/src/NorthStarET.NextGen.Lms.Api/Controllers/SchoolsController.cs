using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.CreateSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.DeleteSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.UpdateSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.GetSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.ListSchools;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using System.Security.Claims;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

/// <summary>
/// API endpoints for school catalog management.
/// </summary>
[ApiController]
[Route("api/districts/{districtId:guid}/schools")]
[Authorize]
public sealed class SchoolsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchoolsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// List schools for a district with optional search and sort.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListAsync(
        Guid districtId,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = "name-asc",
        CancellationToken cancellationToken = default)
    {
        var query = new ListSchoolsQuery(districtId, search, sort);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure(result.Error!);
    }

    /// <summary>
    /// Get detailed school information including grade offerings.
    /// </summary>
    [HttpGet("{schoolId:guid}", Name = "GetSchoolById")]
    public async Task<IActionResult> GetByIdAsync(
        Guid districtId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        var query = new GetSchoolQuery(districtId, schoolId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure(result.Error!);
    }

    /// <summary>
    /// Create a new school within the district.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid districtId,
        [FromBody] CreateSchoolRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSchoolCommand(
            districtId,
            request.Name,
            request.Code,
            request.Notes,
            request.GradeSelections);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtRoute(
                routeName: "GetSchoolById",
                routeValues: new { districtId, schoolId = result.Value!.SchoolId },
                value: result.Value);
        }

        return MapFailure(result.Error!);
    }

    /// <summary>
    /// Update school metadata (excluding grade assignments).
    /// </summary>
    [HttpPut("{schoolId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid districtId,
        Guid schoolId,
        [FromBody] UpdateSchoolRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSchoolCommand(
            districtId,
            schoolId,
            request.Name,
            request.Code,
            request.Notes,
            request.ConcurrencyStamp);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure(result.Error!);
    }

    /// <summary>
    /// Soft delete a school.
    /// </summary>
    [HttpDelete("{schoolId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid districtId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new DeleteSchoolCommand(districtId, schoolId, userId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : MapFailure(result.Error!);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private IActionResult MapFailure(Error error)
    {
        return error.Code switch
        {
            "School.NotFound" => NotFound(new { error.Code, error.Message }),
            "School.Deleted" => Gone(new { error.Code, error.Message }),
            "School.DuplicateName" => Conflict(new { error.Code, error.Message }),
            "School.DuplicateCode" => Conflict(new { error.Code, error.Message }),
            "School.ConcurrencyConflict" => Conflict(new { error.Code, error.Message }),
            "School.AlreadyDeleted" => Conflict(new { error.Code, error.Message }),
            _ => BadRequest(new { error.Code, error.Message })
        };
    }

    private ObjectResult Gone(object value) => new ObjectResult(value) { StatusCode = 410 };
}
