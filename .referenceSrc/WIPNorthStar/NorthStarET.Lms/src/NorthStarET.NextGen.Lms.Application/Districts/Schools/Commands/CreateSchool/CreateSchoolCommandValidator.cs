using FluentValidation;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.CreateSchool;

/// <summary>
/// Validator for CreateSchoolCommand.
/// </summary>
public sealed class CreateSchoolCommandValidator : AbstractValidator<CreateSchoolCommand>
{
    public CreateSchoolCommandValidator()
    {
        RuleFor(x => x.DistrictId)
            .NotEmpty()
            .WithMessage("District ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("School name is required")
            .Length(3, 100)
            .WithMessage("School name must be between 3 and 100 characters");

        RuleFor(x => x.Code)
            .Length(2, 20)
            .When(x => !string.IsNullOrWhiteSpace(x.Code))
            .WithMessage("School code must be between 2 and 20 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes cannot exceed 500 characters");
    }
}
