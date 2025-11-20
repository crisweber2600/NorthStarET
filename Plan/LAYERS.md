# NorthStarET Mono-Repo Layer Architecture

**Version**: 1.0.0  
**Last Updated**: 2025-11-20  
**Status**: Active

## Overview

The NorthStarET mono-repo is organized into **layers** that maintain clear separation boundaries while sharing approved infrastructure. This document defines layer structure, dependency rules, shared infrastructure contracts, and documentation organization principles mandated by the mono-repo constitution (Principle 6: Mono-Repo Layer Isolation).

## Layer Structure

### Foundation Layer

**Purpose**: Core OldNorthStar-to-.NET-10 migration implementation  
**Location**: `Src/Foundation/`, `Plan/Foundation/`  
**Status**: Active Development  
**Scope**: 20 feature specifications (001-020), 11 microservices

The Foundation layer contains the complete modernization of the legacy NorthStar LMS monolithic application, migrating from .NET Framework 4.6 + AngularJS to .NET 10 + modern UI frameworks with Clean Architecture and microservices patterns.

**Services**:
1. Identity & Authentication Service
2. API Gateway (YARP)
3. Configuration Service
4. Student Management Service
5. Staff Management Service
6. Assessment Service
7. Intervention Management Service
8. Section & Roster Service
9. Data Import & Integration Service
10. Reporting & Analytics Service
11. Content & Media Service

**Feature Specifications**:
- **001-019**: Core Foundation features (identity, enrollment, configuration, assessment, staff, intervention, data import, section roster, reporting, content, homeschool features)
- **020**: Digital Ink Service (specialized capability)

**Migration Timeline**: 22-32 weeks (4 phases)

### DigitalInk Layer

**Purpose**: Specialized digital ink capture, synchronized audio recording, and AI-ready data export  
**Location**: `Plan/DigitalInk/`, future `Src/DigitalInk/`  
**Status**: Specification Complete, Implementation Phase 4a-4d (Week 23-28)  
**Scope**: 1 microservice (Digital Ink Service), 12 Given-When-Then scenarios

The DigitalInk layer provides high-fidelity stylus input capture, synchronized audio recording, and playback capabilities for student assessments. This is a greenfield service enabling multimodal assessment data capture optimized for AI/LLM analysis.

**Dependencies on Foundation**:
- Identity Service (JWT authentication, user context)
- Assessment Service (assignment context, `AssessmentAssignedEvent` subscription)
- Student Management Service (student validation, `StudentWithdrawnEvent` subscription)
- Azure Blob Storage (PDF backgrounds, audio files)
- Azure Service Bus (domain events)

**Key Features**:
- High-fidelity stroke data capture (x, y, pressure, timestamp at 60Hz+)
- Synchronized audio recording and playback
- Teacher annotation and feedback overlays
- Offline-first client architecture with background sync
- LLM-optimized data export formats
- PostgreSQL JSONB for flexible stroke storage

**Planning Documentation**: See [`Plan/DigitalInk/`](./DigitalInk/) for complete specifications, implementation plan, and scenarios

### Future Layers

Future layers follow the pattern established by Foundation and DigitalInk. Each new layer:

1. **Maintains Independence**: No direct service-to-service calls across layers
2. **Uses Shared Infrastructure**: Identity, Configuration, ServiceDefaults, Domain primitives only
3. **Documents Dependencies**: Explicitly declares Foundation infrastructure contracts
4. **Follows Standards**: Adheres to patterns in `/docs/architecture/` and `/docs/standards/`
5. **Requires Architecture Review**: Layer proposals go through governance process

**Potential Future Layers**:
- **Analytics**: Advanced analytics and machine learning capabilities
- **Collaboration**: Real-time collaboration and communication features
- **Marketplace**: Third-party integration and extension marketplace
- **Mobile**: Native mobile application layer

## Dependency Rules

### Permitted Dependencies

✅ **Allowed Cross-Layer Dependencies**:
- **Identity & Authentication**: All layers may consume identity services for user context, authentication, authorization
- **Configuration Service**: All layers may read tenant, district, school settings
- **ServiceDefaults**: All layers use Aspire orchestration patterns, logging, telemetry
- **Domain Primitives**: All layers may use shared value objects, base entities, common enums
- **Event Bus**: All layers may publish and subscribe to domain events (async communication only)

### Prohibited Dependencies

❌ **Forbidden Cross-Layer Dependencies**:
- **Direct Service-to-Service Calls**: No synchronous API calls between layers (e.g., DigitalInk → Student Management Service)
- **Database Access**: No cross-layer database queries
- **Internal Domain Logic**: No access to another layer's domain models or business logic
- **UI Coupling**: No UI components that span layers without proper abstraction

