# Foundation Layer: Shared Domain

**Purpose**: Shared domain primitives (value objects, base entities, domain events) used across all Foundation services.

**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Overview

The `Domain` project provides common domain-driven design (DDD) building blocks that ensure consistency across all Foundation services:

- **Base Entities**: Abstract entity classes with identity and audit fields
- **Value Objects**: Immutable value types with equality semantics
- **Domain Events**: Base classes for domain event publishing
- **Common Enumerations**: Shared enums (roles, statuses, etc.)

**Consumed By**: All Foundation service domain projects (Identity.Domain, Student.Domain, Assessment.Domain, etc.)

---

## Key Components

### 1. Base Entities

Abstract base classes for entities:

```csharp
public abstract class EntityBase<TId>
{
    public TId Id { get; protected set; }
    public TenantId TenantId { get; protected set; } // Multi-tenancy
    public DateTime CreatedAt { get; protected set; }
    public UserId CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public UserId? UpdatedBy { get; protected set; }
    public DateTime? DeletedAt { get; protected set; } // Soft delete
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### 2. Value Objects

Immutable value types with structural equality:

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object obj) { /* ... */ }
    public override int GetHashCode() { /* ... */ }
    public static bool operator ==(ValueObject left, ValueObject right) { /* ... */ }
    public static bool operator !=(ValueObject left, ValueObject right) { /* ... */ }
}

public class EmailAddress : ValueObject
{
    public string Value { get; }
    
    private EmailAddress(string value) => Value = value;
    
    public static Result<EmailAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<EmailAddress>.Failure("Email cannot be empty");
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result<EmailAddress>.Failure("Invalid email format");
        return Result<EmailAddress>.Success(new EmailAddress(value));
    }
    
    protected override IEnumerable<object> GetEqualityComponents() { yield return Value; }
}
```

### 3. Domain Events

Base class for domain events (published via MassTransit):

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

// Example domain event
public record StudentEnrolledEvent(
    Guid StudentId,
    Guid TenantId,
    string FirstName,
    string LastName,
    DateTime EnrolledAt
) : DomainEvent;
```

### 4. Common Value Objects

Pre-built value objects used across services:

```csharp
public class TenantId : ValueObject { /* GUID wrapper for district/tenant */ }
public class UserId : ValueObject { /* GUID wrapper for user identity */ }
public class PhoneNumber : ValueObject { /* Phone validation and formatting */ }
public class Address : ValueObject { /* Street, city, state, zip */ }
public class DateRange : ValueObject { /* Start/end date with validation */ }
```

### 5. Common Enumerations

Shared enumerations:

```csharp
public enum UserRole
{
    SuperAdmin,
    DistrictAdmin,
    SchoolAdmin,
    Teacher,
    Parent,
    Student
}

public enum TenantStatus
{
    Active,
    Suspended,
    Archived
}

public enum RecordStatus
{
    Active,
    Inactive,
    Deleted
}
```

---

## Project Structure

```
Domain/
├── Domain.csproj                     # Project file
├── Common/
│   ├── EntityBase.cs                 # Base entity class
│   ├── ValueObject.cs                # Base value object class
│   ├── DomainEvent.cs                # Base domain event
│   └── Result.cs                     # Result monad for error handling
├── ValueObjects/
│   ├── TenantId.cs
│   ├── UserId.cs
│   ├── EmailAddress.cs
│   ├── PhoneNumber.cs
│   ├── Address.cs
│   └── DateRange.cs
├── Enumerations/
│   ├── UserRole.cs
│   ├── TenantStatus.cs
│   └── RecordStatus.cs
└── README.md                         # This file
```

---

## Usage in Service Domains

Services inherit from shared domain primitives:

```csharp
// In Student.Domain project
public class Student : EntityBase<Guid>
{
    public EmailAddress Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Address Address { get; private set; }
    
    public static Result<Student> Create(
        TenantId tenantId,
        EmailAddress email,
        PhoneNumber phone,
        UserId createdBy)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            Phone = phone,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        
        student.AddDomainEvent(new StudentEnrolledEvent(
            student.Id,
            tenantId.Value,
            student.FirstName,
            student.LastName,
            DateTime.UtcNow));
        
        return Result<Student>.Success(student);
    }
}
```

---

## Multi-Tenancy Support

All entities must include `TenantId` for Row-Level Security (RLS):

```csharp
public abstract class EntityBase<TId>
{
    public TenantId TenantId { get; protected set; } // Required for multi-tenancy
    // ... other fields
}
```

EF Core infrastructure applies automatic tenant filtering via interceptors (see `shared/Infrastructure/`).

---

## Dependencies

- `System.ComponentModel.DataAnnotations` - Validation attributes
- No external dependencies (pure domain logic)

---

## Design Principles

### DDD Tactical Patterns

- **Entities**: Identity-based equality (via `EntityBase<TId>`)
- **Value Objects**: Structural equality (via `ValueObject`)
- **Domain Events**: Record state changes for eventual consistency
- **Aggregates**: Enforce invariants, publish events on state transitions

### Immutability

- Value objects are immutable (init-only properties)
- Domain events are immutable (record types)
- Entity state changes via methods, not public setters

### Validation

- Value object creation validates inputs (factory methods return `Result<T>`)
- Entities enforce business rules in constructors/methods
- No invalid state representable in the domain model

---

## References

- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Constitution: Principle 1 - Clean Architecture](../../../../.specify/memory/constitution.md)
- [Constitution: Principle 4 - Event-Driven Data Discipline](../../../../.specify/memory/constitution.md)
- [LAYERS.md: Shared Infrastructure - Domain Primitives](../../../../Plan/Foundation/LAYERS.md#domain-primitives)

---

**Status**: To Be Implemented (Phase 1 - Weeks 1-2)  
**Related Spec**: [001-phase1-foundation-services](../../../../Plan/Foundation/specs/Foundation/001-phase1-foundation-services/)
