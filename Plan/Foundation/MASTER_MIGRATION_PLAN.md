# NorthStar Master Migration Plan: Comprehensive Integration Guide

**Version**: 3.0  
**Date**: November 15, 2025  
**Status**: Active - Integrated Plan  
**Integration Source**: All migration documents, specifications, and codebase analysis

---

## Executive Summary

This master plan integrates all migration planning documents, specifications, and architectural guidelines to provide a **single source of truth** for migrating the legacy OldNorthStar (.NET Framework 4.6 monolith) to UpgradedNorthStar (.NET 10 microservices architecture) following Clean Architecture, .NET Aspire orchestration, and constitutional principles.

### Key Integration Points

1. **Constitutional Foundation** (v1.6.0) - All work follows Clean Architecture, TDD Red→Green, event-driven patterns
2. **Bounded Context Analysis** - 11 microservices derived from DDD analysis of 33+ legacy controllers and 383 entities
3. **Phased Delivery** - 22-32 week timeline across 4 phases using Strangler Fig pattern
4. **WIPNorthStar Leverage** - Features 001/002/004 provide proven templates for Entra ID integration and multi-tenancy
5. **Complete Documentation** - 11 service specs, 9 feature specs, data migration strategy, testing guidelines
6. **Multi-Tenancy Architecture** - Single database with strict tenant isolation instead of database-per-district
7. **UI Preservation** - Reuse existing OldNorthStar UI without requiring Figma redesigns

### Document Integration

This master plan consolidates:
- [MIGRATION_PLAN.md](./MIGRATION_PLAN.md) - Original migration roadmap
- [INTEGRATED_MIGRATION_PLAN.md](./INTEGRATED_MIGRATION_PLAN.md) - Detailed integrated plan v2.0
- [.github/prompts/plan-monolithToMicroservicesMigration.prompt.md](./.github/prompts/plan-monolithToMicroservicesMigration.prompt.md) - 10-step migration strategy
- [microservices/](./microservices/) - Service catalog, specs, architecture
- [docs/](./docs/) - Data migration, testing, API contracts
- [Src/WIPNorthStar/](./Src/WIPNorthStar/) - Working implementation with constitution
- [Src/OldNorthStar/](./Src/OldNorthStar/) - Legacy codebase to migrate
- [Src/UpgradedNorthStar/](./Src/UpgradedNorthStar/) - Target architecture

---

## Table of Contents

