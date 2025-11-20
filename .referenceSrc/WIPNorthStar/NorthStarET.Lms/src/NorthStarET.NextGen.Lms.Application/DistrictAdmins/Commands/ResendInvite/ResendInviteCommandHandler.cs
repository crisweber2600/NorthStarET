using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.ResendInvite;

public sealed class ResendInviteCommandHandler : IRequestHandler<ResendInviteCommand, Result>
{
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ResendInviteCommandHandler(IDistrictAdminRepository adminRepository, IDateTimeProvider dateTimeProvider)
    {
        _adminRepository = adminRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(ResendInviteCommand request, CancellationToken cancellationToken)
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
            // Resend invitation (extends expiry by 7 days)
            admin.ResendInvitation(_dateTimeProvider);
            await _adminRepository.UpdateAsync(admin, cancellationToken);

            // TODO: Dispatch email invitation (will be implemented in Infrastructure)
            // TODO: Create audit record (will be handled by pipeline behavior)
            // Domain events are automatically published by the aggregate

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("DistrictAdmin.ResendFailed", ex.Message));
        }
    }
}
