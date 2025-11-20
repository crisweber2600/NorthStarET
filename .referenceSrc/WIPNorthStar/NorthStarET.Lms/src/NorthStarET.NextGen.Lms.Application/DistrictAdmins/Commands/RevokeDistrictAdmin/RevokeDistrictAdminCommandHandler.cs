using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.RevokeDistrictAdmin;

public sealed class RevokeDistrictAdminCommandHandler : IRequestHandler<RevokeDistrictAdminCommand, Result>
{
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RevokeDistrictAdminCommandHandler(IDistrictAdminRepository adminRepository, IDateTimeProvider dateTimeProvider)
    {
        _adminRepository = adminRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(RevokeDistrictAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _adminRepository.GetByIdAsync(request.AdminId, cancellationToken);
        if (admin == null)
        {
            return Result.Failure(new Error("DistrictAdmin.NotFound", "District admin not found"));
        }

        // Verify admin belongs to the specified district (tenant isolation)
        if (admin.DistrictId != request.DistrictId)
        {
            return Result.Failure(new Error("DistrictAdmin.AccessDenied", "Admin does not belong to this district"));
        }

        try
        {
            // Revoke admin access
            admin.Revoke(request.Reason, _dateTimeProvider);
            await _adminRepository.UpdateAsync(admin, cancellationToken);

            // TODO: Create audit record (will be handled by pipeline behavior)
            // Domain events are automatically published by the aggregate
            // Immediate access revocation is handled via session invalidation (separate concern)

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("DistrictAdmin.RevokeFailed", ex.Message));
        }
    }
}
