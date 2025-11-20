using FluentValidation;
using NorthStarET.NextGen.Lms.Application.DistrictAdmins.Commands.InviteDistrictAdmin;

namespace NorthStarET.NextGen.Lms.Application.DistrictAdmins.Validators;

public sealed class InviteDistrictAdminValidator : AbstractValidator<InviteDistrictAdminCommand>
{
    private const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    public InviteDistrictAdminValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters")
            .Matches(EmailRegex)
            .WithMessage("Email format is invalid (RFC 5322)");

        RuleFor(x => x.DistrictId)
            .NotEmpty()
            .WithMessage("District ID is required");
    }
}
