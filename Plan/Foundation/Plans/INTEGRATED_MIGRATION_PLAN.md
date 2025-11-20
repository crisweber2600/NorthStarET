# NorthStar Integrated Migration Plan: Monolith to Microservices

**Version**: 2.1  
**Date**: November 15, 2025  
**Status**: Active - See [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) for complete integration  
**Integration Source**: MIGRATION_PLAN.md + plan-monolithToMicroservicesMigration.prompt.md + microservices documentation

> **Note**: This document has been superseded by the comprehensive [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) which integrates all migration planning documents, specifications, and architectural guidelines into a single source of truth. This document is retained for historical reference and detailed technical content.

---

## Executive Summary

This unified migration plan integrates three complementary planning documents to create a comprehensive roadmap for migrating OldNorthStar (.NET Framework 4.6 monolith) to UpgradedNorthStar (.NET 10 microservices) using Clean Architecture, .NET Aspire orchestration, and the constitutional principles defined in WIPNorthStar/AGENTS.md (Constitution v1.6.0).

**For the complete, integrated plan including all service specifications, feature specs, and implementation guides, see [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md).**

**Key Integration Points**:
1. **Constitutional Foundation** - All implementation follows Clean Architecture, TDD, Figma-backed UI, event-driven patterns
2. **Bounded Context Analysis** - 11 microservices derived from DDD analysis of 33+ legacy controllers
3. **Phased Delivery** - 22-32 week timeline across 4 phases with iterative cutover using Strangler Fig pattern
4. **WIPNorthStar Leverage** - Features 001/002/004 provide proven templates for Identity, District/School, and Clean Architecture patterns

## Constitutional Compliance

All migration work adheres to **NorthStarET NextGen LMS Constitution v1.6.0**:

✅ **Clean Architecture** - UI → Application → Domain ← Infrastructure  
✅ **.NET Aspire Orchestration** - All services hosted via AppHost  
✅ **TDD Workflow** - Red → Green → Refactor with evidence capture  
✅ **Test Coverage ≥80%** - Unit, Integration (Aspire), BDD (Reqnroll), UI (Playwright)  
✅ **Event-Driven First** - Async communication via Azure Service Bus, sync only with documented latency budgets  
✅ **Figma-Backed UI** - No UI implementation without design assets  
✅ **Phase Review Branches** - `git push origin HEAD:feature-XXX-review-PhaseN`  
✅ **Secrets Management** - Azure Key Vault, no hardcoded credentials

---

## Migration Architecture

### Target Microservices (11 Services)

Derived from bounded context analysis of OldNorthStar controllers:

| # | Service | Phase | Legacy Components | Bounded Context | Priority |
|---|---------|-------|-------------------|-----------------|----------|
| 1 | **Identity & Authentication** | 1 | IdentityServer, AuthController, PasswordResetController | Authentication & Authorization + Microsoft Entra ID | Critical |
| 2 | **API Gateway (YARP)** | 1 | NS4.WebAPI routing | API Gateway pattern | Critical |
| 3 | **Configuration** | 1 | DistrictSettingsController, NavigationController | Platform Configuration | Critical |
| 4 | **Student Management** | 2 | StudentController, StudentDashboardController, StudentImportController | Student Management | High |
| 5 | **Staff Management** | 2 | StaffController | Staff Management | High |
| 6 | **Assessment** | 2 | AssessmentController, BenchmarkController, AssessmentAvailabilityController | Assessment + Benchmark | High |
| 7 | **Intervention Management** | 3 | InterventionGroupController, InterventionToolkitController | Intervention Management | Medium |
| 8 | **Section & Roster** | 3 | SectionController, SectionDataEntryController, RosterRolloverController | Academic Sections + Class Roster | Medium |
| 9 | **Data Import & Integration** | 3 | ImportStateTestDataController, FileUploaderController, DataEntryController | Data Integration | Medium |
| 10 | **Reporting & Analytics** | 4 | LineGraphController, StackedBarGraphController, ExportDataController, PrintController | Reporting + Dashboard/Aggregation | Low |
| 11 | **Content & Media** | 4 | VideoController, HelpController | File Storage + Video/Content | Low |

**Supporting Infrastructure**:
- **System Operations** (Phase 4) - Health monitoring, diagnostics, navigation (new service for observability)

### Technology Stack

**Backend**:
- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core 10.0
- MediatR 12.4.0 (CQRS)
- FluentValidation
- Microsoft Entra ID with Microsoft.Identity.Web (OAuth 2.0/OIDC)
- Custom session authentication (SessionAuthenticationHandler)

