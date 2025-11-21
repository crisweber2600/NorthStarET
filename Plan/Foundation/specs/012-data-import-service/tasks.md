---

description: "Task list for Data Import & Integration Service implementation"
---

# Tasks: Data Import & Integration Service

**Specification Branch**: `Foundation/012-data-import-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/012-data-import-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/012-data-import-service/`  
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/DataImport`  
**Specification Path**: `Plan/Foundation/specs/012-data-import-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/services/DataImport`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/012-data-import-service/`)
- [ ] Shared infrastructure dependencies match between spec and plan
- [ ] Cross-layer dependencies (if any) justified in both spec and plan

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] Verify project references ONLY shared infrastructure from approved layers (`Src/Foundation/shared/*`)
- [ ] Verify NO direct service-to-service references across layers (must use events/contracts for cross-layer communication)
- [ ] Verify AppHost orchestration includes this service with correct layer isolation
- [ ] Verify README.md documents layer position and shared infrastructure dependencies
- [ ] Verify no circular dependencies between layers (Foundation cannot depend on higher layers)

---

## Identity & Authentication Compliance

*MANDATORY: Include if this feature requires authentication/authorization*

- [ ] Verify NO references to Duende IdentityServer or custom token issuance
- [ ] Verify Microsoft.Identity.Web used for JWT token validation (NOT custom JWT generation)
- [ ] Verify SessionAuthenticationHandler registered for session-based API authorization
- [ ] Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] Verify identity.sessions table includes tenant_id for multi-tenancy
- [ ] Verify TokenExchangeService implements BFF pattern (Entra tokens → LMS sessions)
- [ ] Verify authentication flow follows `legacy-identityserver-migration.md` architecture

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create DataImport service directory structure under Src/Foundation/services/DataImport/
- [ ] T002 Initialize .NET 8 API project in Src/Foundation/services/DataImport/Api/DataImport.Api.csproj
- [ ] T003 [P] Initialize Domain project in Src/Foundation/services/DataImport/Domain/DataImport.Domain.csproj
- [ ] T004 [P] Initialize Application project in Src/Foundation/services/DataImport/Application/DataImport.Application.csproj
- [ ] T005 [P] Initialize Infrastructure project in Src/Foundation/services/DataImport/Infrastructure/DataImport.Infrastructure.csproj
- [ ] T006 [P] Initialize Workers project in Src/Foundation/services/DataImport/Workers/DataImport.Workers.csproj
- [ ] T007 [P] Initialize Tests project in Src/Foundation/services/DataImport/Tests/DataImport.Tests.csproj
- [ ] T008 Add NuGet package references (EF Core 9, Npgsql, CsvHelper, ExcelDataReader, Hangfire, MassTransit, FluentValidation, Polly) to Directory.Packages.props
- [ ] T009 [P] Configure project references between Api → Application → Domain and Infrastructure
- [ ] T010 [P] Add service to AppHost orchestration in Src/Foundation/AppHost/Program.cs with PostgreSQL and Redis dependencies
- [ ] T011 Create README.md documenting service purpose, layer position, and shared infrastructure dependencies in Src/Foundation/services/DataImport/
- [ ] T012 [P] Setup .editorconfig and linting rules for DataImport projects

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T013 Setup DataImportDbContext with EF Core 9 in Src/Foundation/services/DataImport/Infrastructure/Persistence/DataImportDbContext.cs
- [ ] T014 Configure PostgreSQL connection and TenantInterceptor/AuditInterceptor in Src/Foundation/services/DataImport/Infrastructure/DependencyInjection.cs
- [ ] T015 Configure blob storage client for file artifacts and error reports in Src/Foundation/services/DataImport/Infrastructure/Storage/BlobStorageService.cs
- [ ] T016 [P] Configure MassTransit with Azure Service Bus for event publishing in Src/Foundation/services/DataImport/Infrastructure/Messaging/MessagingConfiguration.cs
- [ ] T017 [P] Configure Hangfire or Quartz for job scheduling in Src/Foundation/services/DataImport/Infrastructure/Scheduling/SchedulingConfiguration.cs
- [ ] T018 [P] Setup FluentValidation pipeline in Application layer in Src/Foundation/services/DataImport/Application/DependencyInjection.cs
- [ ] T019 [P] Configure Polly resilience policies (retry, circuit breaker) in Src/Foundation/services/DataImport/Infrastructure/Resilience/ResiliencePolicies.cs
- [ ] T020 [P] Setup API middleware (authentication, error handling, logging) in Src/Foundation/services/DataImport/Api/Program.cs
- [ ] T021 [P] Configure Redis for optional progress caching in Src/Foundation/services/DataImport/Infrastructure/Caching/RedisCacheService.cs
- [ ] T022 Add initial migration for base schema in Src/Foundation/services/DataImport/Infrastructure/Persistence/Migrations/
- [ ] T023 Configure multi-tenancy with tenant_id on all entities and RLS policies in Src/Foundation/services/DataImport/Infrastructure/Persistence/DataImportDbContext.cs
- [ ] T024 [P] Create base entity classes (BaseEntity with tenant_id, audit fields) in Src/Foundation/services/DataImport/Domain/Common/BaseEntity.cs
- [ ] T025 [P] Setup OpenTelemetry and health checks via ServiceDefaults in Src/Foundation/services/DataImport/Api/Program.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Upload and validate files before import (Priority: P1) 🎯 MVP

