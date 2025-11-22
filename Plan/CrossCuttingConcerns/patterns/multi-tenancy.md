# Multi-Tenancy Pattern

**Constitution Principle**: Principle 4 - Event-Driven Data Discipline (Multi-Tenancy & Data Isolation)  
**Priority**: 🟠 High  
**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Table of Contents

- [Overview](#overview)
- [ITenantContext Interface](#itenantcontext-interface)
- [TenantMiddleware (JWT Claims Extraction)](#tenantmiddleware-jwt-claims-extraction)
- [ITenantEntity Interface](#itenantentity-interface)
- [EF Core Global Query Filters](#ef-core-global-query-filters)
- [TenantInterceptor (Auto-Set TenantId)](#tenantinterceptor-auto-set-tenantid)
- [PostgreSQL Row-Level Security](#postgresql-row-level-security)
- [Session Variable Management](#session-variable-management)
- [Testing Tenant Isolation](#testing-tenant-isolation)
- [Cross-Tenant Access Prevention](#cross-tenant-access-prevention)
- [Anti-Patterns](#anti-patterns)
- [Performance Considerations](#performance-considerations)
- [References](#references)

---

## Overview

NorthStar LMS consolidates hundreds of district databases into 11 multi-tenant service databases (database-per-service pattern with multi-tenant isolation). Constitution Principle 4 mandates **strict data sovereignty and tenant isolation** through multiple security layers.

**Multi-Tenancy Architecture**:
```
┌─────────────────────────────────────────────────────────────┐
│                         API Request                          │
│  Authorization: Bearer <JWT with tenant_id claim>            │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                      TenantMiddleware                        │
│  ✓ Extract tenant_id from JWT   ✓ Set ITenantContext        │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                   Application Layer (MediatR)                │
│  ✓ ITenantContext.TenantId   ✓ Tenant-scoped commands       │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                    EF Core (Infrastructure)                  │
│  ✓ Global Query Filters   ✓ TenantInterceptor               │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│                 PostgreSQL Database (RLS)                    │
│  ✓ Row-Level Security   ✓ Session Variables   ✓ Audit       │
└─────────────────────────────────────────────────────────────┘
```

**Defense in Depth - 4 Layers**:
1. **JWT Validation**: API Gateway validates tenant_id claim in token
2. **Application Layer**: ITenantContext enforces tenant scope in commands/queries
3. **ORM Layer**: EF Core global query filters + TenantInterceptor
4. **Database Layer**: PostgreSQL Row-Level Security (RLS) policies

---

## ITenantContext Interface

`ITenantContext` provides the current authenticated user's tenant (district) ID throughout the application layer.

### Interface Definition

```csharp
// Location: Src/Foundation/shared/Application/Services/ITenantContext.cs
namespace NorthStarET.Foundation.Application.Services;

/// <summary>
/// Provides the current tenant (district) context for multi-tenant operations
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The tenant ID for the current request context
    /// </summary>
    Guid TenantId { get; }
    
    /// <summary>
    /// Sets the tenant context (called by TenantMiddleware)
    /// </summary>
    void SetTenant(Guid tenantId);
}
```

### Scoped Implementation

```csharp
// Location: Src/Foundation/shared/Infrastructure/MultiTenancy/TenantContext.cs
namespace NorthStarET.Foundation.Infrastructure.MultiTenancy;

/// <summary>
/// Request-scoped implementation of ITenantContext
/// </summary>
public sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;
    
    public Guid TenantId => _tenantId 
        ?? throw new InvalidOperationException("Tenant context not set. " +
            "Ensure TenantMiddleware is registered in the request pipeline.");
    
    public void SetTenant(Guid tenantId)
    {
        if (_tenantId.HasValue && _tenantId.Value != tenantId)
        {
            throw new InvalidOperationException(
                $"Tenant context already set to {_tenantId.Value}, " +
                $"cannot change to {tenantId} within the same request.");
        }
        
        _tenantId = tenantId;
    }
}
```

### Dependency Injection Registration

```csharp
// Location: Src/Foundation/shared/Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // Register ITenantContext as scoped (one instance per HTTP request)
    services.AddScoped<ITenantContext, TenantContext>();
    
    return services;
}
```

### Usage in Command Handler

```csharp
// Location: Src/Foundation/services/Student/Application/Commands/CreateStudentCommandHandler.cs
public sealed class CreateStudentCommandHandler 
    : IRequestHandler<CreateStudentCommand, Result<Guid>>
{
    private readonly IStudentRepository _repository;
    private readonly ITenantContext _tenantContext;
    
    public CreateStudentCommandHandler(
        IStudentRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }
    
    public async Task<Result<Guid>> Handle(
        CreateStudentCommand command,
        CancellationToken cancellationToken)
    {
        // Automatically scoped to current tenant
        var tenantId = _tenantContext.TenantId;
        
        var student = Student.Create(
            firstName: command.FirstName,
            lastName: command.LastName,
            dateOfBirth: command.DateOfBirth,
            tenantId: tenantId); // Explicit tenant assignment
        
        await _repository.AddAsync(student, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return Result.Success(student.Id);
    }
}
```

---

## TenantMiddleware (JWT Claims Extraction)

`TenantMiddleware` runs early in the ASP.NET Core pipeline to extract the `tenant_id` claim from the authenticated user's JWT and populate `ITenantContext`.

### Middleware Implementation

```csharp
// Location: Src/Foundation/shared/Infrastructure/MultiTenancy/TenantMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NorthStarET.Foundation.Infrastructure.MultiTenancy;

/// <summary>
/// Extracts tenant_id from authenticated JWT claims and sets ITenantContext
/// </summary>
public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    
    public TenantMiddleware(
        RequestDelegate next,
        ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                // Extract tenant_id claim from JWT
                var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value
                    ?? throw new InvalidOperationException(
                        "Authenticated user missing tenant_id claim");
                
                if (!Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    throw new InvalidOperationException(
                        $"Invalid tenant_id claim format: {tenantIdClaim}");
                }
                
                // Set tenant context for this request
                tenantContext.SetTenant(tenantId);
                
                _logger.LogDebug(
                    "Tenant context set to {TenantId} for user {UserId}",
                    tenantId,
                    context.User.FindFirst("oid")?.Value ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set tenant context");
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_tenant_context",
                    message = "Unable to determine tenant context from authentication token"
                });
                
                return; // Short-circuit pipeline
            }
        }
        
        await _next(context);
    }
}
```

### Middleware Registration

```csharp
// Location: Src/Foundation/services/Student/Api/Program.cs
var app = builder.Build();

// Order matters: TenantMiddleware AFTER UseAuthentication()
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>(); // Extract tenant from claims
app.UseAuthorization();

app.MapControllers();
```

### Extension Method for Convenience

```csharp
// Location: Src/Foundation/shared/Infrastructure/MultiTenancy/TenantMiddlewareExtensions.cs
using Microsoft.AspNetCore.Builder;

namespace NorthStarET.Foundation.Infrastructure.MultiTenancy;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}

// Usage:
app.UseAuthentication();
app.UseTenantContext(); // Cleaner API
app.UseAuthorization();
```

---

## ITenantEntity Interface

All domain entities that belong to a tenant must implement `ITenantEntity`.

### Interface Definition

```csharp
// Location: Src/Foundation/shared/Domain/Common/ITenantEntity.cs
namespace NorthStarET.Foundation.Domain.Common;

/// <summary>
/// Marker interface for tenant-scoped entities
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Tenant (district) identifier for multi-tenant isolation
    /// </summary>
    Guid TenantId { get; set; }
}
```

### Example Domain Entity

```csharp
// Location: Src/Foundation/services/Student/Domain/Entities/Student.cs
using NorthStarET.Foundation.Domain.Common;

namespace NorthStarET.Foundation.Student.Domain.Entities;

public sealed class Student : Entity, IAggregateRoot, ITenantEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; set; } // Required by ITenantEntity
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateTime DateOfBirth { get; private set; }
    public string? StateStudentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; } // Soft delete
    
    // Private constructor for EF Core
    private Student() { }
    
    public static Student Create(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Guid tenantId)
    {
        return new Student
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### EF Core Configuration

```csharp
// Location: Src/Foundation/services/Student/Infrastructure/Persistence/Configurations/StudentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NorthStarET.Foundation.Student.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Domain.Entities.Student>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Student> builder)
    {
        builder.ToTable("students", "student");
        
        builder.HasKey(s => s.Id);
        
        // Composite index: tenant_id + id for efficient tenant-scoped queries
        builder.HasIndex(s => new { s.TenantId, s.Id })
            .HasDatabaseName("idx_students_tenant_id");
        
        // Index for soft delete queries
        builder.HasIndex(s => new { s.TenantId, s.DeletedAt })
            .HasDatabaseName("idx_students_tenant_deleted");
        
        builder.Property(s => s.TenantId)
            .IsRequired();
        
        builder.Property(s => s.FirstName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.LastName)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.DateOfBirth)
            .IsRequired();
        
        builder.Property(s => s.StateStudentId)
            .HasMaxLength(50);
        
        builder.Property(s => s.CreatedAt)
            .IsRequired();
    }
}
```

---

## EF Core Global Query Filters

Global query filters automatically apply `WHERE tenant_id = @currentTenantId` to all queries.

### DbContext with Global Filters

```csharp
// Location: Src/Foundation/services/Student/Infrastructure/Persistence/StudentDbContext.cs
using Microsoft.EntityFrameworkCore;
using NorthStarET.Foundation.Domain.Common;

namespace NorthStarET.Foundation.Student.Infrastructure.Persistence;

public sealed class StudentDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    
    public DbSet<Domain.Entities.Student> Students => Set<Domain.Entities.Student>();
    
    public StudentDbContext(
        DbContextOptions<StudentDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Apply global query filter to all ITenantEntity implementations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var tenantIdValue = Expression.Constant(_tenantContext.TenantId);
                var equals = Expression.Equal(property, tenantIdValue);
                var lambda = Expression.Lambda(equals, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
```

### Generated SQL with Filter

```sql
-- Original LINQ query:
var students = await dbContext.Students.ToListAsync();

-- Generated SQL (filter applied automatically):
SELECT s.id, s.tenant_id, s.first_name, s.last_name, s.date_of_birth
FROM student.students AS s
WHERE s.tenant_id = 'abc-123-tenant-id'; -- ✅ Automatic filter
```

### Bypassing Filters (Admin Scenarios)

```csharp
// Use IgnoreQueryFilters() ONLY for cross-tenant admin operations
var allStudents = await dbContext.Students
    .IgnoreQueryFilters() // ⚠️ Bypass tenant filter
    .Where(s => s.DeletedAt == null) // Still apply soft delete filter
    .ToListAsync();

// ⚠️ MUST log this operation for audit purposes
_logger.LogWarning(
    "Cross-tenant query executed by user {UserId} for admin operation",
    currentUserId);
```

---

## TenantInterceptor (Auto-Set TenantId)

`TenantInterceptor` automatically sets `TenantId` on new entities and prevents tenant changes on updates.

### Interceptor Implementation

```csharp
// Location: Src/Foundation/shared/Infrastructure/Persistence/Interceptors/TenantInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NorthStarET.Foundation.Domain.Common;

namespace NorthStarET.Foundation.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Automatically sets TenantId on new entities and prevents tenant changes
/// </summary>
public sealed class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TenantInterceptor> _logger;
    
    public TenantInterceptor(
        ITenantContext tenantContext,
        ILogger<TenantInterceptor> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }
    
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }
        
        var tenantId = _tenantContext.TenantId;
        
        // Set session variable for PostgreSQL RLS
        await eventData.Context.Database
            .ExecuteSqlRawAsync(
                $"SET LOCAL app.current_tenant = '{tenantId}'",
                cancellationToken);
        
        foreach (var entry in eventData.Context.ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Auto-set tenant on new entities
                    if (entry.Entity.TenantId == Guid.Empty)
                    {
                        entry.Entity.TenantId = tenantId;
                        
                        _logger.LogDebug(
                            "Auto-set TenantId={TenantId} on new {EntityType}",
                            tenantId,
                            entry.Entity.GetType().Name);
                    }
                    else if (entry.Entity.TenantId != tenantId)
                    {
                        // Security violation: attempting to create entity for different tenant
                        _logger.LogError(
                            "SECURITY VIOLATION: Attempted to create {EntityType} for tenant {AttemptedTenantId} " +
                            "while authenticated as tenant {ActualTenantId}",
                            entry.Entity.GetType().Name,
                            entry.Entity.TenantId,
                            tenantId);
                        
                        throw new InvalidOperationException(
                            $"Cannot create {entry.Entity.GetType().Name} for tenant {entry.Entity.TenantId} " +
                            $"while authenticated as tenant {tenantId}");
                    }
                    break;
                
                case EntityState.Modified:
                    // Prevent tenant_id changes
                    var originalTenantId = (Guid)entry.OriginalValues[nameof(ITenantEntity.TenantId)];
                    
                    if (entry.Entity.TenantId != originalTenantId)
                    {
                        _logger.LogError(
                            "SECURITY VIOLATION: Attempted to change TenantId from {OriginalTenantId} to {NewTenantId} " +
                            "on {EntityType}",
                            originalTenantId,
                            entry.Entity.TenantId,
                            entry.Entity.GetType().Name);
                        
                        throw new InvalidOperationException(
                            $"Cannot change TenantId on {entry.Entity.GetType().Name}. " +
                            "Tenant ownership is immutable.");
                    }
                    
                    // Mark TenantId as unmodified to prevent unnecessary UPDATE
                    entry.Property(e => e.TenantId).IsModified = false;
                    break;
            }
        }
        
        return result;
    }
}
```

### Interceptor Registration

```csharp
// Location: Src/Foundation/services/Student/Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddDbContext<StudentDbContext>((sp, options) =>
    {
        options.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
        
        // Register interceptors
        options.AddInterceptors(
            sp.GetRequiredService<TenantInterceptor>(),
            sp.GetRequiredService<AuditInterceptor>());
    });
    
    // Register interceptors as scoped
    services.AddScoped<TenantInterceptor>();
    services.AddScoped<AuditInterceptor>();
    
    return services;
}
```

---

## PostgreSQL Row-Level Security

Row-Level Security (RLS) is the **final enforcement layer** at the database level, independent of application code.

### Enable RLS on Table

```sql
-- Location: Database migrations
-- Enable RLS on students table
ALTER TABLE student.students ENABLE ROW LEVEL SECURITY;
```

### Create Tenant Isolation Policy

```sql
-- Policy: Users can only access rows matching their session tenant
CREATE POLICY tenant_isolation_policy ON student.students
    USING (tenant_id = current_setting('app.current_tenant')::uuid);

-- Policy applies to SELECT, UPDATE, DELETE
-- INSERT is handled by separate policy (see below)
```

### Insert Policy with Tenant Validation

```sql
-- Policy: Users can only insert rows for their own tenant
CREATE POLICY tenant_insert_policy ON student.students
    FOR INSERT
    WITH CHECK (tenant_id = current_setting('app.current_tenant')::uuid);

-- This prevents SQL injection attacks attempting to insert cross-tenant data
```

### Update Policy Preventing Tenant Change

```sql
-- Policy: Prevent tenant_id modifications
CREATE POLICY prevent_tenant_change_policy ON student.students
    FOR UPDATE
    USING (tenant_id = current_setting('app.current_tenant')::uuid)
    WITH CHECK (tenant_id = current_setting('app.current_tenant')::uuid);

-- USING: Which rows can be updated (must belong to current tenant)
-- WITH CHECK: Validation after update (tenant_id must remain unchanged)
```

### Admin Bypass Policy (Super User Only)

```sql
-- Policy: Database superusers bypass RLS for maintenance
CREATE POLICY admin_bypass_policy ON student.students
    USING (current_user = 'northstar_admin');

-- Only applies to postgres role with BYPASSRLS attribute
ALTER ROLE northstar_admin BYPASSRLS;
```

### Verifying RLS Policies

```sql
-- Check active policies on table
SELECT schemaname, tablename, policyname, permissive, roles, cmd, qual, with_check
FROM pg_policies
WHERE tablename = 'students';

-- Test RLS enforcement:
SET app.current_tenant = 'abc-123-tenant-a';
SELECT * FROM student.students; -- Only returns TenantA students

SET app.current_tenant = 'def-456-tenant-b';
SELECT * FROM student.students; -- Only returns TenantB students
```

---

## Session Variable Management

PostgreSQL session variables (`app.current_tenant`) are set per transaction by `TenantInterceptor`.

### Setting Session Variable

```csharp
// Automatically done by TenantInterceptor in SavingChangesAsync
await dbContext.Database.ExecuteSqlRawAsync(
    $"SET LOCAL app.current_tenant = '{tenantId}'",
    cancellationToken);

// SET LOCAL: Variable scoped to current transaction
// SET: Variable persists for entire connection (dangerous with pooling!)
```

### Connection Pooling Safety

**Problem**: Connection pooling reuses connections across requests. If we use `SET` instead of `SET LOCAL`, tenant context leaks across requests!

**Solution**: Always use `SET LOCAL` which resets after transaction commits.

```csharp
// ✅ GOOD: SET LOCAL resets after transaction
await dbContext.Database.ExecuteSqlRawAsync(
    $"SET LOCAL app.current_tenant = '{tenantId}'",
    cancellationToken);

// ❌ BAD: SET persists across pooled connections!
await dbContext.Database.ExecuteSqlRawAsync(
    $"SET app.current_tenant = '{tenantId}'",
    cancellationToken);
```

### Verifying Session Variable

```sql
-- Check current session variable value
SELECT current_setting('app.current_tenant', true);

-- Returns NULL if not set (second parameter = missing_ok)
```

---

## Testing Tenant Isolation

### Unit Tests (Mocking ITenantContext)

```csharp
// Location: tests/unit/Student.Application.Tests/Commands/CreateStudentCommandHandlerTests.cs
using Moq;

namespace NorthStarET.Foundation.Student.Application.Tests.Commands;

public sealed class CreateStudentCommandHandlerTests
{
    [Fact]
    public async Task CreateStudent_Sets_TenantId_From_Context()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockTenantContext = new Mock<ITenantContext>();
        mockTenantContext.Setup(x => x.TenantId).Returns(tenantId);
        
        var handler = new CreateStudentCommandHandler(
            repository: CreateMockRepository(),
            tenantContext: mockTenantContext.Object,
            dateTimeProvider: new FakeDateTimeProvider());
        
        var command = new CreateStudentCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: new DateTime(2010, 1, 1));
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verify student was created with correct tenant
        var student = await handler._repository.GetByIdAsync(result.Value, CancellationToken.None);
        student!.TenantId.Should().Be(tenantId);
    }
}
```

### Integration Tests (Database Isolation)

```csharp
// Location: tests/integration/Student.IntegrationTests/TenantIsolationTests.cs
using Microsoft.EntityFrameworkCore;

namespace NorthStarET.Foundation.Student.IntegrationTests;

public sealed class TenantIsolationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    
    public TenantIsolationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task Query_Only_Returns_Students_For_Current_Tenant()
    {
        // Arrange
        var dbContext = _fixture.CreateDbContext();
        
        // Seed data: 5 students in TenantA, 5 in TenantB
        await SeedStudentsAsync(dbContext, TenantA, count: 5);
        await SeedStudentsAsync(dbContext, TenantB, count: 5);
        
        // Set tenant context to TenantA
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(TenantA);
        
        var scopedDbContext = new StudentDbContext(
            _fixture.CreateDbContextOptions(),
            tenantContext);
        
        // Act
        var students = await scopedDbContext.Students.ToListAsync();
        
        // Assert
        students.Should().HaveCount(5);
        students.Should().OnlyContain(s => s.TenantId == TenantA);
    }
    
    [Fact]
    public async Task Cannot_Insert_Student_With_Different_TenantId()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(TenantA);
        
        var dbContext = new StudentDbContext(
            _fixture.CreateDbContextOptions(),
            tenantContext);
        
        var student = Domain.Entities.Student.Create(
            firstName: "John",
            lastName: "Doe",
            dateOfBirth: new DateTime(2010, 1, 1),
            tenantId: TenantB); // ⚠️ Attempting to create for different tenant
        
        dbContext.Students.Add(student);
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dbContext.SaveChangesAsync());
        
        ex.Message.Should().Contain("Cannot create");
        ex.Message.Should().Contain(TenantB.ToString());
    }
    
    [Fact]
    public async Task Cannot_Update_Student_TenantId()
    {
        // Arrange
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(TenantA);
        
        var dbContext = new StudentDbContext(
            _fixture.CreateDbContextOptions(),
            tenantContext);
        
        var student = Domain.Entities.Student.Create(
            firstName: "Jane",
            lastName: "Smith",
            dateOfBirth: new DateTime(2011, 5, 15),
            tenantId: TenantA);
        
        dbContext.Students.Add(student);
        await dbContext.SaveChangesAsync();
        
        // Attempt to change tenant
        student.TenantId = TenantB;
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dbContext.SaveChangesAsync());
        
        ex.Message.Should().Contain("Cannot change TenantId");
    }
    
    [Fact]
    public async Task PostgreSQL_RLS_Enforces_Tenant_Isolation()
    {
        // Arrange
        var dbContext = _fixture.CreateDbContext();
        
        // Seed students for both tenants
        await SeedStudentsAsync(dbContext, TenantA, count: 3);
        await SeedStudentsAsync(dbContext, TenantB, count: 3);
        
        // Act: Set session variable to TenantA and query
        await dbContext.Database.ExecuteSqlRawAsync($"SET LOCAL app.current_tenant = '{TenantA}'");
        var studentsForTenantA = await dbContext.Students
            .IgnoreQueryFilters() // Bypass EF filter to test RLS
            .ToListAsync();
        
        // Assert: RLS still filters by tenant
        studentsForTenantA.Should().HaveCount(3);
        studentsForTenantA.Should().OnlyContain(s => s.TenantId == TenantA);
    }
    
    private async Task SeedStudentsAsync(StudentDbContext dbContext, Guid tenantId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var student = Domain.Entities.Student.Create(
                firstName: $"Student{i}",
                lastName: $"Tenant{tenantId.ToString()[..8]}",
                dateOfBirth: new DateTime(2010, 1, 1).AddDays(i),
                tenantId: tenantId);
            
            dbContext.Students.Add(student);
        }
        
        await dbContext.SaveChangesAsync();
    }
}
```

### API Tests (End-to-End Isolation)

```csharp
// Location: tests/api/Student.ApiTests/TenantIsolationApiTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;

