using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;
using NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;
using NorthStarET.NextGen.Lms.Contracts.Districts;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

[ApiController]
[Route("api/districts")]
public sealed class DistrictsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DistrictsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [EnableRateLimiting("district-mutations")]
    // Removed [ValidateAntiForgeryToken]: API endpoints are called server-to-server from Web.
    // CSRF is enforced on Web MVC forms; API relies on authentication/authorization instead.
    public async Task<IActionResult> CreateAsync([FromBody] CreateDistrictRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateDistrictCommand(request.Name, request.Suffix);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            // Use a named route to avoid action-name Async suffix pitfalls and ensure stable link generation
            return CreatedAtRoute(
                routeName: "GetDistrictById",
                routeValues: new { districtId = result.Value!.Id },
                value: result.Value);
        }

        return MapFailure(result.Error!);
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageNumber <= 0)
        {
            pageNumber = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }

        var query = new ListDistrictsQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : MapFailure(result.Error!);
    }

    [HttpGet("{districtId:guid}", Name = "GetDistrictById")]
    public async Task<IActionResult> GetByIdAsync(Guid districtId, CancellationToken cancellationToken)
    {
        var query = new GetDistrictQuery(districtId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Error!);
    }

    [HttpPut("{districtId:guid}")]
    [EnableRateLimiting("district-mutations")]
    // Removed [ValidateAntiForgeryToken] for same reason as above.
    public async Task<IActionResult> UpdateAsync(Guid districtId, [FromBody] UpdateDistrictRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDistrictCommand(districtId, request.Name, request.Suffix);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapFailure(result.Error!);
    }

    [HttpDelete("{districtId:guid}")]
    // Removed [ValidateAntiForgeryToken] for same reason as above.
    public async Task<IActionResult> DeleteAsync(Guid districtId, CancellationToken cancellationToken)
    {
        var command = new DeleteDistrictCommand(districtId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapFailure(result.Error!);
    }

    private IActionResult MapFailure(Error error)
    {
        return error.Code switch
        {
            "District.NotFound" => NotFound(error.Message),
            _ => BadRequest(error.Message)
        };
    }
}