### Dependency Direction

```
┌─────────────────────────────────────────────┐
│           Future Layers (Analytics,          │
│           Collaboration, Marketplace)        │
│                                              │
│  Dependencies: ↓ Foundation Shared Infra     │
└──────────────────────────┬───────────────────┘
                           │
                           ↓
┌──────────────────────────────────────────────┐
│         DigitalInk Layer                     │
│                                              │
│  Dependencies: ↓ Foundation Shared Infra     │
└──────────────────────────┬───────────────────┘
                           │
                           ↓
┌──────────────────────────────────────────────┐
│         Foundation Layer (Core)              │
│                                              │
│  Services: Identity, Config, Student,        │
│            Assessment, Staff, etc.           │
│                                              │
│  Shared Infrastructure:                      │
│  • ServiceDefaults (Aspire)                 │
│  • Domain Primitives                        │
│  • Application Contracts                    │
│  • Infrastructure Utilities                 │
└──────────────────────────────────────────────┘
```

**Rule**: Dependencies flow downward only. Higher layers depend on Foundation shared infrastructure; Foundation never depends on higher layers.

## Shared Infrastructure

### Location

All shared infrastructure resides in `Src/Foundation/shared/`:

```
Src/Foundation/shared/
├── ServiceDefaults/        # Aspire orchestration, logging, telemetry
├── Domain/                 # Shared domain primitives
├── Application/            # Shared application contracts
└── Infrastructure/         # Shared infrastructure utilities
```

### ServiceDefaults

**Purpose**: .NET Aspire service defaults, logging, telemetry, observability  
**Consumed By**: All services in all layers  
**Contents**:
- Aspire service registration extensions
- Structured logging configuration (Serilog/OpenTelemetry)
- Distributed tracing setup
- Health check patterns
- Resilience policies (Polly)

**Example Usage**:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // Aspire defaults for all services
```

### Domain Primitives

**Purpose**: Shared value objects, base entities, common domain patterns  
**Consumed By**: All layers (domain models)  
**Contents**:
- `EntityBase<TId>` - Base entity with ID, audit fields
- `ValueObject` - Base value object with equality
- `DomainEvent` - Base domain event
- Common value objects: `EmailAddress`, `PhoneNumber`, `TenantId`, `UserId`
- Enumerations: `UserRole`, `TenantStatus`, etc.

**Example Usage**:
```csharp
public class Student : EntityBase<Guid>
{
    public TenantId TenantId { get; private set; }
    public EmailAddress Email { get; private set; }
    // ...
}
```

### Application Contracts

**Purpose**: Shared DTOs, interfaces, common application patterns  
**Consumed By**: All layers (application logic, APIs)  
**Contents**:
- `ICurrentUserService` - Access to current authenticated user
- `ITenantContext` - Access to current tenant context
- Common DTOs: `PagedResult<T>`, `ApiResponse<T>`
- CQRS contracts: `ICommand`, `IQuery<TResult>`

**Example Usage**:
```csharp
public class GetStudentHandler : IQueryHandler<GetStudentQuery, StudentDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;
    // ...
}
```

### Infrastructure Utilities

**Purpose**: Shared infrastructure helpers (database, caching, messaging)  
**Consumed By**: All layers (infrastructure implementations)  
**Contents**:
- Database patterns: Multi-tenancy interceptors, audit interceptors
- Caching abstractions: `ICacheService`
- Messaging abstractions: `IEventPublisher`, `IEventSubscriber`
- Azure service clients: Key Vault, Blob Storage

**Example Usage**:
```csharp
public class StudentRepository : IStudentRepository
{
    private readonly ITenantContext _tenantContext;
    private readonly ICacheService _cache;
    // Tenant filtering applied automatically via interceptor
}
```

## Cross-Layer Communication

### Synchronous Communication (Prohibited)

❌ **Direct API Calls Between Layers**:
```csharp
// WRONG: DigitalInk calling Student Management directly
var student = await _httpClient.GetAsync("https://student-service/api/students/123");
```

### Asynchronous Communication (Preferred)

✅ **Event-Based Integration**:
```csharp
// CORRECT: DigitalInk subscribes to Student domain events
public class StudentEnrolledEventHandler : IConsumer<StudentEnrolledEvent>
{
    public async Task Consume(ConsumeContext<StudentEnrolledEvent> context)
    {
        // DigitalInk creates local read model from event
        await _digitalInkService.OnStudentEnrolled(context.Message);
    }
}
```

**Pattern**: Each layer maintains its own read models/projections built from events published by Foundation services.

### Shared Infrastructure Access (Permitted)

✅ **Identity Service Queries**:
```csharp
// CORRECT: Any layer queries Identity via shared interface
var user = await _identityService.GetUserByIdAsync(userId);
```

✅ **Configuration Service Queries**:
```csharp
// CORRECT: Any layer reads tenant configuration
var tenantSettings = await _configService.GetTenantSettingsAsync(tenantId);
```

## Documentation Organization

### Layer-Specific Documentation

**Foundation Layer**:
```
Plan/Foundation/
├── Plans/                  # Migration-specific plans, scenarios
│   ├── MASTER_MIGRATION_PLAN.md
│   ├── INTEGRATED_MIGRATION_PLAN.md
│   ├── MIGRATION_READINESS.md
│   ├── scenarios/          # 13 migration implementation scenarios
│   └── docs/               # Migration-specific tech docs
│
└── specs/                  # Feature specifications
    ├── Foundation/         # 001-019 core features
    └── DigitalInk/         # 020 digital ink feature