namespace NorthStarET.Foundation.Student.ApiTests;

public sealed class TenantIsolationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    [Fact]
    public async Task User_Can_Only_Access_Their_Tenants_Students()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Authenticate as TenantA user
        var tenantAToken = await GetAuthTokenForTenantAsync(TenantA);
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tenantAToken);
        
        // Act
        var response = await client.GetAsync("/api/students");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
        students.Should().OnlyContain(s => s.TenantId == TenantA);
    }
    
    [Fact]
    public async Task User_Cannot_Access_Other_Tenants_Student_By_Id()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create student in TenantB
        var tenantBToken = await GetAuthTokenForTenantAsync(TenantB);
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tenantBToken);
        
        var createResponse = await client.PostAsJsonAsync("/api/students", new
        {
            FirstName = "Cross",
            LastName = "Tenant",
            DateOfBirth = "2010-01-01"
        });
        
        var studentId = (await createResponse.Content.ReadFromJsonAsync<CreateStudentResponse>())!.StudentId;
        
        // Switch to TenantA user
        var tenantAToken = await GetAuthTokenForTenantAsync(TenantA);
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", tenantAToken);
        
        // Act: Attempt to access TenantB's student
        var response = await client.GetAsync($"/api/students/{studentId}");
        
        // Assert: 404 (not 403, to avoid leaking existence)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

