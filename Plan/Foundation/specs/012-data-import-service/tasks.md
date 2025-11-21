---
description: "Task list for Data Import Service implementation"
---

# Tasks: Data Import & Integration Service

**Specification Branch**: `Foundation/012-data-import-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/012-data-import-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/012-data-import-service/`  
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, research.md

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/DataImport/`  
**Specification Path**: `Plan/Foundation/specs/012-data-import-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification (Foundation)
- [ ] Target Layer matches plan.md Layer Identification (Foundation)
- [ ] Implementation path follows layer structure (`Src/Foundation/services/DataImport/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/012-data-import-service/`)
- [ ] Shared infrastructure dependencies include: ServiceDefaults, Domain, Application, Infrastructure
- [ ] Cross-layer dependencies: Will communicate with Student/Staff/Assessment services via events

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] Verify project references ONLY shared infrastructure from `Src/Foundation/shared/{ServiceDefaults,Domain,Application,Infrastructure}`
- [ ] Verify NO direct service-to-service references (will use MassTransit events for cross-service communication)
- [ ] Verify AppHost orchestration includes DataImport service with correct layer isolation
- [ ] Verify README.md documents layer position and shared infrastructure dependencies
- [ ] Verify no circular dependencies between layers (Foundation cannot depend on higher layers)

---

## Identity & Authentication Compliance

*MANDATORY: Include if this feature requires authentication/authorization*

- [ ] Verify NO references to Duende IdentityServer or custom token issuance
- [ ] Verify Microsoft.Identity.Web used for JWT token validation (NOT custom JWT generation)
- [ ] Verify SessionAuthenticationHandler registered for session-based API authorization
- [ ] Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] Verify multi-tenancy enforcement (TenantId in all entities and queries)
- [ ] Verify TokenExchangeService pattern follows BFF architecture from legacy-identityserver-migration.md

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure for Data Import Service

- [ ] T001 Create DataImport service project structure at `Src/Foundation/services/DataImport/`
- [ ] T002 Initialize .NET 9 Web API project with Clean Architecture layers (Domain, Application, Infrastructure, Api)
- [ ] T003 [P] Add NuGet package references: CsvHelper, EPPlus, SSH.NET, Hangfire, MassTransit, Azure.Storage.Blobs
- [ ] T004 [P] Configure centralized package management in Directory.Packages.props
- [ ] T005 [P] Add project references to shared infrastructure: ServiceDefaults, Domain base classes
- [ ] T006 [P] Create README.md documenting service purpose, layer position, dependencies
- [ ] T007 [P] Setup EditorConfig and formatting rules per NorthStarET conventions
- [ ] T008 Register DataImport service in AppHost at `Src/Foundation/AppHost/Program.cs` with PostgreSQL and Redis dependencies

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Database & Persistence

- [ ] T009 Create DbContext at `Src/Foundation/services/DataImport/Infrastructure/Persistence/DataImportDbContext.cs` with multi-tenancy support
- [ ] T010 [P] Configure entity type configurations for all domain entities in `Infrastructure/Persistence/Configurations/`
- [ ] T011 [P] Add TenantInterceptor and AuditInterceptor from shared infrastructure
- [ ] T012 [P] Add global query filters for soft delete and tenant isolation
- [ ] T013 Create initial EF Core migration: ImportJob, ImportRow, ImportTemplate, ValidationRule, ImportError, ImportAudit tables

### Domain Models (All Stories Depend on These)

- [ ] T014 [P] Create ImportJob entity in `Domain/Entities/ImportJob.cs` with Status enum, progress tracking
- [ ] T015 [P] Create ImportRow entity in `Domain/Entities/ImportRow.cs` with RawData and MappedData JSONB fields
- [ ] T016 [P] Create ImportTemplate entity in `Domain/Entities/ImportTemplate.cs` with FieldMappings and ValidationRules JSONB
- [ ] T017 [P] Create ValidationRule entity in `Domain/Entities/ValidationRule.cs` with configurable rule types
- [ ] T018 [P] Create ImportError entity in `Domain/Entities/ImportError.cs` for error tracking
- [ ] T019 [P] Create ImportAudit entity in `Domain/Entities/ImportAudit.cs` for immutable audit log

### Domain Events

- [ ] T020 [P] Create ImportStartedEvent in `Domain/Events/ImportStartedEvent.cs`
- [ ] T021 [P] Create ImportCompletedEvent in `Domain/Events/ImportCompletedEvent.cs`
- [ ] T022 [P] Create ImportFailedEvent in `Domain/Events/ImportFailedEvent.cs`
- [ ] T023 [P] Create RowValidationFailedEvent in `Domain/Events/RowValidationFailedEvent.cs`
- [ ] T024 [P] Create ImportRollbackEvent in `Domain/Events/ImportRollbackEvent.cs`
- [ ] T025 [P] Create StateTestDataImportedEvent in `Domain/Events/StateTestDataImportedEvent.cs`