**Goal**: Enable admins to upload CSV/Excel files, select templates, and run pre-validation to catch schema and rule errors before execution.

**Independent Test**: Upload a CSV file, map it to a student template, run validation, and confirm pass/fail results with actionable error messages.

### Domain Models for User Story 1

- [ ] T026 [P] [US1] Create ImportTemplate entity in Src/Foundation/services/DataImport/Domain/Entities/ImportTemplate.cs
- [ ] T027 [P] [US1] Create ImportJob entity in Src/Foundation/services/DataImport/Domain/Entities/ImportJob.cs
- [ ] T028 [P] [US1] Create FileArtifact entity in Src/Foundation/services/DataImport/Domain/Entities/FileArtifact.cs
- [ ] T029 [P] [US1] Create RowResult entity in Src/Foundation/services/DataImport/Domain/Entities/RowResult.cs
- [ ] T030 [P] [US1] Create ColumnMapping value object in Src/Foundation/services/DataImport/Domain/ValueObjects/ColumnMapping.cs
- [ ] T031 [P] [US1] Create ValidationRule value object in Src/Foundation/services/DataImport/Domain/ValueObjects/ValidationRule.cs

### Database Migrations for User Story 1

- [ ] T032 [US1] Create migration for ImportTemplate, ImportJob, FileArtifact, RowResult tables in Src/Foundation/services/DataImport/Infrastructure/Persistence/Migrations/
- [ ] T033 [US1] Add indexes for tenant_id, status, and started_at in migration

### Application Layer for User Story 1

- [ ] T034 [P] [US1] Create CreateTemplateCommand and handler in Src/Foundation/services/DataImport/Application/Templates/Commands/CreateTemplate/
- [ ] T035 [P] [US1] Create GetTemplatesQuery and handler in Src/Foundation/services/DataImport/Application/Templates/Queries/GetTemplates/
- [ ] T036 [P] [US1] Create UploadFileCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/UploadFile/
- [ ] T037 [US1] Create ValidateFileCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/ValidateFile/
- [ ] T038 [P] [US1] Create FluentValidation validators for CreateTemplateCommand in Src/Foundation/services/DataImport/Application/Templates/Commands/CreateTemplate/CreateTemplateCommandValidator.cs
- [ ] T039 [P] [US1] Create FluentValidation validators for UploadFileCommand in Src/Foundation/services/DataImport/Application/Jobs/Commands/UploadFile/UploadFileCommandValidator.cs