---

## Cross-Tenant Access Prevention

### API Gateway Validation

```csharp
// Location: Src/Foundation/services/ApiGateway/Middleware/TenantValidationMiddleware.cs
public sealed class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Extract tenant from JWT
        var jwtTenantId = context.User.FindFirst("tenant_id")?.Value;
        
        // Extract tenant from request (query string, route, body)
        var requestedTenantId = ExtractRequestedTenantId(context);
        
        if (requestedTenantId.HasValue && 
            jwtTenantId != requestedTenantId.Value.ToString())
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "tenant_mismatch",
                message = "Cannot access resources belonging to another tenant"
            });
            return;
        }
        
        await _next(context);
    }
    
    private static Guid? ExtractRequestedTenantId(HttpContext context)
    {
        // Check query string
        if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
        {
            return Guid.Parse(queryTenantId!);
        }
        
        // Check route values
        if (context.Request.RouteValues.TryGetValue("tenantId", out var routeTenantId))
        {
            return Guid.Parse(routeTenantId!.ToString()!);
        }
        
        return null;
    }
}
```

### Audit Logging for Violations

```csharp
// Log every tenant mismatch attempt
_logger.LogError(
    "SECURITY VIOLATION: User {UserId} in tenant {ActualTenantId} " +
    "attempted to access resource in tenant {RequestedTenantId}",
    userId,
    actualTenantId,
    requestedTenantId);

// Trigger security alert
await _securityAlertService.NotifyAsync(new SecurityAlert
{
    Severity = AlertSeverity.High,
    Type = "CrossTenantAccessAttempt",
    UserId = userId,
    Details = $"User attempted to access tenant {requestedTenantId}"
});
```