1. [Constitutional Compliance](#constitutional-compliance)
2. [Architecture Overview](#architecture-overview)
3. [Microservices Catalog](#microservices-catalog)
4. [Migration Phases](#migration-phases)
5. [Data Migration Strategy](#data-migration-strategy)
6. [Testing Strategy](#testing-strategy)
7. [Implementation Steps](#implementation-steps)
8. [Risk Management](#risk-management)
9. [Success Criteria](#success-criteria)
10. [Quick Reference](#quick-reference)

---

## Constitutional Compliance

All migration work adheres to **NorthStarET NextGen LMS Constitution v1.6.0** (see [constitution.md](./Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md)):

### Core Principles

✅ **Clean Architecture & Aspire Orchestration**
- Enforce Clean Architecture boundaries: UI → Application → Domain ← Infrastructure
- Orchestrate every service through .NET Aspire 13.0.0
- Build Aspire test projects for integration validation

✅ **Test-Driven Quality Gates**
- Red → Green → Refactor workflow with evidence capture
- Reqnroll BDD features before implementation
- Playwright UI tests for Figma-backed journeys
- ≥80% code coverage requirement

✅ **UX Traceability & UI Migration**
- Preserve existing OldNorthStar UI during migration (AngularJS → modern framework)
- UI migration does not require Figma designs - reuse existing layouts and workflows
- Maintain functional parity with legacy UI while modernizing the technology stack
- Incremental UI updates can leverage Figma for new features only (not migration work)

✅ **Event-Driven Data Discipline**
- Prefer async event-driven integration via Azure Service Bus
- Document latency budgets for sync calls
- Idempotent commands with versioned event schemas

✅ **Security & Compliance Safeguards**
- Least privilege + RBAC in Application layer
- No UI → Infrastructure coupling
- Secrets only in Azure Key Vault

✅ **Tool-Assisted Development Workflow**
- Structured thinking tools (`#think`) for planning
- Query official documentation (`#microsoft.docs.mcp`)
- UI migration preserves existing OldNorthStar layouts and workflows

### Delivery Workflow

- Phase review branches: `git push origin HEAD:[feature-XXX]review-Phase[N]`
- Red/Green evidence capture: `dotnet test > test-evidence-{red|green}.txt`
- No direct pushes to main/develop
- Architecture Review for SLO/security changes

---

## Architecture Overview

### Current State Analysis

#### OldNorthStar (Legacy System)
- **Technology**: .NET Framework 4.6, AngularJS, SQL Server
- **Architecture**: Monolithic web application
- **Scale**: ~729 C# files, 33+ controllers, 383 entity/DTO files
- **Components**:
  - NS4.WebAPI - Main API with 35+ controllers
  - NS4.Angular - AngularJS frontend
  - NorthStar.Core - Core business logic
  - NorthStar.EF6 - Entity Framework 6 data access
  - EntityDto - 383 entity and DTO files
  - IdentityServer - Authentication/authorization
  - Background processors (BatchProcessor, BatchPrint, AutomatedRollover)

#### WIPNorthStar (Modern Implementation)
- **Technology**: .NET 10, Aspire orchestration, Clean Architecture
- **Status**: 3 features partially implemented (~239 C# files)
- **Build Status**: ✓ Successful (12 warnings, 0 errors)
- **Features**:
  - Feature 001: Unified SSO & Authorization (70% complete)
  - Feature 002: Bootstrap Tenant Access (60% complete)
  - Feature 004: Manage Schools & Grades (50% complete)
- **Structure**:
  ```
  NorthStarET.NextGen.Lms/
  ├── Domain/          # Domain entities and events
  ├── Application/     # CQRS handlers and services
  ├── Infrastructure/  # Data access, external services
  ├── Api/             # REST API endpoints
  ├── Web/             # Razor Pages UI
  ├── AppHost/         # Aspire orchestration
  └── Tests/           # Unit, integration, BDD, UI tests
  ```

#### UpgradedNorthStar (Target System)
- **Status**: To be created
- **Purpose**: Final integrated modern microservices system
- **Architecture**: 11 microservices + API Gateway + shared infrastructure

### Target Microservices Architecture

| # | Service | Phase | Legacy Components | Priority |
|---|---------|-------|-------------------|----------|
| 1 | **Identity & Authentication** | 1 | IdentityServer, AuthController, PasswordResetController | Critical |
| 2 | **API Gateway (YARP)** | 1 | NS4.WebAPI routing | Critical |
| 3 | **Configuration** | 1 | DistrictSettingsController, NavigationController | Critical |
| 4 | **Student Management** | 2 | StudentController, StudentDashboardController | High |
| 5 | **Staff Management** | 2 | StaffController | High |
| 6 | **Assessment** | 2 | AssessmentController, BenchmarkController | High |
| 7 | **Intervention Management** | 3 | InterventionGroupController, InterventionToolkitController | Medium |
| 8 | **Section & Roster** | 3 | SectionController, RosterRolloverController | Medium |
| 9 | **Data Import & Integration** | 3 | ImportStateTestDataController, FileUploaderController | Medium |
| 10 | **Reporting & Analytics** | 4 | LineGraphController, ExportDataController, PrintController | Low |
| 11 | **Content & Media** | 4 | VideoController, HelpController | Low |

**Supporting**: System Operations Service (Phase 4) - Health monitoring, diagnostics

### Technology Stack

**Backend**:
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core 10.0
- MediatR 12.4.0 (CQRS)
- Microsoft Entra ID with Microsoft.Identity.Web (OAuth 2.0/OIDC)
- Custom session authentication (SessionAuthenticationHandler)

**Data**:
- PostgreSQL (database-per-service with multi-tenancy via tenant_id + Row-Level Security)
- Moving away from database-per-district to shared multi-tenant databases
- Redis (distributed caching)
- Azure Blob Storage (files/media)
- LMS service manages base database layers and schema evolution

**Messaging**:
- MassTransit 8.x
- Azure Service Bus (production) / RabbitMQ (dev)

**Orchestration**:
- .NET Aspire 13.0.0
- YARP (API Gateway)

**Testing**:
- xUnit 2.9.0
- Reqnroll (BDD)
- Playwright 1.47.0 (UI tests)
- Aspire integration tests

**Infrastructure**:
- Docker containers
- Kubernetes (Azure AKS)
- GitHub Actions (CI/CD)
- Azure Application Insights (monitoring)

---

## Microservices Catalog

Complete service documentation available in [microservices/](./microservices/):

### Service Documentation Structure

Each service has:
1. **Detailed Service Specification** (`architecture/services/*.md`)
   - Current state (legacy) and target state (.NET 10)
   - Clean Architecture layer structure
   - Domain events published/subscribed
   - API functional intent
   - SLOs, idempotency, consistency models
   - Migration strategy

2. **Spec-Kit Feature Prompts** (`../specs/*.md`)
   - 150-300 word specifications
   - WHAT & WHY (not HOW)
   - Vertical slice features
   - Boundary guarantees (SLOs, idempotency)
   - Acceptance signals

### Available Documentation

**Service Specifications** (11 total):
- ✓ [Identity & Authentication Service](./architecture/services/identity-service.md)
- ✓ [Student Management Service](./architecture/services/student-management-service.md)
- ✓ [Assessment Service](./architecture/services/assessment-service.md)
- ✓ [Staff Management Service](./architecture/services/staff-management-service.md)
- ✓ [Intervention Management Service](./architecture/services/intervention-management-service.md)
- ✓ [Section & Roster Service](./architecture/services/section-roster-service.md)
- ✓ [Data Import Service](./architecture/services/data-import-service.md)
- ✓ [Reporting & Analytics Service](./architecture/services/reporting-analytics-service.md)
- ✓ [Content & Media Service](./architecture/services/content-media-service.md)
- ✓ [Configuration Service](./architecture/services/configuration-service.md)
- ✓ [System Operations Service](./architecture/services/system-operations-service.md)

**Feature Specifications** (9 total):
- ✓ [Identity: User Authentication](./../specs/identity-authentication-spec.md)
- ✓ [Student: Enrollment Management](./../specs/student-enrollment-spec.md)
- ✓ [Homeschool: Parent Registration](./../specs/homeschool-parent-registration-spec.md)
- ✓ [Homeschool: Student Enrollment](./../specs/homeschool-student-enrollment-spec.md)
- ✓ [Homeschool: Activity Logging](./../specs/homeschool-activity-logging-spec.md)
- ✓ [Homeschool: Multi-Grade Tracking](./../specs/homeschool-multigrade-spec.md)
- ✓ [Homeschool: Compliance Dashboard](./../specs/homeschool-compliance-dashboard-spec.md)
- ✓ [Homeschool: Annual Report](./../specs/homeschool-annual-report-spec.md)
- ✓ [Homeschool: Co-op Roster](./../specs/homeschool-coop-roster-spec.md)

**Architecture Documentation**:
- ✓ [Bounded Contexts](./architecture/bounded-contexts.md)
- ✓ [Development Guide](./docs/development-guide.md)
- ✓ [Deployment Guide](./docs/deployment-guide.md)
- ✓ [API Gateway Configuration](./docs/api-gateway-config.md)

**Data & Testing**:
- ✓ [Data Migration Specification](./docs/DATA_MIGRATION_SPECIFICATION.md)
- ✓ [API Contracts Specification](./docs/API_CONTRACTS_SPECIFICATION.md)
- ✓ [Testing Strategy](./docs/TESTING_STRATEGY.md)

### Service Dependency Graph

```
┌─────────────────────────┐
│  Identity Service       │ ← Foundation (Microsoft Entra ID)
│  (Phase 1)              │
└───────────┬─────────────┘
            │ provides auth tokens
            ↓
┌───────────────────────────────────────────┐
│  API Gateway (YARP)                       │
│  (Phase 1)                                │
└───────────┬───────────────────────────────┘
            │ routes & validates
            ↓
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│ Student Service   │  │ Staff Service     │  │ Assessment Service│
│ (Phase 2)         │  │ (Phase 2)         │  │ (Phase 2)         │
└─────────┬─────────┘  └─────────┬─────────┘  └─────────┬─────────┘
          │ publishes events      │                      │
          └───────────────────────┼──────────────────────┘
                                  ↓ Azure Service Bus
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│ Intervention Svc  │  │ Section Service   │  │ Data Import Svc   │
│ (Phase 3)         │  │ (Phase 3)         │  │ (Phase 3)         │
└───────────────────┘  └───────────────────┘  └───────────────────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 ↓
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│ Reporting Service │  │ Media Service     │  │ Operations Svc    │
│ (Phase 4 - CQRS)  │  │ (Phase 4)         │  │ (Phase 4)         │
└───────────────────┘  └───────────────────┘  └───────────────────┘
```

---

## Migration Phases

### Phase 1: Foundation Services (Weeks 1-8)

**Objective**: Establish foundational infrastructure and security services

**Services**: Identity & Authentication, API Gateway (YARP), Configuration

**Key Deliverables**:
- ✅ Aspire AppHost orchestration configured
- ✅ Microsoft Entra ID integration for staff/admin SSO (following WIPNorthStar Feature 001 pattern)
- ✅ Identity service migrated from IdentityServer to Entra ID
- ✅ JWT token authentication across all services
- ✅ YARP gateway routing to legacy + new services
- ✅ PostgreSQL databases per service with multi-tenant isolation (tenant_id + Row-Level Security)
- ✅ LMS service establishes base database layers and admin features
- ✅ Redis caching layer
- ✅ Azure Service Bus integration
- ✅ ServiceDefaults (health checks, observability, logging)

**WIPNorthStar Integration**:
- Leverage Feature 001 (SSO/Auth) implementation patterns
- Complete remaining Feature 001 Phase 5-7 tasks
- Refactor `NorthStarET.NextGen.Lms.*` → `NorthStar.*` namespaces

**Testing Requirements**:
- Reqnroll BDD scenarios for login flows (local + Entra ID)
- Aspire integration tests for service discovery
- Performance testing: <50ms auth decision (P95)

### Phase 2: Core Domain Services (Weeks 9-16)

**Objective**: Migrate core educational domain services

**Services**: Student Management, Staff Management, Assessment

**Key Deliverables**:
- ✅ Student enrollment, demographics, dashboard
- ✅ Staff profiles, team management
- ✅ Assessment definitions, scoring, benchmarks
- ✅ Domain events flowing via Azure Service Bus
- ✅ Data migration scripts for Students, Staff, Assessments
- ✅ Dual-write pattern (write to legacy + new DBs)

**Data Migration Strategy**:
1. Analyze `NorthStar.EF6/DistrictContext.cs` schema
2. Create EF Core 10 migration scripts per service
3. Implement ETL jobs for historical data
4. Setup dual-write synchronization (temporary)
5. Validate data integrity with reconciliation scripts

**Testing Requirements**:
- ≥80% unit test coverage per service
- Reqnroll BDD scenarios for enrollment, assessment workflows
- Aspire integration tests for cross-service event flows
- Performance: <100ms query response (P95)

**WIPNorthStar Integration**:
- Leverage Feature 002 (Tenant Bootstrap) for district/school context
- Leverage Feature 004 (Schools/Grades) for organizational hierarchy

### Phase 3: Secondary Domain Services (Weeks 17-22)

**Objective**: Migrate intervention, roster, and data import capabilities

**Services**: Intervention Management, Section & Roster, Data Import & Integration

**Key Deliverables**:
- ✅ Intervention groups, student assignments, attendance tracking
- ✅ Section management, rosters, automated rollover
- ✅ CSV/Excel imports, state test data integration
- ✅ Background job processing migrated to Worker Services

**Migration Activities**:

**Intervention Service**:
- Migrate `InterventionGroupController`, `InterventionToolkitController`
- Implement intervention planning and tracking
- Subscribe to AssessmentCompletedEvent
- Publish: InterventionCreatedEvent, StudentAddedToInterventionEvent

**Section & Roster Service**:
- Migrate `SectionController`, `SectionDataEntryController`, `RosterRolloverController`
- Implement section enrollment, teacher assignments
- Implement automated year-end rollover (saga pattern)
- Publish: SectionCreatedEvent, StudentRosteredEvent, RolloverCompletedEvent

**Data Import Service**:
- Migrate `ImportStateTestDataController`, `FileUploaderController`
- Implement CSV/Excel import with validation
- Support state test data formats
- Publish: DataImportCompletedEvent, StateTestDataImportedEvent

**Background Jobs**:
- Migrate `NorthStar.BatchProcessor` to .NET 10 Worker Service
- Migrate `NorthStar4.BatchPrint` to queue-based processing
- Migrate `NorthStar.AutomatedRollover` to scheduled jobs

**Testing Requirements**:
- BDD scenarios for intervention workflows, roster management
- Integration tests for saga patterns (rollover coordination)
- Load testing for bulk data imports

### Phase 4: Supporting Services (Weeks 23-28)

**Objective**: Complete migration with reporting, media, and operations

**Services**: Reporting & Analytics, Content & Media, System Operations

**Key Deliverables**:
- ✅ Report generation, data visualization, exports
- ✅ File upload, Azure Blob Storage integration, video management
- ✅ System health monitoring, diagnostics, navigation

**Migration Activities**:

**Reporting Service**:
- Migrate `LineGraphController`, `StackedBarGraphController`, `ExportDataController`
- Implement CQRS read models (materialized views from events)
- Subscribe to all domain events for reporting aggregation
- Generate PDF reports, Excel exports
- Support scheduled report generation

**Content & Media Service**:
- Migrate `VideoController`, `HelpController`
- Integrate Azure Blob Storage for file uploads
- Implement video transcoding pipeline
- Manage file metadata, download/streaming

**System Operations Service**:
- Migrate `NavigationController`, `ProbeController`
- Implement comprehensive health checks
- Setup Application Insights integration
- Expose Prometheus metrics
- Provide diagnostic endpoints

**Testing Requirements**:
- UI tests (Playwright) for report generation workflows
- Performance testing for large report generation
- Load testing for file uploads

### UI Migration (Weeks 20-28, parallel with Phases 3-4)

**Objective**: Migrate NS4.Angular to modern UI while preserving existing layouts

**Approach**: UI Preservation Strategy (No Figma Required)
- Reuse existing OldNorthStar UI layouts and workflows
- Migrate AngularJS components to Angular 18 or Blazor incrementally
- Maintain visual and functional parity with legacy system
- No design work required - focus on technology modernization

**Recommended**: Angular 18 for incremental migration from AngularJS, or Blazor Web App for .NET ecosystem alignment

**Actions**:
1. Analyze existing NS4.Angular components and routes
2. Create modern framework project structure (Angular 18 or Blazor)
3. Integrate WIPNorthStar UI patterns from Features 001/002/004
4. Implement authentication flows (Entra ID SSO)
5. Migrate screens incrementally, preserving existing UX
6. Map existing AngularJS controllers/views to new components

**Testing**:
- Playwright UI tests for all user workflows
- Visual regression testing to ensure UI parity
- Accessibility testing (WCAG 2.1 AA)
- Cross-browser compatibility testing

### Iterative Cutover (Weeks 29-32)

**Objective**: Gradually route production traffic using Strangler Fig pattern

**Cutover Sequence**:
- **Week 29-30**: Foundation Services (10% → 50% → 100%)
- **Week 30-31**: Core Services (10% → 50% → 100%)
- **Week 31-32**: Secondary Services (gradual rollout)

**Feature Flags**:
```csharp
if (featureFlags.IsEnabled("UseNewStudentService", userId))
{
    return await _newStudentServiceClient.GetStudentAsync(studentId);
}
else
{
    return await _legacyApiClient.GetStudentAsync(studentId);
}
```

**Monitoring**:
- Side-by-side comparison: new vs. legacy response times
- Error rate tracking
- Data consistency validation
- User feedback collection

**Rollback Plan**:
- Keep legacy system running for 4 weeks post-cutover
- Immediate rollback capability via feature flags
- Database rollback scripts

---

## Data Migration Strategy

Complete specification: [docs/DATA_MIGRATION_SPECIFICATION.md](./docs/DATA_MIGRATION_SPECIFICATION.md)

### Migration Principles

1. **Zero Data Loss** - All historical data preserved
2. **Minimal Downtime** - Dual-write pattern during cutover
3. **Incremental Migration** - Service-by-service approach
4. **Validation First** - Extensive reconciliation before cutover
5. **Rollback Ready** - Ability to revert at any phase

### Source Database Analysis

**Legacy Schema** (SQL Server):
- Per-district databases: `NorthStar_District001`, `NorthStar_District002`, etc.
- `DistrictContext` - Main application database
- `LoginContext` - Cross-district authentication database
- **383 entity types** across all domains
- **~15 years** of historical data

### Target Database Design (PostgreSQL)

**Database-Per-Service with Multi-Tenancy Architecture**:

Moving away from the legacy **database-per-district** model to a modern **database-per-service** approach with **strict multi-tenancy** within each service database:

**Key Change**: Instead of separate databases for each school district (OldNorthStar approach), all districts share the same service databases with tenant isolation.

**Database Structure**:

| Service | Database Name | Multi-Tenancy Approach | Primary Tables |
|---------|---------------|------------------------|----------------|
| Identity | `northstar_identity` | Tenant-scoped via `tenant_id` + RLS | Users, Roles, Claims, RefreshTokens, ExternalProviders |
| Student | `northstar_students` | Tenant-scoped via `tenant_id` + RLS | Students, Enrollments, Demographics, Contacts |
| Staff | `northstar_staff` | Tenant-scoped via `tenant_id` + RLS | Staff, StaffAssignments, Teams, TeamMembers |
| Assessment | `northstar_assessments` | Tenant-scoped via `tenant_id` + RLS | Assessments, Fields, Results, Benchmarks |
| Intervention | `northstar_interventions` | Tenant-scoped via `tenant_id` + RLS | Interventions, Groups, Attendance |
| Section | `northstar_sections` | Tenant-scoped via `tenant_id` + RLS | Sections, Rosters, Schedules |
| Configuration | `northstar_configuration` | Tenant-scoped via `tenant_id` + RLS | DistrictSettings, Calendars, Schools, Grades |
| DataImport | `northstar_dataimport` | Tenant-scoped via `tenant_id` + RLS | ImportJobs, ImportErrors, ValidationResults |
| Reporting | `northstar_reporting` | Tenant-scoped via `tenant_id` + RLS | Reports, Aggregations (Read Models) |
| Media | `northstar_media` | Tenant-scoped via `tenant_id` + RLS | Files, Videos, FileMetadata |

**Multi-Tenancy Strategy**:
- **Tenant Isolation**: Every table includes `tenant_id` (district identifier) as part of composite keys
- **Row-Level Security (RLS)**: PostgreSQL RLS policies enforce tenant boundaries at database level
- **Application-Level Filtering**: Services set tenant context via claims/session for automatic query filtering
- **LMS Service Responsibility**: Base database layers, migrations, and admin features managed by LMS service
- **Data Sovereignty**: Each district's data isolated within shared infrastructure
- **Connection Pooling**: Single connection pool per service database, tenant context in session variables

**Migration Impact**:
- **Legacy**: `NorthStar_District001`, `NorthStar_District002`, ... (N databases for N districts)
- **Target**: `northstar_students`, `northstar_staff`, ... (11 databases for all districts with tenant_id separation)
- **Benefit**: Consolidate 100s of district databases into 11 multi-tenant service databases

**Benefits**:
- Simplified operations (11 databases vs. hundreds)
- Easier cross-service queries with consistent tenant context
- Reduced infrastructure and maintenance costs
- Centralized backup and disaster recovery per service
- Horizontal scaling via database sharding if needed
- LMS service owns base schema evolution across all tenants

### ETL Framework

**Tool**: Custom .NET 10 console application

**Architecture**:
```
DataMigration.Console/
├── Program.cs                      # Main orchestration
├── Configuration/
│   └── MigrationSettings.json     # Connection strings, batch sizes
├── Jobs/
│   ├── StudentMigrationJob.cs
│   ├── StaffMigrationJob.cs
│   ├── AssessmentMigrationJob.cs
│   └── ... (one per service)
├── Mappers/
│   ├── StudentMapper.cs           # Legacy → New entity mapping
│   └── ...
├── Validators/
│   ├── DataIntegrityValidator.cs  # Post-migration validation
│   └── ReconciliationReport.cs
└── Infrastructure/
    ├── LegacyDbContext.cs         # EF6 → EF Core adapter
    ├── TargetDbContexts/          # Per-service EF Core contexts
    └── BatchProcessor.cs          # Batch insert optimization
```

### Dual-Write Pattern

During cutover, implement dual-write to maintain data consistency:

```csharp
public async Task<Student> AddAsync(Student student)
{
    // Write to new PostgreSQL database (primary)
    await _newContext.Students.AddAsync(student);
    await _newContext.SaveChangesAsync();
    
    // Also write to legacy SQL Server (temporary)
    try
    {
        var legacyStudent = MapToLegacy(student);
        _legacyContext.Students.Add(legacyStudent);
        await _legacyContext.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Dual-write to legacy DB failed");
        // Don't fail - new DB is source of truth
    }
    
    return student;
}
```

### Validation Strategy

**Reconciliation Queries**:
- Count validation: Compare record counts between legacy and new DBs
- Sample validation: Field-by-field comparison on 1000+ random samples
- Foreign key validation: Ensure referential integrity in new system
- Automated nightly validation reports during dual-write period

---

## Testing Strategy

Complete specification: [docs/TESTING_STRATEGY.md](./docs/TESTING_STRATEGY.md)

### Test Pyramid

```
        /\
       /UI\ Playwright (E2E) - Existing layouts/workflows
      /----\
     /  BDD \ Reqnroll (Acceptance) - Given/When/Then
    /--------\
   /Integration\ Aspire Integration Tests
  /------------\
 /  Unit Tests  \ xUnit (≥80% coverage)
/----------------\
```

### TDD Workflow (Constitutional Requirement)

**Red → Green → Refactor**:

1. **RED**: Write failing test first
   ```bash
   dotnet test > docs/evidence/feature-XXX-red.txt
   ```

2. **GREEN**: Implement feature to pass test
   ```bash
   dotnet test > docs/evidence/feature-XXX-green.txt
   ```

3. **REFACTOR**: Optimize while maintaining tests

4. **COMMIT**: Commit with evidence
   ```bash
   git add .
   git commit -m "Feature: Student enrollment [Phase2]"
   git push origin HEAD:002review-Phase2
   ```

### Test Coverage Requirements

**Per Service**:
- ≥80% code coverage (constitutional gate)
- Unit tests for all domain logic
- Integration tests for database operations
- BDD tests for all user stories
- UI tests for all migrated screens reusing OldNorthStar layouts (Figma not required)
- UI tests for all new features/screens with Figma designs

### Test Execution Gates

- **Phase Review**: All tests passing before phase completion
- **CI/CD**: No merge without passing test suite
- **Pre-Deployment**: Full regression suite in staging
- **Post-Deployment**: Smoke tests in production

---

## Implementation Steps

### Step 1: Inventory and Component Mapping (Weeks 1-2)

**Objective**: Create comprehensive mapping of OldNorthStar to microservice boundaries

**Actions**:
1. Analyze 33+ controllers in `Src/OldNorthStar/NS4.WebAPI/Controllers`
2. Map 383 entities in `Src/OldNorthStar/EntityDto` to bounded contexts
3. Analyze `Src/OldNorthStar/NorthStar.EF6` DbContexts
4. Document controller→service mapping with dependency graph

**Deliverables**:
- `docs/COMPONENT_INVENTORY.md` - Controller/entity mapping
- `docs/DEPENDENCY_GRAPH.md` - Service dependencies
- `docs/DATABASE_SCHEMA_ANALYSIS.md` - Table-to-service mapping

**Duration**: 1-2 weeks

### Step 2: Define Service Contracts (Weeks 2-4)

**Objective**: Establish clear API contracts and event schemas

**Actions**:
1. Create contract assemblies in `Src/UpgradedNorthStar/src/Contracts/`
2. Define shared kernel in `NorthStar.Common`
3. Document integration patterns (sync/async)
4. Define OpenAPI specifications
5. Define event schemas (Azure Service Bus contracts)

**Deliverables**:
- Contract assemblies with versioned DTOs
- `docs/API_CONTRACTS.md` - OpenAPI per service (✓ available)
- `docs/EVENT_CATALOG.md` - Event schemas
- `docs/INTEGRATION_PATTERNS.md` - Communication rules

**Duration**: 2 weeks

### Step 3: Setup UpgradedNorthStar Foundation (Weeks 3-5)

**Objective**: Create Aspire-orchestrated microservice infrastructure

**Actions**:
1. Create solution structure in `Src/UpgradedNorthStar/`
2. Configure ServiceDefaults
3. Setup centralized package management (`Directory.Packages.props`)
4. Configure Aspire AppHost

**Deliverables**:
- Aspire solution with AppHost and ServiceDefaults
- Shared contract and common libraries
- Infrastructure configuration (PostgreSQL, Redis, Azure Service Bus)

**Duration**: 2 weeks

### Step 4: Implement Phase 1 Services (Weeks 5-8)

**Objective**: Deploy foundation services (Identity, Gateway, Configuration)

**Process per Service**:
1. Create Clean Architecture project structure
2. Define domain entities and value objects
3. Implement MediatR command/query handlers
4. Create EF Core DbContext and migrations
5. Implement API controllers
6. Write tests (TDD): Unit → BDD → Integration → UI
7. Configure in Aspire AppHost
8. Deploy to development environment

**Testing Workflow (TDD)**:
```bash
# 1. RED: Write failing test
dotnet test > test-evidence-red.txt

# 2. GREEN: Implement feature
# (code implementation)

# 3. GREEN: Verify tests pass
dotnet test > test-evidence-green.txt

# 4. Commit with evidence
git add .
git commit -m "Feature: User login via Entra ID [Phase1]"
git push origin HEAD:001review-Phase1
```

**Deliverables**:
- Identity, Gateway, Configuration services deployed
- ≥80% test coverage per service
- Aspire integration tests passing
- Microsoft Entra ID SSO working
- API Gateway routing to legacy + new services

**Duration**: 4 weeks

### Step 5: Data Migration Strategy (Weeks 6-10, parallel with Step 4)

**Objective**: Plan and execute database migration from SQL Server to PostgreSQL

**Actions**:
1. Schema analysis and mapping
2. Per-service database creation
3. EF Core migrations
4. Data migration ETL
5. Dual-write pattern implementation
6. Foreign key elimination (replace with events)

**Deliverables**:
- EF Core DbContexts per service
- Migration scripts with rollback procedures
- ETL jobs for historical data migration
- Data validation reports
- Dual-write synchronization monitoring

**Duration**: 4 weeks (parallel)

### Step 6-10: Continue with Phases 2-4

Continue following the phased approach detailed in [Migration Phases](#migration-phases) section.

---

## Risk Management

### High Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Data Migration Failures** | Critical | Medium | Extensive validation, dual-write period, rollback scripts |
| **Performance Degradation** | High | Medium | Load testing, caching (Redis), performance budgets |
| **Microsoft Entra ID Integration Issues** | High | Low | Early PoC, Microsoft support, fallback to local auth |
| **Distributed Transaction Complexity** | High | Medium | Saga patterns, idempotent operations, event sourcing |
| **Feature Parity Gaps** | Medium | Medium | Comprehensive feature mapping, user testing |

### Medium Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Team Learning Curve** | Medium | High | Training, pairing, documentation, WIPNorthStar examples |
| **Timeline Slippage** | Medium | Medium | Buffer time, MVP approach, prioritization |
| **Multi-Tenant Data Isolation** | Medium | Medium | PostgreSQL RLS testing, tenant context validation, security audits |
| **Third-Party Dependencies** | Medium | Low | Vendor SLAs, circuit breakers, fallbacks |

### Low Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Kubernetes Orchestration** | Low | Low | Aspire simplification, managed AKS |
| **Secrets Management** | Medium | Low | Azure Key Vault, automated rotation |

---

## Success Criteria

### Functional

✅ **Feature Parity**:
- All OldNorthStar functionality available in UpgradedNorthStar
- Microsoft Entra ID SSO working for staff/administrators
- Zero data loss during migration
- User acceptance testing passed

### Architectural

✅ **Clean Architecture Compliance**:
- All services follow Clean Architecture (UI → Application → Domain ← Infrastructure)
- All services orchestrated via .NET Aspire
- Event-driven communication via Azure Service Bus
- No direct UI → Infrastructure coupling

### Quality

✅ **Test Coverage**:
- ≥80% test coverage across all services
- All Reqnroll BDD scenarios passing
- All Aspire integration tests passing
- All Playwright UI tests passing

### Performance

✅ **SLOs Met**:
- Authorization decisions <50ms (P95)
- API response times <100ms (P95)
- Report generation <5s for standard reports
- File uploads support up to 100MB

### Security

✅ **Security Posture**:
- Zero hardcoded credentials (all in Key Vault)
- All secrets managed via Azure Key Vault
- OAuth 2.0/OIDC authentication enforced
- Role-based authorization on all endpoints

### Operations

✅ **Operational Excellence**:
- Comprehensive health checks on all services
- Distributed tracing via OpenTelemetry
- Centralized logging in Application Insights
- Prometheus metrics exposed
- Automated deployments via GitHub Actions

### Documentation

✅ **Complete Documentation**:
- API documentation (OpenAPI/Swagger)
- Architecture Decision Records (ADRs)
- Deployment runbooks
- User documentation
- Training materials

---

## Quick Reference

### Key Documents

**Planning & Architecture**:
- [This Master Plan](./MASTER_MIGRATION_PLAN.md) - Single source of truth
- [Original Migration Plan](./MIGRATION_PLAN.md) - Initial roadmap
- [Integrated Migration Plan](./INTEGRATED_MIGRATION_PLAN.md) - Detailed v2.0
- [Constitution v1.6.0](./Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md) - Governing principles
- [Service Catalog](./microservices/SERVICE_CATALOG.md) - All 11 services

**Technical Specifications**:
- [Data Migration Spec](./docs/DATA_MIGRATION_SPECIFICATION.md) - ETL strategy
- [API Contracts Spec](./docs/API_CONTRACTS_SPECIFICATION.md) - API design
- [Testing Strategy](./docs/TESTING_STRATEGY.md) - Test approach
- [Bounded Contexts](./architecture/bounded-contexts.md) - DDD analysis

**Implementation Guides**:
- [Development Guide](./docs/development-guide.md) - Developer setup
- [Deployment Guide](./docs/deployment-guide.md) - Deployment instructions
- [API Gateway Config](./docs/api-gateway-config.md) - YARP configuration

### Timeline Summary

| Phase | Duration | Services | Weeks |
|-------|----------|----------|-------|
| Phase 1: Foundation | 8 weeks | Identity (Entra ID), Gateway, Configuration | 1-8 |
| Phase 2: Core Domain | 8 weeks | Student, Staff, Assessment | 9-16 |
| Phase 3: Secondary Domain | 6 weeks | Intervention, Section, Data Import | 17-22 |
| Phase 4: Supporting | 6 weeks | Reporting, Media, Operations | 23-28 |
| UI Migration | 8 weeks | Angular/Blazor (preserving existing UI, parallel 20-28) | 20-28 |
| Iterative Cutover | 4 weeks | Gradual production rollout | 29-32 |
| **Total** | **32 weeks** | **11 microservices + multi-tenant database** | |

### Constitutional Checkpoints

Before **ANY** work:
- [ ] Structured thinking plan documented
- [ ] UI work preserves existing OldNorthStar layouts (no Figma required for migration)
- [ ] Red/Green test evidence prepared

Before **ANY** commit:
- [ ] Red test evidence captured (`dotnet test > red.txt`)
- [ ] Green test evidence captured (`dotnet test > green.txt`)
- [ ] ≥80% code coverage verified
- [ ] Phase review branch targeted (not main/develop)
- [ ] Multi-tenant isolation tested (tenant_id filtering verified)

Before **ANY** merge:
- [ ] All tests passing (unit, BDD, integration, UI)
- [ ] Constitution compliance verified
- [ ] Architecture review (if SLO/security changed)
- [ ] Evidence attached to PR
- [ ] Row-Level Security policies validated (if database changes)

### Command Reference

**Build & Test**:
```bash
# Build entire solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test tests/NorthStar.Students.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true

# Aspire integration tests
dotnet test tests/NorthStar.Students.IntegrationTests

# BDD tests (Reqnroll)
dotnet test tests/NorthStar.Students.BddTests

# UI tests (Playwright)
pwsh tests/ui/playwright.ps1
```

**Phase Review Branch**:
```bash
# Feature 001, Phase 1 complete
git push origin HEAD:001review-Phase1

# Feature 002, Phase 3 complete
git push origin HEAD:002review-Phase3
```

**Aspire Orchestration**:
```bash
# Run Aspire AppHost
dotnet run --project src/NorthStar.AppHost

# Access Aspire Dashboard
# Navigate to http://localhost:15888
```

### Contact & Support

- **Architecture Questions**: Architecture Team
- **Migration Questions**: Migration Team
- **Technical Issues**: DevOps Team
- **Constitution Clarifications**: See [AGENTS.md](./Src/WIPNorthStar/NorthStarET.Lms/AGENTS.md)

---

## Next Immediate Actions

1. **Review and Approve** - Stakeholder sign-off on master plan
2. **Start Step 1** - Create `docs/COMPONENT_INVENTORY.md` by analyzing OldNorthStar
3. **Complete WIPNorthStar** - Finish Feature 001 Phases 5-7 (Entra ID integration patterns)
4. **Start UI Migration** - Begin AngularJS to modern framework migration using existing layouts
5. **Setup Project Tracking** - Create GitHub project with issues per deliverable
6. **Configure Multi-Tenancy** - Design Row-Level Security policies and tenant isolation strategy

---

**Document Version**: 3.0  
**Integration Date**: November 15, 2025  
**Status**: Active Master Plan - Single Source of Truth  
**Next Review**: Start of Phase 1 (Week 1)  
**Maintained By**: Architecture & Migration Teams