### Infrastructure Services for User Story 1

- [ ] T040 [P] [US1] Implement ITemplateRepository in Src/Foundation/services/DataImport/Infrastructure/Persistence/Repositories/TemplateRepository.cs
- [ ] T041 [P] [US1] Implement IJobRepository in Src/Foundation/services/DataImport/Infrastructure/Persistence/Repositories/JobRepository.cs
- [ ] T042 [US1] Implement FileParserService (CSV/Excel) in Src/Foundation/services/DataImport/Infrastructure/Parsing/FileParserService.cs
- [ ] T043 [US1] Implement TemplateValidationService with rule engine in Src/Foundation/services/DataImport/Application/Validation/TemplateValidationService.cs
- [ ] T044 [US1] Implement BlobStorageService methods for file upload and retrieval in Src/Foundation/services/DataImport/Infrastructure/Storage/BlobStorageService.cs

### API Endpoints for User Story 1

- [ ] T045 [P] [US1] Create POST /api/import/templates endpoint in Src/Foundation/services/DataImport/Api/Controllers/TemplatesController.cs
- [ ] T046 [P] [US1] Create GET /api/import/templates endpoint in Src/Foundation/services/DataImport/Api/Controllers/TemplatesController.cs
- [ ] T047 [US1] Create POST /api/import/jobs endpoint with file upload in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs
- [ ] T048 [US1] Create POST /api/import/jobs/{jobId}/validate endpoint in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs
- [ ] T049 [US1] Add validation error response models in Src/Foundation/services/DataImport/Api/Models/ValidationErrorResponse.cs

### Testing for User Story 1

- [ ] T050 [P] [US1] Unit tests for CreateTemplateCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Templates/CreateTemplateCommandHandlerTests.cs
- [ ] T051 [P] [US1] Unit tests for UploadFileCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/UploadFileCommandHandlerTests.cs
- [ ] T052 [P] [US1] Unit tests for ValidateFileCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/ValidateFileCommandHandlerTests.cs
- [ ] T053 [P] [US1] Unit tests for FileParserService (CSV/Excel parsing) in Src/Foundation/services/DataImport/Tests/Infrastructure/Parsing/FileParserServiceTests.cs
- [ ] T054 [P] [US1] Unit tests for TemplateValidationService in Src/Foundation/services/DataImport/Tests/Application/Validation/TemplateValidationServiceTests.cs
- [ ] T055 [US1] Integration test for complete upload and validate flow in Src/Foundation/services/DataImport/Tests/Integration/UploadValidateFlowTests.cs
- [ ] T056 [US1] BDD feature file for upload and validation scenarios in specs/012-data-import-service/features/upload-validate.feature

**Checkpoint**: At this point, User Story 1 should be fully functional - users can upload files, map to templates, and validate before import

---

## Phase 4: User Story 2 - Schedule, monitor, and resume imports (Priority: P2)

**Goal**: Enable admins to schedule recurring imports (e.g., nightly SFTP), monitor progress, and resume interrupted jobs safely.

**Independent Test**: Configure a nightly SFTP import, observe job progress metrics, simulate an interruption, and verify resumable processing with correct status.

### Domain Models for User Story 2

- [ ] T057 [P] [US2] Create Schedule entity in Src/Foundation/services/DataImport/Domain/Entities/Schedule.cs
- [ ] T058 [P] [US2] Create JobProgress value object in Src/Foundation/services/DataImport/Domain/ValueObjects/JobProgress.cs
- [ ] T059 [P] [US2] Add resume_token field to ImportJob entity for resumable processing in Src/Foundation/services/DataImport/Domain/Entities/ImportJob.cs

### Database Migrations for User Story 2

- [ ] T060 [US2] Create migration for Schedule table and resume_token column in Src/Foundation/services/DataImport/Infrastructure/Persistence/Migrations/

