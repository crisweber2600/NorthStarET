# Foundation Layer: Shared Application

**Purpose**: Shared application layer contracts (DTOs, interfaces, CQRS abstractions) used across all Foundation services.

**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Overview

The `Application` project provides common application-layer abstractions that ensure consistency across all Foundation services:

- **CQRS Contracts**: `ICommand`, `IQuery<TResult>`, handler interfaces
- **Common DTOs**: `PagedResult<T>`, `ApiResponse<T>`, result wrappers
- **Current User Service**: Access to authenticated user context
- **Tenant Context**: Access to current tenant (district) context
- **Common Interfaces**: Shared service contracts

**Consumed By**: All Foundation service application projects (Identity.Application, Student.Application, etc.)

---

## Key Components

### 1. CQRS Abstractions

Marker interfaces for MediatR commands and queries:

```csharp
// Commands (modify state, no return value or simple result)
public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

// Queries (read-only, return data)
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

// Handlers
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

**Example Command**:

```csharp
public record CreateStudentCommand(
    Guid TenantId,
    string FirstName,
    string LastName,
    string Email
) : ICommand<Guid>;

public class CreateStudentCommandHandler : ICommandHandler<CreateStudentCommand, Guid>
{
    private readonly IStudentRepository _repository;
    private readonly ICurrentUserService _currentUser;
    
    public async Task<Result<Guid>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Create student entity, publish domain events, etc.
    }
}
```

### 2. Result Pattern

Functional result type for error handling without exceptions:

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
    }
    
    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    
    protected Result(T value, bool isSuccess, string error) : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static Result<T> Success(T value) => new(value, true, string.Empty);
    public static Result<T> Failure(string error) => new(default, false, error);
}
```

### 3. Current User Service

Interface for accessing authenticated user context:

```csharp
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
}
```

Implementation resides in `Identity.Infrastructure` and reads from JWT claims.

### 4. Tenant Context

Interface for accessing current tenant (district) context:

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantName { get; }
    bool IsTenantSet { get; }
    void SetTenant(Guid tenantId, string tenantName);
    void ClearTenant();
}
```

Tenant context is set from:
- HTTP request headers (`X-Tenant-Id`)
- JWT claims (`tenant_id`)
- Session storage

EF Core interceptors use `ITenantContext` to apply automatic tenant filtering (Row-Level Security).

### 5. Common DTOs

Reusable data transfer objects:

```csharp
// Paged result for list queries
public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// API response wrapper
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T Data { get; init; }
    public string ErrorMessage { get; init; }
    public IReadOnlyCollection<string> ValidationErrors { get; init; }
    
    public static ApiResponse<T> SuccessResponse(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> ErrorResponse(string error) => new() { Success = false, ErrorMessage = error };
}
```

### 6. Common Interfaces

Shared service contracts:

```csharp
// Email service (implemented in Infrastructure)
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

// Notification service (implemented in Infrastructure)
public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string message, CancellationToken cancellationToken = default);
}

// Cache service (implemented in Infrastructure)
public interface ICacheService
{
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
```

---

## Project Structure

```
Application/
├── Application.csproj                # Project file
├── Abstractions/
│   ├── ICommand.cs                   # Command interface
│   ├── IQuery.cs                     # Query interface
│   ├── ICommandHandler.cs            # Command handler interface
│   └── IQueryHandler.cs              # Query handler interface
├── Common/
│   ├── Result.cs                     # Result monad
│   ├── PagedResult.cs                # Paged result DTO
│   └── ApiResponse.cs                # API response wrapper
├── Interfaces/
│   ├── ICurrentUserService.cs        # Current user context
│   ├── ITenantContext.cs             # Tenant context
│   ├── IEmailService.cs              # Email abstraction
│   ├── INotificationService.cs       # Notification abstraction
│   └── ICacheService.cs              # Cache abstraction
└── README.md                         # This file
```

---

## Usage in Services

Services implement application logic using shared abstractions:

```csharp
// In Student.Application project
public record GetStudentQuery(Guid StudentId) : IQuery<StudentDto>;

public class GetStudentQueryHandler : IQueryHandler<GetStudentQuery, StudentDto>
{
    private readonly IStudentRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;
    
    public async Task<Result<StudentDto>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        // Check cache first
        var cached = await _cache.GetAsync<StudentDto>($"student:{request.StudentId}", cancellationToken);
        if (cached != null) return Result<StudentDto>.Success(cached);
        
        // Query repository with automatic tenant filtering
        var student = await _repository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null) return Result<StudentDto>.Failure("Student not found");
        
        // Map to DTO and cache
        var dto = MapToDto(student);
        await _cache.SetAsync($"student:{request.StudentId}", dto, TimeSpan.FromMinutes(5), cancellationToken);
        
        return Result<StudentDto>.Success(dto);
    }
}
```

---

## Dependencies

- `MediatR` - CQRS command/query handling
- `FluentValidation` - Input validation (optional)
- No other external dependencies (interface contracts only)

---

## Design Principles

### CQRS Pattern

- **Commands**: Modify state (CreateStudentCommand, UpdateStudentCommand, DeleteStudentCommand)
- **Queries**: Read state (GetStudentQuery, ListStudentsQuery)
- Separation enables independent scaling of reads/writes

### Dependency Inversion

- Application layer defines interfaces (`IEmailService`, `ICurrentUserService`)
- Infrastructure layer provides implementations
- No application dependency on infrastructure (Clean Architecture)

### Result Pattern

- Avoids exceptions for expected failures (validation, not found, etc.)
- Explicit error handling at call site
- Performance benefit (no stack unwinding)

---

## Multi-Tenancy Support

All queries/commands automatically filter by tenant via `ITenantContext`:

```csharp
public class ListStudentsQueryHandler : IQueryHandler<ListStudentsQuery, PagedResult<StudentDto>>
{
    private readonly IStudentRepository _repository;
    private readonly ITenantContext _tenantContext; // Injected by DI
    
    public async Task<Result<PagedResult<StudentDto>>> Handle(ListStudentsQuery request, CancellationToken cancellationToken)
    {
        // Repository automatically filters by _tenantContext.TenantId via EF Core interceptor
        var students = await _repository.ListAsync(request.Page, request.PageSize, cancellationToken);
        // ...
    }
}
```

---

## References

- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR](https://github.com/jbogard/MediatR)
- [Constitution: Principle 1 - Clean Architecture](../../../../.specify/memory/constitution.md)
- [Constitution: Principle 4 - Event-Driven Data Discipline](../../../../.specify/memory/constitution.md)
- [LAYERS.md: Shared Infrastructure - Application Contracts](../../../../Plan/Foundation/LAYERS.md#application-contracts)

---

**Status**: To Be Implemented (Phase 1 - Weeks 1-2)  
**Related Spec**: [001-phase1-foundation-services](../../../../Plan/Foundation/specs/Foundation/001-phase1-foundation-services/)
