using System;
using System.Linq;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.UpdateSchool;

/// <summary>
/// Handler for updating school metadata.
/// </summary>
public sealed class UpdateSchoolCommandHandler : IRequestHandler<UpdateSchoolCommand, Result<SchoolDetailResponse>>
{
    private readonly ISchoolRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSchoolCommandHandler(ISchoolRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<Result<SchoolDetailResponse>> Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId;
        if (actorId is null || actorId.Value == Guid.Empty)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("Authorization.ActorMissing", "Authenticated user context is required to update a school."));
        }

        // Get existing school
        var school = await _repository.GetByIdWithGradesAsync(
            request.SchoolId,
            request.DistrictId,
            cancellationToken);

        if (school == null)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("School.NotFound", "School not found in the specified district."));
        }

        if (school.DeletedAt.HasValue)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("School.Deleted", "Cannot update a deleted school."));
        }

        // Check concurrency
        if (school.ConcurrencyStamp != request.ConcurrencyStamp)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("School.ConcurrencyConflict", "The school has been modified by another user. Please refresh and try again."));
        }

        // Check for duplicate name (excluding current school)
        if (!string.Equals(school.Name, request.Name, StringComparison.Ordinal))
        {
            var nameExists = await _repository.ExistsWithNameAsync(
                request.DistrictId,
                request.Name,
                excludeSchoolId: request.SchoolId,
                cancellationToken);

            if (nameExists)
            {
                return Result.Failure<SchoolDetailResponse>(
                    new Error("School.DuplicateName", $"A school named '{request.Name}' already exists in this district."));
            }
        }

        // Check for duplicate code (excluding current school)
        if (!string.IsNullOrWhiteSpace(request.Code) && !string.Equals(school.Code, request.Code, StringComparison.Ordinal))
        {
            var codeExists = await _repository.ExistsWithCodeAsync(
                request.DistrictId,
                request.Code,
                excludeSchoolId: request.SchoolId,
                cancellationToken);

            if (codeExists)
            {
                return Result.Failure<SchoolDetailResponse>(
                    new Error("School.DuplicateCode", $"A school with code '{request.Code}' already exists in this district."));
            }
        }

        var beforeState = new
        {
            school.Id,
            school.Name,
            school.Code,
            school.Notes,
            school.Status,
            school.ConcurrencyStamp,
            Grades = school.GradeOfferings
                .Where(g => g.IsActive)
                .Select(g => new { Grade = g.GradeLevel.ToString(), Type = g.SchoolType.ToString() })
                .ToList()
        };

        // Update school
        school.Update(
            request.Name,
            request.Code,
            request.Notes,
            school.Status, // Keep existing status
            actorId.Value);

        // Save changes
        await _repository.UpdateAsync(school, cancellationToken);

        request.CaptureAuditState(
            beforeState,
            new
            {
                school.Id,
                school.Name,
                school.Code,
                school.Notes,
                school.Status,
                school.ConcurrencyStamp,
                Grades = school.GradeOfferings
                    .Where(g => g.IsActive)
                    .Select(g => new { Grade = g.GradeLevel.ToString(), Type = g.SchoolType.ToString() })
                    .ToList()
            });

        return Result.Success(MapToDetailResponse(school));
    }

    private static SchoolDetailResponse MapToDetailResponse(School school)
    {
        var gradeSelections = school.GradeOfferings
            .Where(g => g.IsActive)
            .Select(g => new GradeSelectionDto
            {
                GradeId = g.GradeLevel.ToString(),
                SchoolType = g.SchoolType.ToString(),
                Selected = true
            }).ToList();

        return new SchoolDetailResponse
        {
            SchoolId = school.Id,
            DistrictId = school.DistrictId,
            Name = school.Name,
            Code = school.Code,
            Notes = school.Notes,
            Status = school.Status.ToString(),
            GradeSelections = gradeSelections,
            ConcurrencyStamp = school.ConcurrencyStamp,
            Audit = new SchoolAuditDto
            {
                CreatedBy = school.CreatedBy,
                CreatedAt = school.CreatedAtUtc,
                UpdatedBy = school.UpdatedBy,
                UpdatedAt = school.UpdatedAtUtc
            }
        };
    }
}
