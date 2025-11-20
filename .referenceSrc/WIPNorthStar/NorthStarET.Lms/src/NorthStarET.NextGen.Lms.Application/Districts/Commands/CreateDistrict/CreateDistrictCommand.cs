using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using NorthStarET.NextGen.Lms.Application.Common;
using NorthStarET.NextGen.Lms.Application.Common.Behaviors;
using NorthStarET.NextGen.Lms.Contracts.Districts;

namespace NorthStarET.NextGen.Lms.Application.Districts.Commands.CreateDistrict;

/// <summary>
/// Command to create a new district with audit and idempotency metadata.
/// </summary>
public sealed class CreateDistrictCommand :
    IRequest<Result<CreateDistrictResponse>>,
    ICommand,
    IIdempotentCommand,
    IAuditableCommand
{
    private readonly Guid _idempotencyEntityId;
    private string? _afterPayload;

    public CreateDistrictCommand(string name, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);

        Name = name;
        Suffix = suffix.Trim();
        NormalizedSuffix = Suffix.ToLowerInvariant();
        _idempotencyEntityId = CreateDeterministicGuid(NormalizedSuffix);
        DistrictId = Guid.Empty;
    }

    /// <summary>
    /// Raw district name provided by the caller.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// District suffix prior to normalization.
    /// </summary>
    public string Suffix { get; }

    /// <summary>
    /// Normalized suffix (lowercase, trimmed) used for uniqueness/idempotency checks.
    /// </summary>
    public string NormalizedSuffix { get; }

    /// <summary>
    /// District identifier assigned once the handler creates the aggregate.
    /// </summary>
    public Guid DistrictId { get; private set; }

    /// <summary>
    /// Exposes the idempotency entity identifier for testing/diagnostics.
    /// </summary>
    public Guid IdempotencyEntityId => _idempotencyEntityId;

    /// <summary>
    /// Provides read access to the serialized after-state captured for auditing.
    /// </summary>
    public string? AuditAfterPayload => _afterPayload;

    string IIdempotentCommand.Operation => "Districts.Create";
    Guid IIdempotentCommand.EntityId => _idempotencyEntityId;

    string IAuditableCommand.Action => "CreateDistrict";
    string IAuditableCommand.EntityType => "District";
    Guid? IAuditableCommand.EntityId => DistrictId == Guid.Empty ? null : DistrictId;
    string? IAuditableCommand.BeforePayload => null;
    string? IAuditableCommand.AfterPayload => _afterPayload;

    /// <summary>
    /// Updates the command with the district identifier and serialized after-state for auditing.
    /// </summary>
    /// <param name="districtId">Identifier of the created district.</param>
    /// <param name="afterState">Anonymous object representing the persisted state.</param>
    internal void CaptureAuditState(Guid districtId, object afterState)
    {
        DistrictId = districtId;
        _afterPayload = SerializePayload(afterState);
    }

    internal static Guid CreateDeterministicGuid(string value)
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