---

## Anti-Patterns

### ❌ Accepting Tenant ID from Client

**Never**:
```csharp
// ❌ BAD: Accepting tenant_id from request body
[HttpPost("students")]
public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentRequest request)
{
    var command = new CreateStudentCommand(
        FirstName: request.FirstName,
        LastName: request.LastName,
        TenantId: request.TenantId); // ⚠️ Client-provided tenant!
    
    // Attacker can create students in any tenant!
}
```

**Instead**:
```csharp
// ✅ GOOD: Extract tenant from authenticated context
[HttpPost("students")]
public async Task<IActionResult> CreateStudentAsync(
    [FromBody] CreateStudentRequest request,
    [FromServices] ITenantContext tenantContext)
{
    var command = new CreateStudentCommand(
        FirstName: request.FirstName,
        LastName: request.LastName,
        TenantId: tenantContext.TenantId); // From JWT claims
}
```

### ❌ Using `IgnoreQueryFilters()` Without Justification

**Never**:
```csharp
// ❌ BAD: Bypassing filters without clear reason
var allStudents = await dbContext.Students
    .IgnoreQueryFilters() // ⚠️ Why?
    .ToListAsync();
```

**Instead**:
```csharp
// ✅ GOOD: Only for documented cross-tenant operations
[Authorize(Roles = "SuperAdmin")]
[HttpGet("admin/students/all")]
public async Task<IActionResult> GetAllStudentsAcrossTenants()
{
    // Explicit admin operation with audit logging
    _logger.LogWarning(
        "Cross-tenant query executed by super admin {UserId}",
        User.GetUserId());
    
    var allStudents = await _dbContext.Students
        .IgnoreQueryFilters() // Justified for admin dashboard
        .ToListAsync();
    
    return Ok(allStudents);
}
```