**Data**:
- PostgreSQL (per-service databases)
- Redis (distributed caching)
- Azure Blob Storage (files/media)

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
- Docker
- Kubernetes (Azure AKS)
- GitHub Actions (CI/CD)
- Azure Application Insights (monitoring)

---

## Phased Migration Roadmap

### Phase 1: Foundation Services (Weeks 1-8)

**Objective**: Establish foundational infrastructure and security services

**Services**:
1. Identity & Authentication Service
2. API Gateway (YARP)
3. Configuration Service

**Key Deliverables**:
- ✅ Aspire AppHost orchestration configured
- ✅ Microsoft Entra ID integration for staff/admin SSO
- ✅ JWT token authentication across all services
- ✅ YARP gateway routing to legacy + new services
- ✅ PostgreSQL databases per service
- ✅ Redis caching layer
- ✅ Azure Service Bus integration
- ✅ ServiceDefaults (health checks, observability, logging)

**Migration Activities**:
- Migrate `IdentityServer` to Microsoft Entra ID with session authentication (see legacy-identityserver-migration.md)
- Extract district settings from `DistrictSettingsController`
- Configure API Gateway with routes to both legacy NS4.WebAPI and new services
- Setup dual-authentication during 90-day parallel run (legacy IdentityServer read-only, new Entra ID active)

**Testing**:
- Reqnroll BDD scenarios for login flows (local + Entra ID)
- Aspire integration tests for service discovery
- Performance testing: <50ms auth decision (P95)

**WIPNorthStar Integration**:
- Leverage Feature 001 (SSO/Auth) implementation patterns
- Complete remaining Feature 001 Phase 5-7 tasks
- Refactor `NorthStarET.NextGen.Lms.*` → `NorthStar.*` namespaces

### Phase 2: Core Domain Services (Weeks 9-16)

**Objective**: Migrate core educational domain services

**Services**:
1. Student Management Service
2. Staff Management Service
3. Assessment Service

**Key Deliverables**:
- ✅ Student enrollment, demographics, dashboard
- ✅ Staff profiles, team management
- ✅ Assessment definitions, scoring, benchmarks
- ✅ Domain events flowing via Azure Service Bus
- ✅ Data migration scripts for Students, Staff, Assessments
- ✅ Dual-write pattern (write to legacy + new DBs)

**Migration Activities**:

**Student Service**:
- Extract 383 entities from `EntityDto` to Student domain
- Migrate `StudentController`, `StudentDashboardController`, `StudentImportController`
- Map `NorthStar.EF6/DistrictContext` Student tables → PostgreSQL
- Implement commands: CreateStudent, UpdateStudent, EnrollStudent
- Implement queries: GetStudent, SearchStudents, GetStudentDashboard
- Publish events: StudentCreatedEvent, StudentEnrolledEvent, StudentDemographicsChangedEvent

**Staff Service**:
- Extract staff entities from `EntityDto`
- Migrate `StaffController` logic
- Map Staff, StaffDistrict, StaffRole tables
- Implement staff-to-team assignments
- Publish events: StaffCreatedEvent, TeamMemberAddedEvent

**Assessment Service**:
- Extract assessment entities (Assessment, AssessmentField, Benchmark)
- Migrate `AssessmentController`, `BenchmarkController`, `AssessmentAvailabilityController`
- Implement assessment result tracking
- Publish events: AssessmentCompletedEvent, BenchmarkDefinedEvent

**Data Migration Strategy**:
1. Analyze `NorthStar.EF6/DistrictContext.cs` schema
2. Create EF Core 10 migration scripts per service
3. Implement ETL jobs for historical data
4. Setup dual-write synchronization (temporary)
5. Validate data integrity with reconciliation scripts

**Testing**:
- ≥80% unit test coverage per service
- Reqnroll BDD scenarios for enrollment, assessment workflows
- Aspire integration tests for cross-service event flows
- Performance: <100ms query response (P95)

**WIPNorthStar Integration**:
- Leverage Feature 002 (Tenant Bootstrap) for district/school context
- Leverage Feature 004 (Schools/Grades) for organizational hierarchy

### Phase 3: Secondary Domain Services (Weeks 17-22)

**Objective**: Migrate intervention, roster, and data import capabilities

**Services**:
1. Intervention Management Service
2. Section & Roster Service
3. Data Import & Integration Service

**Key Deliverables**:
- ✅ Intervention groups, student assignments, attendance tracking
- ✅ Section management, rosters, automated rollover
- ✅ CSV/Excel imports, state test data integration
- ✅ Background job processing migrated to Worker Services