### Application Layer for User Story 2

- [ ] T061 [P] [US2] Create CreateScheduleCommand and handler in Src/Foundation/services/DataImport/Application/Schedules/Commands/CreateSchedule/
- [ ] T062 [P] [US2] Create GetSchedulesQuery and handler in Src/Foundation/services/DataImport/Application/Schedules/Queries/GetSchedules/
- [ ] T063 [P] [US2] Create StartJobCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/StartJob/
- [ ] T064 [P] [US2] Create GetJobStatusQuery and handler in Src/Foundation/services/DataImport/Application/Jobs/Queries/GetJobStatus/
- [ ] T065 [US2] Create ResumeJobCommand and handler for interrupted jobs in Src/Foundation/services/DataImport/Application/Jobs/Commands/ResumeJob/

### Infrastructure Services for User Story 2

- [ ] T066 [P] [US2] Implement IScheduleRepository in Src/Foundation/services/DataImport/Infrastructure/Persistence/Repositories/ScheduleRepository.cs
- [ ] T067 [US2] Implement SftpClientService for fetching files from SFTP in Src/Foundation/services/DataImport/Infrastructure/FileTransfer/SftpClientService.cs
- [ ] T068 [US2] Implement ScheduledJobService to poll schedules and trigger jobs in Src/Foundation/services/DataImport/Workers/ScheduledJobService.cs
- [ ] T069 [US2] Implement ImportProcessorWorker with resumable processing logic in Src/Foundation/services/DataImport/Workers/ImportProcessorWorker.cs
- [ ] T070 [US2] Implement ProgressTrackingService with Redis caching in Src/Foundation/services/DataImport/Infrastructure/Caching/ProgressTrackingService.cs

### Events for User Story 2

- [ ] T071 [P] [US2] Create ImportStarted event in Src/Foundation/services/DataImport/Domain/Events/ImportStarted.cs
- [ ] T072 [P] [US2] Create ImportProgressed event in Src/Foundation/services/DataImport/Domain/Events/ImportProgressed.cs
- [ ] T073 [P] [US2] Create ImportCompleted event in Src/Foundation/services/DataImport/Domain/Events/ImportCompleted.cs
- [ ] T074 [P] [US2] Create ImportFailed event in Src/Foundation/services/DataImport/Domain/Events/ImportFailed.cs
- [ ] T075 [US2] Implement event publishing in ImportProcessorWorker using MassTransit in Src/Foundation/services/DataImport/Workers/ImportProcessorWorker.cs

### API Endpoints for User Story 2

- [ ] T076 [P] [US2] Create POST /api/import/schedules endpoint in Src/Foundation/services/DataImport/Api/Controllers/SchedulesController.cs
- [ ] T077 [P] [US2] Create GET /api/import/schedules endpoint in Src/Foundation/services/DataImport/Api/Controllers/SchedulesController.cs
- [ ] T078 [P] [US2] Create POST /api/import/jobs/{jobId}/start endpoint in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs
- [ ] T079 [US2] Create GET /api/import/jobs/{jobId} endpoint with progress metrics in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs

### Testing for User Story 2