### ❌ Forgetting to Set Session Variable

**Never**:
```csharp
// ❌ BAD: Querying without setting session variable
await dbContext.Students.ToListAsync();
// PostgreSQL RLS will reject query if app.current_tenant not set!
```

**Instead**:
```csharp
// ✅ GOOD: TenantInterceptor automatically sets session variable
// No manual action required - interceptor handles it in SavingChangesAsync
```

---

## Performance Considerations

### Indexing Strategy

**Tenant-first composite indexes**:

```sql
-- ✅ GOOD: Tenant-first index for efficient filtering
CREATE INDEX idx_students_tenant_created 
    ON students(tenant_id, created_at DESC);

-- Query uses index efficiently:
SELECT * FROM students 
WHERE tenant_id = 'abc-123'  -- Index seek
ORDER BY created_at DESC     -- Index covers sort
LIMIT 10;

-- ❌ BAD: Index without tenant_id
CREATE INDEX idx_students_created ON students(created_at DESC);
-- Forces full table scan even with global query filter!
```

**All tenant-scoped tables** should have `tenant_id` as the first column in their primary index.

### Connection Pooling Configuration

```csharp
// Npgsql connection string for optimal pooling
var connectionString = configuration.GetConnectionString("PostgreSQL")
    + ";Pooling=true;MinPoolSize=5;MaxPoolSize=100;ConnectionLifetime=300";

services.AddDbContext<StudentDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
```