### Application Infrastructure

- [ ] T026 Add MediatR configuration in `Application/DependencyInjection.cs` with pipeline behaviors
- [ ] T027 [P] Add FluentValidation for all commands in `Application/DependencyInjection.cs`
- [ ] T028 [P] Create Result/Result<T> pattern utilities in Application layer (or use shared)
- [ ] T029 [P] Configure Hangfire with PostgreSQL storage in `Infrastructure/DependencyInjection.cs`
- [ ] T030 [P] Configure MassTransit with RabbitMQ for event publishing in `Infrastructure/DependencyInjection.cs`
- [ ] T031 [P] Configure Azure Blob Storage client in `Infrastructure/Storage/BlobStorageService.cs`
- [ ] T032 [P] Configure Redis for idempotency windows in `Infrastructure/Caching/IdempotencyService.cs`

### API Infrastructure

- [ ] T033 Create API project at `Src/Foundation/services/DataImport/Api/` with Controllers, Middleware
- [ ] T034 [P] Configure authentication/authorization middleware using SessionAuthenticationHandler
- [ ] T035 [P] Configure problem+json error handling globally
- [ ] T036 [P] Configure rate limiting (10 req/min for upload/start endpoints)
- [ ] T037 [P] Configure CORS, anti-forgery tokens for state-changing operations
- [ ] T038 [P] Add Swagger/OpenAPI generation with authentication schemes
- [ ] T039 Add health checks for database, Redis, blob storage, Hangfire
- [ ] T040 Configure service telemetry and logging via ServiceDefaults

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - CSV Upload & Schema Validation (Priority: P1) 🎯 MVP

**Goal**: Upload CSV file, validate schema (columns, types), store in Azure Blob, create ImportJob with basic status tracking

**Independent Test**: Upload valid CSV → job created with Status=Uploaded; upload invalid CSV → 400 with schema errors

### Implementation for User Story 1

- [ ] T041 [P] [US1] Create UploadFileCommand in `Application/Commands/UploadFileCommand.cs` with file validation
- [ ] T042 [P] [US1] Create UploadFileCommandHandler in `Application/Commands/UploadFileCommandHandler.cs` implementing Azure Blob upload
- [ ] T043 [P] [US1] Create CSV schema validator in `Application/Validators/CsvSchemaValidator.cs` checking required columns, types
- [ ] T044 [P] [US1] Create FileParserFactory in `Infrastructure/Parsers/FileParserFactory.cs` strategy pattern
- [ ] T045 [P] [US1] Implement CsvParser in `Infrastructure/Parsers/CsvParser.cs` using CsvHelper with error handling
- [ ] T046 [US1] Create ImportsController POST /api/v1/imports at `Api/Controllers/ImportsController.cs` (multipart/form-data)
- [ ] T047 [US1] Add DTO: UploadFileRequest/Response in `Api/Models/UploadFileDto.cs`
- [ ] T048 [US1] Add idempotency check in UploadFileCommandHandler (10-minute deduplication window via Redis)
- [ ] T049 [US1] Add tenant context extraction from SessionAuthenticationHandler claims
- [ ] T050 [US1] Add audit logging for upload action to ImportAudit table

**Checkpoint**: CSV upload works end-to-end with validation and storage

---

## Phase 4: User Story 2 - Excel Multi-Sheet Mapping (Priority: P1)

**Goal**: Support Excel uploads with multiple sheets, handle merged cells, date serials, configurable sheet-to-entity mapping

**Independent Test**: Upload Excel with multiple sheets → correctly parsed with transformations applied

### Implementation for User Story 2

- [ ] T051 [P] [US2] Implement ExcelParser in `Infrastructure/Parsers/ExcelParser.cs` using EPPlus
- [ ] T052 [P] [US2] Add merged cell detection and expansion in ExcelParser
- [ ] T053 [P] [US2] Add date serial conversion in ExcelParser (Excel date → DateTime)
- [ ] T054 [P] [US2] Create ExcelMappingConfiguration in `Domain/ValueObjects/ExcelMappingConfiguration.cs`
- [ ] T055 [US2] Update UploadFileCommandHandler to detect Excel format and route to ExcelParser
- [ ] T056 [US2] Add validation for multi-sheet structure in CsvSchemaValidator (rename to FileSchemaValidator)
- [ ] T057 [US2] Update ImportsController to accept Excel file formats (.xlsx, .xls)

**Checkpoint**: Excel parsing works with merged cells, date conversions, multi-sheet support

---

## Phase 5: User Story 3 - SFTP State Test Import (Priority: P2)

**Goal**: Nightly SFTP fetch from state test servers, automatic import with schedule, emit StateTestDataImportedEvent

**Independent Test**: Trigger SFTP job manually → file fetched, imported, event published

### Implementation for User Story 3