- [ ] T080 [P] [US2] Unit tests for CreateScheduleCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Schedules/CreateScheduleCommandHandlerTests.cs
- [ ] T081 [P] [US2] Unit tests for StartJobCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/StartJobCommandHandlerTests.cs
- [ ] T082 [P] [US2] Unit tests for ResumeJobCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/ResumeJobCommandHandlerTests.cs
- [ ] T083 [P] [US2] Unit tests for ScheduledJobService in Src/Foundation/services/DataImport/Tests/Workers/ScheduledJobServiceTests.cs
- [ ] T084 [P] [US2] Unit tests for ImportProcessorWorker with resumable processing in Src/Foundation/services/DataImport/Tests/Workers/ImportProcessorWorkerTests.cs
- [ ] T085 [US2] Integration test for scheduled SFTP import flow in Src/Foundation/services/DataImport/Tests/Integration/ScheduledImportFlowTests.cs
- [ ] T086 [US2] Integration test for job resumption after interruption in Src/Foundation/services/DataImport/Tests/Integration/JobResumptionTests.cs
- [ ] T087 [US2] Consumer tests for ImportStarted, ImportProgressed, ImportCompleted events in Src/Foundation/services/DataImport/Tests/Messaging/EventConsumerTests.cs
- [ ] T088 [US2] BDD feature file for scheduling and monitoring scenarios in specs/012-data-import-service/features/schedule-monitor.feature

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can upload/validate files AND schedule/monitor recurring imports

---

## Phase 5: User Story 3 - Handle errors, duplicates, and rollback (Priority: P3)

**Goal**: Enable operators to manage duplicates, validation failures, and rollback partial imports with auditability.

**Independent Test**: Run an import with duplicate records and validation failures, resolve duplicates, generate an error export, and perform a rollback that restores prior state.

### Domain Models for User Story 3

- [ ] T089 [P] [US3] Create DuplicateCandidate entity in Src/Foundation/services/DataImport/Domain/Entities/DuplicateCandidate.cs
- [ ] T090 [P] [US3] Create ErrorReport entity in Src/Foundation/services/DataImport/Domain/Entities/ErrorReport.cs
- [ ] T091 [P] [US3] Create AuditRecord entity in Src/Foundation/services/DataImport/Domain/Entities/AuditRecord.cs
- [ ] T092 [P] [US3] Create ResolutionAction enum (Accept, Merge, Reject) in Src/Foundation/services/DataImport/Domain/Enums/ResolutionAction.cs

### Database Migrations for User Story 3

- [ ] T093 [US3] Create migration for DuplicateCandidate, ErrorReport, AuditRecord tables in Src/Foundation/services/DataImport/Infrastructure/Persistence/Migrations/

### Application Layer for User Story 3

- [ ] T094 [P] [US3] Create DetectDuplicatesCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/DetectDuplicates/
- [ ] T095 [P] [US3] Create ResolveDuplicateCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/ResolveDuplicate/
- [ ] T096 [P] [US3] Create GenerateErrorReportCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/GenerateErrorReport/
- [ ] T097 [P] [US3] Create RollbackJobCommand and handler in Src/Foundation/services/DataImport/Application/Jobs/Commands/RollbackJob/
- [ ] T098 [US3] Create GetErrorReportQuery and handler in Src/Foundation/services/DataImport/Application/Jobs/Queries/GetErrorReport/

### Infrastructure Services for User Story 3

- [ ] T099 [P] [US3] Implement IDuplicateRepository in Src/Foundation/services/DataImport/Infrastructure/Persistence/Repositories/DuplicateRepository.cs
- [ ] T100 [P] [US3] Implement IAuditRepository in Src/Foundation/services/DataImport/Infrastructure/Persistence/Repositories/AuditRepository.cs
- [ ] T101 [US3] Implement DuplicateDetectionService with configurable key matching in Src/Foundation/services/DataImport/Application/Services/DuplicateDetectionService.cs
- [ ] T102 [US3] Implement ErrorReportGenerator to create CSV exports in Src/Foundation/services/DataImport/Infrastructure/Reporting/ErrorReportGenerator.cs
- [ ] T103 [US3] Implement RollbackService with compensating transactions in Src/Foundation/services/DataImport/Application/Services/RollbackService.cs
- [ ] T104 [US3] Implement AuditLoggingService for all operations in Src/Foundation/services/DataImport/Infrastructure/Auditing/AuditLoggingService.cs

### Events for User Story 3

