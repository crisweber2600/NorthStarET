using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.Schools;

namespace NorthStarET.NextGen.Lms.Bdd.Support;

/// <summary>
/// In-memory implementation of <see cref="ISchoolRepository"/> for BDD scenarios.
/// Stores school aggregates keyed by identifier and captures domain events for assertions.
/// </summary>
public sealed class InMemorySchoolRepository : ISchoolRepository
{
    private readonly Dictionary<Guid, School> _schools = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<School> Schools => _schools.Values.ToList().AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Task<School?> GetByIdAsync(Guid schoolId, Guid districtId, CancellationToken cancellationToken = default)
    {
        if (_schools.TryGetValue(schoolId, out var school) && school.DistrictId == districtId)
        {
            return Task.FromResult<School?>(school);
        }

        return Task.FromResult<School?>(null);
    }

    public Task<School?> GetByIdWithGradesAsync(Guid schoolId, Guid districtId, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(schoolId, districtId, cancellationToken);
    }

    public Task<IReadOnlyList<School>> ListByDistrictAsync(
        Guid districtId,
        string? searchTerm = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _schools.Values.Where(s => s.DistrictId == districtId);

        if (!includeDeleted)
        {
            query = query.Where(s => !s.DeletedAt.HasValue);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(s.Code) && s.Code.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        var result = query
            .OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<School>>(result);
    }

    public Task<bool> ExistsWithNameAsync(
        Guid districtId,
        string name,
        Guid? excludeSchoolId = null,
        CancellationToken cancellationToken = default)
    {
        var exists = _schools.Values.Any(s =>
            s.DistrictId == districtId &&
            (!s.DeletedAt.HasValue) &&
            s.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (!excludeSchoolId.HasValue || s.Id != excludeSchoolId.Value));

        return Task.FromResult(exists);
    }

    public Task<bool> ExistsWithCodeAsync(
        Guid districtId,
        string code,
        Guid? excludeSchoolId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Task.FromResult(false);
        }

        var exists = _schools.Values.Any(s =>
            s.DistrictId == districtId &&
            (!s.DeletedAt.HasValue) &&
            !string.IsNullOrWhiteSpace(s.Code) &&
            s.Code.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (!excludeSchoolId.HasValue || s.Id != excludeSchoolId.Value));

        return Task.FromResult(exists);
    }

    public Task AddAsync(School school, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(school);
        _schools[school.Id] = school;
        CaptureEvents(school);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(School school, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(school);
        _schools[school.Id] = school;
        CaptureEvents(school);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(School school, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(school);
        _schools[school.Id] = school;
        CaptureEvents(school);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public void Clear()
    {
        _schools.Clear();
        _domainEvents.Clear();
    }

    public School? FindByName(Guid districtId, string name)
    {
        return _schools.Values.FirstOrDefault(s =>
            s.DistrictId == districtId &&
            s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private void CaptureEvents(School school)
    {
        foreach (var domainEvent in school.DomainEvents)
        {
            _domainEvents.Add(domainEvent);
        }

        school.ClearDomainEvents();
    }
}