- [ ] T058 [P] [US3] Create SftpFetcher in `Infrastructure/Parsers/SftpFetcher.cs` using SSH.NET
- [ ] T059 [P] [US3] Add SFTP connection configuration in appsettings.json with credentials from Azure Key Vault
- [ ] T060 [P] [US3] Create FetchSftpFileCommand in `Application/Commands/FetchSftpFileCommand.cs`
- [ ] T061 [P] [US3] Create FetchSftpFileCommandHandler downloading file to blob storage
- [ ] T062 [US3] Create Hangfire recurring job in `Infrastructure/Jobs/SftpImportJob.cs` with cron schedule
- [ ] T063 [US3] Register SftpImportJob in Hangfire dashboard with configurable cron expression
- [ ] T064 [US3] Publish StateTestDataImportedEvent after successful import via MassTransit
- [ ] T065 [US3] Add retry logic with exponential backoff for SFTP connection failures

**Checkpoint**: SFTP import runs on schedule, fetches files, publishes events

---

## Phase 6: User Story 4 - Field Mapping & Transformation Rules (Priority: P1)

**Goal**: Configurable field mappings per template (source column → target field), transformations (trim, uppercase, date format)

**Independent Test**: Create template with mappings → import applies transformations correctly

### Implementation for User Story 4

- [ ] T066 [P] [US4] Create MappingService in `Application/Services/MappingService.cs` with transformation pipeline
- [ ] T067 [P] [US4] Implement transformation functions: Trim, Uppercase, DateFormat, Substring, Concatenate
- [ ] T068 [P] [US4] Create CreateTemplateCommand in `Application/Commands/CreateTemplateCommand.cs`
- [ ] T069 [P] [US4] Create CreateTemplateCommandHandler in `Application/Commands/CreateTemplateCommandHandler.cs`
- [ ] T070 [US4] Create TemplatesController POST /api/v1/imports/templates at `Api/Controllers/TemplatesController.cs`
- [ ] T071 [US4] Create TemplatesController GET /api/v1/imports/templates with tenant filtering
- [ ] T072 [US4] Add DTO: CreateTemplateRequest/Response in `Api/Models/TemplateDto.cs`
- [ ] T073 [US4] Update ImportRow processing to apply MappingService transformations
- [ ] T074 [US4] Log mapping decisions to ImportAudit for traceability

**Checkpoint**: Templates work with field mappings and transformations

---

## Phase 7: User Story 5 - Large File Error Segregation (Priority: P2)

**Goal**: For large imports with failures, generate failed records CSV export + email summary report

**Independent Test**: Import with 50% failure rate → CSV export with failed rows + email sent

### Implementation for User Story 5

- [ ] T075 [P] [US5] Create ErrorReportGenerator in `Application/Services/ErrorReportGenerator.cs` exporting failed rows to CSV
- [ ] T076 [P] [US5] Create EmailService in `Infrastructure/Notifications/EmailService.cs` using Azure Communication Services
- [ ] T077 [P] [US5] Add error summary template in `Infrastructure/Notifications/Templates/ImportErrorSummary.html`
- [ ] T078 [US5] Update ImportProcessorJob to call ErrorReportGenerator on completion if errors > threshold
- [ ] T079 [US5] Upload error report CSV to blob storage with SAS token link
- [ ] T080 [US5] Send email to UploadedBy user with error count, download link, top 5 errors
- [ ] T081 [US5] Add configuration for error threshold triggering report (e.g., >100 errors or >10%)

**Checkpoint**: Error reports generated and emailed for large import failures

---

## Phase 8: User Story 6 - Scheduled Jobs & History Tracking (Priority: P1)

**Goal**: Cron-based job scheduling, retry with exponential backoff, queryable job history with filters

**Independent Test**: Schedule job with cron expression → runs on schedule, retries on failure, history queryable

### Implementation for User Story 6

- [ ] T082 [P] [US6] Create ScheduleImportCommand in `Application/Commands/ScheduleImportCommand.cs` with cron expression
- [ ] T083 [P] [US6] Create ScheduleImportCommandHandler registering recurring Hangfire job
- [ ] T084 [P] [US6] Add retry policy with exponential backoff in Hangfire job configuration
- [ ] T085 [US6] Create GetImportHistoryQuery in `Application/Queries/GetImportHistoryQuery.cs` with filters (status, entity, date range)
- [ ] T086 [US6] Create GetImportHistoryQueryHandler with pagination support
- [ ] T087 [US6] Add ImportsController GET /api/v1/imports/history endpoint with query parameters
- [ ] T088 [US6] Add job execution tracking: last run, next run, success/failure count to ImportJob entity
- [ ] T089 [US6] Expose Hangfire dashboard at /hangfire with authorization

**Checkpoint**: Scheduled jobs work with retry logic, history is queryable

---

## Phase 9: User Story 7 - Import Templates (Priority: P2)

**Goal**: Reusable templates with mappings and validation rules, tenant-scoped, CRUD operations

