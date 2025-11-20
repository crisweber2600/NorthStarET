using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Queries.GetDistrict;

/// <summary>
/// Handler for retrieving a single district by ID with admin counts.
/// </summary>
public sealed class GetDistrictQueryHandler : IRequestHandler<GetDistrictQuery, Result<DistrictResponse>>
{
    private readonly IDistrictRepository _repository;

    public GetDistrictQueryHandler(IDistrictRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DistrictResponse>> Handle(GetDistrictQuery request, CancellationToken cancellationToken)
    {
        // Load district
        var district = await _repository.GetByIdAsync(request.DistrictId, cancellationToken);
        if (district == null)
        {
            return Result.Failure<DistrictResponse>(
                new Error("District.NotFound", $"District with ID '{request.DistrictId}' not found"));
        }

        // Get admin counts
        var activeCount = await _repository.GetActiveAdminCountAsync(request.DistrictId, cancellationToken);
        var pendingCount = await _repository.GetPendingAdminCountAsync(request.DistrictId, cancellationToken);
        var revokedCount = await _repository.GetRevokedAdminCountAsync(request.DistrictId, cancellationToken);

        // Map to response
        var response = new DistrictResponse
        {
            Id = district.Id,
            Name = district.Name,
            Suffix = district.Suffix,
            CreatedAtUtc = district.CreatedAtUtc,
            UpdatedAtUtc = district.UpdatedAtUtc,
            IsDeleted = district.IsDeleted,
            DeletedAt = district.DeletedAt,
            ActiveAdminCount = activeCount,
            PendingAdminCount = pendingCount,
            RevokedAdminCount = revokedCount
        };

        return Result.Success(response);
    }
}
