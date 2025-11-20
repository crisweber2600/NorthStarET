using System.Text.Json;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.UpdateDistrict;

/// <summary>
/// Command to update an existing district's name and suffix with audit metadata.
/// </summary>
public sealed class UpdateDistrictCommand :
    IRequest<Result>,
    ICommand,
    IIdempotentCommand,
    IAuditableCommand,
    ITenantScoped
{
    private string? _beforePayload;
    private string? _afterPayload;

    public UpdateDistrictCommand(Guid districtId, string name, string suffix)
    {
        if (districtId == Guid.Empty)
        {
            throw new ArgumentException("DistrictId must be provided.", nameof(districtId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);

        DistrictId = districtId;
        Name = name;
        Suffix = suffix.Trim();
        NormalizedSuffix = Suffix.ToLowerInvariant();
    }

    public Guid DistrictId { get; }

    public string Name { get; }

    public string Suffix { get; }

    public string NormalizedSuffix { get; }

    public string? AuditBeforePayload => _beforePayload;

    public string? AuditAfterPayload => _afterPayload;

    string IIdempotentCommand.Operation => "Districts.Update";
    Guid IIdempotentCommand.EntityId => DistrictId;

    string IAuditableCommand.Action => "UpdateDistrict";
    string IAuditableCommand.EntityType => "District";
    Guid? IAuditableCommand.EntityId => DistrictId;
    string? IAuditableCommand.BeforePayload => _beforePayload;
    string? IAuditableCommand.AfterPayload => _afterPayload;

    Guid ITenantScoped.DistrictId => DistrictId;

    /// <summary>
    /// Captures before and after payload snapshots for auditing.
    /// </summary>
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

