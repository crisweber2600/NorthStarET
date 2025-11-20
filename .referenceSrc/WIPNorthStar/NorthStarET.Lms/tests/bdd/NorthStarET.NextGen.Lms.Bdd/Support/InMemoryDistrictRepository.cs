using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Common.Interfaces;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Districts;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// In-memory implementation of <see cref="IDistrictRepository"/> used by BDD scenarios.
/// Stores aggregates in a dictionary and surfaces emitted domain events for verification.
/// </summary>
public sealed class InMemoryDistrictRepository : IDistrictRepository
{
    private readonly Dictionary<Guid, District> _districts = new();
    private readonly Dictionary<Guid, AdminCounters> _adminCounters = new();
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public InMemoryDistrictRepository(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public IReadOnlyCollection<District> Districts => _districts.Values.ToList().AsReadOnly();

    public District? Find(Guid id)
    {
        _districts.TryGetValue(id, out var district);
        return district;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _districts.TryGetValue(id, out var district);
        return Task.FromResult(district);
    }

    public Task<District?> GetBySuffixAsync(string suffix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);
        var normalized = suffix.ToLowerInvariant();
        var district = _districts.Values.FirstOrDefault(d => d.Suffix == normalized);
        return Task.FromResult(district);
    }

    public Task<List<District>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var result = _districts.Values
            .Where(d => !d.IsDeleted)
            .Select(Clone)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<bool> SuffixExistsAsync(string suffix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);
        var exists = _districts.Values.Any(d => d.Suffix == suffix.ToLowerInvariant() && !d.IsDeleted);
        return Task.FromResult(exists);
    }

    public Task<bool> IsSuffixUniqueAsync(string suffix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);
        var normalized = suffix.ToLowerInvariant();
        var unique = !_districts.Values.Any(d => d.Suffix == normalized && !d.IsDeleted);
        return Task.FromResult(unique);
    }

    public Task<bool> IsSuffixUniqueAsync(string suffix, Guid excludeDistrictId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);
        var normalized = suffix.ToLowerInvariant();
        var unique = !_districts.Values.Any(d => d.Id != excludeDistrictId && d.Suffix == normalized && !d.IsDeleted);
        return Task.FromResult(unique);
    }

    public Task<List<District>> ListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var skip = Math.Max(pageNumber - 1, 0) * pageSize;

        var items = _districts.Values
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Skip(skip)
            .Take(pageSize)
            .Select(Clone)
            .ToList();

        return Task.FromResult(items);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        var count = _districts.Values.Count(d => !d.IsDeleted);
        return Task.FromResult(count);
    }

    public Task<int> GetActiveAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        var count = GetCounters(districtId).Active;
        return Task.FromResult(count);
    }

    public Task<int> GetPendingAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        var count = GetCounters(districtId).Pending;
        return Task.FromResult(count);
    }

    public Task<int> GetRevokedAdminCountAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        var count = GetCounters(districtId).Revoked;
        return Task.FromResult(count);
    }

    public Task AddAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        _districts[district.Id] = district;
        EnsureCounters(district.Id);
        CaptureEvents(district);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        _districts[district.Id] = district;
        EnsureCounters(district.Id);

        var counters = GetCounters(district.Id);
        if (district.IsDeleted && counters.Active > 0)
        {
            for (var i = 0; i < counters.Active; i++)
            {
                var adminId = Guid.NewGuid();
                var email = $"revoked+{i}@{district.Suffix}.example";
                _domainEvents.Add(new DistrictAdminRevokedEvent(adminId, district.Id, email, _dateTimeProvider.UtcNow, "District deleted"));
            }

            _adminCounters[district.Id] = counters with { Active = 0, Revoked = counters.Revoked + counters.Active };
        }

        CaptureEvents(district);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(District district, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(district);

        _districts.Remove(district.Id);
        _adminCounters.Remove(district.Id);
        CaptureEvents(district);

        return Task.CompletedTask;
    }

    public void Clear()
    {
        _districts.Clear();
        _adminCounters.Clear();
        _domainEvents.Clear();
    }

    public void SetAdminCounts(Guid districtId, int active, int pending, int revoked)
    {
        _adminCounters[districtId] = new AdminCounters(active, pending, revoked);
    }

    private void EnsureCounters(Guid districtId)
    {
        if (!_adminCounters.ContainsKey(districtId))
        {
            _adminCounters[districtId] = new AdminCounters(0, 0, 0);
        }
    }

    private AdminCounters GetCounters(Guid districtId)
    {
        EnsureCounters(districtId);
        return _adminCounters[districtId];
    }

    private void CaptureEvents(District district)
    {
        foreach (var domainEvent in district.DomainEvents)
        {
            _domainEvents.Add(domainEvent);
        }

        district.ClearDomainEvents();
    }

    private District Clone(District district)
    {
        // Cloning by creating a new instance ensures test assertions don't mutate stored aggregates.
        var clone = District.Create(district.Id, district.Name, district.Suffix, _dateTimeProvider);

        typeof(District).GetProperty("CreatedAtUtc")!.SetValue(clone, district.CreatedAtUtc);
        typeof(District).GetProperty("UpdatedAtUtc")!.SetValue(clone, district.UpdatedAtUtc);
        typeof(District).GetProperty("DeletedAt")!.SetValue(clone, district.DeletedAt);

        clone.ClearDomainEvents();
        return clone;
    }

    private sealed record AdminCounters(int Active, int Pending, int Revoked);
}