**Migration Activities**:

**Intervention Service**:
- Migrate `InterventionGroupController`, `InterventionToolkitController`
- Implement intervention planning and tracking
- Subscribe to AssessmentCompletedEvent for data-driven interventions
- Publish: InterventionCreatedEvent, StudentAddedToInterventionEvent

**Section & Roster Service**:
- Migrate `SectionController`, `SectionDataEntryController`, `RosterRolloverController`
- Implement section enrollment, teacher assignments
- Subscribe to StudentEnrolledEvent, StaffUpdatedEvent
- Implement automated year-end rollover (saga pattern)
- Publish: SectionCreatedEvent, StudentRosteredEvent, RolloverCompletedEvent

**Data Import Service**:
- Migrate `ImportStateTestDataController`, `FileUploaderController`, `DataEntryController`
- Implement CSV/Excel import with validation
- Support state test data formats
- Publish: DataImportCompletedEvent, StateTestDataImportedEvent

**Background Jobs**:
- Migrate `NorthStar.BatchProcessor` to .NET 8 Worker Service
- Migrate `NorthStar4.BatchPrint` to queue-based processing
- Migrate `NorthStar.AutomatedRollover` to scheduled jobs via Hangfire/Quartz

**Testing**:
- BDD scenarios for intervention workflows, roster management, data imports
- Integration tests for saga patterns (rollover coordination)
- Load testing for bulk data imports

### Phase 4: Supporting Services (Weeks 23-28)

**Objective**: Complete migration with reporting, media, and operations

**Services**:
1. Reporting & Analytics Service
2. Content & Media Service
3. System Operations Service

**Key Deliverables**:
- ✅ Report generation, data visualization, exports
- ✅ File upload, Azure Blob Storage integration, video management
- ✅ System health monitoring, diagnostics, navigation

**Migration Activities**:

**Reporting Service**:
- Migrate `LineGraphController`, `StackedBarGraphController`, `ExportDataController`, `PrintController`
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
- Implement comprehensive health checks (database, message bus, Redis)
- Setup Application Insights integration
- Expose Prometheus metrics
- Provide diagnostic endpoints

**Testing**:
- UI tests (Playwright) for report generation workflows
- Performance testing for large report generation
- Load testing for file uploads

---

## Migration Execution Steps

### Step 1: Inventory and Component Mapping (Weeks 1-2)

**Objective**: Create comprehensive mapping of OldNorthStar to microservice boundaries

**Actions**:
1. Analyze 33+ controllers in `NS4.WebAPI/Controllers`:
   - Student: Student, StudentReporting, StudentImport, StudentDashboard
   - Assessment: Assessment, BenchmarkAssessment, AssessmentAvailability, DataValidation
   - Staff: Staff, Team, StaffDistrict
   - Sections: Section, SectionDataEntry, SectionReport, RosterRollover
   - Interventions: InterventionGroup, InterventionDashboard, InterventionToolkit
   - District/Settings: DistrictSettings, School, Grade, Calendar, Navigation
   - Reporting: Report, LineGraph, StackedBarGraph, ExportData, Print
   - Data: ImportStateTestData, FileUploader, DataEntry
   - Media: Video, Help
   - Auth: Auth, PasswordReset

2. Map 383 entities in `EntityDto` to bounded contexts and domain aggregates

3. Analyze `NorthStar.EF6` DbContexts:
   - `DistrictContext` - primary database with student, staff, assessment data
   - `LoginContext` - cross-district authentication database

4. Document controller→service mapping with dependency graph

**Deliverables**:
- `docs/COMPONENT_INVENTORY.md` - Complete controller/entity mapping
- `docs/DEPENDENCY_GRAPH.md` - Service dependencies with event flows
- `docs/DATABASE_SCHEMA_ANALYSIS.md` - Table-to-service mapping

**Duration**: 1-2 weeks

### Step 2: Define Service Contracts (Weeks 2-4)

**Objective**: Establish clear API contracts and event schemas

**Actions**:
1. Create contract assemblies in `UpgradedNorthStar/src/Contracts`:
   ```
   NorthStar.Contracts/
   ├── Identity/
   │   ├── Commands/ (LoginCommand, RegisterUserCommand)
   │   ├── Queries/ (GetUserQuery, GetClaimsQuery)
   │   ├── Events/ (UserLoggedInEvent, PasswordChangedEvent)
   │   └── DTOs/ (UserDto, ClaimDto)
   ├── Students/
   │   ├── Commands/ (CreateStudentCommand, EnrollStudentCommand)
   │   ├── Queries/ (GetStudentQuery, SearchStudentsQuery)
   │   ├── Events/ (StudentCreatedEvent, StudentEnrolledEvent)
   │   └── DTOs/ (StudentDto, EnrollmentDto)
   └── ... (repeat for all services)
   ```

