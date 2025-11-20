using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.DeleteSchool;

/// <summary>
/// Handler for soft deleting a school.
/// </summary>
public sealed class DeleteSchoolCommandHandler : IRequestHandler<DeleteSchoolCommand, Result>
{
    private readonly ISchoolRepository _repository;

    public DeleteSchoolCommandHandler(ISchoolRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
    {
        // Get existing school
        var school = await _repository.GetByIdAsync(
            request.SchoolId,
            request.DistrictId,
            cancellationToken);

        if (school == null)
        {
            return Result.Failure(
                new Error("School.NotFound", "School not found in the specified district."));
        }

        if (school.DeletedAt.HasValue)
        {
            return Result.Failure(
                new Error("School.AlreadyDeleted", "School has already been deleted."));
        }

        // Soft delete the school
        school.Delete(request.DeletedBy);

        // Save changes
        await _repository.UpdateAsync(school, cancellationToken);

        return Result.Success();
    }
}