- [ ] T105 [P] [US3] Create ImportRollbackExecuted event in Src/Foundation/services/DataImport/Domain/Events/ImportRollbackExecuted.cs
- [ ] T106 [P] [US3] Create RowValidationFailed event in Src/Foundation/services/DataImport/Domain/Events/RowValidationFailed.cs
- [ ] T107 [US3] Implement event publishing for rollback and validation failures using MassTransit in Src/Foundation/services/DataImport/Workers/ImportProcessorWorker.cs

### API Endpoints for User Story 3

- [ ] T108 [P] [US3] Create GET /api/import/jobs/{jobId}/duplicates endpoint in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs
- [ ] T109 [P] [US3] Create PATCH /api/import/jobs/{jobId}/duplicates/{rowId} endpoint for resolution in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs
- [ ] T110 [P] [US3] Create GET /api/import/jobs/{jobId}/errors endpoint for error report download in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs
- [ ] T111 [US3] Create POST /api/import/jobs/{jobId}/rollback endpoint in Src/Foundation/services/DataImport/Api/Controllers/JobsController.cs

### Testing for User Story 3

- [ ] T112 [P] [US3] Unit tests for DetectDuplicatesCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/DetectDuplicatesCommandHandlerTests.cs
- [ ] T113 [P] [US3] Unit tests for ResolveDuplicateCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/ResolveDuplicateCommandHandlerTests.cs
- [ ] T114 [P] [US3] Unit tests for GenerateErrorReportCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/GenerateErrorReportCommandHandlerTests.cs
- [ ] T115 [P] [US3] Unit tests for RollbackJobCommandHandler in Src/Foundation/services/DataImport/Tests/Application/Jobs/RollbackJobCommandHandlerTests.cs
- [ ] T116 [P] [US3] Unit tests for DuplicateDetectionService in Src/Foundation/services/DataImport/Tests/Application/Services/DuplicateDetectionServiceTests.cs
- [ ] T117 [P] [US3] Unit tests for ErrorReportGenerator in Src/Foundation/services/DataImport/Tests/Infrastructure/Reporting/ErrorReportGeneratorTests.cs
- [ ] T118 [P] [US3] Unit tests for RollbackService in Src/Foundation/services/DataImport/Tests/Application/Services/RollbackServiceTests.cs
- [ ] T119 [US3] Integration test for duplicate detection and resolution flow in Src/Foundation/services/DataImport/Tests/Integration/DuplicateHandlingFlowTests.cs
- [ ] T120 [US3] Integration test for error report generation and download in Src/Foundation/services/DataImport/Tests/Integration/ErrorReportFlowTests.cs
- [ ] T121 [US3] Integration test for rollback within retention window in Src/Foundation/services/DataImport/Tests/Integration/RollbackFlowTests.cs
- [ ] T122 [US3] Consumer tests for ImportRollbackExecuted and RowValidationFailed events in Src/Foundation/services/DataImport/Tests/Messaging/RollbackEventConsumerTests.cs
- [ ] T123 [US3] BDD feature file for error handling and rollback scenarios in specs/012-data-import-service/features/errors-rollback.feature

**Checkpoint**: All three user stories should now be independently functional - complete import lifecycle from upload to rollback

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final quality gates

### Documentation

- [ ] T124 [P] Update README.md with complete API documentation in Src/Foundation/services/DataImport/README.md
- [ ] T125 [P] Document error codes and troubleshooting guide in Src/Foundation/services/DataImport/docs/error-codes.md
- [ ] T126 [P] Create deployment guide in Src/Foundation/services/DataImport/docs/deployment.md
- [ ] T127 [P] Update quickstart.md with tested examples in Plan/Foundation/specs/012-data-import-service/quickstart.md

### Performance & Optimization

