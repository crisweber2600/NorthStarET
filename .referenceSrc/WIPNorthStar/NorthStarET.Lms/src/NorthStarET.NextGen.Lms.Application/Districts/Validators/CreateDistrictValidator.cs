using FluentValidation;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;

namespace NorthStarET.NextGen.Lms.Application.Districts.Validators;

public sealed class CreateDistrictValidator : AbstractValidator<CreateDistrictCommand>
{
    public CreateDistrictValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("District name is required")
            .Length(3, 100)
            .WithMessage("District name must be between 3 and 100 characters");

        // Validate the normalized suffix so callers may provide case-insensitive input
        // (the command normalizes the suffix to lowercase). This allows inputs like
        // "DEMO" to be accepted and normalized to "demo" for uniqueness checks.
        RuleFor(x => x.NormalizedSuffix)
            .NotEmpty()
            .WithMessage("District suffix is required")
            .Matches("^[a-z0-9.-]+$")
            .WithMessage("Suffix must contain only lowercase letters, numbers, dots, and hyphens");
    }
}
