using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;

/// <summary>
/// Handler for updating an existing district with validation and audit.
/// </summary>
public sealed class UpdateDistrictCommandHandler : IRequestHandler<UpdateDistrictCommand, Result>
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateDistrictCommandHandler(IDistrictRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(UpdateDistrictCommand request, CancellationToken cancellationToken)
    {
        // Load district
        var district = await _repository.GetByIdAsync(request.DistrictId, cancellationToken);
        if (district == null)
        {
            return Result.Failure(new Error("District.NotFound", $"District with ID '{request.DistrictId}' not found"));
        }

        // Check suffix uniqueness (excluding self)
        var isUnique = await _repository.IsSuffixUniqueAsync(
            request.NormalizedSuffix,
            request.DistrictId,
            cancellationToken
        );
        if (!isUnique)
        {
            return Result.Failure(new Error("District.SuffixNotUnique", $"Suffix '{request.Suffix}' is already in use"));
        }

        var beforeSnapshot = new
        {
            id = district.Id,
            name = district.Name,
            suffix = district.Suffix,
            updatedAtUtc = district.UpdatedAtUtc,
            deletedAtUtc = district.DeletedAt
        };

        // Update district (validation happens in Update method)
        district.Update(request.Name, request.Suffix, _dateTimeProvider);

        // Persist changes
        await _repository.UpdateAsync(district, cancellationToken);

        var afterSnapshot = new
        {
            id = district.Id,
            name = district.Name,
            suffix = district.Suffix,
            updatedAtUtc = district.UpdatedAtUtc,
            deletedAtUtc = district.DeletedAt
        };

        request.CaptureAuditState(beforeSnapshot, afterSnapshot);

        return Result.Success();
    }
}
