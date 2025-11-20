# Domain Events Schema Standard

**Purpose**: Define the standardized schema for all domain events across NorthStar microservices  
**Version**: 1.0  
**Last Updated**: 2025-11-20  
**Status**: Specification Complete

---

## Overview

All domain events in the NorthStar LMS follow a standardized schema to ensure consistency, traceability, and reliable event-driven communication across microservices. This document defines the base schema structure, naming conventions, and implementation guidelines.

---

## Base Event Schema

### SharedEventBase Structure

All domain events MUST inherit from or implement the following base schema:

```csharp
// Location: Src/Foundation/shared/Domain/Common/DomainEvent.cs
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this specific event instance
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// UTC timestamp when the event occurred
    /// </summary>
    DateTime OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance (must be explicitly provided)
    /// </summary>
    public required Guid EventId { get; init; }
    
    /// <summary>
    /// UTC timestamp when the event was created (must be provided explicitly)
    /// Recommended: Use IDateTimeProvider.UtcNow for testability
    /// </summary>
    public required DateTime OccurredAt { get; init; }
}
```

### Required Multi-Tenancy Field

All business events MUST include `TenantId` for tenant isolation:

```csharp
public abstract record TenantAwareDomainEvent : DomainEvent
{
    /// <summary>
    /// Tenant (district) identifier for multi-tenant isolation
    /// </summary>
    public required Guid TenantId { get; init; }
}
```

---

## Standard Event Properties

### Core Properties (Required for all events)

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `EventId` | `Guid` | ✅ | Unique identifier for event deduplication |
| `OccurredAt` | `DateTime` | ✅ | UTC timestamp for event ordering and auditing |
| `TenantId` | `Guid` | ✅ | Tenant identifier for multi-tenant isolation |

### Aggregate Properties (Required for entity events)

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `AggregateId` | `Guid` | ✅ | Identifier of the entity this event relates to (e.g., StudentId, AssessmentId) |
| `AggregateType` | `string` | Recommended | Type of aggregate (e.g., "Student", "Assessment") |

### Audit Properties (Recommended)

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `UserId` | `Guid` | Recommended | User who triggered the event (if user-initiated) |
| `CorrelationId` | `Guid` | Recommended | For tracing requests across services |
| `CausationId` | `Guid` | Optional | ID of the command that caused this event |

### Creating Events with Explicit Properties

Events must be created with explicit `EventId` and `OccurredAt` values for testability and determinism:

```csharp
// In production code - use IDateTimeProvider for testability
public class CreateStudentCommandHandler
{
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public async Task Handle(CreateStudentCommand command)
    {
        var student = new Student(/* ... */);
        
        var @event = new StudentCreatedEvent(
            EventId: Guid.NewGuid(),
            TenantId: command.TenantId,
            StudentId: student.Id,
            // ... other properties
            OccurredAt: _dateTimeProvider.UtcNow
        );
        
        await _eventPublisher.PublishAsync(@event);
    }
}

// In tests - use fixed timestamps for deterministic assertions
var fixedTime = new DateTime(2025, 11, 20, 12, 0, 0, DateTimeKind.Utc);
var @event = new StudentCreatedEvent(
    EventId: knownGuid,
    TenantId: tenantId,
    StudentId: studentId,
    // ... other properties
    OccurredAt: fixedTime
);
```

**Why explicit values?**
- **Testability**: Tests can use fixed timestamps and GUIDs for deterministic assertions
- **Idempotency**: EventId can be derived from aggregate state for exact-once processing
- **Auditability**: OccurredAt represents the actual event time, not publishing time

---

## Event Naming Conventions

### Pattern: `{Aggregate}{Action}Event`

**Examples**:
- `StudentCreatedEvent`
- `AssessmentResultRecordedEvent`
- `InterventionCompletedEvent`
- `StaffAssignedToSchoolEvent`

### Action Verb Guidelines

- **Past tense**: Events represent facts that have already occurred
  - ✅ `StudentEnrolledEvent`
  - ❌ `EnrollStudentEvent`
  
- **Specific and descriptive**:
  - ✅ `AssessmentResultRecordedEvent`
  - ❌ `AssessmentUpdatedEvent` (too generic)
  
- **State change focused**:
  - ✅ `InterventionStatusChangedEvent` (includes old/new status)
  - ✅ `StudentDemographicsUpdatedEvent` (includes changed fields)

---

## Service-Specific Event Examples

### Identity Service

