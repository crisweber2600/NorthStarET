using FluentValidation;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;

/// <summary>
/// Validator for UpdateDistrictCommand.
/// </summary>
public sealed class UpdateDistrictCommandValidator : AbstractValidator<UpdateDistrictCommand>
{
    public UpdateDistrictCommandValidator()
    {
        RuleFor(x => x.DistrictId)
            .NotEmpty()
            .WithMessage("District ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .Length(3, 100)
            .WithMessage("Name must be between 3 and 100 characters");

        RuleFor(x => x.Suffix)
            .NotEmpty()
            .WithMessage("Suffix is required")
            .Matches(@"^[a-z0-9.-]+$")
            .WithMessage("Suffix must contain only lowercase letters, numbers, dots, and hyphens");
    }
}
