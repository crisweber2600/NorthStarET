using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.Districts;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Queries.ListDistricts;

/// <summary>
/// Handler for retrieving paginated list of active districts.
/// </summary>
public sealed class ListDistrictsQueryHandler : IRequestHandler<ListDistrictsQuery, Result<PagedResult<DistrictSummaryResponse>>>
{
    private readonly IDistrictRepository _repository;

    public ListDistrictsQueryHandler(IDistrictRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<DistrictSummaryResponse>>> Handle(ListDistrictsQuery request, CancellationToken cancellationToken)
    {
        // Get paginated districts
        var districts = await _repository.ListAsync(request.PageNumber, request.PageSize, cancellationToken);

        // Get total count
        var totalCount = await _repository.CountAsync(cancellationToken);

        // Map to response
        var summaryTasks = districts.Select(async district =>
        {
            var adminCount = await _repository.GetActiveAdminCountAsync(district.Id, cancellationToken);

            return new DistrictSummaryResponse(
                district.Id,
                district.Name,
                district.Suffix,
                adminCount,
                district.CreatedAtUtc,
                district.UpdatedAtUtc
            );
        });

        var items = (await Task.WhenAll(summaryTasks)).ToList();

        var pagedResult = new PagedResult<DistrictSummaryResponse>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount
        );

        return Result.Success(pagedResult);
    }
}
