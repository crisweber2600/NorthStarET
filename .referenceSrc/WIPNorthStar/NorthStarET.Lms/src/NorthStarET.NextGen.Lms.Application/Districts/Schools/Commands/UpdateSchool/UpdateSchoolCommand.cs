using System;
using System.Text.Json;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Schools;

namespace NorthStarET.NextGen.Lms.Application.Districts.Schools.Commands.UpdateSchool;

/// <summary>
/// Command to update school metadata (excluding grade assignments).
/// </summary>
public sealed class UpdateSchoolCommand :
    IRequest<Result<SchoolDetailResponse>>,
    ICommand,
    IIdempotentCommand,
    IAuditableCommand,
    ITenantScoped
{
    private string? _beforePayload;
    private string? _afterPayload;

    public UpdateSchoolCommand(
        Guid districtId,
        Guid schoolId,
        string name,
        string? code,
        string? notes,
        string concurrencyStamp)
    {
        if (districtId == Guid.Empty)
        {
            throw new ArgumentException("DistrictId must be provided.", nameof(districtId));
        }

        if (schoolId == Guid.Empty)
        {
            throw new ArgumentException("SchoolId must be provided.", nameof(schoolId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(concurrencyStamp);

        DistrictId = districtId;
        SchoolId = schoolId;
        Name = name.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        ConcurrencyStamp = concurrencyStamp.Trim();
    }

    public Guid DistrictId { get; }

    public Guid SchoolId { get; }

    public string Name { get; }

    public string? Code { get; }

    public string? Notes { get; }

    public string ConcurrencyStamp { get; }

    string IIdempotentCommand.Operation => "Schools.Update";

    Guid IIdempotentCommand.EntityId => SchoolId;

    string IAuditableCommand.Action => "UpdateSchool";

    string IAuditableCommand.EntityType => "School";

    Guid? IAuditableCommand.EntityId => SchoolId;

    string? IAuditableCommand.BeforePayload => _beforePayload;

    string? IAuditableCommand.AfterPayload => _afterPayload;

    Guid ITenantScoped.DistrictId => DistrictId;

    internal void CaptureAuditState(object beforeState, object afterState)
    {
        _beforePayload = SerializePayload(beforeState);
        _afterPayload = SerializePayload(afterState);
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