**Independent Test**: Create template → reuse across multiple imports → mappings applied consistently

### Implementation for User Story 7

- [ ] T090 [P] [US7] Create GetTemplateByIdQuery in `Application/Queries/GetTemplateByIdQuery.cs`
- [ ] T091 [P] [US7] Create UpdateTemplateCommand in `Application/Commands/UpdateTemplateCommand.cs`
- [ ] T092 [P] [US7] Create DeleteTemplateCommand in `Application/Commands/DeleteTemplateCommand.cs` with soft delete
- [ ] T093 [US7] Add TemplatesController PUT /api/v1/imports/templates/{id}
- [ ] T094 [US7] Add TemplatesController DELETE /api/v1/imports/templates/{id}
- [ ] T095 [US7] Add TemplatesController GET /api/v1/imports/templates/{id}
- [ ] T096 [US7] Add template versioning: increment version on update, maintain previous versions
- [ ] T097 [US7] Add tenant isolation check in all template queries (TenantId filtering)

**Checkpoint**: Template CRUD works with tenant isolation and versioning

---

## Phase 10: User Story 8 - Business Validation Rules (Priority: P1)

**Goal**: Configurable validation rules (grade range, DOB, uniqueness), hierarchical execution (structural → business)

**Independent Test**: Define grade range rule → import violating records → validation errors captured

### Implementation for User Story 8

- [ ] T098 [P] [US8] Create ValidationService in `Application/Services/ValidationService.cs` with rule engine
- [ ] T099 [P] [US8] Implement structural validators: RequiredField, DataType, MaxLength, RegexPattern
- [ ] T100 [P] [US8] Implement business validators: GradeRange, DateOfBirthRange, Uniqueness, ReferentialIntegrity
- [ ] T101 [P] [US8] Create CreateValidationRuleCommand in `Application/Commands/CreateValidationRuleCommand.cs`
- [ ] T102 [US8] Create ValidationRulesController POST /api/v1/validation-rules at `Api/Controllers/ValidationRulesController.cs`
- [ ] T103 [US8] Add hierarchical validation execution: structural first, then business if passed
- [ ] T104 [US8] Collect all errors per row in ImportError table with field-level detail
- [ ] T105 [US8] Update ImportRow status to Failed on validation failure, Validated on success

**Checkpoint**: Validation rules work with structural and business logic

---

## Phase 11: User Story 9 - Duplicate Detection (Priority: P2)

**Goal**: Exact and fuzzy duplicate detection (normalized name + DOB), manual resolution workflow

**Independent Test**: Import with duplicates → duplicates flagged, resolution options presented

### Implementation for User Story 9

- [ ] T106 [P] [US9] Create DuplicateDetectionService in `Application/Services/DuplicateDetectionService.cs`
- [ ] T107 [P] [US9] Implement exact duplicate check: hash of normalized fields
- [ ] T108 [P] [US9] Implement fuzzy duplicate check: Levenshtein distance on name + DOB match
- [ ] T109 [P] [US9] Add bounded comparison limit (max 100 candidates per record to avoid O(n²))
- [ ] T110 [US9] Create ResolveDuplicateCommand in `Application/Commands/ResolveDuplicateCommand.cs` (merge/keep/skip)
- [ ] T111 [US9] Add duplicate candidates to ImportRow.MappedData with similarity scores
- [ ] T112 [US9] Add ImportsController POST /api/v1/imports/{id}/resolve-duplicate endpoint
- [ ] T113 [US9] Update ImportRow status to AwaitingDuplicateResolution when duplicates found

**Checkpoint**: Duplicate detection works with exact and fuzzy matching

---

## Phase 12: User Story 10 - Immutable Audit Log (Priority: P1)

**Goal**: Every import operation logged (who/what/when/source), immutable audit trail, queryable

**Independent Test**: Perform import actions → audit log entries created, cannot be modified

### Implementation for User Story 10

- [ ] T114 [P] [US10] Add audit logging interceptor in `Infrastructure/Persistence/AuditInterceptor.cs`
- [ ] T115 [P] [US10] Log all state changes to ImportAudit: upload, start, complete, rollback, validation
- [ ] T116 [P] [US10] Include user identity, timestamp, tenant ID, action type, before/after state in Details JSONB
- [ ] T117 [US10] Create GetAuditLogQuery in `Application/Queries/GetAuditLogQuery.cs` with filters
- [ ] T118 [US10] Add ImportsController GET /api/v1/imports/{id}/audit endpoint
- [ ] T119 [US10] Enforce immutability: no update/delete on ImportAudit records, insert-only
- [ ] T120 [US10] Add audit log retention policy configuration (e.g., 7 years for FERPA compliance)

**Checkpoint**: Audit log captures all operations immutably

---

## Phase 13: User Story 11 - Rollback Partial Failures (Priority: P2)

**Goal**: Rollback imported records on partial failure, emit ImportRollbackEvent, coordinate cleanup across services

