using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;

/// <summary>
/// Handler for soft-deleting a district with cascade revoke.
/// </summary>
public sealed class DeleteDistrictCommandHandler : IRequestHandler<DeleteDistrictCommand, Result>
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteDistrictCommandHandler(IDistrictRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(DeleteDistrictCommand request, CancellationToken cancellationToken)
    {
        // Load district
        var district = await _repository.GetByIdAsync(request.DistrictId, cancellationToken);
        if (district == null)
        {
            return Result.Failure(new Error("District.NotFound", $"District with ID '{request.DistrictId}' not found"));
        }

        if (district.IsDeleted)
        {
            var deletedSnapshot = new
            {
                id = district.Id,
                name = district.Name,
                suffix = district.Suffix,
                deletedAtUtc = district.DeletedAt
            };

            request.CaptureAuditState(deletedSnapshot, deletedSnapshot);
            return Result.Success();
        }

        var beforeSnapshot = new
        {
            id = district.Id,
            name = district.Name,
            suffix = district.Suffix,
            deletedAtUtc = district.DeletedAt
        };

        // Soft delete (idempotent - safe to call multiple times)
        district.Delete(_dateTimeProvider);

        // Persist changes
        await _repository.UpdateAsync(district, cancellationToken);

        var afterSnapshot = new
        {
            id = district.Id,
            name = district.Name,
            suffix = district.Suffix,
            deletedAtUtc = district.DeletedAt
        };

        request.CaptureAuditState(beforeSnapshot, afterSnapshot);

        return Result.Success();
    }
}
