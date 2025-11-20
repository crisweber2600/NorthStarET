using System.Text.RegularExpressions;
using FluentValidation;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;

/// <summary>
/// Validator for CreateDistrictCommand.
/// </summary>
public sealed class CreateDistrictCommandValidator : AbstractValidator<CreateDistrictCommand>
{
    public CreateDistrictCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .Length(3, 100)
            .WithMessage("Name must be between 3 and 100 characters");

        RuleFor(x => x.Suffix)
            .NotEmpty()
            .WithMessage("Suffix is required")
            .Matches(@"^[a-z0-9.-]+$", RegexOptions.IgnoreCase)
            .WithMessage("Suffix must contain only letters, numbers, dots, and hyphens");
    }
}
