using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthStarET.NextGen.Lms.Application.Audit.Queries;
using NorthStarET.NextGen.Lms.Contracts.Audit;

namespace NorthStarET.NextGen.Lms.Api.Audit;

/// <summary>
/// API controller for audit trail queries
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AuditController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<AuditController> _logger;

    public AuditController(ISender sender, ILogger<AuditController> logger)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get audit records with optional filtering and pagination
    /// </summary>
    /// <param name="districtId">Optional district ID filter</param>
    /// <param name="actorId">Optional actor ID filter</param>
    /// <param name="action">Optional action filter</param>
    /// <param name="entityType">Optional entity type filter</param>
    /// <param name="pageNumber">Page number (1-based, default 1)</param>
    /// <param name="pageSize">Page size (default 20, max 100)</param>
    /// <param name="count">Optional count limit (overrides pagination)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit records</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedAuditRecordsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditRecords(
        [FromQuery] Guid? districtId = null,
        [FromQuery] Guid? actorId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? count = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAuditRecordsQuery
            {
                DistrictId = districtId,
                ActorId = actorId,
                Action = action,
                EntityType = entityType,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Count = count
            };

            var response = await _sender.Send(query, cancellationToken);

            _logger.LogInformation(
                "Retrieved {RecordCount} audit records (page {PageNumber}/{TotalPages})",
                response.Records.Count,
                response.PageNumber,
                response.TotalPages);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit records");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving audit records");
        }
    }

    /// <summary>
    /// Get audit records filtered by district
    /// </summary>
    /// <param name="districtId">District ID</param>
    /// <param name="pageNumber">Page number (1-based, default 1)</param>
    /// <param name="pageSize">Page size (default 20, max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated audit records for the district</returns>
    [HttpGet("district/{districtId:guid}")]
    [ProducesResponseType(typeof(PagedAuditRecordsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditRecordsByDistrict(
        Guid districtId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAuditRecordsQuery
            {
                DistrictId = districtId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _sender.Send(query, cancellationToken);

            _logger.LogInformation(
                "Retrieved {RecordCount} audit records for district {DistrictId}",
                response.Records.Count,
                districtId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit records for district {DistrictId}", districtId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving audit records");
        }
    }

    /// <summary>
    /// Get the most recent N audit records
    /// </summary>
    /// <param name="count">Number of records to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent audit records</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(PagedAuditRecordsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentAuditRecords(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (count < 1 || count > 100)
            {
                return BadRequest("Count must be between 1 and 100");
            }

            var query = new GetAuditRecordsQuery
            {
                Count = count
            };

            var response = await _sender.Send(query, cancellationToken);

            _logger.LogInformation("Retrieved {RecordCount} recent audit records", response.Records.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent audit records");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving audit records");
        }
    }
}