2. Define OpenAPI specifications for each service:
   - Versioned API routes: `/api/v1/{resource}`
   - Consistent error responses
   - Pagination patterns
   - Filtering/sorting conventions

3. Define event schemas (Azure Service Bus message contracts):
   - Event naming: `{Entity}{Action}Event`
   - Include correlation IDs, timestamps, causation IDs
   - Define event versioning strategy

4. Create shared kernel in `NorthStar.Common`:
   - Value objects: StudentId, AssessmentId, DistrictId
   - Common exceptions: DomainException, NotFoundException
   - Pagination types: PagedResult<T>
   - Base entities: BaseEntity, AuditableEntity

**Deliverables**:
- Contract assemblies with versioned DTOs
- `docs/API_CONTRACTS.md` - OpenAPI specifications per service
- `docs/EVENT_CATALOG.md` - Event schemas with examples
- `docs/INTEGRATION_PATTERNS.md` - Sync/async communication rules

**Duration**: 2 weeks

### Step 3: Setup UpgradedNorthStar Foundation (Weeks 3-5)

**Objective**: Create Aspire-orchestrated microservice infrastructure

**Actions**:
1. Create solution structure:
   ```bash
   cd d:\NorthStarPlan\Src\UpgradedNorthStar
   dotnet new sln -n NorthStar
   
   # Create shared projects
   dotnet new classlib -n NorthStar.Contracts
   dotnet new classlib -n NorthStar.Common
   
   # Create Aspire projects
   dotnet new aspire-apphost -n NorthStar.AppHost
   dotnet new classlib -n NorthStar.ServiceDefaults
   
   # Add to solution
   dotnet sln add src/NorthStar.Contracts
   dotnet sln add src/NorthStar.Common
   dotnet sln add src/NorthStar.AppHost
   dotnet sln add src/NorthStar.ServiceDefaults
   ```

2. Configure ServiceDefaults:
   - OpenTelemetry instrumentation
   - Health check base configuration
   - Logging with structured output
   - Service discovery helpers

3. Setup centralized package management in `Directory.Packages.props`:
   ```xml
   <Project>
     <PropertyGroup>
       <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
     </PropertyGroup>
     <ItemGroup>
       <PackageVersion Include="Aspire.Hosting.AppHost" Version="13.0.0" />
       <PackageVersion Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.0" />
       <PackageVersion Include="MediatR" Version="12.4.0" />
       <PackageVersion Include="MassTransit.Azure.ServiceBus.Core" Version="8.x" />
       <PackageVersion Include="Microsoft.Identity.Web" Version="3.x" />
       <PackageVersion Include="Microsoft.Identity.Web.MicrosoftGraph" Version="3.x" />
       <PackageVersion Include="StackExchange.Redis" Version="2.x" />
       <!-- ... -->
     </ItemGroup>
   </Project>
   ```

4. Configure Aspire AppHost:
   ```csharp
   var builder = DistributedApplication.CreateBuilder(args);
   
   // Infrastructure
   var postgres = builder.AddPostgres("postgres")
       .WithDataVolume()
       .WithPgAdmin();
   
   var redis = builder.AddRedis("redis");
   
   var serviceBus = builder.AddAzureServiceBus("servicebus");
   
   // Databases
   var identityDb = postgres.AddDatabase("identitydb");
   var studentsDb = postgres.AddDatabase("studentsdb");
   var assessmentsDb = postgres.AddDatabase("assessmentsdb");
   
   // Services (to be added incrementally)
   var identity = builder.AddProject<Projects.NorthStar_Identity_Api>("identity-api")
       .WithReference(identityDb)
       .WithReference(redis)
       .WithReference(serviceBus);
   
   var gateway = builder.AddProject<Projects.NorthStar_Gateway>("gateway")
       .WithReference(identity);
   
   builder.Build().Run();
   ```

**Deliverables**:
- Aspire solution with AppHost and ServiceDefaults
- Shared contract and common libraries
- Directory.Packages.props with centralized dependencies
- Initial infrastructure configuration (PostgreSQL, Redis, Azure Service Bus)

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

