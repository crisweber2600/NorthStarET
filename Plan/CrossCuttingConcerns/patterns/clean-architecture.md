# Clean Architecture Pattern

**Constitution Principle**: Principle 1 - Clean Architecture & Aspire Orchestration  
**Priority**: 🟠 High  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Layer Organization](#layer-organization)
- [Domain Layer Patterns](#domain-layer-patterns)
- [Application Layer Patterns](#application-layer-patterns)
- [Infrastructure Layer Patterns](#infrastructure-layer-patterns)
- [API Layer Patterns](#api-layer-patterns)
- [Dependency Rules](#dependency-rules)
- [CQRS with MediatR](#cqrs-with-mediatr)
- [Testing Patterns](#testing-patterns)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

Clean Architecture is the foundational design pattern mandated by Constitution Principle 1 for all NorthStar LMS services. It enforces separation of concerns through concentric layers with strict dependency rules pointing inward.

**Core Tenets**:
1. **Independence**: Domain logic independent of frameworks, UI, databases
2. **Testability**: Business rules testable without external dependencies
3. **Dependency Inversion**: Outer layers depend on inner layers, never reverse
4. **Single Responsibility**: Each layer has one reason to change

**Why Clean Architecture?**
- Enables database-per-service pattern (swap PostgreSQL for another DB)
- Allows UI migration (AngularJS → Razor Pages → Blazor) without touching domain
- Facilitates testing (unit test domain without infrastructure)
- Supports long-term maintainability (16-year LMS lifecycle)

---

## Core Concepts

### The Dependency Rule

**"Dependencies point inward"** - Outer layers can depend on inner layers, but inner layers NEVER depend on outer layers.

```
┌─────────────────────────────────────────┐
│         API Layer (Presentation)        │ ← Controllers, Middleware, DTOs
├─────────────────────────────────────────┤
│       Infrastructure Layer              │ ← EF Core, MassTransit, Redis
├─────────────────────────────────────────┤
│        Application Layer                │ ← Commands, Queries, Handlers (MediatR)
├─────────────────────────────────────────┤
│          Domain Layer (Core)            │ ← Entities, Value Objects, Events
└─────────────────────────────────────────┘
       ↓ Dependencies point INWARD ↓
```

**Dependency Flow**:
- API → Application → Domain ✅
- API → Infrastructure ✅
- Infrastructure → Application ✅
- Infrastructure → Domain ✅
- Application → Domain ✅
- Domain → (nothing) ✅
- Domain → Infrastructure ❌ FORBIDDEN
- Domain → Application ❌ FORBIDDEN
- Application → API ❌ FORBIDDEN

---

## Layer Organization

### Standard Service Structure

```
Student.API/                              # API Layer (Controllers, Middleware)
├── Controllers/
│   ├── StudentsController.cs            # REST endpoints
│   └── StudentDashboardController.cs
├── Middleware/
│   ├── ExceptionHandlerMiddleware.cs
│   └── TenantResolutionMiddleware.cs
├── Program.cs                            # Service startup
├── appsettings.json
└── Student.API.csproj

Student.Application/                      # Application Layer (Use Cases)
├── Commands/
│   ├── CreateStudentCommand.cs          # Write operations
│   ├── CreateStudentCommandHandler.cs
│   ├── UpdateStudentCommand.cs
│   └── WithdrawStudentCommand.cs
├── Queries/
│   ├── GetStudentByIdQuery.cs           # Read operations
│   ├── GetStudentByIdQueryHandler.cs
│   ├── GetStudentsByGradeQuery.cs
│   └── GetStudentDashboardQuery.cs
├── DTOs/
│   ├── StudentDto.cs                    # Data transfer objects
│   ├── StudentEnrollmentDto.cs
│   └── StudentDashboardDto.cs
├── Validators/
│   ├── CreateStudentCommandValidator.cs # FluentValidation
│   └── UpdateStudentCommandValidator.cs
├── Interfaces/
│   └── IStudentRepository.cs            # Repository abstraction
├── DependencyInjection.cs               # Application layer DI
└── Student.Application.csproj

Student.Domain/                           # Domain Layer (Business Logic)
├── Entities/
│   ├── Student.cs                       # Aggregate root
│   ├── StudentEnrollment.cs
│   ├── StudentDemographics.cs
│   └── StudentContact.cs
├── Events/
│   ├── StudentCreatedEvent.cs           # Domain events
│   ├── StudentEnrolledEvent.cs
│   ├── StudentWithdrawnEvent.cs
│   └── StudentGradePromotedEvent.cs
├── ValueObjects/
│   ├── StudentId.cs                     # Strongly-typed IDs
│   ├── Grade.cs
│   └── EnrollmentStatus.cs
├── Exceptions/
│   ├── StudentNotFoundException.cs
│   └── InvalidEnrollmentException.cs
└── Student.Domain.csproj

Student.Infrastructure/                   # Infrastructure Layer (External Concerns)
├── Data/
│   ├── StudentDbContext.cs              # EF Core DbContext
│   ├── Repositories/
│   │   └── StudentRepository.cs         # Repository implementation
│   ├── Configurations/
│   │   ├── StudentConfiguration.cs      # EF entity configuration
│   │   └── StudentEnrollmentConfiguration.cs
│   ├── Interceptors/
│   │   ├── TenantInterceptor.cs         # Multi-tenancy
│   │   └── AuditInterceptor.cs          # Audit logging
│   └── Migrations/                      # EF Core migrations
├── Caching/
│   └── RedisCacheService.cs             # Redis integration
├── Messaging/
│   ├── EventPublisher.cs                # MassTransit publisher
│   └── Consumers/
│       └── SectionCreatedEventConsumer.cs
├── DependencyInjection.cs               # Infrastructure layer DI
└── Student.Infrastructure.csproj
```

---

## Domain Layer Patterns

### Pattern 1: Aggregate Root

The Domain layer contains business entities with behavior, not just data containers.

```csharp
// Student.Domain/Entities/Student.cs
using NorthStar.SharedKernel.Domain;

namespace NorthStar.Student.Domain.Entities;

/// <summary>
/// Student aggregate root - manages student lifecycle and enrollment
/// </summary>
public sealed class Student : AggregateRoot
{
    // Private setters enforce invariants (can only change through methods)
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public Grade Grade { get; private set; } // Value object
    public EnrollmentStatus Status { get; private set; } // Value object
    public Guid DistrictId { get; private set; } // Tenant isolation
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    private readonly List<StudentEnrollment> _enrollments = new();
    public IReadOnlyCollection<StudentEnrollment> Enrollments => _enrollments.AsReadOnly();

    private readonly List<StudentDemographics> _demographics = new();
    public IReadOnlyCollection<StudentDemographics> Demographics => _demographics.AsReadOnly();

    // Domain events collection (published after commit)
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor for EF Core
    private Student() { }

    // Factory method enforces business rules
    public static Student Create(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Grade grade,
        Guid districtId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));
        if (dateOfBirth > DateTime.UtcNow.AddYears(-4))
            throw new ArgumentException("Student must be at least 4 years old", nameof(dateOfBirth));

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Grade = grade,
            Status = EnrollmentStatus.Active,
            DistrictId = districtId,
            CreatedAt = DateTime.UtcNow
        };

        // Raise domain event
        student.AddDomainEvent(new StudentCreatedEvent(
            student.Id,
            student.FirstName,
            student.LastName,
            student.Grade.Value,
            student.DistrictId,
            student.CreatedAt));

        return student;
    }

    // Business logic methods
    public void Enroll(Guid schoolId, DateTime enrollDate)
    {
        if (Status == EnrollmentStatus.Withdrawn)
            throw new InvalidOperationException("Cannot enroll withdrawn student");

        var enrollment = new StudentEnrollment(Id, schoolId, enrollDate);
        _enrollments.Add(enrollment);

        AddDomainEvent(new StudentEnrolledEvent(Id, schoolId, enrollDate, Grade.Value));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Withdraw(DateTime withdrawDate, string reason)
    {
        if (Status == EnrollmentStatus.Withdrawn)
            throw new InvalidOperationException("Student already withdrawn");

        Status = EnrollmentStatus.Withdrawn;
        UpdatedAt = DateTime.UtcNow;

        // Close current enrollment
        var currentEnrollment = _enrollments.FirstOrDefault(e => !e.WithdrawDate.HasValue);
        currentEnrollment?.Withdraw(withdrawDate);

        AddDomainEvent(new StudentWithdrawnEvent(Id, withdrawDate, reason));
    }

    public void PromoteGrade()
    {
        if (Grade.Value >= 12)
            throw new InvalidOperationException("Cannot promote beyond grade 12");

        var oldGrade = Grade.Value;
        Grade = Grade.FromValue(Grade.Value + 1);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StudentGradePromotedEvent(Id, oldGrade, Grade.Value, DateTime.UtcNow));
    }

    public void UpdateDemographics(string gender, string ethnicity, bool isEll, bool isSped)
    {
        var demographics = new StudentDemographics(Id, gender, ethnicity, isEll, isSped);
        _demographics.Add(demographics);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StudentDemographicsChangedEvent(
            Id,
            new Dictionary<string, object>
            {
                ["Gender"] = gender,
                ["Ethnicity"] = ethnicity,
                ["IsELL"] = isEll,
                ["IsSPED"] = isSped
            },
            Guid.Empty, // ChangedBy user ID (passed from Application layer)
            DateTime.UtcNow));
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Key Patterns**:
- Private setters enforce invariants
- Factory methods validate construction
- Business logic methods (Enroll, Withdraw) enforce rules
- Domain events raised for significant state changes
- No persistence concerns (no SaveChanges, no repository calls)

### Pattern 2: Value Objects

Value objects represent concepts with no identity, compared by value.

```csharp
// Student.Domain/ValueObjects/Grade.cs
namespace NorthStar.Student.Domain.ValueObjects;

/// <summary>
/// Grade level value object (K-12)
/// </summary>
public sealed class Grade : ValueObject
{
    public int Value { get; private set; }

    private Grade() { }

    private Grade(int value)
    {
        Value = value;
    }

    public static Grade FromValue(int value)
    {
        if (value < -1 || value > 12)
            throw new ArgumentException("Grade must be between -1 (Pre-K) and 12", nameof(value));

        return new Grade(value);
    }

    // Predefined grade instances
    public static Grade PreKindergarten => new(-1);
    public static Grade Kindergarten => new(0);
    public static Grade FirstGrade => new(1);
    // ... etc

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value switch
    {
        -1 => "Pre-K",
        0 => "Kindergarten",
        _ => $"Grade {Value}"
    };
}
```

```csharp
// Student.Domain/ValueObjects/EnrollmentStatus.cs
namespace NorthStar.Student.Domain.ValueObjects;

/// <summary>
/// Enrollment status value object (Active, Inactive, Withdrawn)
/// </summary>
public sealed class EnrollmentStatus : ValueObject
{
    public string Value { get; private set; }

    private EnrollmentStatus() { }

    private EnrollmentStatus(string value)
    {
        Value = value;
    }

    public static EnrollmentStatus Active => new("Active");
    public static EnrollmentStatus Inactive => new("Inactive");
    public static EnrollmentStatus Withdrawn => new("Withdrawn");

    public static EnrollmentStatus FromString(string value)
    {
        return value switch
        {
            "Active" => Active,
            "Inactive" => Inactive,
            "Withdrawn" => Withdrawn,
            _ => throw new ArgumentException($"Invalid enrollment status: {value}", nameof(value))
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
```

### Pattern 3: Domain Events

Domain events represent things that have happened in the domain.

```csharp
// Student.Domain/Events/StudentCreatedEvent.cs
using NorthStar.SharedKernel.Domain;

namespace NorthStar.Student.Domain.Events;

/// <summary>
/// Raised when a new student is created in the system
/// </summary>
public sealed record StudentCreatedEvent(
    Guid StudentId,
    string FirstName,
    string LastName,
    int Grade,
    Guid DistrictId,
    DateTime CreatedAt
) : IDomainEvent;
```

```csharp
// Student.Domain/Events/StudentEnrolledEvent.cs
public sealed record StudentEnrolledEvent(
    Guid StudentId,
    Guid SchoolId,
    DateTime EnrollmentDate,
    int Grade
) : IDomainEvent;
```

```csharp
// Student.Domain/Events/StudentWithdrawnEvent.cs
public sealed record StudentWithdrawnEvent(
    Guid StudentId,
    DateTime WithdrawDate,
    string Reason
) : IDomainEvent;
```

**Key Patterns**:
- Use C# records for immutability
- Past tense naming (StudentCreated, not CreateStudent)
- Include all context needed by subscribers
- Implement IDomainEvent marker interface

---

## Application Layer Patterns

### Pattern 1: CQRS Commands

Commands represent write operations (state changes).

```csharp
// Student.Application/Commands/CreateStudentCommand.cs
using MediatR;
using NorthStar.SharedKernel.Application;

namespace NorthStar.Student.Application.Commands;

/// <summary>
/// Command to create a new student
/// </summary>
public sealed record CreateStudentCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    int Grade,
    Guid DistrictId
) : IRequest<Result<Guid>>; // Returns student ID wrapped in Result
```

```csharp
// Student.Application/Commands/CreateStudentCommandHandler.cs
using MediatR;
using NorthStar.Student.Domain.Entities;
using NorthStar.Student.Domain.ValueObjects;
using NorthStar.Student.Application.Interfaces;
using NorthStar.SharedKernel.Application;

namespace NorthStar.Student.Application.Commands;

public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStudentCommandHandler(
        IStudentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create domain entity (validates business rules)
            var grade = Grade.FromValue(request.Grade);
            var student = Domain.Entities.Student.Create(
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                grade,
                request.DistrictId);

            // Persist via repository
            await _repository.AddAsync(student, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Domain events published automatically by UnitOfWork

            return Result<Guid>.Success(student.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
```

### Pattern 2: CQRS Queries

Queries represent read operations (no state changes).

```csharp
// Student.Application/Queries/GetStudentByIdQuery.cs
using MediatR;
using NorthStar.Student.Application.DTOs;
using NorthStar.SharedKernel.Application;

namespace NorthStar.Student.Application.Queries;

/// <summary>
/// Query to retrieve student by ID
/// </summary>
public sealed record GetStudentByIdQuery(Guid StudentId) : IRequest<Result<StudentDto>>;
```

```csharp
// Student.Application/Queries/GetStudentByIdQueryHandler.cs
using MediatR;
using NorthStar.Student.Application.Interfaces;
using NorthStar.Student.Application.DTOs;
using NorthStar.SharedKernel.Application;

namespace NorthStar.Student.Application.Queries;

public sealed class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    private readonly IStudentRepository _repository;

    public GetStudentByIdQueryHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);

        if (student is null)
        {
            return Result<StudentDto>.Failure($"Student with ID {request.StudentId} not found");
        }

        // Map to DTO (Application layer responsibility)
        var dto = new StudentDto(
            student.Id,
            student.FirstName,
            student.LastName,
            student.DateOfBirth,
            student.Grade.Value,
            student.Status.Value,
            student.DistrictId,
            student.CreatedAt,
            student.UpdatedAt);

        return Result<StudentDto>.Success(dto);
    }
}
```

### Pattern 3: FluentValidation

Validation logic in Application layer using FluentValidation.

```csharp
// Student.Application/Validators/CreateStudentCommandValidator.cs
using FluentValidation;
using NorthStar.Student.Application.Commands;

namespace NorthStar.Student.Application.Validators;

public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-4)).WithMessage("Student must be at least 4 years old")
            .GreaterThan(DateTime.UtcNow.AddYears(-25)).WithMessage("Student cannot be older than 25");

        RuleFor(x => x.Grade)
            .InclusiveBetween(-1, 12).WithMessage("Grade must be between Pre-K (-1) and 12");

        RuleFor(x => x.DistrictId)
            .NotEmpty().WithMessage("District ID is required");
    }
}
```

**Validation Registration** (in Application `DependencyInjection.cs`):
```csharp
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>))); // Validates all commands
```

### Pattern 4: Repository Interface

Application layer defines repository interface, Infrastructure implements.

```csharp
// Student.Application/Interfaces/IStudentRepository.cs
using NorthStar.Student.Domain.Entities;

namespace NorthStar.Student.Application.Interfaces;

/// <summary>
/// Student repository contract (implementation in Infrastructure layer)
/// </summary>
public interface IStudentRepository
{
    Task<Domain.Entities.Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Student>> GetByGradeAsync(int grade, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Entities.Student>> GetByDistrictAsync(Guid districtId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Entities.Student student, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.Entities.Student student, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

---

## Infrastructure Layer Patterns

### Pattern 1: EF Core DbContext

```csharp
// Student.Infrastructure/Data/StudentDbContext.cs
using Microsoft.EntityFrameworkCore;
using NorthStar.Student.Domain.Entities;
using NorthStar.Student.Infrastructure.Data.Interceptors;

namespace NorthStar.Student.Infrastructure.Data;

public sealed class StudentDbContext : DbContext
{
    public DbSet<Domain.Entities.Student> Students => Set<Domain.Entities.Student>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<StudentDemographics> StudentDemographics => Set<StudentDemographics>();

    public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudentDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add interceptors for multi-tenancy and audit logging
        optionsBuilder.AddInterceptors(
            new TenantInterceptor(),
            new AuditInterceptor());
    }
}
```

### Pattern 2: Entity Configuration (Fluent API)

```csharp
// Student.Infrastructure/Data/Configurations/StudentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NorthStar.Student.Domain.Entities;
using NorthStar.Student.Domain.ValueObjects;

namespace NorthStar.Student.Infrastructure.Data.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Domain.Entities.Student>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DateOfBirth)
            .IsRequired();

        // Value object conversion
        builder.Property(s => s.Grade)
            .HasConversion(
                grade => grade.Value,
                value => Grade.FromValue(value))
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion(
                status => status.Value,
                value => EnrollmentStatus.FromString(value))
            .IsRequired()
            .HasMaxLength(20);

        // Multi-tenancy
        builder.Property(s => s.DistrictId)
            .IsRequired();

        builder.HasIndex(s => s.DistrictId);

        // Global query filter for tenant isolation
        builder.HasQueryFilter(s => EF.Property<Guid>(s, "DistrictId") == TenantContext.Current.DistrictId);

        // Audit fields
        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        // Navigation properties
        builder.HasMany(s => s.Enrollments)
            .WithOne()
            .HasForeignKey("StudentId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Demographics)
            .WithOne()
            .HasForeignKey("StudentId")
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events (not persisted)
        builder.Ignore(s => s.DomainEvents);
    }
}
```

### Pattern 3: Repository Implementation

```csharp
// Student.Infrastructure/Data/Repositories/StudentRepository.cs
using Microsoft.EntityFrameworkCore;
using NorthStar.Student.Application.Interfaces;
using NorthStar.Student.Domain.Entities;

namespace NorthStar.Student.Infrastructure.Data.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly StudentDbContext _context;

    public StudentRepository(StudentDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.Enrollments)
            .Include(s => s.Demographics)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Student>> GetByGradeAsync(int grade, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Where(s => s.Grade.Value == grade)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Student>> GetByDistrictAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        // Global query filter already applies DistrictId filter
        return await _context.Students
            .Include(s => s.Enrollments)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Domain.Entities.Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
    }

    public Task UpdateAsync(Domain.Entities.Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await GetByIdAsync(id, cancellationToken);
        if (student is not null)
        {
            _context.Students.Remove(student);
        }
    }
}
```

### Pattern 4: Unit of Work

```csharp
// Student.Infrastructure/Data/UnitOfWork.cs
using NorthStar.Student.Application.Interfaces;
using NorthStar.SharedKernel.Infrastructure.Messaging;

namespace NorthStar.Student.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly StudentDbContext _context;
    private readonly IDomainEventPublisher _eventPublisher;

    public UnitOfWork(StudentDbContext context, IDomainEventPublisher eventPublisher)
    {
        _context = context;
        _eventPublisher = eventPublisher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from all tracked entities
        var domainEvents = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        // Save changes to database
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Publish domain events AFTER successful commit
        foreach (var domainEvent in domainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        // Clear domain events from entities
        foreach (var entry in _context.ChangeTracker.Entries<AggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}
```

---

## API Layer Patterns

### Pattern 1: REST Controllers

```csharp
// Student.API/Controllers/StudentsController.cs
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NorthStar.Student.Application.Commands;
using NorthStar.Student.Application.Queries;
using NorthStar.Student.Application.DTOs;

namespace NorthStar.Student.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetStudent), new { id = result.Value }, result.Value);
        }

        return BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudent(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetStudentByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(result.Error);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.StudentId)
        {
            return BadRequest("Student ID mismatch");
        }

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudent(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteStudentCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return NotFound(result.Error);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentsByGrade([FromQuery] int grade, CancellationToken cancellationToken)
    {
        var query = new GetStudentsByGradeQuery(grade);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(result.Error);
    }
}
```

**Key Patterns**:
- Controllers delegate to MediatR (no business logic)
- Commands/Queries passed directly from request body
- Result pattern for error handling
- ProducesResponseType for OpenAPI documentation

### Pattern 2: Global Exception Handling

```csharp
// Student.API/Middleware/ExceptionHandlerMiddleware.cs
using System.Net;
using System.Text.Json;

namespace NorthStar.Student.API.Middleware;

public sealed class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = JsonSerializer.Serialize(new { error = exception.Message });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
```

---

## Dependency Rules

### Allowed Dependencies

```csharp
// API Layer can depend on:
using NorthStar.Student.Application; // ✅
using NorthStar.Student.Infrastructure; // ✅
using NorthStar.Student.Domain; // ✅

// Infrastructure Layer can depend on:
using NorthStar.Student.Application; // ✅
using NorthStar.Student.Domain; // ✅

// Application Layer can depend on:
using NorthStar.Student.Domain; // ✅

// Domain Layer has NO dependencies (pure business logic)
```

### Forbidden Dependencies

```csharp
// Domain Layer CANNOT depend on:
using NorthStar.Student.Application; // ❌
using NorthStar.Student.Infrastructure; // ❌
using NorthStar.Student.API; // ❌
using Microsoft.EntityFrameworkCore; // ❌
using MassTransit; // ❌

// Application Layer CANNOT depend on:
using NorthStar.Student.API; // ❌
using Microsoft.EntityFrameworkCore; // ❌ (only via interfaces)
using MassTransit; // ❌ (only via interfaces)
```

---

## CQRS with MediatR

### MediatR Registration

```csharp
// Student.Application/DependencyInjection.cs
using MediatR;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace NorthStar.Student.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register validation pipeline behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
```

### Validation Pipeline Behavior

```csharp
// Student.Application/Behaviors/ValidationBehavior.cs
using MediatR;
using FluentValidation;

namespace NorthStar.Student.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

---

## Testing Patterns

### Pattern 1: Domain Layer Tests (Pure Unit Tests)

```csharp
// tests/Unit/Student.Domain.Tests/StudentTests.cs
using NorthStar.Student.Domain.Entities;
using NorthStar.Student.Domain.ValueObjects;
using Xunit;
using FluentAssertions;

namespace NorthStar.Student.Domain.Tests;

public sealed class StudentTests
{
    [Fact]
    public void Create_ValidData_CreatesStudent()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var dateOfBirth = DateTime.UtcNow.AddYears(-10);
        var grade = Grade.FromValue(5);
        var districtId = Guid.NewGuid();

        // Act
        var student = Domain.Entities.Student.Create(firstName, lastName, dateOfBirth, grade, districtId);

        // Assert
        student.Should().NotBeNull();
        student.FirstName.Should().Be(firstName);
        student.LastName.Should().Be(lastName);
        student.Grade.Should().Be(grade);
        student.DistrictId.Should().Be(districtId);
        student.Status.Should().Be(EnrollmentStatus.Active);
        student.DomainEvents.Should().ContainSingle(e => e is StudentCreatedEvent);
    }

    [Fact]
    public void Create_EmptyFirstName_ThrowsArgumentException()
    {
        // Arrange
        var firstName = "";
        var lastName = "Doe";
        var dateOfBirth = DateTime.UtcNow.AddYears(-10);
        var grade = Grade.FromValue(5);
        var districtId = Guid.NewGuid();

        // Act
        Action act = () => Domain.Entities.Student.Create(firstName, lastName, dateOfBirth, grade, districtId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*First name is required*");
    }

    [Fact]
    public void Enroll_ValidData_AddsEnrollment()
    {
        // Arrange
        var student = CreateTestStudent();
        var schoolId = Guid.NewGuid();
        var enrollDate = DateTime.UtcNow;

        // Act
        student.Enroll(schoolId, enrollDate);

        // Assert
        student.Enrollments.Should().ContainSingle();
        student.DomainEvents.Should().Contain(e => e is StudentEnrolledEvent);
    }

    private Domain.Entities.Student CreateTestStudent()
    {
        return Domain.Entities.Student.Create(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            Grade.FromValue(5),
            Guid.NewGuid());
    }
}
```

### Pattern 2: Application Layer Tests (with Mocks)

```csharp
// tests/Unit/Student.Application.Tests/CreateStudentCommandHandlerTests.cs
using NorthStar.Student.Application.Commands;
using NorthStar.Student.Application.Interfaces;
using NorthStar.Student.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;

namespace NorthStar.Student.Application.Tests;

public sealed class CreateStudentCommandHandlerTests
{
    private readonly Mock<IStudentRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateStudentCommandHandler _handler;

    public CreateStudentCommandHandlerTests()
    {
        _repositoryMock = new Mock<IStudentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateStudentCommandHandler(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithStudentId()
    {
        // Arrange
        var command = new CreateStudentCommand(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            5,
            Guid.NewGuid());

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Student>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidGrade_ReturnsFailure()
    {
        // Arrange
        var command = new CreateStudentCommand(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            99, // Invalid grade
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Grade must be between");
    }
}
```

### Pattern 3: Integration Tests (with Real Database)

```csharp
// tests/Integration/Student.Infrastructure.Tests/StudentRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using NorthStar.Student.Infrastructure.Data;
using NorthStar.Student.Infrastructure.Data.Repositories;
using NorthStar.Student.Domain.Entities;
using NorthStar.Student.Domain.ValueObjects;
using Xunit;
using FluentAssertions;

namespace NorthStar.Student.Infrastructure.Tests;

public sealed class StudentRepositoryTests : IAsyncLifetime
{
    private StudentDbContext _context = null!;
    private StudentRepository _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StudentDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new StudentRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidStudent_PersistsToDatabase()
    {
        // Arrange
        var student = Domain.Entities.Student.Create(
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            Grade.FromValue(5),
            Guid.NewGuid());

        // Act
        await _repository.AddAsync(student);
        await _context.SaveChangesAsync();

        // Assert
        var savedStudent = await _repository.GetByIdAsync(student.Id);
        savedStudent.Should().NotBeNull();
        savedStudent!.FirstName.Should().Be("John");
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }
}
```

---

## Anti-Patterns

### ❌ Anti-Pattern 1: Domain Logic in Controllers

```csharp
// BAD: Business logic in controller
[HttpPost]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
{
    if (string.IsNullOrEmpty(request.FirstName))
        return BadRequest("First name is required");

    var student = new Student
    {
        Id = Guid.NewGuid(),
        FirstName = request.FirstName,
        // ... validation and creation logic in controller!
    };

    _context.Students.Add(student);
    await _context.SaveChangesAsync();

    return Ok(student.Id);
}
```

**✅ Correct**: Use MediatR command
```csharp
[HttpPost]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentCommand command)
{
    var result = await _mediator.Send(command);
    return result.IsSuccess ? CreatedAtAction(nameof(GetStudent), new { id = result.Value }, result.Value) : BadRequest(result.Error);
}
```

### ❌ Anti-Pattern 2: Infrastructure Dependencies in Domain

```csharp
// BAD: EF Core in Domain layer
public class Student
{
    public void Enroll(StudentDbContext context, Guid schoolId)
    {
        var enrollment = new StudentEnrollment { StudentId = Id, SchoolId = schoolId };
        context.StudentEnrollments.Add(enrollment); // DOMAIN SHOULD NOT KNOW ABOUT EF CORE!
        context.SaveChanges();
    }
}
```

**✅ Correct**: Domain raises event, Infrastructure handles persistence
```csharp
public void Enroll(Guid schoolId, DateTime enrollDate)
{
    var enrollment = new StudentEnrollment(Id, schoolId, enrollDate);
    _enrollments.Add(enrollment);
    AddDomainEvent(new StudentEnrolledEvent(Id, schoolId, enrollDate, Grade.Value));
}
```

### ❌ Anti-Pattern 3: Anemic Domain Model

```csharp
// BAD: No behavior, just getters/setters
public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Grade { get; set; }
}

// Business logic in Application layer (should be in Domain)
public class CreateStudentCommandHandler
{
    public async Task Handle(CreateStudentCommand command)
    {
        if (command.Grade < -1 || command.Grade > 12)
            throw new Exception("Invalid grade"); // VALIDATION IN WRONG LAYER!

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            // ...
        };
    }
}
```

**✅ Correct**: Rich domain model
```csharp
public class Student
{
    // Private setters + factory method enforce invariants
    public static Student Create(string firstName, string lastName, DateTime dateOfBirth, Grade grade, Guid districtId)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required");
        // Domain logic here!
    }
}
```

---

## Performance Considerations

### Query Optimization

```csharp
// Use AsNoTracking for read-only queries
public async Task<IEnumerable<StudentDto>> GetStudentsByGrade(int grade)
{
    return await _context.Students
        .AsNoTracking() // Don't track for read-only
        .Where(s => s.Grade.Value == grade)
        .Select(s => new StudentDto(s.Id, s.FirstName, s.LastName, ...))
        .ToListAsync();
}
```

### Projection (Select Only Needed Fields)

```csharp
// Project to DTO in database query (not in memory)
public async Task<IEnumerable<StudentSummaryDto>> GetStudentSummaries()
{
    return await _context.Students
        .Select(s => new StudentSummaryDto(s.Id, s.FirstName, s.LastName)) // Select only needed columns
        .ToListAsync();
}
```

---

## References

### Constitution & Architecture

- [NorthStarET Constitution v2.0.0](../../../.specify/memory/constitution.md) - Principle 1: Clean Architecture
- [LAYERS.md](../../LAYERS.md) - Layer isolation principles
- [Domain Shared README](../../../Src/Foundation/shared/Domain/README.md)
- [Application Shared README](../../../Src/Foundation/shared/Application/README.md)
- [Infrastructure Shared README](../../../Src/Foundation/shared/Infrastructure/README.md)

### Service Architectures

- [Identity Service](../architecture/services/identity-service.md)
- [Student Management Service](../architecture/services/student-management-service.md)
- [Assessment Service](../architecture/services/assessment-service.md)

### Related Patterns

- [Aspire Orchestration](./aspire-orchestration.md)
- [Dependency Injection](./dependency-injection.md)
- [Messaging & Integration](./messaging-integration.md)
- [Multi-Tenancy](./multi-tenancy.md)

---

**Last Updated**: 2025-11-20  
**Pattern Owner**: Platform Team  
**Constitution Version**: 2.2.0