**Independent Test**: Import half successful → rollback → created entities deleted, audit trail maintained

### Implementation for User Story 11

- [ ] T121 [P] [US11] Create RollbackImportCommand in `Application/Commands/RollbackImportCommand.cs`
- [ ] T122 [P] [US11] Create RollbackImportCommandHandler enumerating created entity IDs from ImportRow
- [ ] T123 [P] [US11] Publish ImportRollbackEvent with entity IDs to MassTransit for cross-service cleanup
- [ ] T124 [US11] Add ImportsController POST /api/v1/imports/{id}/rollback endpoint
- [ ] T125 [US11] Update ImportJob status to RolledBack, log rollback reason
- [ ] T126 [US11] Add rollback audit entries: who initiated, when, which records affected
- [ ] T127 [US11] Handle eventual consistency: retry event publishing on failure, idempotency for rollback

**Checkpoint**: Rollback works with cross-service coordination

---

## Phase 14: User Story 12 - Real-Time Progress Tracking (Priority: P2)

**Goal**: Live progress updates (processed/total, ETA, cancellation), broadcaster updates ImportJob every N rows

**Independent Test**: Start import → watch progress updates in real-time → cancel mid-import → job stops

### Implementation for User Story 12

- [ ] T128 [P] [US12] Create ProgressBroadcaster in `Application/Services/ProgressBroadcaster.cs` using SignalR
- [ ] T129 [P] [US12] Add SignalR hub at `Api/Hubs/ImportProgressHub.cs` with tenant-scoped groups
- [ ] T130 [P] [US12] Update ImportJob progress fields: ProcessedCount, TotalRows, EstimatedCompletionTime
- [ ] T131 [US12] Add progress broadcast every 100 rows in ImportProcessorJob
- [ ] T132 [US12] Create CancelImportCommand in `Application/Commands/CancelImportCommand.cs`
- [ ] T133 [US12] Add CancellationToken support in ImportProcessorJob, check every batch
- [ ] T134 [US12] Add ImportsController POST /api/v1/imports/{id}/cancel endpoint
- [ ] T135 [US12] Update ImportJob status to Cancelled, log cancellation user and time

**Checkpoint**: Real-time progress works with cancellation support

---

## Phase 15: Core Processing Pipeline (Foundational)

**Goal**: Integrate all components into end-to-end import processing pipeline

**Independent Test**: Upload file → start import → validation → mapping → dispatch → completion with events

### Implementation for Core Pipeline

- [ ] T136 Create StartImportCommand in `Application/Commands/StartImportCommand.cs` enqueuing Hangfire job
- [ ] T137 Create StartImportCommandHandler validating job exists and status is Uploaded
- [ ] T138 Add ImportsController POST /api/v1/imports/{id}/start endpoint
- [ ] T139 Create ImportProcessorJob in `Infrastructure/Jobs/ImportProcessorJob.cs` orchestrating pipeline
- [ ] T140 Integrate FileParser → ValidationService → MappingService → dispatch in ImportProcessorJob
- [ ] T141 Add row dispatch: call target service clients (Student/Staff/Assessment) via HTTP or events
- [ ] T142 Add partial failure handling: continue processing on row failures, track errors
- [ ] T143 Add backpressure: check target service latency, throttle if > threshold
- [ ] T144 Add parallel row batches with bounded concurrency (e.g., 10 concurrent batches)
- [ ] T145 Publish ImportStartedEvent on job start
- [ ] T146 Publish ImportCompletedEvent on success with success/error counts
- [ ] T147 Publish ImportFailedEvent on catastrophic failure
- [ ] T148 Update ImportJob status transitions: Uploaded → Processing → Completed/Failed/Cancelled

**Checkpoint**: Full pipeline works end-to-end with event publishing

---

## Phase 16: Query & Reporting Endpoints

**Goal**: Implement remaining read endpoints for job details, errors, validation

**Independent Test**: Query endpoints return correct data with tenant isolation

### Implementation for Queries

- [ ] T149 [P] Create GetImportJobQuery in `Application/Queries/GetImportJobQuery.cs`
- [ ] T150 [P] Create GetImportErrorsQuery in `Application/Queries/GetImportErrorsQuery.cs` with pagination
- [ ] T151 [P] Create ValidateImportCommand in `Application/Commands/ValidateImportCommand.cs` for dry-run
- [ ] T152 [P] Add ImportsController GET /api/v1/imports/{id} endpoint
- [ ] T153 [P] Add ImportsController GET /api/v1/imports/{id}/errors endpoint with pagination
- [ ] T154 [P] Add ImportsController POST /api/v1/imports/validate endpoint for dry-run validation
- [ ] T155 Add DTOs: ImportJobDto, ImportErrorDto, ValidationResultDto in `Api/Models/`

**Checkpoint**: All query endpoints work with correct filtering and pagination

---

## Phase 17: Performance Optimization