**Identity Service Example**:
```bash
# Create projects
dotnet new webapi -n NorthStar.Identity.Api
dotnet new classlib -n NorthStar.Identity.Application
dotnet new classlib -n NorthStar.Identity.Domain
dotnet new classlib -n NorthStar.Identity.Infrastructure

# Create test projects
dotnet new xunit -n NorthStar.Identity.UnitTests
dotnet new xunit -n NorthStar.Identity.IntegrationTests
# Add Reqnroll project for BDD
# Add Playwright project for UI tests
```

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
- Microsoft Entra ID SSO working for staff/admin
- API Gateway routing to legacy + new services

**Duration**: 4 weeks

### Step 5: Data Migration Strategy (Weeks 6-10, parallel with Step 4)

**Objective**: Plan and execute database migration from SQL Server to PostgreSQL

**Actions**:

1. **Schema Analysis**:
   - Map `NorthStar.EF6` Entity Framework 6 models to EF Core 10
   - Identify cross-table dependencies requiring coordination
   - Document foreign key relationships to be replaced with events

2. **Per-Service Database Creation**:
   ```sql
   -- Students database
   CREATE DATABASE northstar_students;
   -- Tables: Students, StudentEnrollments, StudentDemographics, StudentContacts
   
   -- Assessments database
   CREATE DATABASE northstar_assessments;
   -- Tables: Assessments, AssessmentFields, AssessmentResults, Benchmarks
   
   -- ... (repeat for all services)
   ```

3. **EF Core Migrations**:
   ```bash
   cd src/NorthStar.Students.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../NorthStar.Students.Api
   dotnet ef database update --startup-project ../NorthStar.Students.Api
   ```

4. **Data Migration ETL**:
   - Create ETL jobs to copy historical data from legacy SQL Server to PostgreSQL
   - Handle data transformations (e.g., denormalized → normalized)
   - Validate data integrity with reconciliation scripts

5. **Dual-Write Pattern** (temporary during cutover):
   - Write to both legacy DB and new service DB
   - Background sync jobs reconcile data
   - Monitoring dashboards show data consistency

6. **Foreign Key Elimination**:
   - Replace database FKs with application-level references (StudentId as value object)
   - Use eventual consistency for cross-service data
   - Implement outbox pattern for reliable event publishing

**Deliverables**:
- EF Core DbContexts per service
- Migration scripts with rollback procedures
- ETL jobs for historical data migration
- Data validation reports
- Dual-write synchronization monitoring

**Duration**: 4 weeks (parallel with service implementation)

### Step 6: Implement Phase 2 Services (Weeks 9-16)

**Objective**: Deploy core domain services (Student, Staff, Assessment)

**Follow same process as Step 4 for each service**

**Key Integration Points**:
- Student Service subscribes to `SectionCreatedEvent`
- Assessment Service subscribes to `StudentEnrolledEvent`
- All services publish domain events via Azure Service Bus
- YARP Gateway routes traffic to new services

**Testing Focus**:
- End-to-end workflows: Student enrollment → Assessment assignment → Score entry
- Event-driven integration: Verify event publication/subscription
- Performance: <100ms query response (P95), <50ms auth (P95)
- Data consistency: Validate dual-write reconciliation

**Deliverables**:
- Student, Staff, Assessment services deployed
- Domain events flowing across services
- Data migration completed for Students, Staff, Assessments
- BDD scenarios passing for core workflows

**Duration**: 8 weeks

### Step 7: Implement Phase 3 Services (Weeks 17-22)

**Objective**: Deploy secondary domain services (Intervention, Section, Data Import)

**Key Challenges**:
- **Section Rollover**: Complex saga pattern coordinating multiple services
- **Data Import**: Bulk processing with validation and error handling
- **Background Jobs**: Migrate batch processors to Worker Services

**Testing Focus**:
- Saga patterns: Automated rollover coordination
- Bulk operations: Load testing with 10,000+ student imports
- Idempotency: Verify duplicate prevention in event handlers

**Deliverables**:
- Intervention, Section, Data Import services deployed
- Background job processing migrated
- Saga patterns implemented and tested

**Duration**: 6 weeks

### Step 8: Implement Phase 4 Services (Weeks 23-28)

**Objective**: Complete migration with Reporting, Media, Operations services

**Key Focus**:
- **Reporting**: CQRS read models from event sourcing
- **Media**: Azure Blob Storage integration, video transcoding
- **Operations**: Comprehensive monitoring, health checks

**Testing Focus**:
- Report generation performance (large datasets)
- File upload/download reliability
- Health check coverage

**Deliverables**:
- All 11 microservices deployed
- Complete observability (Application Insights, Prometheus)
- Feature parity with OldNorthStar

