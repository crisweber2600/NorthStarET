using System.Text.Json;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.DeleteDistrict;

/// <summary>
/// Command to soft-delete a district and cascade revoke all associated district admins.
/// Implements audit logging and tenant scoping.
/// </summary>
public sealed class DeleteDistrictCommand : IRequest<Result>, ICommand, IAuditableCommand, ITenantScoped
{
    private string? _beforePayload;
    private string? _afterPayload;

    public DeleteDistrictCommand(Guid districtId)
    {
        if (districtId == Guid.Empty)
        {
            throw new ArgumentException("DistrictId must be provided.", nameof(districtId));
        }

        DistrictId = districtId;
    }

    public Guid DistrictId { get; }

    public string? AuditBeforePayload => _beforePayload;

    public string? AuditAfterPayload => _afterPayload;

    string IAuditableCommand.Action => "DeleteDistrict";
    string IAuditableCommand.EntityType => "District";
    Guid? IAuditableCommand.EntityId => DistrictId;
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
