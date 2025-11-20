using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.CreateSchool;

/// <summary>
/// Command to create a new school within a district.
/// </summary>
public sealed class CreateSchoolCommand :
    IRequest<Result<SchoolDetailResponse>>,
    ICommand,
    IIdempotentCommand,
    IAuditableCommand,
    ITenantScoped
{
    private readonly List<GradeSelectionDto> _gradeSelections;
    private readonly Guid _idempotencyEntityId;
    private Guid _schoolId;
    private string? _afterPayload;

    public CreateSchoolCommand(
        Guid districtId,
        string name,
        string? code,
        string? notes,
        IEnumerable<GradeSelectionDto>? gradeSelections)
    {
        if (districtId == Guid.Empty)
        {
            throw new ArgumentException("DistrictId must be provided.", nameof(districtId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        DistrictId = districtId;
        Name = name.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        _gradeSelections = gradeSelections?
            .Where(static selection => selection is not null)
            .Select(selection => new GradeSelectionDto
            {
                GradeId = (selection.GradeId ?? string.Empty).Trim(),
                SchoolType = (selection.SchoolType ?? string.Empty).Trim(),
                Selected = selection.Selected
            })
            .ToList() ?? new List<GradeSelectionDto>();

        _idempotencyEntityId = CreateDeterministicGuid(
            $"{districtId:D}:{Name.ToLowerInvariant()}:{Code?.ToLowerInvariant() ?? string.Empty}");
    }

    public Guid DistrictId { get; }

    public string Name { get; }

    public string? Code { get; }

    public string? Notes { get; }

    public IReadOnlyList<GradeSelectionDto> GradeSelections => _gradeSelections;

    string IIdempotentCommand.Operation => "Schools.Create";

    Guid IIdempotentCommand.EntityId => _idempotencyEntityId;

    string IAuditableCommand.Action => "CreateSchool";

    string IAuditableCommand.EntityType => "School";

    Guid? IAuditableCommand.EntityId => _schoolId == Guid.Empty ? null : _schoolId;

    string? IAuditableCommand.BeforePayload => null;

    string? IAuditableCommand.AfterPayload => _afterPayload;

    Guid ITenantScoped.DistrictId => DistrictId;

    internal void CaptureAuditState(Guid schoolId, object afterState)
    {
        _schoolId = schoolId;
        _afterPayload = SerializePayload(afterState);
    }

    private static Guid CreateDeterministicGuid(string value)
    {
        using var md5 = MD5.Create();
        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = md5.ComputeHash(bytes);
        return new Guid(hash);
    }

    private static string SerializePayload(object payload)
    {
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }
}