**Duration**: 6 weeks

### Step 9: UI Migration (Weeks 20-28, parallel with Phases 3-4)

**Objective**: Migrate NS4.Angular to modern UI

**Blockers**: ⚠️ Requires Figma design assets (constitutional requirement)

**Options**:
1. **Blazor Web App** - Aligns with .NET stack, server-side rendering + WebAssembly
2. **Angular 18** - Incremental migration from AngularJS
3. **Next.js + React** - Modern SPA with SSR

**Recommended**: Blazor Web App for .NET ecosystem alignment

**Actions**:
1. Wait for Figma design completion
2. Create Blazor project structure
3. Integrate WIPNorthStar UI from Features 001/002/004
4. Implement authentication flows (Entra ID SSO)
5. Migrate screens incrementally:
   - Student dashboard
   - Assessment entry
   - Intervention planning
   - Reporting interface

**Testing**:
- Playwright UI tests for all user workflows
- Figma design validation
- Accessibility testing (WCAG 2.1 AA)

**Deliverables**:
- Modern UI application (Blazor)
- Playwright E2E tests
- Figma design compliance
- API client libraries

**Duration**: 8 weeks (after Figma unblock)

### Step 10: Iterative Cutover (Weeks 29-32)

**Objective**: Gradually route production traffic using Strangler Fig pattern

**Cutover Strategy**:

**Week 29-30: Foundation Services**:
- Identity Service: 10% → 50% → 100% traffic over 2 weeks
- Monitor authentication success rates, latency
- Validate Entra ID SSO working for staff

**Week 30-31: Core Services**:
- Student Service: 10% → 50% → 100%
- Staff Service: 10% → 50% → 100%
- Assessment Service: 10% → 50% → 100%
- Monitor data consistency, event flows

**Week 31-32: Secondary Services**:
- Intervention, Section, Data Import: gradual rollout
- Reporting, Media: gradual rollout
- Final cutover of all services

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

**Deliverables**:
- 100% traffic on new microservices
- Legacy system decommissioned (after 4-week stability period)
- Production deployment complete

**Duration**: 4 weeks

---

## Data Migration Specifications

### Entity Mapping: OldNorthStar → Microservices

**Source**: `EntityDto` (383 entity files) + `NorthStar.EF6/DistrictContext`

| Legacy Entity/Table | Target Service | Target Entity | Notes |
|---------------------|----------------|---------------|-------|
| Student, StudentContact, StudentDemographic | Student Management | Student, StudentEnrollment, StudentDemographics | Normalize denormalized fields |
| Staff, StaffDistrict, StaffRole | Staff Management | Staff, StaffAssignment, Role | Extract role management |
| Assessment, AssessmentField, AssessmentFieldCategory | Assessment | Assessment, AssessmentField, AssessmentFieldGroup | Preserve field hierarchy |
| Benchmark, BenchmarkGrade | Assessment | Benchmark | Link to assessments |
| InterventionGroup, InterventionStudent, InterventionAttendance | Intervention | Intervention, InterventionGroup, InterventionAttendance | Preserve group structure |
| Section, SectionStudent, SectionStaff | Section & Roster | Section, SectionRoster, SectionSchedule | Many-to-many relationships |
| ImportJob, ImportError, ImportValidation | Data Import | DataImport, ImportValidation, ImportError | Preserve import history |
| DistrictSettings, Calendar, CalendarEvent | Configuration | DistrictSettings, Calendar | Per-district configuration |
| AspNetUsers, AspNetRoles, AspNetUserClaims | Identity | User, Role, Claim | Migrate to Identity schema with Entra ID provider links (see legacy-identityserver-migration.md) |

### ETL Process

**Tools**: Custom .NET 8 console application using EF Core

**Process**:
1. Connect to legacy SQL Server `DistrictContext`
2. Read entities in batches (pagination)
3. Transform:
   - Map legacy IDs to new GUIDs
   - Denormalize → normalize schema changes
   - Handle null values, data validation
4. Load into PostgreSQL service databases
5. Validate with reconciliation queries

