using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Queries.ListDistrictAdmins;

public sealed class ListDistrictAdminsQueryHandler
    : IRequestHandler<ListDistrictAdminsQuery, Result<IReadOnlyList<DistrictAdminResponse>>>
{
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ListDistrictAdminsQueryHandler(IDistrictAdminRepository adminRepository, IDateTimeProvider dateTimeProvider)
    {
        _adminRepository = adminRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<DistrictAdminResponse>>> Handle(
        ListDistrictAdminsQuery request,
        CancellationToken cancellationToken)
    {
        // Get admins filtered by district (tenant isolation)
        var admins = await _adminRepository.GetByDistrictIdAsync(request.DistrictId, cancellationToken);

        // Apply status filter if specified
        if (!string.IsNullOrEmpty(request.StatusFilter))
        {
            admins = admins
                .Where(a => a.Status.ToString().Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var response = admins
            .Select(a => new DistrictAdminResponse
            {
                Id = a.Id,
                DistrictId = a.DistrictId,
                Email = a.Email,
                Status = a.Status.ToString(),
                InvitedAtUtc = a.InvitedAtUtc,
                VerifiedAtUtc = a.VerifiedAtUtc,
                RevokedAtUtc = a.RevokedAtUtc,
                InvitationExpiresAtUtc = a.InvitationExpiresAtUtc,
                IsInvitationExpired = a.IsInvitationExpired(_dateTimeProvider)
            })
            .ToList();

        return Result.Success<IReadOnlyList<DistrictAdminResponse>>(response);
    }
}
