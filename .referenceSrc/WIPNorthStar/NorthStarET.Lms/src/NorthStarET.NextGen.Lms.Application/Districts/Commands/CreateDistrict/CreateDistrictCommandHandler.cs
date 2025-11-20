using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;

/// <summary>
/// Handler for creating a new district with validation and audit.
/// </summary>
public sealed class CreateDistrictCommandHandler : IRequestHandler<CreateDistrictCommand, Result<CreateDistrictResponse>>
{
    private readonly IDistrictRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDistrictCommandHandler(IDistrictRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<CreateDistrictResponse>> Handle(CreateDistrictCommand request, CancellationToken cancellationToken)
    {
        // Check suffix uniqueness (case-insensitive)
        var isUnique = await _repository.IsSuffixUniqueAsync(request.NormalizedSuffix, cancellationToken);
        if (!isUnique)
        {
            return Result.Failure<CreateDistrictResponse>(
                new Error("District.SuffixNotUnique", $"Suffix '{request.Suffix}' is already in use"));
        }

        // Create district aggregate (validation happens in factory method)
        var districtId = Guid.NewGuid();
        var district = District.Create(
            districtId,
            request.Name,
            request.Suffix,
            _dateTimeProvider
        );

        // Persist to repository
        await _repository.AddAsync(district, cancellationToken);

        request.CaptureAuditState(
            district.Id,
            new
            {
                id = district.Id,
                name = district.Name,
                suffix = district.Suffix,
                createdAtUtc = district.CreatedAtUtc
            });

        // Map to response
        var response = new CreateDistrictResponse(
            district.Id,
            district.Name,
            district.Suffix,
            district.CreatedAtUtc
        );

        return Result.Success(response);
    }
}