```

**DigitalInk Layer** (when implemented):
```
Plan/DigitalInk/
├── Plans/                  # DigitalInk-specific plans
└── specs/                  # DigitalInk feature specifications
```

### Cross-Layer Documentation (Repository Root)

**Architecture** (layer-agnostic patterns):
```
docs/architecture/
├── bounded-contexts.md
└── services/
    ├── identity-service.md
    ├── student-management-service.md
    └── ... (13 service architectures)
```

**Standards** (reusable across all layers):
```
docs/standards/
├── API_CONTRACTS_SPECIFICATION.md
├── api-gateway-config.md
└── TESTING_STRATEGY.md
```

**Principle**: Migration plans are time-bound and layer-specific; architecture patterns and standards are evergreen and cross-layer.

## Implementation Guidelines

### Starting a New Layer

1. **Architecture Review**: Submit layer proposal with:
   - Business justification
   - Bounded context analysis
   - Foundation shared infrastructure dependencies
   - Independent deployment strategy
   - Specification organization plan

2. **Create Directory Structure**:
   ```bash
   mkdir -p Src/{LayerName}/{services,shared}
   mkdir -p Plan/{LayerName}/{Plans,specs}
   ```

3. **Document Dependencies**:
   - Create `Plan/{LayerName}/DEPENDENCIES.md`
   - List all Foundation shared infrastructure contracts
   - Document event subscriptions from Foundation services
   - Specify any new shared infrastructure contributions

4. **Follow Standards**:
   - Use `/docs/architecture/` patterns
   - Implement `/docs/standards/` specifications
   - Apply constitution principles (Clean Architecture, TDD, etc.)

### Adding Services to Existing Layer

1. **Identify Bounded Context**: Ensure service aligns with layer purpose
2. **Review Shared Infrastructure**: Use existing contracts before creating new ones
3. **Document Service Architecture**: Add to `/docs/architecture/services/`
4. **Create Feature Specification**: Add to `Plan/{LayerName}/specs/`
5. **Implement Clean Architecture**: Follow `Domain → Application → Infrastructure → API` pattern

### Managing Shared Infrastructure

1. **Minimize Shared Code**: Only truly cross-cutting concerns belong in `shared/`
2. **Versioning**: Shared infrastructure uses semantic versioning
3. **Breaking Changes**: Require Architecture Review and migration plan
4. **Documentation**: Every shared contract must have XML docs and examples

## Testing Strategy

### Layer Isolation Tests

**Purpose**: Validate layer boundaries are not violated

```csharp
[Fact]
public void DigitalInk_ShouldNotReference_StudentManagementDomain()
{
    // Use NetArchTest or similar to enforce architectural rules
    var result = Types.InAssembly(typeof(DigitalInkService).Assembly)
        .ShouldNot().HaveDependencyOn("StudentManagement.Domain")
        .GetResult();
    
    Assert.True(result.IsSuccessful);
}
```

### Cross-Layer Integration Tests

**Purpose**: Validate event-based communication between layers

```csharp
[Fact]
public async Task WhenStudentEnrolled_DigitalInkShouldReceiveEvent()
{
    // Arrange
    await _studentService.EnrollStudentAsync(studentDto);
    
    // Act - wait for async event processing
    await Task.Delay(1000);
    
    // Assert
    var digitalInkProfile = await _digitalInkService.GetStudentProfileAsync(studentId);
    Assert.NotNull(digitalInkProfile);
}
```

### Shared Infrastructure Tests

**Purpose**: Ensure shared contracts work correctly for all consumers

```csharp
[Fact]
public async Task ServiceDefaults_ShouldApplyTenantContext()
{
    var tenantId = TenantId.Create(Guid.NewGuid());
    _tenantContext.SetTenant(tenantId);
    
    var students = await _studentRepository.GetAllAsync();
    
    // All results should be filtered by tenant
    Assert.All(students, s => Assert.Equal(tenantId, s.TenantId));
}
```

## Governance

### Constitution Alignment

This layer architecture implements **Principle 6: Mono-Repo Layer Isolation** from the NorthStarET Constitution v2.0.0. All specifications, plans, and implementations must:

- ✅ Explicitly identify target layer
- ✅ Document cross-layer dependencies (must be shared infrastructure only)
- ✅ Validate no direct service-to-service calls across layers
- ✅ Maintain layer-specific documentation boundaries

### Architecture Review Requirements

**New Layer Proposals**: Require Architecture Review approval  
**Shared Infrastructure Changes**: Require Architecture Review if breaking  
**Cross-Layer Dependencies**: Require Architecture Review (prohibited except shared infra)  

### Compliance Validation

CI/CD pipeline enforces:
- No direct references between layer services (except shared infrastructure)
- All specs include layer identification
- Tests validate architectural boundaries
- Documentation follows organizational rules

## Migration Context

### Foundation Layer Migration

The Foundation layer represents the OldNorthStar-to-.NET-10 migration (22-32 weeks). Characteristics:

- **Source**: .NET Framework 4.6 monolith (~729 C# files, 33+ controllers, 383 entities)
- **Target**: .NET 10 microservices (11 services, Clean Architecture)
- **Pattern**: Strangler Fig (gradual cutover via API Gateway)
- **Data**: Multi-tenancy (100s of SQL Server DBs → 11 PostgreSQL DBs with RLS)
- **UI**: AngularJS → Modern framework (preserve layouts, no Figma required)

### Post-Migration Layers

After Foundation stabilizes, new layers (DigitalInk, Analytics, etc.) can be added without impacting the core migration implementation. This enables:

- **Parallel Development**: Teams work on new capabilities while Foundation matures
- **Independent Deployment**: Layers deploy separately without coordination
- **Risk Isolation**: Issues in new layers don't affect Foundation services
- **Incremental Value**: New features ship without waiting for full migration completion

## Examples

### ✅ Correct: DigitalInk Uses Shared Infrastructure

```csharp
// DigitalInk service using Foundation shared infrastructure
public class DigitalInkService
{
    private readonly ICurrentUserService _currentUser; // Shared
    private readonly ITenantContext _tenant;           // Shared
    private readonly IEventPublisher _events;          // Shared
    