**Goal**: Achieve NFRs: 100 records/s processing, validation <5s for 1000 rows, upload p95 <2s for 10MB

**Independent Test**: Performance benchmarks meet targets

### Performance Tasks

- [ ] T156 [P] Add database indexes: ImportJob (TenantId, Status, CreatedAt), ImportRow (JobId, Status)
- [ ] T157 [P] Add composite index on ImportError (JobId, FieldName) for error queries
- [ ] T158 [P] Optimize validation queries: batch lookups for uniqueness checks
- [ ] T159 [P] Add database connection pooling configuration in DbContext
- [ ] T160 Add batch insert for ImportRow records (use AddRange instead of individual adds)
- [ ] T161 Add memory streaming for large file parsing (avoid loading entire file)
- [ ] T162 Add parallel batch processing with configurable concurrency limit
- [ ] T163 Add caching for validation rules, templates (Redis with 10-minute TTL)
- [ ] T164 Run performance benchmarks: measure throughput, latency, memory usage
- [ ] T165 Tune Hangfire worker count and batch sizes based on benchmarks

**Checkpoint**: Performance targets met per NFRs

---

## Phase 18: Security & Compliance

**Goal**: Malware scanning, encryption, FERPA compliance, secure file handling

**Independent Test**: Security measures verified

### Security Tasks

- [ ] T166 [P] Add malware scan hook in UploadFileCommandHandler (integrate with Azure Defender or ClamAV)
- [ ] T167 [P] Add file size limits: 100MB max per upload, reject larger files
- [ ] T168 [P] Add file type validation: whitelist CSV, XLSX, XLS only
- [ ] T169 [P] Configure blob storage encryption at rest (Azure Storage Service Encryption)
- [ ] T170 [P] Generate short-lived SAS tokens (15 minutes) for blob access
- [ ] T171 Add content security policy headers in API responses
- [ ] T172 Add sensitive data masking in logs (PII fields like SSN, DOB)
- [ ] T173 Add FERPA compliance documentation in README: data retention, access controls
- [ ] T174 Add authorization checks: verify user belongs to tenant for all operations
- [ ] T175 Add rate limiting: 10 req/min per tenant for upload/start endpoints

**Checkpoint**: Security and compliance measures implemented

---

## Phase 19: Observability & Monitoring

**Goal**: Distributed tracing, metrics, health checks, alerting for import failures

**Independent Test**: Telemetry visible in Aspire dashboard, alerts triggered on failures

### Observability Tasks

- [ ] T176 [P] Add Activity tracing spans for: upload, validation, mapping, dispatch, completion
- [ ] T177 [P] Add custom metrics: import_jobs_total, import_rows_processed, import_errors_total
- [ ] T178 [P] Add performance metrics: upload_duration, validation_duration, processing_throughput
- [ ] T179 Add structured logging with correlation IDs across pipeline stages
- [ ] T180 Add health check endpoints: database, blob storage, Redis, Hangfire, RabbitMQ
- [ ] T181 Configure Aspire dashboard integration for DataImport service
- [ ] T182 Add alerting rules: job failures, high error rates, performance degradation
- [ ] T183 Add logging for idempotency hits, duplicate detection results, rollback events
- [ ] T184 Document observability practices in README: metrics, logs, traces

**Checkpoint**: Full observability implemented

---

## Phase 20: Integration Testing

**Goal**: End-to-end tests for all user stories, contract tests for all endpoints

**Independent Test**: All integration tests pass

### Integration Test Tasks

- [ ] T185 [P] Create integration test project at `tests/DataImport.Integration.Tests/`
- [ ] T186 [P] Setup test fixtures: test database, blob storage emulator, Redis
- [ ] T187 [P] Create test data builders for ImportJob, ImportTemplate, ValidationRule
- [ ] T188 [P] Test US1: CSV upload end-to-end with valid and invalid schemas
- [ ] T189 [P] Test US2: Excel parsing with merged cells, date conversions
- [ ] T190 [P] Test US3: SFTP fetch (mock SFTP server), event publishing
- [ ] T191 [P] Test US4: Field mapping transformations applied correctly
- [ ] T192 [P] Test US5: Error report generation and email sending (mock SMTP)
- [ ] T193 [P] Test US6: Scheduled job execution, retry backoff
- [ ] T194 [P] Test US7: Template CRUD with tenant isolation
- [ ] T195 [P] Test US8: Business validation rules enforced
- [ ] T196 [P] Test US9: Duplicate detection with exact and fuzzy matching
- [ ] T197 [P] Test US10: Audit log entries created immutably
- [ ] T198 [P] Test US11: Rollback with event publishing, entity cleanup
- [ ] T199 [P] Test US12: Progress updates via SignalR, cancellation
- [ ] T200 [P] Test multi-tenancy: cross-tenant data isolation verified
- [ ] T201 [P] Test idempotency: duplicate upload requests handled correctly
- [ ] T202 Test full pipeline: upload → process → validate → map → dispatch → complete