### Query Performance Targets

| Query Type | Target (p95) |
|------------|--------------|
| Single entity by ID (tenant-scoped) | <10ms |
| List query (tenant-scoped, paginated) | <50ms |
| Aggregate query (tenant-scoped) | <100ms |
| Cross-tenant admin query (IgnoreQueryFilters) | <500ms |

### Monitoring RLS Overhead

```sql
-- Check query plans to ensure RLS policies use indexes
EXPLAIN ANALYZE
SELECT * FROM students
WHERE tenant_id = current_setting('app.current_tenant')::uuid
LIMIT 10;

-- Should show "Index Scan" not "Seq Scan"
```

---

## References

### Internal Documents
- [Constitution Principle 4: Event-Driven Data Discipline](../../.specify/memory/constitution.md#4-event-driven-data-discipline)
- [Multi-Tenant Database Architecture Scenario](../../Foundation/scenarios/02-multi-tenant-database-architecture.md)
- [Clean Architecture Pattern](./clean-architecture.md)
- [Security & Compliance Pattern](../standards/security-compliance.md)

### External References
- [PostgreSQL Row-Level Security](https://www.postgresql.org/docs/current/ddl-rowsecurity.html)
- [EF Core Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [Multi-Tenant Data Architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/considerations/tenancy-models)

### Service Architectures
- [Student Management Service](../architecture/services/student-management-service-detailed.md)
- [Assessment Service](../architecture/services/assessment-service-detailed.md)

---

**Version History**:
- 1.0.0 (2025-11-20): Initial multi-tenancy pattern document