- [ ] T128 [P] Implement batch processing optimization in ImportProcessorWorker for 100+ records/sec target
- [ ] T129 [P] Add database query optimization and indexes for job status queries
- [ ] T130 [P] Optimize blob storage access with caching strategies
- [ ] T131 Performance test for validation speed (<5s for 10MB files) in Src/Foundation/services/DataImport/Tests/Performance/ValidationPerformanceTests.cs
- [ ] T132 Performance test for import throughput (>=100 records/sec) in Src/Foundation/services/DataImport/Tests/Performance/ImportThroughputTests.cs

### Security Hardening

- [ ] T133 [P] Implement file size limits and validation (50MB max per research.md)
- [ ] T134 [P] Add malware scanning integration hook before file processing
- [ ] T135 [P] Implement rate limiting on upload and schedule endpoints (10 requests/min)
- [ ] T136 [P] Audit all SFTP credential storage and ensure encryption at rest
- [ ] T137 Verify all audit logs include actor and timestamp per FR-011

### Additional Testing

- [ ] T138 [P] Contract tests for all API endpoints in Src/Foundation/services/DataImport/Tests/Contract/ApiContractTests.cs
- [ ] T139 [P] Contract tests for all published events in Src/Foundation/services/DataImport/Tests/Contract/EventContractTests.cs
- [ ] T140 [P] End-to-end test for complete import lifecycle in Src/Foundation/services/DataImport/Tests/E2E/CompleteImportLifecycleTests.cs
- [ ] T141 Chaos test for worker resilience and recovery in Src/Foundation/services/DataImport/Tests/Chaos/WorkerResilienceTests.cs

### Observability

- [ ] T142 [P] Add custom metrics for import throughput, error rates, validation times
- [ ] T143 [P] Add distributed tracing correlation for cross-service events
- [ ] T144 [P] Configure alerting thresholds for failed jobs and SLA violations
- [ ] T145 Create operational runbook in Src/Foundation/services/DataImport/docs/runbook.md

### Final Validation

- [ ] T146 Run all tests and ensure >=80% code coverage
- [ ] T147 Run quickstart.md validation with sample files
- [ ] T148 Verify all success criteria from spec.md (SC-001 through SC-005)
- [ ] T149 Code review and refactoring for maintainability
- [ ] T150 Final security scan with CodeQL

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion
- **User Story 2 (Phase 4)**: Depends on Foundational phase completion - Can run in parallel with US1
- **User Story 3 (Phase 5)**: Depends on Foundational phase completion - Can run in parallel with US1/US2
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent of US1, but may integrate for scheduled validation
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent of US1/US2, but integrates with both for duplicate/error handling

### Within Each User Story

- Domain models can be created in parallel [P]
- Migrations must complete before infrastructure services
- Application commands/queries can be developed in parallel [P]
- Infrastructure services before API endpoints
- Tests can be written in parallel [P] before implementation (TDD)
- API endpoints require application and infrastructure layers complete

### Parallel Opportunities

#### Phase 1: Setup
- T003-T007 (all project initialization) can run in parallel
- T008-T012 (configuration tasks) can run in parallel

#### Phase 2: Foundational
- T016-T021 (messaging, scheduling, validation, resilience, caching) can run in parallel
- T024-T025 (base entities, OpenTelemetry) can run in parallel

#### Phase 3: User Story 1
- T026-T031 (all domain models) can run in parallel
- T034-T035 (template commands/queries) can run in parallel
- T036-T037 (job commands) can run sequentially
- T038-T039 (validators) can run in parallel
- T040-T041 (repositories) can run in parallel
- T045-T046 (template endpoints) can run in parallel
- T050-T054 (unit tests) can run in parallel

#### Phase 4: User Story 2
- T057-T059 (domain models) can run in parallel
- T061-T064 (commands/queries) can run in parallel
- T071-T074 (events) can run in parallel
- T076-T078 (endpoints) can run in parallel
- T080-T084 (unit tests) can run in parallel

#### Phase 5: User Story 3
- T089-T092 (domain models) can run in parallel
- T094-T097 (commands) can run in parallel
- T099-T100 (repositories) can run in parallel
- T105-T106 (events) can run in parallel
- T108-T110 (endpoints) can run in parallel
- T112-T118 (unit tests) can run in parallel