**Example ETL Script**:
```csharp
// src/DataMigration/StudentMigrationJob.cs
public class StudentMigrationJob
{
    private readonly LegacyDbContext _legacyContext;
    private readonly StudentDbContext _newContext;
    
    public async Task MigrateAsync(CancellationToken ct)
    {
        var batchSize = 1000;
        var skip = 0;
        
        while (true)
        {
            var legacyStudents = await _legacyContext.Students
                .Include(s => s.Contacts)
                .Include(s => s.Demographics)
                .OrderBy(s => s.Id)
                .Skip(skip)
                .Take(batchSize)
                .ToListAsync(ct);
            
            if (!legacyStudents.Any()) break;
            
            var newStudents = legacyStudents.Select(MapToNewStudent).ToList();
            
            await _newContext.Students.AddRangeAsync(newStudents, ct);
            await _newContext.SaveChangesAsync(ct);
            
            skip += batchSize;
            Console.WriteLine($"Migrated {skip} students");
        }
    }
    
    private Student MapToNewStudent(LegacyStudent legacy)
    {
        return new Student
        {
            Id = Guid.NewGuid(),
            LegacyId = legacy.Id, // for reconciliation
            FirstName = legacy.FirstName,
            LastName = legacy.LastName,
            DateOfBirth = legacy.BirthDate,
            GradeLevel = legacy.Grade,
            // ... map all fields
        };
    }
}
```

---

## Testing Strategy

### Test Pyramid

```
        /\
       /UI\ Playwright (E2E)
      /----\
     /  BDD \ Reqnroll (Acceptance)
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
   # Create test
   # Run: dotnet test
   # Capture: dotnet test > docs/evidence/feature-XXX-red.txt
   ```

2. **GREEN**: Implement feature to pass test
   ```bash
   # Implement code
   # Run: dotnet test
   # Capture: dotnet test > docs/evidence/feature-XXX-green.txt
   ```

3. **REFACTOR**: Optimize while maintaining tests
   ```bash
   # Refactor
   # Verify: dotnet test (all still pass)
   ```

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
- UI tests for all Figma-backed screens

**Example Unit Test**:
```csharp
public class StudentServiceTests
{
    [Fact]
    public async Task CreateStudent_WithValidData_PublishesStudentCreatedEvent()
    {
        // Arrange
        var mockRepo = new Mock<IStudentRepository>();
        var mockEventPublisher = new Mock<IEventPublisher>();
        var service = new StudentService(mockRepo.Object, mockEventPublisher.Object);
        
        var command = new CreateStudentCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(2010, 1, 1),
            GradeLevel = 5
        };
        
        // Act
        var result = await service.CreateStudentAsync(command);
        
        // Assert
        Assert.NotNull(result);
        mockEventPublisher.Verify(
            p => p.PublishAsync(It.Is<StudentCreatedEvent>(
                e => e.StudentId == result.Id)), 
            Times.Once);
    }
}
```

**Example BDD Test** (Reqnroll):
```gherkin
Feature: Student Enrollment
  As a school administrator
  I want to enroll students
  So that they can access the LMS

Scenario: Enroll new student successfully
  Given I am authenticated as a school administrator
  And I have a valid student enrollment form
  When I submit the student enrollment
  Then a new student record is created
  And a StudentEnrolledEvent is published
  And I see a success confirmation message
```

**Example Integration Test** (Aspire):
```csharp
public class StudentServiceIntegrationTests : IClassFixture<AspireTestFixture>
{
    private readonly AspireTestFixture _fixture;
    
    public StudentServiceIntegrationTests(AspireTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task CreateStudent_IntegratesWithDatabase()
    {
        // Arrange
        var client = _fixture.CreateHttpClient("students-api");
        var request = new CreateStudentRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            // ...
        };
        
        // Act
        var response = await client.PostAsJsonAsync("/api/v1/students", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(student);
        Assert.Equal("Jane", student.FirstName);
        
        // Verify in database
        var dbStudent = await _fixture.GetDatabaseContext<StudentDbContext>()
            .Students.FindAsync(student.Id);
        Assert.NotNull(dbStudent);
    }
}
```

---

## Risk Management

### High Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Data Migration Failures** | Critical | Medium | Extensive validation, dual-write period, rollback scripts |
| **Performance Degradation** | High | Medium | Load testing, caching strategy (Redis), performance budgets |
| **Microsoft Entra ID Integration Issues** | High | Low | Early PoC, Microsoft support engagement, fallback to local auth |
| **Distributed Transaction Complexity** | High | Medium | Saga patterns, idempotent operations, event sourcing |
| **Feature Parity Gaps** | Medium | Medium | Comprehensive feature mapping, acceptance criteria, user testing |

### Medium Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Team Learning Curve** | Medium | High | Training, pairing, documentation, WIPNorthStar examples |
| **Timeline Slippage** | Medium | Medium | Buffer time, MVP approach, prioritization, agile sprints |
| **UI Migration Blocking** | Medium | High | Figma design prioritization, parallel backend work |
| **Third-Party Service Dependencies** | Medium | Low | Vendor SLAs, circuit breakers, fallback mechanisms |

