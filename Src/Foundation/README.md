# Foundation Layer - Aspire Orchestration & Cross-Cutting Infrastructure

This directory contains the foundational scaffolding for the NorthStarET platform using .NET Aspire orchestration.

## Overview

The Foundation layer provides:
- **Aspire AppHost**: Orchestrates all services with PostgreSQL, Redis, and RabbitMQ
- **Shared Infrastructure**: Common patterns for multi-tenancy, event-driven architecture, caching, and observability
- **Service Scaffolding**: Scripts to quickly generate new microservices

## Directory Structure

```
Foundation/
├── AppHost/                    # Aspire orchestration host
│   ├── AppHost.cs             # Resource definitions (Postgres, Redis, RabbitMQ)
│   └── AppHost.csproj
├── services/                   # Microservices (scaffolded with scripts)
│   ├── ApiGateway/
│   ├── Configuration/
│   └── Identity/
└── shared/                     # Shared libraries
    ├── ServiceDefaults/        # Aspire defaults (OpenTelemetry, health checks)
    ├── Domain/                 # Domain primitives (EntityBase, ITenantEntity)
    ├── Application/            # CQRS primitives (MediatR, FluentValidation)
    └── Infrastructure/         # Shared infrastructure
        ├── Persistence/        # TenantInterceptor, ApplicationDbContext
        ├── Messaging/          # MassTransit extensions
        ├── Caching/            # Redis idempotency service
        └── Middleware/         # CorrelationId, Idempotency
```

## Quick Start

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for containers)
- Aspire templates: `dotnet new install Aspire.ProjectTemplates`

### Running the Full Stack

```bash
# Navigate to AppHost
cd Src/Foundation/AppHost

# Run Aspire orchestration
dotnet run
```

**Note**: Aspire requires DCP (Developer Control Plane) for local orchestration. If not available, services can be run individually or with Docker Compose.

### Scaffolding a New Service

**PowerShell (Windows):**
```powershell
cd .specify/scripts/powershell
.\new-service.ps1 -ServiceName "StudentManagement"
```

**Bash (Linux/macOS):**
```bash
cd .specify/scripts/bash
./new-service.sh StudentManagement
```

This creates:
- `Src/Foundation/services/StudentManagement/` with Domain, Application, Infrastructure, and API projects
- Registers the service in AppHost with PostgreSQL, Redis, and RabbitMQ references
- Adds projects to the solution file

## Key Features

### 1. Multi-Tenancy (Tenant Isolation)

**TenantInterceptor** automatically enforces tenant filtering on all EF Core operations:

```csharp
// Entities that inherit ITenantEntity are automatically filtered by TenantId
public class Student : EntityBase, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; }
}

// Queries automatically filter by current tenant
var students = await _context.Students.ToListAsync();
// Returns only students for current tenant
```

**Opt-out with audit logging:**

```csharp
[IgnoreTenantFilter] // Automatically logs to AuditLog table
public async Task<List<Student>> GetAllStudentsAcrossTenantsAsync()
{
    return await _context.Students.ToListAsync();
}
```

### 2. Event-Driven Messaging

**MassTransit** with RabbitMQ, retry policies, and circuit breaker:

```csharp
// In Infrastructure/DependencyInjection.cs
services.AddMassTransitWithRabbitMq(
    connectionString: builder.Configuration.GetConnectionString("rabbitmq"),
    configure: bus =>
    {
        bus.AddConsumer<StudentCreatedConsumer>();
    }
);

// Domain events automatically published
student.AddDomainEvent(new StudentCreatedEvent(student.Id, student.Name));
await _context.SaveChangesAsync(); // Event published after transaction commits
```

### 3. Idempotency

**Redis-backed idempotency** with 10-minute TTL:

```csharp
// Client sends X-Idempotency-Key header
POST /api/students
X-Idempotency-Key: create-student-123
{ "name": "John Doe" }

// First request: 201 Created, result stored in Redis
// Duplicate request: 202 Accepted with original student ID (not reprocessed)
```

### 4. Observability

**OpenTelemetry** with distributed tracing, metrics, and logs:

```csharp
// Automatically configured via ServiceDefaults
builder.AddServiceDefaults();

// Correlation IDs propagated across service boundaries
// View traces in Aspire dashboard at http://localhost:15000
```

## Shared Infrastructure Components

### Domain Layer

- **EntityBase**: Base class with Id, timestamps, soft delete, and domain events
- **ITenantEntity**: Interface for multi-tenant entities
- **IDomainEvent**: Marker interface for domain events

### Infrastructure Layer

- **TenantInterceptor**: EF Core interceptor for automatic tenant filtering
- **IgnoreTenantFilterAttribute**: Opt-out attribute with automatic audit logging
- **MassTransitExtensions**: Configures RabbitMQ with retry, circuit breaker, and DLQ
- **IdempotencyService**: Redis-backed with 10-minute TTL
- **CorrelationIdMiddleware**: Propagates correlation IDs for distributed tracing
- **IdempotencyMiddleware**: Handles X-Idempotency-Key header
- **ApplicationDbContext**: Base DbContext with AuditLog table

### ServiceDefaults

Generated by Aspire templates, includes:
- OpenTelemetry tracing, metrics, and logging
- Health checks (/health, /alive endpoints)
- Service discovery
- Resilient HTTP clients

## Architecture Principles

1. **Clean Architecture**: Domain → Application → Infrastructure → API
2. **CQRS**: MediatR for command/query separation
3. **Event-Driven**: MassTransit for async integration events
4. **Multi-Tenancy**: Enforced by default with auditable opt-out
5. **Idempotency**: Prevents duplicate operations with Redis
6. **Observability**: OpenTelemetry for distributed tracing

## Building & Testing

```bash
# Build entire solution
dotnet build

# Run tests (when added)
dotnet test

# Build specific project
dotnet build Src/Foundation/AppHost/AppHost.csproj
```

## Next Steps

1. **Add Services**: Use scaffolding scripts to create new services
2. **Implement Features**: Add domain entities, commands, queries, and controllers
3. **Add Tests**: Unit tests, integration tests, and BDD scenarios
4. **Deploy**: Configure for production with Azure resources

## Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [MassTransit Documentation](https://masstransit.io/)
- [OpenTelemetry Documentation](https://opentelemetry.io/)
- Project Specification: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/`
