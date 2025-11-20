using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.InviteDistrictAdmin;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.ResendInvite;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.RevokeDistrictAdmin;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Queries.ListDistrictAdmins;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common;

namespace NorthStarET.NextGen.Lms.Api.Controllers;

/// <summary>
/// API controller for managing district administrators.
/// </summary>
[ApiController]
[Route("api/districts/{districtId:guid}/admins")]
public sealed class DistrictAdminsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DistrictAdminsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Invites a new district admin.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="request">Invitation request with admin details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created admin details</returns>
    [HttpPost]
    [EnableRateLimiting("admin-invitations")]
    public async Task<IActionResult> InviteAsync(
        Guid districtId,
        [FromBody] InviteDistrictAdminRequest request,
        CancellationToken cancellationToken)
    {
        var command = new InviteDistrictAdminCommand(
            districtId,
            request.FirstName,
            request.LastName,
            request.Email);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(ListAsync),
                new { districtId },
                result.Value);
        }

        return MapFailure(result.Error!);
    }

    /// <summary>
    /// Lists all district admins for a specific district.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of district admins</returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync(Guid districtId, CancellationToken cancellationToken)
    {
        var query = new ListDistrictAdminsQuery(districtId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapFailure(result.Error!);
    }

    /// <summary>
    /// Resends invitation email to an unverified admin.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpPost("{adminId:guid}/resend")]
    [EnableRateLimiting("admin-invitations")]
    public async Task<IActionResult> ResendInviteAsync(
        Guid districtId,
        Guid adminId,
        CancellationToken cancellationToken)
    {
        var command = new ResendInviteCommand(districtId, adminId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapFailure(result.Error!);
    }

    /// <summary>
    /// Revokes admin access.
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="adminId">Admin ID</param>
    /// <param name="reason">Reason for revocation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{adminId:guid}")]
    public async Task<IActionResult> RevokeAsync(
        Guid districtId,
        Guid adminId,
        [FromQuery] string reason = "Admin revoked by system administrator",
        CancellationToken cancellationToken = default)
    {
        var command = new RevokeDistrictAdminCommand(districtId, adminId, reason);
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
            "District.NotFound" or "DistrictAdmin.NotFound" => NotFound(new { error.Code, error.Message }),
            "DistrictAdmin.AccessDenied" => Forbid(),
            _ => BadRequest(new { error.Code, error.Message })
        };
    }
}
