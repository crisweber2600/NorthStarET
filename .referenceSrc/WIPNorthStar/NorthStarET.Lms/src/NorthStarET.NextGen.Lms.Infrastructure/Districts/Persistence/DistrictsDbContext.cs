using Microsoft.EntityFrameworkCore;
using NorthStarET.NextGen.Lms.Domain.Auditing;
using NorthStarET.NextGen.Lms.Domain.Common;
using NorthStarET.NextGen.Lms.Domain.DistrictAdmins;
using NorthStarET.NextGen.Lms.Domain.Districts;
using NorthStarET.NextGen.Lms.Domain.Schools;
using NorthStarET.NextGen.Lms.Infrastructure.Auditing.Persistence;

namespace NorthStarET.NextGen.Lms.Infrastructure.Districts.Persistence;

/// <summary>
/// DbContext for districts schema containing District, DistrictAdmin, School, GradeOffering, and AuditRecord tables.
/// Supports domain event publishing via SaveChangesAsync override.
/// </summary>
public sealed class DistrictsDbContext : DbContext
{
    private readonly IDomainEventPublisher? _domainEventPublisher;

    public DistrictsDbContext(
        DbContextOptions<DistrictsDbContext> options,
        IDomainEventPublisher? domainEventPublisher = null)
        : base(options)
    {
        _domainEventPublisher = domainEventPublisher;
    }

    public DbSet<District> Districts => Set<District>();
    public DbSet<DistrictAdmin> DistrictAdmins => Set<DistrictAdmin>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new DistrictEntityConfiguration());
        modelBuilder.ApplyConfiguration(new DistrictAdminEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SchoolEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AuditRecordEntityConfiguration());

        // Set default schema
        modelBuilder.HasDefaultSchema("districts");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events before saving
        var domainEvents = CollectDomainEvents();

        // Save changes to database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publish domain events after successful save
        if (_domainEventPublisher != null)
        {
            await PublishDomainEventsAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Clear domain events from aggregates
        aggregates.ForEach(a => a.ClearDomainEvents());

        return domainEvents;
    }

    private async Task PublishDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventPublisher!.PublishAsync(domainEvent, cancellationToken);
        }
    }
}

/// <summary>
/// Interface for publishing domain events (implemented by infrastructure).
/// </summary>
public interface IDomainEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
