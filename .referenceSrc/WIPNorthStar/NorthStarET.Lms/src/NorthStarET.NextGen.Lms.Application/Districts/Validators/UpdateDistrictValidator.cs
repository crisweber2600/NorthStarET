using FluentValidation;
using NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;

namespace NorthStarET.NextGen.Lms.Application.Districts.Validators;

public sealed class UpdateDistrictValidator : AbstractValidator<UpdateDistrictCommand>
{
    public UpdateDistrictValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("District name is required")
            .Length(3, 100)
            .WithMessage("District name must be between 3 and 100 characters");
    }
}
