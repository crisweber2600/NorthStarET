using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Abstractions;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.CreateSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.DeleteSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.UpdateSchool;
using NorthStarET.NextGen.Lms.Application.Districts.Schools.Queries.ListSchools;
using NorthStarET.NextGen.Lms.Contracts.Schools;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.Schools;
using NorthStarET.NextGen.Lms.Infrastructure.Common.Services;
using NorthStarET.NextGen.Lms.Infrastructure.Idempotency;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// Scenario context for Schools & Grades BDD tests. Configures an in-memory pipeline with MediatR handlers,
/// validators, and the in-memory school repository to emulate API behavior without external dependencies.
/// </summary>
public sealed class SchoolCatalogScenarioContext : IDisposable
{
    private readonly ServiceProvider _provider;

    public SchoolCatalogScenarioContext()
    {
        var services = new ServiceCollection();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSchoolCommand).Assembly));
        services.AddValidatorsFromAssembly(typeof(CreateSchoolCommand).Assembly);

        services.AddSingleton<TestClock>();
        services.AddSingleton<IDateTimeProvider>(sp => sp.GetRequiredService<TestClock>());
        services.AddSingleton<TestCurrentUserService>();
        services.AddSingleton<IGradeTaxonomyProvider, GradeTaxonomyProvider>();
        services.AddSingleton<FakeIdempotencyService>();
        services.AddSingleton<IIdempotencyService>(sp => sp.GetRequiredService<FakeIdempotencyService>());
        services.AddSingleton<InMemorySchoolRepository>();
        services.AddSingleton<ISchoolRepository>(sp => sp.GetRequiredService<InMemorySchoolRepository>());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TestIdempotencyBehavior<,>));

        _provider = services.BuildServiceProvider();

        Mediator = _provider.GetRequiredService<IMediator>();
        Repository = _provider.GetRequiredService<InMemorySchoolRepository>();
        Clock = _provider.GetRequiredService<TestClock>();
        CurrentUser = _provider.GetRequiredService<TestCurrentUserService>();
        IdempotencyService = _provider.GetRequiredService<FakeIdempotencyService>();
    }

    public IMediator Mediator { get; }
    public InMemorySchoolRepository Repository { get; }
    public TestClock Clock { get; }
    public TestCurrentUserService CurrentUser { get; }
    public FakeIdempotencyService IdempotencyService { get; }

    public Guid CurrentDistrictId { get; private set; }
    public string CurrentDistrictName { get; private set; } = string.Empty;
    public Guid CurrentUserId { get; private set; }

    public string? ActiveSearchTerm { get; set; }
    public string SortOrder { get; set; } = "name-asc";
    public string ActiveTab { get; set; } = "Details";

    public List<SchoolListItemResponse> LastListItems { get; private set; } = new();
    public SchoolDetailResponse? LastSchoolDetail { get; private set; }
    public Result? LastCommandResult { get; private set; }
    public Result<SchoolDetailResponse>? LastDetailResult { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public string? LastSuccessMessage { get; private set; }
    public bool SearchControlsVisible { get; private set; }

    public Guid? PendingDeleteSchoolId { get; private set; }
    public string? PendingDeleteSchoolName { get; private set; }

    public CreateSchoolDraft? CreateDraft { get; private set; }
    public UpdateSchoolDraft? UpdateDraft { get; private set; }

    private readonly Dictionary<string, Guid> _districtLookup = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterDistrict(string name, Guid districtId)
    {
        _districtLookup[name] = districtId;
    }

    public Guid GetDistrictId(string name)
    {
        if (_districtLookup.TryGetValue(name, out var districtId))
        {
            return districtId;
        }

        throw new InvalidOperationException($"District '{name}' has not been registered in the scenario.");
    }

    public void SetCurrentDistrict(string name)
    {
        var districtId = GetDistrictId(name);

        CurrentDistrictName = name;
        CurrentDistrictId = districtId;
        CurrentUser.SetDistrictAdmin(districtId);
        CurrentUserId = CurrentUser.UserId ?? Guid.NewGuid();
    }

    public void ResetOutcome()
    {
        LastCommandResult = null;
        LastDetailResult = null;
        LastSchoolDetail = null;
        LastErrorMessage = null;
        LastSuccessMessage = null;
    }

    public void BeginCreateDraft()
    {
        ResetOutcome();
        CreateDraft = new CreateSchoolDraft();
    }

    public void BeginUpdateDraft(School school)
    {
        ResetOutcome();
        UpdateDraft = new UpdateSchoolDraft
        {
            SchoolId = school.Id,
            ConcurrencyStamp = school.ConcurrencyStamp,
            Name = school.Name,
            Code = school.Code,
            Notes = school.Notes
        };
    }

    public void ClearDrafts()
    {
        CreateDraft = null;
        UpdateDraft = null;
    }

    public async Task ExecuteListQueryAsync(CancellationToken cancellationToken = default)
    {
        ResetOutcome();
        SearchControlsVisible = true;
        var query = new ListSchoolsQuery(CurrentDistrictId, ActiveSearchTerm, SortOrder);
        var result = await Mediator.Send(query, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess && result.Value is not null)
        {
            LastListItems = result.Value.Items.ToList();
            LastSuccessMessage = "School list loaded";
        }
        else
        {
            LastListItems = new List<SchoolListItemResponse>();
            LastErrorMessage = result.Error?.Message;
        }
    }

    public async Task SubmitCreateAsync(CancellationToken cancellationToken = default)
    {
        if (CreateDraft is null)
        {
            throw new InvalidOperationException("Create draft has not been started.");
        }

        var gradeSelections = CreateDraft.Grades
            .Select(grade => new GradeSelectionDto
            {
                GradeId = grade.ToString(),
                SchoolType = GradeTaxonomy.GetTypicalSchoolType(grade).ToString(),
                Selected = true
            })
            .Cast<GradeSelectionDto>()
            .ToList();

        var command = new CreateSchoolCommand(
            CurrentDistrictId,
            CreateDraft.Name ?? throw new InvalidOperationException("School name is required for create."),
            CreateDraft.Code,
            CreateDraft.Notes,
            gradeSelections);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);
        LastDetailResult = result;
        LastCommandResult = result.IsSuccess ? Result.Success() : Result.Failure(result.Error ?? Error.NullValue);

        if (result.IsSuccess)
        {
            LastSchoolDetail = result.Value;
            LastSuccessMessage = "School created successfully";
            LastErrorMessage = null;
        }
        else
        {
            LastErrorMessage = MapError(result.Error);
        }

        ClearDrafts();
    }

    public async Task SubmitUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (UpdateDraft is null)
        {
            throw new InvalidOperationException("Update draft has not been started.");
        }

        var command = new UpdateSchoolCommand(
            CurrentDistrictId,
            UpdateDraft.SchoolId,
            UpdateDraft.Name ?? throw new InvalidOperationException("School name is required for update."),
            UpdateDraft.Code,
            UpdateDraft.Notes,
            UpdateDraft.ConcurrencyStamp ?? string.Empty);

        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);
        LastDetailResult = result;
        LastCommandResult = result.IsSuccess ? Result.Success() : Result.Failure(result.Error ?? Error.NullValue);

        if (result.IsSuccess)
        {
            LastSchoolDetail = result.Value;
            LastSuccessMessage = "School updated successfully";
            LastErrorMessage = null;
        }
        else
        {
            LastErrorMessage = MapError(result.Error);
        }

        ClearDrafts();
    }

    public async Task SubmitDeleteAsync(CancellationToken cancellationToken = default)
    {
        if (PendingDeleteSchoolId is null)
        {
            throw new InvalidOperationException("No school is pending deletion.");
        }

        var command = new DeleteSchoolCommand(CurrentDistrictId, PendingDeleteSchoolId.Value, CurrentUserId);
        var result = await Mediator.Send(command, cancellationToken).ConfigureAwait(false);
        LastCommandResult = result;

        if (result.IsSuccess)
        {
            LastSuccessMessage = "School deleted successfully";
            LastErrorMessage = null;
        }
        else
        {
            LastErrorMessage = MapError(result.Error);
        }

        PendingDeleteSchoolId = null;
        PendingDeleteSchoolName = null;
    }

    public void SetPendingDelete(School school)
    {
        PendingDeleteSchoolId = school.Id;
        PendingDeleteSchoolName = school.Name;
        LastSuccessMessage = null;
        LastErrorMessage = null;
    }

    public void CancelPendingDelete()
    {
        PendingDeleteSchoolId = null;
        PendingDeleteSchoolName = null;
    }

    public async Task SetSchoolGradesAsync(List<GradeLevel> grades, CancellationToken cancellationToken = default)
    {
        if (UpdateDraft is null)
        {
            throw new InvalidOperationException("No school is being updated; cannot set grades.");
        }

        // For BDD purposes, we directly modify the school in the repository
        var school = await Repository.GetByIdWithGradesAsync(UpdateDraft.SchoolId, CurrentDistrictId, cancellationToken).ConfigureAwait(false);
        if (school is null)
        {
            throw new InvalidOperationException("School not found for grade assignment.");
        }

        school.SetGradeOfferings(grades, CurrentUserId);
        await Repository.UpdateAsync(school, cancellationToken).ConfigureAwait(false);
    }

    public async Task SubmitGradeAssignmentAsync(CancellationToken cancellationToken = default)
    {
        if (UpdateDraft is null && CreateDraft is null)
        {
            throw new InvalidOperationException("No draft is active for grade assignment.");
        }

        if (UpdateDraft is not null)
        {
            // For update scenario, grades are already in the draft
            await SetSchoolGradesAsync(UpdateDraft.Grades, cancellationToken).ConfigureAwait(false);
            LastSuccessMessage = "Grades updated successfully";
            LastErrorMessage = null;
            LastCommandResult = Result.Success();
        }
        else if (CreateDraft is not null)
        {
            // For create scenario, grades will be set during school creation
            await SubmitCreateAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public string MapError(Error? error)
    {
        if (error is null)
        {
            return "An unexpected error occurred.";
        }

        return error.Code switch
        {
            "School.DuplicateName" => "A school with this name already exists in your district",
            "School.DuplicateCode" => "A school with this code already exists in your district",
            "School.ConcurrencyConflict" => "The school was modified by another user. Refresh and try again.",
            "School.NotFound" => "School not found in the specified district.",
            "School.AlreadyDeleted" => "School has already been deleted.",
            _ => error.Message
        };
    }

    public void Dispose()
    {
        _provider.Dispose();
    }

    public sealed class CreateSchoolDraft
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Notes { get; set; }
        public List<GradeLevel> Grades { get; } = new();
    }

    public sealed class UpdateSchoolDraft
    {
        public Guid SchoolId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Notes { get; set; }
        public string? ConcurrencyStamp { get; set; }
        public List<GradeLevel> Grades { get; } = new();
    }
}
