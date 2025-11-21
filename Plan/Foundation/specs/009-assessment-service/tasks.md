# Tasks: Assessment Service Migration

**Specification Branch**: `Foundation/009-assessment-service-migration-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/009-assessment-service-migration` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/009-assessment-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Assessment`  
**Specification Path**: `Plan/Foundation/specs/009-assessment-service/`

### Layer Consistency Checklist

- [x] Target Layer matches spec.md Layer Identification
- [x] Target Layer matches plan.md Layer Identification
- [x] Implementation path follows layer structure (`Src/Foundation/services/Assessment`)
- [x] Specification path follows layer structure (`Plan/Foundation/specs/009-assessment-service/`)
- [x] Shared infrastructure dependencies match between spec and plan
- [x] Cross-layer dependencies justified in both spec and plan (Configuration, Student, Section via events/APIs)

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] T001 Verify project references ONLY shared infrastructure from `Src/Foundation/shared/*`
- [ ] T002 Verify NO direct service-to-service references across layers (use events/contracts)
- [ ] T003 Verify AppHost orchestration includes Assessment service with correct layer isolation
- [ ] T004 Verify README.md documents layer position and shared infrastructure dependencies
- [ ] T005 Verify no circular dependencies between layers

---

## Identity & Authentication Compliance

*MANDATORY: Include if this feature requires authentication/authorization*

- [ ] T006 Verify NO references to Duende IdentityServer or custom token issuance
- [ ] T007 Verify Microsoft.Identity.Web used for JWT token validation (NOT custom JWT generation)
- [ ] T008 Verify SessionAuthenticationHandler registered for session-based API authorization
- [ ] T009 Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] T010 Verify authentication flow follows `legacy-identityserver-migration.md` architecture

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T011 Create Assessment service solution structure at `Src/Foundation/services/Assessment/`
- [ ] T012 [P] Create Assessment.Api project with .NET 8 and Aspire references in `Src/Foundation/services/Assessment/Assessment.Api/`
- [ ] T013 [P] Create Assessment.Domain project with domain entities in `Src/Foundation/services/Assessment/Assessment.Domain/`
- [ ] T014 [P] Create Assessment.Application project with CQRS patterns in `Src/Foundation/services/Assessment/Assessment.Application/`
- [ ] T015 [P] Create Assessment.Infrastructure project with EF Core and messaging in `Src/Foundation/services/Assessment/Assessment.Infrastructure/`
- [ ] T016 [P] Create Assessment.Messaging project for event contracts in `Src/Foundation/services/Assessment/Assessment.Messaging/`
- [ ] T017 [P] Create Assessment.Tests project for unit and integration tests in `tests/assessment-service/`
- [ ] T018 Configure centralized package management in `Src/Foundation/services/Assessment/Directory.Packages.props`
- [ ] T019 Add Assessment service to AppHost orchestration in `Src/Foundation/AppHost/Program.cs`
- [ ] T020 [P] Create README.md documenting service architecture at `Src/Foundation/services/Assessment/README.md`
- [ ] T021 [P] Configure solution-level .editorconfig and style rules in `Src/Foundation/services/Assessment/.editorconfig`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database Infrastructure

- [ ] T022 Create AssessmentDbContext with tenant isolation in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/AssessmentDbContext.cs`
- [ ] T023 [P] Configure PostgreSQL connection with Aspire in `Src/Foundation/services/Assessment/Assessment.Infrastructure/DependencyInjection.cs`
- [ ] T024 [P] Implement TenantInterceptor for automatic tenant filtering in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Interceptors/TenantInterceptor.cs`
- [ ] T025 [P] Implement AuditInterceptor for change tracking in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Interceptors/AuditInterceptor.cs`
- [ ] T026 Configure entity mappings and global query filters in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/`
- [ ] T027 Create initial database migration for Assessment schema in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Migrations/`

### Authentication & Authorization

- [ ] T028 Register SessionAuthenticationHandler in API Program.cs at `Src/Foundation/services/Assessment/Assessment.Api/Program.cs`
- [ ] T029 [P] Configure tenant context service in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/ITenantContext.cs`
- [ ] T030 [P] Implement authorization policies for assessment operations in `Src/Foundation/services/Assessment/Assessment.Api/Authorization/`

### Messaging Infrastructure

- [ ] T031 Configure MassTransit with Azure Service Bus in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Messaging/MassTransitConfiguration.cs`
- [ ] T032 [P] Create base event contracts with tenant_id and correlation_id in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/`
- [ ] T033 [P] Implement event publishing service interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/IEventPublisher.cs`
- [ ] T034 [P] Implement event publishing service with MassTransit in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Messaging/EventPublisher.cs`

### Validation & Error Handling

- [ ] T035 Configure FluentValidation pipeline in `Src/Foundation/services/Assessment/Assessment.Application/DependencyInjection.cs`
- [ ] T036 [P] Create global exception handler middleware in `Src/Foundation/services/Assessment/Assessment.Api/Middleware/GlobalExceptionHandler.cs`
- [ ] T037 [P] Implement Result pattern for command/query responses in `Src/Foundation/services/Assessment/Assessment.Application/Common/Models/Result.cs`

### Caching Infrastructure (Optional)

- [ ] T038 Configure Redis for trend caching with Aspire in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Caching/`
- [ ] T039 [P] Implement caching service interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/ICacheService.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Create Assessments and Assign to Rosters (Priority: P1) 🎯 MVP

**Goal**: Educators/admins create assessments with custom fields and assign to rosters/groups

**Independent Test**: Create an assessment with custom fields, assign it to a roster, verify assignment events and visibility for targeted students

### Domain Layer (US1)

- [ ] T040 [P] [US1] Create AssessmentDefinition aggregate root in `Src/Foundation/services/Assessment/Assessment.Domain/Aggregates/AssessmentDefinition.cs`
- [ ] T041 [P] [US1] Create AssessmentAssignment entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/AssessmentAssignment.cs`
- [ ] T042 [P] [US1] Create AssessmentStatus enum (Draft/Published/Archived) in `Src/Foundation/services/Assessment/Assessment.Domain/Enums/AssessmentStatus.cs`
- [ ] T043 [P] [US1] Create AssignmentStatus enum (Assigned/InProgress/Submitted/Closed) in `Src/Foundation/services/Assessment/Assessment.Domain/Enums/AssignmentStatus.cs`
- [ ] T044 [P] [US1] Create CustomFieldDefinition value object in `Src/Foundation/services/Assessment/Assessment.Domain/ValueObjects/CustomFieldDefinition.cs`
- [ ] T045 [US1] Implement domain validation for assessment definitions in `Src/Foundation/services/Assessment/Assessment.Domain/Aggregates/AssessmentDefinition.cs`
- [ ] T046 [US1] Implement idempotency check for assignments in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/AssessmentAssignment.cs`

### Application Layer - Commands (US1)

- [ ] T047 [P] [US1] Create CreateAssessmentCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/CreateAssessment/CreateAssessmentCommand.cs`
- [ ] T048 [P] [US1] Create CreateAssessmentCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/CreateAssessment/CreateAssessmentCommandValidator.cs`
- [ ] T049 [US1] Implement CreateAssessmentCommandHandler with event publishing in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/CreateAssessment/CreateAssessmentCommandHandler.cs`
- [ ] T050 [P] [US1] Create UpdateAssessmentCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/UpdateAssessment/UpdateAssessmentCommand.cs`
- [ ] T051 [P] [US1] Create UpdateAssessmentCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/UpdateAssessment/UpdateAssessmentCommandValidator.cs`
- [ ] T052 [US1] Implement UpdateAssessmentCommandHandler with versioning in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/UpdateAssessment/UpdateAssessmentCommandHandler.cs`
- [ ] T053 [P] [US1] Create AssignAssessmentCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/AssignAssessment/AssignAssessmentCommand.cs`
- [ ] T054 [P] [US1] Create AssignAssessmentCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/AssignAssessment/AssignAssessmentCommandValidator.cs`
- [ ] T055 [US1] Implement AssignAssessmentCommandHandler with idempotency and event publishing in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Commands/AssignAssessment/AssignAssessmentCommandHandler.cs`

### Application Layer - Queries (US1)

- [ ] T056 [P] [US1] Create GetAssessmentsQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Queries/GetAssessments/GetAssessmentsQuery.cs`
- [ ] T057 [US1] Implement GetAssessmentsQueryHandler with filtering in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Queries/GetAssessments/GetAssessmentsQueryHandler.cs`
- [ ] T058 [P] [US1] Create GetAssessmentByIdQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Queries/GetAssessmentById/GetAssessmentByIdQuery.cs`
- [ ] T059 [US1] Implement GetAssessmentByIdQueryHandler in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Queries/GetAssessmentById/GetAssessmentByIdQueryHandler.cs`
- [ ] T060 [P] [US1] Create GetAssignmentsQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Queries/GetAssignments/GetAssignmentsQuery.cs`
- [ ] T061 [US1] Implement GetAssignmentsQueryHandler with roster filtering in `Src/Foundation/services/Assessment/Assessment.Application/Assessments/Queries/GetAssignments/GetAssignmentsQueryHandler.cs`

### Infrastructure Layer (US1)

- [ ] T062 [P] [US1] Create IAssessmentRepository interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/IAssessmentRepository.cs`
- [ ] T063 [US1] Implement AssessmentRepository with EF Core in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Repositories/AssessmentRepository.cs`
- [ ] T064 [P] [US1] Configure AssessmentDefinition entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/AssessmentDefinitionConfiguration.cs`
- [ ] T065 [P] [US1] Configure AssessmentAssignment entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/AssessmentAssignmentConfiguration.cs`
- [ ] T066 [US1] Create migration for AssessmentDefinition and AssessmentAssignment tables in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Migrations/`

### Event Contracts (US1)

- [ ] T067 [P] [US1] Create AssessmentCreated event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentCreated.cs`
- [ ] T068 [P] [US1] Create AssessmentAssigned event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentAssigned.cs`

### API Endpoints (US1)

- [ ] T069 [US1] Implement POST /api/assessments endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/AssessmentsEndpoints.cs`
- [ ] T070 [US1] Implement GET /api/assessments endpoint with filtering in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/AssessmentsEndpoints.cs`
- [ ] T071 [US1] Implement GET /api/assessments/{id} endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/AssessmentsEndpoints.cs`
- [ ] T072 [US1] Implement PUT /api/assessments/{id} endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/AssessmentsEndpoints.cs`
- [ ] T073 [US1] Implement POST /api/assessments/{id}/assignments endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/AssignmentsEndpoints.cs`
- [ ] T074 [US1] Implement GET /api/assessments/assignments endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/AssignmentsEndpoints.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional - can create assessments, assign to rosters, and emit events

---

## Phase 4: User Story 2 - Record Results and Apply Benchmarks (Priority: P2)

**Goal**: Authorized staff record results with rubric/weighted scoring and benchmark classification per student

**Independent Test**: Record results for assigned students, verify benchmark category calculation, ensure auditing and notifications for score changes

### Domain Layer (US2)

- [ ] T075 [P] [US2] Create AssessmentResult entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/AssessmentResult.cs`
- [ ] T076 [P] [US2] Create Benchmark entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/Benchmark.cs`
- [ ] T077 [P] [US2] Create BenchmarkLevel enum in `Src/Foundation/services/Assessment/Assessment.Domain/Enums/BenchmarkLevel.cs`
- [ ] T078 [P] [US2] Create WeightedScore value object in `Src/Foundation/services/Assessment/Assessment.Domain/ValueObjects/WeightedScore.cs`
- [ ] T079 [US2] Implement benchmark calculation logic in `Src/Foundation/services/Assessment/Assessment.Domain/Services/BenchmarkCalculator.cs`
- [ ] T080 [US2] Implement benchmark overlap validation in `Src/Foundation/services/Assessment/Assessment.Domain/Services/BenchmarkValidator.cs`
- [ ] T081 [US2] Implement score correction with audit tracking in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/AssessmentResult.cs`

### Application Layer - Commands (US2)

- [ ] T082 [P] [US2] Create ConfigureBenchmarksCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Benchmarks/Commands/ConfigureBenchmarks/ConfigureBenchmarksCommand.cs`
- [ ] T083 [P] [US2] Create ConfigureBenchmarksCommandValidator with overlap checking in `Src/Foundation/services/Assessment/Assessment.Application/Benchmarks/Commands/ConfigureBenchmarks/ConfigureBenchmarksCommandValidator.cs`
- [ ] T084 [US2] Implement ConfigureBenchmarksCommandHandler in `Src/Foundation/services/Assessment/Assessment.Application/Benchmarks/Commands/ConfigureBenchmarks/ConfigureBenchmarksCommandHandler.cs`
- [ ] T085 [P] [US2] Create RecordResultCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Results/Commands/RecordResult/RecordResultCommand.cs`
- [ ] T086 [P] [US2] Create RecordResultCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Results/Commands/RecordResult/RecordResultCommandValidator.cs`
- [ ] T087 [US2] Implement RecordResultCommandHandler with benchmark calculation and event publishing in `Src/Foundation/services/Assessment/Assessment.Application/Results/Commands/RecordResult/RecordResultCommandHandler.cs`
- [ ] T088 [P] [US2] Create CorrectResultCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Results/Commands/CorrectResult/CorrectResultCommand.cs`
- [ ] T089 [P] [US2] Create CorrectResultCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Results/Commands/CorrectResult/CorrectResultCommandValidator.cs`
- [ ] T090 [US2] Implement CorrectResultCommandHandler with audit and change notification in `Src/Foundation/services/Assessment/Assessment.Application/Results/Commands/CorrectResult/CorrectResultCommandHandler.cs`

### Application Layer - Queries (US2)

- [ ] T091 [P] [US2] Create GetResultsQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Results/Queries/GetResults/GetResultsQuery.cs`
- [ ] T092 [US2] Implement GetResultsQueryHandler with student/roster filtering in `Src/Foundation/services/Assessment/Assessment.Application/Results/Queries/GetResults/GetResultsQueryHandler.cs`
- [ ] T093 [P] [US2] Create GetBenchmarksQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Benchmarks/Queries/GetBenchmarks/GetBenchmarksQuery.cs`
- [ ] T094 [US2] Implement GetBenchmarksQueryHandler in `Src/Foundation/services/Assessment/Assessment.Application/Benchmarks/Queries/GetBenchmarks/GetBenchmarksQueryHandler.cs`

### Infrastructure Layer (US2)

- [ ] T095 [P] [US2] Create IResultRepository interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/IResultRepository.cs`
- [ ] T096 [US2] Implement ResultRepository with EF Core in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Repositories/ResultRepository.cs`
- [ ] T097 [P] [US2] Create IBenchmarkRepository interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/IBenchmarkRepository.cs`
- [ ] T098 [US2] Implement BenchmarkRepository with EF Core in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Repositories/BenchmarkRepository.cs`
- [ ] T099 [P] [US2] Configure AssessmentResult entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/AssessmentResultConfiguration.cs`
- [ ] T100 [P] [US2] Configure Benchmark entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/BenchmarkConfiguration.cs`
- [ ] T101 [US2] Create migration for AssessmentResult and Benchmark tables in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Migrations/`
- [ ] T102 [P] [US2] Create AuditRecord entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/AuditRecord.cs`
- [ ] T103 [P] [US2] Configure AuditRecord entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/AuditRecordConfiguration.cs`

### Event Contracts (US2)

- [ ] T104 [P] [US2] Create AssessmentResultRecorded event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentResultRecorded.cs`
- [ ] T105 [P] [US2] Create AssessmentResultCorrected event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentResultCorrected.cs`

### API Endpoints (US2)

- [ ] T106 [US2] Implement POST /api/assessments/{id}/benchmarks endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/BenchmarksEndpoints.cs`
- [ ] T107 [US2] Implement GET /api/assessments/{id}/benchmarks endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/BenchmarksEndpoints.cs`
- [ ] T108 [US2] Implement POST /api/assessments/{assignmentId}/results endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ResultsEndpoints.cs`
- [ ] T109 [US2] Implement PATCH /api/assessments/{assignmentId}/results/{resultId} endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ResultsEndpoints.cs`
- [ ] T110 [US2] Implement GET /api/assessments/results endpoint with filtering in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ResultsEndpoints.cs`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - can create, assign, score, and apply benchmarks with audit trail

---

## Phase 5: User Story 3 - Analyze Trends, Imports, and Exports (Priority: P3)

**Goal**: Staff view assessment trends, import state test data, and export authorized results for analysis

**Independent Test**: Import a state test file, generate a trend view for student group, export results while confirming performance targets and audit coverage

### Domain Layer (US3)

- [ ] T111 [P] [US3] Create ImportJob entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/ImportJob.cs`
- [ ] T112 [P] [US3] Create ExportJob entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/ExportJob.cs`
- [ ] T113 [P] [US3] Create ImportStatus enum (Pending/InProgress/Completed/Failed/PartialSuccess) in `Src/Foundation/services/Assessment/Assessment.Domain/Enums/ImportStatus.cs`
- [ ] T114 [P] [US3] Create TrendDirection enum (Improving/Declining/Stable) in `Src/Foundation/services/Assessment/Assessment.Domain/Enums/TrendDirection.cs`
- [ ] T115 [P] [US3] Create TrendMetrics value object in `Src/Foundation/services/Assessment/Assessment.Domain/ValueObjects/TrendMetrics.cs`

### Application Layer - Trends (US3)

- [ ] T116 [P] [US3] Create GetTrendsQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Trends/Queries/GetTrends/GetTrendsQuery.cs`
- [ ] T117 [US3] Implement GetTrendsQueryHandler with caching in `Src/Foundation/services/Assessment/Assessment.Application/Trends/Queries/GetTrends/GetTrendsQueryHandler.cs`
- [ ] T118 [P] [US3] Implement trend calculation service in `Src/Foundation/services/Assessment/Assessment.Application/Trends/Services/TrendCalculator.cs`
- [ ] T119 [US3] Implement trend cache invalidation on result updates in `Src/Foundation/services/Assessment/Assessment.Application/Trends/Services/TrendCacheManager.cs`

### Application Layer - Imports (US3)

- [ ] T120 [P] [US3] Create StartImportCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Commands/StartImport/StartImportCommand.cs`
- [ ] T121 [P] [US3] Create StartImportCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Commands/StartImport/StartImportCommandValidator.cs`
- [ ] T122 [US3] Implement StartImportCommandHandler in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Commands/StartImport/StartImportCommandHandler.cs`
- [ ] T123 [P] [US3] Implement CSV parser for state test format in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Services/StateTestCsvParser.cs`
- [ ] T124 [P] [US3] Implement import validation service in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Services/ImportValidator.cs`
- [ ] T125 [US3] Implement import processor with batch handling in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Services/ImportProcessor.cs`
- [ ] T126 [US3] Implement error reporting for failed import rows in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Services/ErrorReporter.cs`
- [ ] T127 [P] [US3] Create GetImportStatusQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Queries/GetImportStatus/GetImportStatusQuery.cs`
- [ ] T128 [US3] Implement GetImportStatusQueryHandler in `Src/Foundation/services/Assessment/Assessment.Application/Imports/Queries/GetImportStatus/GetImportStatusQueryHandler.cs`

### Application Layer - Exports (US3)

- [ ] T129 [P] [US3] Create StartExportCommand record in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Commands/StartExport/StartExportCommand.cs`
- [ ] T130 [P] [US3] Create StartExportCommandValidator in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Commands/StartExport/StartExportCommandValidator.cs`
- [ ] T131 [US3] Implement StartExportCommandHandler in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Commands/StartExport/StartExportCommandHandler.cs`
- [ ] T132 [P] [US3] Implement export authorization service in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Services/ExportAuthorizer.cs`
- [ ] T133 [P] [US3] Implement CSV generator with CsvHelper in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Services/CsvExportGenerator.cs`
- [ ] T134 [US3] Implement blob storage service for export files in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Storage/BlobStorageService.cs`
- [ ] T135 [P] [US3] Create GetExportStatusQuery record in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Queries/GetExportStatus/GetExportStatusQuery.cs`
- [ ] T136 [US3] Implement GetExportStatusQueryHandler in `Src/Foundation/services/Assessment/Assessment.Application/Exports/Queries/GetExportStatus/GetExportStatusQueryHandler.cs`

### Infrastructure Layer (US3)

- [ ] T137 [P] [US3] Create IImportJobRepository interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/IImportJobRepository.cs`
- [ ] T138 [US3] Implement ImportJobRepository with EF Core in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Repositories/ImportJobRepository.cs`
- [ ] T139 [P] [US3] Create IExportJobRepository interface in `Src/Foundation/services/Assessment/Assessment.Application/Common/Interfaces/IExportJobRepository.cs`
- [ ] T140 [US3] Implement ExportJobRepository with EF Core in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Repositories/ExportJobRepository.cs`
- [ ] T141 [P] [US3] Configure ImportJob entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/ImportJobConfiguration.cs`
- [ ] T142 [P] [US3] Configure ExportJob entity mapping in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/ExportJobConfiguration.cs`
- [ ] T143 [US3] Create migration for ImportJob and ExportJob tables in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Migrations/`
- [ ] T144 [P] [US3] Create materialized view for trend queries in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Migrations/`
- [ ] T145 [P] [US3] Configure blob storage with Aspire in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Storage/BlobStorageConfiguration.cs`

### Event Contracts (US3)

- [ ] T146 [P] [US3] Create AssessmentImportCompleted event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentImportCompleted.cs`
- [ ] T147 [P] [US3] Create AssessmentImportFailed event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentImportFailed.cs`
- [ ] T148 [P] [US3] Create AssessmentExportCompleted event contract in `Src/Foundation/services/Assessment/Assessment.Messaging/Events/AssessmentExportCompleted.cs`

### API Endpoints (US3)

- [ ] T149 [US3] Implement GET /api/assessments/trends endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/TrendsEndpoints.cs`
- [ ] T150 [US3] Implement POST /api/assessments/imports endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ImportsEndpoints.cs`
- [ ] T151 [US3] Implement GET /api/assessments/imports/{jobId} endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ImportsEndpoints.cs`
- [ ] T152 [US3] Implement POST /api/assessments/exports endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ExportsEndpoints.cs`
- [ ] T153 [US3] Implement GET /api/assessments/exports/{jobId} endpoint in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/ExportsEndpoints.cs`

**Checkpoint**: All user stories should now be independently functional - create, assign, score, benchmark, trend, import, and export

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

### Performance Optimization

- [ ] T154 [P] Add database indexes for common query patterns in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/Configurations/`
- [ ] T155 [P] Optimize trend query performance with materialized view refresh strategy in `Src/Foundation/services/Assessment/Assessment.Infrastructure/Data/`
- [ ] T156 [P] Configure connection pooling and query timeouts in `Src/Foundation/services/Assessment/Assessment.Infrastructure/DependencyInjection.cs`

### Observability & Monitoring

- [ ] T157 [P] Add OpenTelemetry metrics for assessment operations in `Src/Foundation/services/Assessment/Assessment.Api/Telemetry/`
- [ ] T158 [P] Add structured logging for key operations in all handlers
- [ ] T159 [P] Configure health checks for database, messaging, and cache in `Src/Foundation/services/Assessment/Assessment.Api/Program.cs`
- [ ] T160 [P] Add custom metrics for performance targets (p95 latencies) in `Src/Foundation/services/Assessment/Assessment.Api/Telemetry/`

### Security Hardening

- [ ] T161 [P] Review and harden authorization policies in `Src/Foundation/services/Assessment/Assessment.Api/Authorization/`
- [ ] T162 [P] Add rate limiting for import/export endpoints in `Src/Foundation/services/Assessment/Assessment.Api/Middleware/`
- [ ] T163 [P] Validate all tenant isolation enforcement in queries and commands

### Documentation

- [ ] T164 [P] Complete API documentation with OpenAPI/Swagger in `Src/Foundation/services/Assessment/Assessment.Api/`
- [ ] T165 [P] Update service README with architecture diagrams in `Src/Foundation/services/Assessment/README.md`
- [ ] T166 [P] Document event contracts and message flows in `Src/Foundation/services/Assessment/Assessment.Messaging/README.md`
- [ ] T167 [P] Create deployment guide in `Src/Foundation/services/Assessment/docs/deployment.md`

### Quality Assurance

- [ ] T168 [P] Run quickstart.md smoke tests from `Plan/Foundation/specs/009-assessment-service/quickstart.md`
- [ ] T169 [P] Verify all acceptance scenarios from spec.md are met
- [ ] T170 [P] Validate performance targets (SC-001 through SC-005)
- [ ] T171 [P] Complete code review checklist from constitution

### Template Library Feature

- [ ] T172 [P] Create AssessmentTemplateLibrary entity in `Src/Foundation/services/Assessment/Assessment.Domain/Entities/AssessmentTemplateLibrary.cs`
- [ ] T173 [P] Implement template CRUD operations in `Src/Foundation/services/Assessment/Assessment.Application/Templates/`
- [ ] T174 [P] Add template endpoints to API in `Src/Foundation/services/Assessment/Assessment.Api/Endpoints/TemplatesEndpoints.cs`

### Reminder System Feature

- [ ] T175 [P] Implement configurable reminder service for upcoming/overdue assessments in `Src/Foundation/services/Assessment/Assessment.Application/Reminders/`
- [ ] T176 [P] Add background job for reminder processing in `Src/Foundation/services/Assessment/Assessment.Infrastructure/BackgroundJobs/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after both Foundational (Phase 2) and User Story 1 (P1) are complete (needs AssessmentAssignment)
- **User Story 3 (P3)**: Can start after both Foundational (Phase 2) and User Story 2 (P2) are complete (needs AssessmentResult for trends/exports)

### Within Each User Story

- Domain entities before application layer
- Commands/queries before handlers
- Handlers before API endpoints
- Event contracts before publishing
- Repository interfaces before implementations
- Entity configurations before migrations

### Parallel Opportunities

- **Phase 1**: Tasks T012-T017 and T020-T021 can run in parallel (different projects)
- **Phase 2**: 
  - Database tasks T023-T025 can run in parallel
  - Auth tasks T029-T030 can run in parallel
  - Messaging tasks T032-T034 can run in parallel
  - T036-T037 and T039 can run in parallel
- **Phase 3 (US1)**: 
  - Domain entities T040-T044 can run in parallel
  - Commands T047-T048, T050-T051, T053-T054 can run in parallel (different command types)
  - Queries T056, T058, T060 can run in parallel
  - Event contracts T067-T068 can run in parallel
  - Configurations T064-T065 can run in parallel
- **Phase 4 (US2)**:
  - Domain entities T075-T078 can run in parallel
  - Commands T082-T083, T085-T086, T088-T089 can run in parallel (different command types)
  - Queries T091, T093 can run in parallel
  - Repositories T095-T098 interfaces can run in parallel
  - Configurations T099-T100, T102-T103 can run in parallel
  - Event contracts T104-T105 can run in parallel
- **Phase 5 (US3)**:
  - Domain entities T111-T115 can run in parallel
  - All query records, command records, and event contracts can run in parallel
  - Repositories T137-T140 interfaces can run in parallel
  - Configurations T141-T145 can run in parallel
  - Event contracts T146-T148 can run in parallel
- **Phase 6**: Most tasks T154-T171 can run in parallel as they affect different areas

---

## Parallel Example: User Story 1

```bash
# After Foundational phase completes, launch all US1 domain entities:
Task T040: Create AssessmentDefinition aggregate root
Task T041: Create AssessmentAssignment entity
Task T042: Create AssessmentStatus enum
Task T043: Create AssignmentStatus enum
Task T044: Create CustomFieldDefinition value object

# Then launch all US1 command definitions:
Task T047: Create CreateAssessmentCommand record
Task T048: Create CreateAssessmentCommandValidator
Task T050: Create UpdateAssessmentCommand record
Task T051: Create UpdateAssessmentCommandValidator
Task T053: Create AssignAssessmentCommand record
Task T054: Create AssignAssessmentCommandValidator

# Then launch all US1 query definitions:
Task T056: Create GetAssessmentsQuery record
Task T058: Create GetAssessmentByIdQuery record
Task T060: Create GetAssignmentsQuery record
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T021)
2. Complete Phase 2: Foundational (T022-T039) - CRITICAL
3. Complete Phase 3: User Story 1 (T040-T074)
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo assessment creation and assignment

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP! - Create & Assign)
3. Add User Story 2 → Test independently → Deploy/Demo (Scoring & Benchmarks)
4. Add User Story 3 → Test independently → Deploy/Demo (Analytics & Data Exchange)
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (T040-T074)
   - Note: User Story 2 can start after US1 completes (needs AssessmentAssignment)
   - Note: User Story 3 can start after US2 completes (needs AssessmentResult)
3. Stories integrate and test independently

### Performance Validation Points

- After US1: Verify SC-001 (95% of create/assign < 100ms)
- After US2: Verify SC-002 (99% of result recording < 2s) and SC-005 (100% audit entries)
- After US3: Verify SC-003 (95% of trend queries < 200ms) and SC-004 (imports < 2% error rate)

---

## Notes

- [P] tasks = different files, no dependencies - can run in parallel
- [Story] label maps task to specific user story (US1, US2, US3) for traceability
- Tests NOT included: Not explicitly requested in spec.md per constitution guidelines
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Performance targets must be validated at phase boundaries
- All entities enforce tenant isolation via TenantInterceptor
- All score changes generate audit records
- All key operations emit domain events for downstream consumers
- Import/export jobs run asynchronously with status tracking

---

## Success Metrics (from spec.md)

- **SC-001**: 95% of assessment creation and assignment operations complete in under 100ms
- **SC-002**: 99% of result recordings compute benchmark classification and emit events within 2 seconds
- **SC-003**: Trend queries for a class or roster return in under 200ms for 95% of requests
- **SC-004**: Imports process at least 1,000 assessment records with an error rate below 2% per job
- **SC-005**: 100% of create/update/delete operations generate audit entries with actor and before/after values

**Total Tasks**: 176 tasks organized into 6 phases across 3 user stories