**Checkpoint**: All integration tests pass

---

## Phase 21: Documentation

**Goal**: Complete API documentation, runbooks, architecture diagrams

**Independent Test**: Documentation accurate and complete

### Documentation Tasks

- [ ] T203 [P] Update README.md with service overview, architecture, dependencies
- [ ] T204 [P] Document API contracts in OpenAPI spec with examples
- [ ] T205 [P] Create architecture diagram: pipeline flow, event publishing, service interactions
- [ ] T206 [P] Document data model: entity relationships, JSONB schemas
- [ ] T207 [P] Create runbook: deployment, configuration, troubleshooting
- [ ] T208 [P] Document performance tuning: concurrency, batch sizes, caching
- [ ] T209 [P] Document security measures: malware scanning, encryption, FERPA compliance
- [ ] T210 [P] Document monitoring: metrics, logs, alerts, health checks
- [ ] T211 Create quickstart guide: local setup, sample import, validation

**Checkpoint**: Documentation complete

---

## Phase 22: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements, cleanup, validation

- [ ] T212 [P] Code review and refactoring for clarity, maintainability
- [ ] T213 [P] Remove dead code, unused dependencies
- [ ] T214 [P] Standardize error messages across all endpoints
- [ ] T215 [P] Validate all DTOs have XML documentation comments
- [ ] T216 [P] Run code coverage analysis, ensure ≥80% coverage
- [ ] T217 Validate layer isolation: no violations of Clean Architecture boundaries
- [ ] T218 Validate shared infrastructure usage: only approved dependencies
- [ ] T219 Run full test suite: unit, integration, Aspire orchestration
- [ ] T220 Deploy to dev environment and smoke test all user stories
- [ ] T221 Performance test under load: 10,000 row import, verify targets met
- [ ] T222 Security audit: verify all inputs validated, authorization enforced
- [ ] T223 Run Aspire dashboard and verify service health, telemetry
- [ ] T224 Final README.md update: deployment instructions, known issues

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-14)**: All depend on Foundational phase completion
  - Core stories (US1, US4, US6, US8, US10) should be prioritized as P1
  - Enhancement stories (US2, US3, US5, US7, US9, US11, US12) can follow as P2
- **Core Pipeline (Phase 15)**: Depends on US1, US4, US6, US8 being complete
- **Query Endpoints (Phase 16)**: Depends on Core Pipeline completion
- **Performance (Phase 17)**: Depends on Core Pipeline completion
- **Security (Phase 18)**: Can start after Foundational phase
- **Observability (Phase 19)**: Can start after Core Pipeline completion
- **Integration Testing (Phase 20)**: Depends on all features being complete
- **Documentation (Phase 21)**: Can start in parallel with development
- **Polish (Phase 22)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (CSV Upload)**: Can start after Foundational - No dependencies on other stories ✅ MVP
- **US2 (Excel)**: Extends US1 - Should complete US1 first
- **US3 (SFTP)**: Independent of US1/US2 - Can run in parallel
- **US4 (Mapping)**: Required by all import types - Should complete early (P1)
- **US5 (Error Reports)**: Depends on Core Pipeline - Can be added later
- **US6 (Scheduling)**: Independent - Can run in parallel with other stories
- **US7 (Templates)**: Extends US4 - Should complete US4 first
- **US8 (Validation)**: Required by Core Pipeline - Must complete early (P1)
- **US9 (Duplicates)**: Extends US8 - Can be added after validation works
- **US10 (Audit)**: Foundation requirement - Should be in place early (P1)
- **US11 (Rollback)**: Depends on Core Pipeline - Can be added later
- **US12 (Progress)**: Enhancement - Can be added later

### Within Each User Story

- Domain models before services
- Services before commands/queries
- Commands/queries before controllers
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational domain entities [P] can run in parallel (T014-T019)
- All Foundational domain events [P] can run in parallel (T020-T025)
- All Foundational infrastructure [P] can run in parallel after entities/events
- All API infrastructure [P] can run in parallel (T034-T040)
- Once Foundational phase completes:
  - US1, US4, US6, US8, US10 can all start in parallel (if team capacity allows)
  - US3 can run completely independently
- All query endpoint tasks marked [P] can run in parallel (Phase 16)
- All performance tasks marked [P] can run in parallel (Phase 17)
- All security tasks marked [P] can run in parallel (Phase 18)
- All observability tasks marked [P] can run in parallel (Phase 19)
- All integration test tasks marked [P] can run in parallel (Phase 20)
- All documentation tasks marked [P] can run in parallel (Phase 21)
- All polish tasks marked [P] can run in parallel (Phase 22)

---

## Parallel Example: Foundational Phase

