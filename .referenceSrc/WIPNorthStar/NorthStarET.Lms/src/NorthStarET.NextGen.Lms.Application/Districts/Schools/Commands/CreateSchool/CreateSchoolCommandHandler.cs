using System.Collections.Generic;
using System.Linq;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Abstractions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.CreateSchool;

/// <summary>
/// Handler for creating a new school within a district.
/// </summary>
public sealed class CreateSchoolCommandHandler : IRequestHandler<CreateSchoolCommand, Result<SchoolDetailResponse>>
{
    private readonly ISchoolRepository _repository;
    private readonly IGradeTaxonomyProvider _gradeTaxonomyProvider;
    private readonly ICurrentUserService _currentUserService;

    public CreateSchoolCommandHandler(
        ISchoolRepository repository,
        IGradeTaxonomyProvider gradeTaxonomyProvider,
        ICurrentUserService currentUserService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _gradeTaxonomyProvider = gradeTaxonomyProvider ?? throw new ArgumentNullException(nameof(gradeTaxonomyProvider));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<Result<SchoolDetailResponse>> Handle(CreateSchoolCommand request, CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId;
        if (actorId is null || actorId.Value == Guid.Empty)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("Authorization.ActorMissing", "Authenticated user context is required to create a school."));
        }

        // Check for duplicate name
        var nameExists = await _repository.ExistsWithNameAsync(
            request.DistrictId,
            request.Name,
            excludeSchoolId: null,
            cancellationToken);

        if (nameExists)
        {
            return Result.Failure<SchoolDetailResponse>(
                new Error("School.DuplicateName", $"A school named '{request.Name}' already exists in this district."));
        }

        // Check for duplicate code if provided
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codeExists = await _repository.ExistsWithCodeAsync(
                request.DistrictId,
                request.Code,
                excludeSchoolId: null,
                cancellationToken);

            if (codeExists)
            {
                return Result.Failure<SchoolDetailResponse>(
                    new Error("School.DuplicateCode", $"A school with code '{request.Code}' already exists in this district."));
            }
        }

        var selectedGrades = ValidateAndParseGrades(request);
        if (selectedGrades.IsFailure)
        {
            return Result.Failure<SchoolDetailResponse>(selectedGrades.Error!);
        }

        // Create school aggregate
        var school = School.Create(
            request.DistrictId,
            request.Name,
            request.Code,
            request.Notes,
            actorId.Value);

        if (selectedGrades.Value!.Count > 0)
        {
            school.SetGradeOfferings(selectedGrades.Value, actorId.Value);
        }

        await _repository.AddAsync(school, cancellationToken);

        request.CaptureAuditState(
            school.Id,
            new
            {
                school.Id,
                school.DistrictId,
                school.Name,
                school.Code,
                school.Status,
                Grades = school.GradeOfferings
                    .Where(g => g.IsActive)
                    .Select(g => new { Grade = g.GradeLevel.ToString(), Type = g.SchoolType.ToString() })
            });

        return Result.Success(MapToDetailResponse(school));
    }

    private Result<List<GradeLevel>> ValidateAndParseGrades(CreateSchoolCommand request)
    {
        var result = new List<GradeLevel>();
        var seen = new HashSet<GradeLevel>();

        foreach (var selection in request.GradeSelections.Where(static s => s.Selected))
        {
            if (string.IsNullOrWhiteSpace(selection.GradeId))
            {
                return Result.Failure<List<GradeLevel>>(
                    new Error("School.InvalidGrade", "Selected grades must include a valid identifier."));
            }

            if (!Enum.TryParse<GradeLevel>(selection.GradeId, ignoreCase: true, out var gradeLevel))
            {
                return Result.Failure<List<GradeLevel>>(
                    new Error("School.InvalidGrade", $"Grade '{selection.GradeId}' is not recognized."));
            }

            if (!_gradeTaxonomyProvider.IsValidGrade(gradeLevel))
            {
                return Result.Failure<List<GradeLevel>>(
                    new Error("School.InvalidGrade", $"Grade '{selection.GradeId}' is not available in the taxonomy."));
            }

            if (seen.Add(gradeLevel))
            {
                result.Add(gradeLevel);
            }
        }

        return Result.Success(result);
    }

    private static SchoolDetailResponse MapToDetailResponse(School school)
    {
        var activeGrades = school.GradeOfferings
            .Where(g => g.IsActive)
            .Select(g => new GradeSelectionDto
            {
                GradeId = g.GradeLevel.ToString(),
                SchoolType = g.SchoolType.ToString(),
                Selected = true
            })
            .ToList();

        return new SchoolDetailResponse
        {
            SchoolId = school.Id,
            DistrictId = school.DistrictId,
            Name = school.Name,
            Code = school.Code,
            Notes = school.Notes,
            Status = school.Status.ToString(),
            GradeSelections = activeGrades,
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