#### Phase 6: Polish
- T124-T127 (documentation) can run in parallel
- T128-T130 (optimizations) can run in parallel
- T133-T136 (security) can run in parallel
- T138-T139 (contract tests) can run in parallel
- T142-T144 (observability) can run in parallel

---

## Parallel Example: User Story 1 Models

```bash
# Launch all domain models for User Story 1 together:
Task T026: "Create ImportTemplate entity in Src/Foundation/services/DataImport/Domain/Entities/ImportTemplate.cs"
Task T027: "Create ImportJob entity in Src/Foundation/services/DataImport/Domain/Entities/ImportJob.cs"
Task T028: "Create FileArtifact entity in Src/Foundation/services/DataImport/Domain/Entities/FileArtifact.cs"
Task T029: "Create RowResult entity in Src/Foundation/services/DataImport/Domain/Entities/RowResult.cs"
Task T030: "Create ColumnMapping value object in Src/Foundation/services/DataImport/Domain/ValueObjects/ColumnMapping.cs"
Task T031: "Create ValidationRule value object in Src/Foundation/services/DataImport/Domain/ValueObjects/ValidationRule.cs"

# All can be worked on simultaneously as they are in different files with no dependencies
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T012)
2. Complete Phase 2: Foundational (T013-T025) - CRITICAL - blocks all stories
3. Complete Phase 3: User Story 1 (T026-T056)
4. **STOP and VALIDATE**: Test User Story 1 independently with quickstart.md examples
5. Deploy/demo if ready - Basic file upload and validation capability

**Estimated Tasks**: ~56 tasks for MVP
**Value Delivered**: Admins can upload CSV/Excel files and validate them against templates before import

### Incremental Delivery

1. **Iteration 1**: Setup + Foundational → Foundation ready (T001-T025)
2. **Iteration 2**: Add User Story 1 → Test independently → Deploy/Demo (T026-T056) - **MVP!**
3. **Iteration 3**: Add User Story 2 → Test independently → Deploy/Demo (T057-T088) - Scheduling capability
4. **Iteration 4**: Add User Story 3 → Test independently → Deploy/Demo (T089-T123) - Error handling and rollback
5. **Iteration 5**: Polish & hardening (T124-T150) - Production ready

Each iteration adds value without breaking previous stories.

### Parallel Team Strategy

With multiple developers:

1. **Week 1**: Team completes Setup + Foundational together (T001-T025)
2. **Week 2-3**: Once Foundational is done:
   - Developer A: User Story 1 (T026-T056)
   - Developer B: User Story 2 (T057-T088)
   - Developer C: User Story 3 (T089-T123)
3. **Week 4**: Team works on Polish together (T124-T150)
4. Stories complete and integrate independently

---

## Success Metrics Validation

Track progress against spec.md success criteria:

- **SC-001**: 95% of uploads ≤10MB validated in <5s → Validate with T131 performance tests
- **SC-002**: Scheduled imports start within 2 minutes of trigger time for 99% of runs → Validate with T085 integration tests
- **SC-003**: Import processing ≥100 records/sec with <2% error rate → Validate with T132 performance tests
- **SC-004**: 99% of rollback requests execute successfully within 5 minutes → Validate with T121 integration tests
- **SC-005**: 100% of import jobs produce audit entries and downloadable error reports → Validate with T137 and T140

---

## Notes

- [P] tasks = different files, no dependencies - can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing (TDD approach)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- File size limit: 50MB per research.md open question
- Template version pinning prevents drift between scheduling and execution
- Rollback retention window TBD (propose 30 days per research.md)
- All operations must include audit logs with actor and timestamp
- Multi-tenancy enforced via tenant_id + RLS policies on all entities
- SFTP credentials stored encrypted per security requirements
