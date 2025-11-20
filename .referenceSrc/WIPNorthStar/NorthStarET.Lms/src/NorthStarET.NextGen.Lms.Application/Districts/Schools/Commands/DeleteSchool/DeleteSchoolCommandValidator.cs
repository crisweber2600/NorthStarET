using FluentValidation;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.DeleteSchool;

/// <summary>
/// Validator for DeleteSchoolCommand.
/// </summary>
public sealed class DeleteSchoolCommandValidator : AbstractValidator<DeleteSchoolCommand>
{
    public DeleteSchoolCommandValidator()
    {
        RuleFor(x => x.DistrictId)
            .NotEmpty()
            .WithMessage("District ID is required");

        RuleFor(x => x.SchoolId)
            .NotEmpty()
            .WithMessage("School ID is required");

        RuleFor(x => x.DeletedBy)
            .NotEmpty()
            .WithMessage("DeletedBy is required");
    }
}