```csharp
public record UserRegisteredEvent(
    Guid UserId,
    Guid TenantId,
    string Email,
    string[] Roles
) : TenantAwareDomainEvent;

public record UserLoggedInEvent(
    Guid UserId,
    Guid TenantId,
    DateTime LoginTimestamp,
    string IPAddress
) : TenantAwareDomainEvent;

public record UserRoleChangedEvent(
    Guid UserId,
    Guid TenantId,
    string[] AddedRoles,
    string[] RemovedRoles,
    Guid ChangedBy
) : TenantAwareDomainEvent;
```

### Student Management Service

```csharp
public record StudentCreatedEvent(
    Guid StudentId,
    Guid TenantId,
    Guid SchoolId,
    int GradeLevel,
    string FirstName,
    string LastName
) : TenantAwareDomainEvent;

public record StudentEnrolledEvent(
    Guid StudentId,
    Guid TenantId,
    Guid SchoolId,
    DateTime EnrollmentDate,
    string EnrollmentStatus
) : TenantAwareDomainEvent;

public record StudentWithdrawnEvent(
    Guid StudentId,
    Guid TenantId,
    DateTime WithdrawalDate,
    string Reason
) : TenantAwareDomainEvent;

public record StudentDemographicsChangedEvent(
    Guid StudentId,
    Guid TenantId,
    Dictionary<string, object> ChangedFields, // Field name -> new value
    Guid ChangedBy
) : TenantAwareDomainEvent;
```

### Assessment Service

```csharp
public record AssessmentCreatedEvent(
    Guid AssessmentId,
    Guid TenantId,
    string AssessmentName,
    string Subject,
    int[] GradeLevels,
    Guid CreatedBy
) : TenantAwareDomainEvent;

public record AssessmentAssignedEvent(
    Guid AssignmentId,
    Guid TenantId,
    Guid AssessmentId,
    Guid StudentId,
    DateTime DueDate,
    Guid AssignedBy
) : TenantAwareDomainEvent;

public record AssessmentResultRecordedEvent(
    Guid ResultId,
    Guid TenantId,
    Guid AssessmentId,
    Guid StudentId,
    decimal TotalScore,
    string BenchmarkLevel,
    DateTime CompletedDate,
    Guid RecordedBy
) : TenantAwareDomainEvent;
```

### Staff Management Service

```csharp
public record StaffCreatedEvent(
    Guid StaffId,
    Guid TenantId,
    string Email,
    string Role,
    Guid CreatedBy
) : TenantAwareDomainEvent;

public record StaffAssignedToSchoolEvent(
    Guid AssignmentId,
    Guid TenantId,
    Guid StaffId,
    Guid SchoolId,
    decimal FtePercentage,
    DateTime StartDate
) : TenantAwareDomainEvent;

public record TeamCreatedEvent(
    Guid TeamId,
    Guid TenantId,
    string TeamName,
    Guid TeamLeadId,
    Guid[] MemberIds
) : TenantAwareDomainEvent;
```

### Configuration Service

```csharp
public record DistrictCreatedEvent(
    Guid DistrictId,
    Guid TenantId,
    string DistrictName,
    string State
) : TenantAwareDomainEvent;

public record SchoolCreatedEvent(
    Guid SchoolId,
    Guid TenantId,
    Guid DistrictId,
    string SchoolName,
    int[] GradeLevels
) : TenantAwareDomainEvent;

public record ConfigurationChangedEvent(
    Guid TenantId,
    string SettingKey,
    object OldValue,
    object NewValue,
    Guid ChangedBy
) : TenantAwareDomainEvent;
```

### Intervention Management Service

```csharp
public record InterventionCreatedEvent(
    Guid InterventionId,
    Guid TenantId,
    string InterventionName,
    string InterventionType,
    Guid CreatedBy
) : TenantAwareDomainEvent;

public record StudentAddedToInterventionEvent(
    Guid InterventionId,
    Guid TenantId,
    Guid StudentId,
    DateTime StartDate
) : TenantAwareDomainEvent;

public record InterventionProgressRecordedEvent(
    Guid ProgressId,
    Guid TenantId,
    Guid InterventionId,
    Guid StudentId,
    string ProgressNote,
    DateTime RecordedDate,
    Guid RecordedBy
) : TenantAwareDomainEvent;
```

### Section & Roster Service