### Low Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **Kubernetes Orchestration Complexity** | Low | Low | Aspire simplification, managed AKS, DevOps expertise |
| **Secrets Management Issues** | Medium | Low | Azure Key Vault, automated rotation, auditing |

---

## Success Criteria

✅ **Functional**:
- All OldNorthStar functionality available in UpgradedNorthStar
- Microsoft Entra ID SSO working for staff/administrators
- Zero data loss during migration
- Feature parity validated by user acceptance testing

✅ **Architectural**:
- All services follow Clean Architecture (UI → Application → Domain ← Infrastructure)
- All services orchestrated via .NET Aspire
- Event-driven communication via Azure Service Bus
- No direct UI → Infrastructure coupling

✅ **Quality**:
- ≥80% test coverage across all services
- All Reqnroll BDD scenarios passing
- All Aspire integration tests passing
- All Playwright UI tests passing (post-Figma)

✅ **Performance**:
- Authorization decisions <50ms (P95)
- API response times <100ms (P95)
- Report generation <5s for standard reports
- File uploads support up to 100MB

✅ **Security**:
- Zero hardcoded credentials (all in Key Vault)
- All secrets managed via Azure Key Vault
- OAuth 2.0/OIDC authentication enforced
- Role-based authorization on all endpoints

✅ **Operations**:
- Comprehensive health checks on all services
- Distributed tracing via OpenTelemetry
- Centralized logging in Application Insights
- Prometheus metrics exposed
- Automated deployments via GitHub Actions

✅ **Documentation**:
- API documentation (OpenAPI/Swagger)
- Architecture Decision Records (ADRs)
- Deployment runbooks
- User documentation

---

## Deliverables Checklist

### Phase 1 (Foundation)
- [ ] Aspire AppHost configured
- [ ] ServiceDefaults implemented
- [ ] Identity Service with Entra ID integration
- [ ] API Gateway (YARP) routing
- [ ] Configuration Service
- [ ] PostgreSQL databases per service
- [ ] Azure Service Bus integration
- [ ] Redis caching layer

### Phase 2 (Core Domain)
- [ ] Student Management Service
- [ ] Staff Management Service
- [ ] Assessment Service
- [ ] Data migration scripts
- [ ] Dual-write synchronization
- [ ] Domain events flowing

### Phase 3 (Secondary Domain)
- [ ] Intervention Management Service
- [ ] Section & Roster Service
- [ ] Data Import Service
- [ ] Background job migration
- [ ] Saga patterns implemented

### Phase 4 (Supporting Services)
- [ ] Reporting & Analytics Service
- [ ] Content & Media Service
- [ ] System Operations Service
- [ ] Complete observability

### UI Migration
- [ ] Figma designs complete
- [ ] Blazor Web App (or chosen framework)
- [ ] Playwright UI tests
- [ ] Accessibility compliance

### Production Deployment
- [ ] All services in production
- [ ] Feature flags for gradual rollout
- [ ] Legacy system decommissioned
- [ ] User acceptance complete

---

## Next Immediate Actions

1. **Review Master Plan** - See [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) for complete integrated plan
2. **Confirm Migration Plan** - Stakeholder approval
3. **Start Step 1** - Create `COMPONENT_INVENTORY.md` by analyzing OldNorthStar
4. **Complete WIPNorthStar** - Finish Feature 001 Phases 5-7
5. **Unblock UI** - Proceed using existing OldNorthStar layouts for Features 001/002/004 (Figma designs may be referenced for future enhancements, not required for migration)
6. **Setup Project Tracking** - Create GitHub project with issues for each deliverable

## Additional Resources

**Comprehensive Planning**:
- [MASTER_MIGRATION_PLAN.md](./MASTER_MIGRATION_PLAN.md) - Complete integrated plan (v3.0)
- [microservices/SERVICE_CATALOG.md](./microservices/SERVICE_CATALOG.md) - All 11 service specifications
- [../specs/](./../specs/) - 9 feature specifications
- [docs/DATA_MIGRATION_SPECIFICATION.md](./docs/DATA_MIGRATION_SPECIFICATION.md) - Data migration strategy
- [Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md](./Src/WIPNorthStar/NorthStarET.Lms/.specify/memory/constitution.md) - Constitution v1.6.0

---

**Document Version**: 2.1  
**Integration Date**: November 15, 2025  
**Status**: Superseded by MASTER_MIGRATION_PLAN.md - Retained for reference  
**Next Review**: Start of Phase 1 (Week 1)