```bash
# Launch all domain entities together:
Task T014: "Create ImportJob entity"
Task T015: "Create ImportRow entity"
Task T016: "Create ImportTemplate entity"
Task T017: "Create ValidationRule entity"
Task T018: "Create ImportError entity"
Task T019: "Create ImportAudit entity"

# Then launch all domain events together:
Task T020: "Create ImportStartedEvent"
Task T021: "Create ImportCompletedEvent"
Task T022: "Create ImportFailedEvent"
Task T023: "Create RowValidationFailedEvent"
Task T024: "Create ImportRollbackEvent"
Task T025: "Create StateTestDataImportedEvent"
```

---

## Parallel Example: User Story 1

```bash
# Launch all parallel US1 tasks together:
Task T041: "Create UploadFileCommand"
Task T042: "Create UploadFileCommandHandler"
Task T043: "Create CSV schema validator"
Task T044: "Create FileParserFactory"
Task T045: "Implement CsvParser"
```

---

## Implementation Strategy

### MVP First (Core Import Functionality)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete US1: CSV Upload & Validation
4. Complete US4: Field Mapping & Transformations
5. Complete US8: Business Validation Rules
6. Complete US10: Audit Logging
7. Complete Phase 15: Core Processing Pipeline
8. **STOP and VALIDATE**: Test end-to-end CSV import pipeline
9. Deploy/demo if ready

**MVP Deliverable**: CSV file upload → validation → mapping → dispatch with audit trail

### Incremental Delivery (Feature Expansion)

1. Add US6: Scheduled Jobs (enables automation)
2. Add US2: Excel Support (expand file format support)
3. Add US12: Progress Tracking (UX improvement)
4. Add US9: Duplicate Detection (data quality)
5. Add US11: Rollback (reliability)
6. Add US5: Error Reports (operational support)
7. Add US7: Templates (efficiency)
8. Add US3: SFTP (integration)

Each addition is independently testable and deployable.

### Parallel Team Strategy

With multiple developers after Foundational phase:

**Team A (Core Pipeline)**: US1 → US4 → US8 → Phase 15  
**Team B (Automation)**: US6 → US7  
**Team C (Advanced Features)**: US9 → US11 → US12  
**Team D (Integrations)**: US2 → US3 → US5  
**Team E (Cross-Cutting)**: US10 → Phase 17 → Phase 18 → Phase 19

Stories integrate independently with minimal merge conflicts.

---

## Notes

- **[P] tasks**: Different files, no dependencies - safe to parallelize
- **[Story] label**: Maps task to specific user story for traceability
- Each user story delivers independent, testable functionality
- MVP (US1 + US4 + US8 + US10 + Pipeline) provides core value
- Incremental delivery allows early feedback and course correction
- Multi-tenancy enforced at entity, query, and API levels
- Idempotency ensures safe retries for uploads and processing
- Event-driven architecture enables loose coupling with target services
- Clean Architecture maintains separation of concerns
- Performance NFRs validated in Phase 17 before production deployment

---

## Total Task Count: 224 tasks

**By Phase**:
- Phase 1 (Setup): 8 tasks
- Phase 2 (Foundational): 32 tasks
- Phase 3 (US1): 10 tasks
- Phase 4 (US2): 7 tasks
- Phase 5 (US3): 8 tasks
- Phase 6 (US4): 9 tasks
- Phase 7 (US5): 7 tasks
- Phase 8 (US6): 8 tasks
- Phase 9 (US7): 8 tasks
- Phase 10 (US8): 8 tasks
- Phase 11 (US9): 8 tasks
- Phase 12 (US10): 7 tasks
- Phase 13 (US11): 7 tasks
- Phase 14 (US12): 8 tasks
- Phase 15 (Core Pipeline): 13 tasks
- Phase 16 (Query Endpoints): 7 tasks
- Phase 17 (Performance): 10 tasks
- Phase 18 (Security): 10 tasks
- Phase 19 (Observability): 9 tasks
- Phase 20 (Integration Testing): 18 tasks
- Phase 21 (Documentation): 9 tasks
- Phase 22 (Polish): 13 tasks

**By User Story**:
- US1 (CSV Upload): 10 tasks
- US2 (Excel): 7 tasks
- US3 (SFTP): 8 tasks
- US4 (Mapping): 9 tasks
- US5 (Error Reports): 7 tasks
- US6 (Scheduling): 8 tasks
- US7 (Templates): 8 tasks
- US8 (Validation): 8 tasks
- US9 (Duplicates): 8 tasks
- US10 (Audit): 7 tasks
- US11 (Rollback): 7 tasks
- US12 (Progress): 8 tasks

**Parallel Opportunities**: 119 tasks marked [P] (53% parallelizable)

**Independent Test Criteria**: Each user story has clear verification steps

**Suggested MVP Scope**: US1 + US4 + US8 + US10 + Core Pipeline (48 tasks + 40 foundational = 88 tasks)

**Format Validation**: ✅ ALL tasks follow checklist format (checkbox, ID, [P] for parallel, [Story] for user stories, file paths included)