```csharp
public record SectionCreatedEvent(
    Guid SectionId,
    Guid TenantId,
    Guid SchoolId,
    string SectionName,
    string Subject,
    int GradeLevel,
    Guid TeacherId
) : TenantAwareDomainEvent;

public record StudentAddedToSectionEvent(
    Guid SectionId,
    Guid TenantId,
    Guid StudentId,
    DateTime EnrollmentDate
) : TenantAwareDomainEvent;

public record SectionRolloverCompletedEvent(
    Guid TenantId,
    int FromAcademicYear,
    int ToAcademicYear,
    int SectionsCreated,
    DateTime CompletedAt
) : TenantAwareDomainEvent;
```

---

## Event Payload Guidelines

### 1. Include Sufficient Context

Events should contain enough data for consumers to act without additional queries:

✅ **Good**:
```csharp
public record StudentCreatedEvent(
    Guid StudentId,
    Guid TenantId,
    Guid SchoolId,
    int GradeLevel,
    string FirstName,
    string LastName  // Enough context for notifications
) : TenantAwareDomainEvent;
```

❌ **Bad**:
```csharp
public record StudentCreatedEvent(
    Guid StudentId,
    Guid TenantId
    // Missing context - consumers must query Student Service
) : TenantAwareDomainEvent;
```

### 2. Use Primitive Types When Possible

Avoid complex nested objects in events:

✅ **Good**:
```csharp
public record AddressChangedEvent(
    Guid StudentId,
    Guid TenantId,
    string Street,
    string City,
    string State,
    string Zip
) : TenantAwareDomainEvent;
```

❌ **Bad**:
```csharp
public record AddressChangedEvent(
    Guid StudentId,
    Guid TenantId,
    Address Address  // Complex object - versioning nightmare
) : TenantAwareDomainEvent;
```

### 3. Include Delta for Update Events

For update events, include what changed:

```csharp
public record StudentDemographicsChangedEvent(
    Guid StudentId,
    Guid TenantId,
    Dictionary<string, object> ChangedFields, // { "Email": "new@example.com", "Phone": "555-1234" }
    Guid ChangedBy
) : TenantAwareDomainEvent;
```

---

## Event Versioning Strategy

### Schema Evolution Rules

1. **Additive Changes Only** (non-breaking):
   - ✅ Add new optional properties
   - ✅ Add new event types
   
2. **Breaking Changes Require New Event Type**:
   - ❌ Remove properties
   - ❌ Rename properties
   - ❌ Change property types
   - ✅ Create `StudentEnrolledEventV2` instead

### Versioning Pattern

```csharp
// Version 1 (original)
public record StudentEnrolledEvent(
    Guid StudentId,
    Guid TenantId,
    Guid SchoolId
) : TenantAwareDomainEvent;

// Version 2 (new version with breaking changes)
public record StudentEnrolledEventV2(
    Guid StudentId,
    Guid TenantId,
    Guid SchoolId,
    int GradeLevel,  // New required field
    DateTime EnrollmentDate  // New required field
) : TenantAwareDomainEvent;
```

Consumers handle both versions:
```csharp
public class StudentEnrollmentHandler :
    IConsumer<StudentEnrolledEvent>,
    IConsumer<StudentEnrolledEventV2>
{
    public async Task Consume(ConsumeContext<StudentEnrolledEvent> context) { /* V1 logic */ }
    public async Task Consume(ConsumeContext<StudentEnrolledEventV2> context) { /* V2 logic */ }
}
```

---

## Event Publishing Guidelines

### 1. Transactional Outbox Pattern

Events are published reliably using the Outbox pattern:

```csharp
public class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    
    public async Task<Student> CreateAsync(Student student, CancellationToken ct)
    {
        // Add entity
        _context.Students.Add(student);
        
        // Store events in outbox
        foreach (var domainEvent in student.DomainEvents)
        {
            _context.OutboxEvents.Add(new OutboxEvent
            {
                EventType = domainEvent.GetType().Name,
                EventData = JsonSerializer.Serialize(domainEvent),
                OccurredAt = domainEvent.OccurredAt
            });
        }
        
        // Commit transaction (atomic: entity + outbox)
        await _context.SaveChangesAsync(ct);
        
        // Publish events (separate process via background worker)
        await _eventPublisher.PublishAsync(student.DomainEvents, ct);
        
        student.ClearDomainEvents();
        return student;
    }
}
```

### 2. Idempotent Event Handlers

All event consumers MUST be idempotent:

```csharp
public class StudentEnrollmentHandler : IConsumer<StudentEnrolledEvent>
{
    private readonly IEnrollmentRepository _repository;
    
    public async Task Consume(ConsumeContext<StudentEnrolledEvent> context)
    {
        var @event = context.Message;
        
        // Idempotency check using EventId
        if (await _repository.EventAlreadyProcessedAsync(@event.EventId))
            return; // Skip duplicate event
        
        // Process event
        await _repository.CreateEnrollmentAsync(@event.StudentId, @event.SchoolId);
        
        // Record processed event ID
        await _repository.RecordProcessedEventAsync(@event.EventId);
    }
}
```

### 3. Event Retry and Dead Letter Queue

Configure MassTransit with exponential backoff and DLQ:

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<StudentEnrollmentHandler>();
    
    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromMinutes(5),
            intervalDelta: TimeSpan.FromSeconds(2)));
        
        cfg.UseDelayedRedelivery(r => r.Intervals(
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(30)));
        
        cfg.ConfigureEndpoints(context);
    });
});
```

---

## Event Monitoring and Observability

### Distributed Tracing

All events include correlation IDs for distributed tracing:

```csharp
public record StudentCreatedEvent(
    Guid StudentId,
    Guid TenantId,
    // ...other properties
    Guid CorrelationId  // For OpenTelemetry tracing
) : TenantAwareDomainEvent;
```

### Event Logging

Log all published and consumed events:

```csharp
_logger.LogInformation(
    "Published {EventType} with EventId {EventId} for Student {StudentId} in Tenant {TenantId}",
    nameof(StudentCreatedEvent),
    @event.EventId,
    @event.StudentId,
    @event.TenantId);
```

### Metrics

Track event throughput and latency:

```csharp
// Prometheus metrics
_eventPublishedCounter.WithLabels(eventType, tenantId).Inc();
_eventProcessingDuration.WithLabels(eventType).Observe(duration);
_eventFailureCounter.WithLabels(eventType, errorType).Inc();
```

---

## Testing Domain Events

### Unit Tests

Test event creation and properties:

```csharp
[Fact]
public void StudentCreatedEvent_Should_Contain_Required_Properties()
{
    // Arrange
    var eventId = Guid.NewGuid();
    var studentId = Guid.NewGuid();
    var tenantId = Guid.NewGuid();
    var occurredAt = new DateTime(2025, 11, 20, 12, 0, 0, DateTimeKind.Utc);
    
    // Act
    var @event = new StudentCreatedEvent(
        EventId: eventId,
        StudentId: studentId,
        TenantId: tenantId,
        SchoolId: Guid.NewGuid(),
        GradeLevel: 5,
        FirstName: "John",
        LastName: "Doe",
        OccurredAt: occurredAt
    );
    
    // Assert
    @event.EventId.Should().Be(eventId);
    @event.OccurredAt.Should().Be(occurredAt);
    @event.StudentId.Should().Be(studentId);
    @event.TenantId.Should().Be(tenantId);
}
```

### Integration Tests

Test event publishing and consumption:

```csharp
[Fact]
public async Task Should_Publish_And_Consume_StudentCreatedEvent()
{
    // Arrange
    var harness = await _testHarness.StartAsync();
    
    // Act
    await _bus.Publish(new StudentCreatedEvent(
        EventId: Guid.NewGuid(),
        StudentId: Guid.NewGuid(),
        TenantId: Guid.NewGuid(),
        SchoolId: Guid.NewGuid(),
        GradeLevel: 5,
        FirstName: "Jane",
        LastName: "Smith",
        OccurredAt: DateTime.UtcNow
    ));
    
    // Assert
    (await harness.Published.Any<StudentCreatedEvent>()).Should().BeTrue();
    (await harness.Consumed.Any<StudentCreatedEvent>()).Should().BeTrue();
}
```

---

## Constitutional Compliance

This schema standard ensures compliance with:

- **Principle 4: Event-Driven Data Discipline** - Async events preferred, idempotent commands
- **Principle 1: Clean Architecture** - Domain events in Domain layer, publishing in Infrastructure
- **Multi-Tenant Isolation** - Required `TenantId` in all business events

---

## References

- [Domain-Driven Design: Events](https://www.domainlanguage.com/ddd/reference/)
- [MassTransit Documentation](https://masstransit-project.com/)
- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Transactional Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
- [Constitution: Principle 4 - Event-Driven Data Discipline](../../.specify/memory/constitution.md)

---

**Version**: 1.0  
**Last Updated**: 2025-11-20  
**Status**: Specification Complete, Ready for Implementation
