using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.InviteDistrictAdmin;

public sealed class InviteDistrictAdminCommandHandler
    : IRequestHandler<InviteDistrictAdminCommand, Result<InviteDistrictAdminResponse>>
{
    private readonly IDistrictRepository _districtRepository;
    private readonly IDistrictAdminRepository _adminRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public InviteDistrictAdminCommandHandler(
        IDistrictRepository districtRepository,
        IDistrictAdminRepository adminRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _districtRepository = districtRepository;
        _adminRepository = adminRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<InviteDistrictAdminResponse>> Handle(
        InviteDistrictAdminCommand request,
        CancellationToken cancellationToken)
    {
        // Validate district exists
        var district = await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken);
        if (district == null)
        {
            return Result.Failure<InviteDistrictAdminResponse>(
                new Error("District.NotFound", "District not found"));
        }

        // Validate email suffix matches district
        var emailDomain = request.Email.Split('@').LastOrDefault()?.ToLowerInvariant();
        if (string.IsNullOrEmpty(emailDomain) || !district.Suffix.Equals(emailDomain, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<InviteDistrictAdminResponse>(
                new Error("DistrictAdmin.InvalidSuffix", "Email domain does not match district suffix"));
        }

        // Check for duplicate email within district
        var existingAdmin = await _adminRepository.GetByEmailAsync(request.DistrictId, request.Email, cancellationToken);
        if (existingAdmin != null)
        {
            return Result.Failure<InviteDistrictAdminResponse>(
                new Error("DistrictAdmin.DuplicateEmail", "Admin with this email already exists"));
        }

        // Create district admin
        var admin = DistrictAdmin.Create(
            Guid.NewGuid(),
            request.DistrictId,
            request.Email,
            _dateTimeProvider);

        await _adminRepository.AddAsync(admin, cancellationToken);

        // TODO: Dispatch email invitation (will be implemented in Infrastructure)
        // TODO: Create audit record (will be handled by pipeline behavior)
        // Domain events are automatically published by the aggregate

        var response = new InviteDistrictAdminResponse
        {
            Id = admin.Id,
            DistrictId = admin.DistrictId,
            Email = admin.Email,
            Status = admin.Status.ToString(),
            InvitedAtUtc = admin.InvitedAtUtc,
            InvitationExpiresAtUtc = admin.InvitationExpiresAtUtc
        };

        return Result.Success(response);
    }
}