    public async Task ProcessInkAsync(InkData inkData)
    {
        var userId = _currentUser.UserId;
        var tenantId = _tenant.TenantId;
        
        // Process ink...
        
        await _events.PublishAsync(new InkProcessedEvent(userId, tenantId, inkData.Id));
    }
}
```

### ❌ Incorrect: DigitalInk Calls Student Management Directly

```csharp
// VIOLATION: Direct service-to-service call across layers
public class DigitalInkService
{
    private readonly HttpClient _httpClient;
    
    public async Task ProcessInkAsync(InkData inkData)
    {
        // WRONG: Synchronous call to another layer's service
        var student = await _httpClient.GetFromJsonAsync<Student>(
            $"https://student-service/api/students/{inkData.StudentId}");
        
        // Process ink...
    }
}
```

### ✅ Correct: Event-Based Integration

```csharp
// DigitalInk subscribes to Student domain events (async)
public class DigitalInkEventHandlers
{
    public async Task Handle(StudentEnrolledEvent @event)
    {
        // Create local read model for DigitalInk needs
        await _digitalInkRepository.CreateStudentProfileAsync(
            new DigitalInkStudentProfile
            {
                StudentId = @event.StudentId,
                TenantId = @event.TenantId,
                EnrolledAt = @event.EnrolledAt
            });
    }
}
```

## References

- [NorthStarET Constitution v2.0.0](../../.specify/memory/constitution.md) - Principle 6: Mono-Repo Layer Isolation
- [Foundation Migration Plans](./Plans/) - Migration roadmaps and scenarios
- [Foundation Specifications](./specs/) - Feature specs for Foundation and DigitalInk layers
- [Architecture Patterns](../../docs/architecture/) - Bounded contexts and service architectures
- [Technical Standards](../../docs/standards/) - API contracts, testing strategy, gateway config

---

**Maintained By**: Architecture Team  
**Review Cycle**: Quarterly  
**Next Review**: 2026-02-20
