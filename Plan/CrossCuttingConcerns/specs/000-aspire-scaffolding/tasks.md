# Tasks: Aspire Orchestration & Cross-Cutting Scaffolding

**Specification Branch**: `CrossCuttingConcerns/000-aspire-scaffolding-spec` *(current branch - planning artifacts)*
**Implementation Branch**: `CrossCuttingConcerns/000-aspire-scaffolding` *(created by `/speckit.implement`)*
**Feature**: 000-aspire-scaffolding
**Date**: 2025-11-20

---

## Layer Context (MANDATORY)

**Target Layer**: CrossCuttingConcerns
**Implementation Path**: `Src/Foundation/AppHost`, `Src/Foundation/shared/ServiceDefaults`, `Src/Foundation/shared/Infrastructure`
**Specification Path**: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/`

**Note**: CrossCuttingConcerns are implemented in Foundation shared libraries as they provide scaffolding for all other layers.

### Layer Consistency Checklist

- [x] Target Layer matches spec.md declaration (CrossCuttingConcerns)
- [x] Implementation path follows layer structure (Foundation shared libraries)
- [x] Specification path follows layer structure (Plan/CrossCuttingConcerns/specs/)
- [x] No cross-layer service dependencies (this feature builds the base layer)

### Layer Compliance Validation

**Phase 2 Validation Tasks** (MANDATORY before implementation):

- [ ] T001 Verify no direct service-to-service references across layers in Src/Foundation/shared/
- [ ] T002 Verify all shared infrastructure is isolated in ServiceDefaults, Domain, Application, Infrastructure
- [ ] T003 Verify AppHost only references projects, not other layers' services
- [ ] T004 Review dependency graph to confirm Foundation shared libraries have no upstream layer dependencies

---

## Identity & Authentication Compliance (OPTIONAL - adjust if feature requires auth)

**Status**: This feature establishes authentication scaffolding (ServiceDefaults includes Entra ID configuration)

**Verification Tasks**:

- [ ] T005 [P] Verify ServiceDefaults uses Microsoft.Identity.Web for JWT validation in Src/Foundation/shared/ServiceDefaults/Extensions.cs
- [ ] T006 [P] Verify no Duende IdentityServer references in ServiceDefaults
- [ ] T007 Verify Entra ID authority URL configuration follows `Plan/Foundation/docs/legacy-identityserver-migration.md`

---

## Phase 1: Setup

**Purpose**: Initialize Aspire projects and install required packages

- [ ] T008 Create Aspire AppHost project at Src/Foundation/AppHost/AppHost.csproj with Aspire.Hosting 9.5.x
- [ ] T009 [P] Add ServiceDefaults project at Src/Foundation/shared/ServiceDefaults/ServiceDefaults.csproj with Aspire.Hosting.AppHost 9.5.x reference
- [ ] T010 [P] Install Entity Framework Core 10 (Npgsql.EntityFrameworkCore.PostgreSQL 10.0.x) in Src/Foundation/shared/Infrastructure/Infrastructure.csproj
- [ ] T011 [P] Install MassTransit 8.x (MassTransit.RabbitMQ) in Src/Foundation/shared/Infrastructure/Infrastructure.csproj
- [ ] T012 [P] Install StackExchange.Redis 2.x in Src/Foundation/shared/Infrastructure/Infrastructure.csproj
- [ ] T013 [P] Install OpenTelemetry 1.x packages (OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.AspNetCore) in Src/Foundation/shared/ServiceDefaults/ServiceDefaults.csproj
- [ ] T014 [P] Create scaffolding scripts directory at .specify/scripts/powershell/ and .specify/scripts/bash/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core shared infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T015 Create EntityBase abstract class in Src/Foundation/shared/Domain/Entities/EntityBase.cs with Id, CreatedAt, UpdatedAt, DeletedAt, DomainEvents properties
- [ ] T016 [P] Create ITenantEntity interface in Src/Foundation/shared/Domain/Entities/ITenantEntity.cs with TenantId property
- [ ] T017 [P] Create AuditLog entity in Src/Foundation/shared/Infrastructure/Persistence/Entities/AuditLog.cs matching data-model.md schema
- [ ] T018 Create shared DbContext base class in Src/Foundation/shared/Infrastructure/Persistence/ApplicationDbContext.cs with AuditLogs DbSet

**Checkpoint**: Foundation entities defined - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - AppHost Boot with Resource Orchestration (Priority: P1) 🎯 MVP

**Goal**: Run `dotnet run --project Src/Foundation/AppHost` and boot PostgreSQL, Redis, RabbitMQ with health checks in <15s

**Independent Test**: Execute AppHost, verify Aspire dashboard shows all resources healthy at http://localhost:15000

### Implementation for User Story 1

- [ ] T019 [P] [US1] Add PostgreSQL resource definition in Src/Foundation/AppHost/Program.cs using AddPostgres()
- [ ] T020 [P] [US1] Add Redis resource definition in Src/Foundation/AppHost/Program.cs using AddRedis()
- [ ] T021 [P] [US1] Add RabbitMQ resource definition in Src/Foundation/AppHost/Program.cs using AddRabbitMQ()
- [ ] T022 [US1] Configure health checks for PostgreSQL, Redis, RabbitMQ in Src/Foundation/AppHost/Program.cs
- [ ] T023 [US1] Add WaitFor() dependencies between resources in Src/Foundation/AppHost/Program.cs
- [ ] T024 [US1] Configure Aspire dashboard port (15000) in Src/Foundation/AppHost/Properties/launchSettings.json
- [ ] T025 [US1] Test AppHost startup time (<15s requirement) and verify dashboard accessibility

**Checkpoint**: At this point, AppHost should boot all resources successfully and display healthy status in dashboard

---

## Phase 4: User Story 3 - Tenant Isolation with TenantInterceptor (Priority: P1)

**Goal**: Enforce TenantId on all EF Core queries/saves with opt-out via `[IgnoreTenantFilter]` attribute and audit logging

**Independent Test**: Create entity without TenantId, verify save fails; use `[IgnoreTenantFilter]` method, verify audit log entry created

### Implementation for User Story 3

- [ ] T026 [P] [US3] Create IgnoreTenantFilterAttribute in Src/Foundation/shared/Infrastructure/Attributes/IgnoreTenantFilterAttribute.cs
- [ ] T027 [P] [US3] Create TenantInterceptor (SaveChangesInterceptor) in Src/Foundation/shared/Infrastructure/Persistence/Interceptors/TenantInterceptor.cs
- [ ] T028 [US3] Implement SavingChanges override to enforce TenantId on ITenantEntity instances in TenantInterceptor.cs
- [ ] T029 [US3] Implement attribute detection logic in TenantInterceptor.cs to check for IgnoreTenantFilterAttribute on calling method
- [ ] T030 [US3] Implement automatic audit log creation when IgnoreTenantFilterAttribute is detected in TenantInterceptor.cs
- [ ] T031 [US3] Add TenantInterceptor registration to DbContext in Src/Foundation/shared/Infrastructure/Persistence/ApplicationDbContext.cs
- [ ] T032 [US3] Configure PostgreSQL Row-Level Security (RLS) policy in migration script for tenant_id column
- [ ] T033 [US3] Test tenant isolation: Attempt to save entity without TenantId, verify exception thrown
- [ ] T034 [US3] Test opt-out: Call method with `[IgnoreTenantFilter]`, verify audit log entry in AuditLogs table

**Checkpoint**: At this point, tenant isolation should be enforced with auditable opt-out mechanism

---

## Phase 5: User Story 4 - Event Publication with MassTransit (Priority: P1)

**Goal**: Publish domain events to RabbitMQ with retry/DLQ, delivering events <500ms P95

**Independent Test**: Trigger event publication, verify message in RabbitMQ queue via management UI; simulate failure, verify DLQ routing

### Implementation for User Story 4

- [ ] T035 [P] [US4] Create MassTransitExtensions in Src/Foundation/shared/Infrastructure/Messaging/MassTransitExtensions.cs
- [ ] T036 [US4] Configure MassTransit with RabbitMQ transport in MassTransitExtensions.cs (UseRabbitMq())
- [ ] T037 [US4] Configure retry policy (3 attempts with exponential backoff) in MassTransitExtensions.cs
- [ ] T038 [US4] Configure dead-letter queue (DLQ) routing in MassTransitExtensions.cs
- [ ] T039 [US4] Add MassTransit registration to IServiceCollection in MassTransitExtensions.cs (AddMassTransit())
- [ ] T040 [US4] Create sample domain event (e.g., TenantCreatedEvent) in Src/Foundation/shared/Domain/Events/TenantCreatedEvent.cs
- [ ] T041 [US4] Integrate domain event publication in EntityBase after SaveChanges in Src/Foundation/shared/Infrastructure/Persistence/ApplicationDbContext.cs
- [ ] T042 [US4] Test event publication: Publish TenantCreatedEvent, verify message in RabbitMQ management UI
- [ ] T043 [US4] Test DLQ routing: Simulate consumer failure, verify message routed to DLQ after retries

**Checkpoint**: At this point, domain events should publish reliably to RabbitMQ with retry/DLQ support

---

## Phase 6: User Story 5 - Redis Caching & Idempotency (Priority: P2)

**Goal**: Implement Redis-backed idempotency with 10-minute TTL, returning 202 Accepted with original entity ID on duplicates

**Independent Test**: POST duplicate request with same X-Idempotency-Key, verify 202 response with original entity ID

### Implementation for User Story 5

- [ ] T044 [P] [US5] Create IdempotencyService in Src/Foundation/shared/Infrastructure/Caching/IdempotencyService.cs
- [ ] T045 [US5] Implement CheckIdempotency method in IdempotencyService.cs to query Redis by idempotency key
- [ ] T046 [US5] Implement StoreIdempotency method in IdempotencyService.cs to store entity ID with 10-minute TTL
- [ ] T047 [US5] Configure Redis connection in ServiceDefaults at Src/Foundation/shared/ServiceDefaults/Extensions.cs
- [ ] T048 [US5] Create middleware IdempotencyMiddleware in Src/Foundation/shared/Infrastructure/Middleware/IdempotencyMiddleware.cs to extract X-Idempotency-Key header
- [ ] T049 [US5] Integrate IdempotencyService in IdempotencyMiddleware.cs to check/store idempotency envelopes
- [ ] T050 [US5] Return 202 Accepted with original entity ID when duplicate detected in IdempotencyMiddleware.cs
- [ ] T051 [US5] Test idempotency: POST request with X-Idempotency-Key header, repeat request, verify 202 response with original ID
- [ ] T052 [US5] Test TTL expiration: POST with idempotency key, wait 10+ minutes, repeat request, verify new 201 response

**Checkpoint**: At this point, Redis-backed idempotency should prevent duplicate operations and return consistent responses

---

## Phase 7: User Story 6 - Observability with OpenTelemetry (Priority: P2)

**Goal**: Configure distributed tracing, metrics, and logs with correlation IDs across all services

**Independent Test**: Execute API request, verify trace with correlation ID in Aspire dashboard telemetry view

### Implementation for User Story 6

- [ ] T053 [P] [US6] Configure OpenTelemetry tracing in Src/Foundation/shared/ServiceDefaults/Extensions.cs using AddOpenTelemetry()
- [ ] T054 [P] [US6] Configure OpenTelemetry metrics in Src/Foundation/shared/ServiceDefaults/Extensions.cs (AddMeter())
- [ ] T055 [P] [US6] Configure OpenTelemetry logging in Src/Foundation/shared/ServiceDefaults/Extensions.cs (AddLogging())
- [ ] T056 [US6] Create CorrelationIdMiddleware in Src/Foundation/shared/Infrastructure/Middleware/CorrelationIdMiddleware.cs to extract/generate X-Correlation-ID header
- [ ] T057 [US6] Add correlation ID to activity baggage in CorrelationIdMiddleware.cs for trace propagation
- [ ] T058 [US6] Configure OTLP exporter endpoint in ServiceDefaults for Aspire dashboard
- [ ] T059 [US6] Test tracing: Execute API request, verify trace appears in Aspire dashboard with correlation ID
- [ ] T060 [US6] Test metrics: Execute API requests, verify request count/duration metrics in dashboard
- [ ] T061 [US6] Test logging: Trigger log event, verify log entry with correlation ID in dashboard logs view

**Checkpoint**: At this point, full observability should be configured with distributed tracing, metrics, and correlated logs

---

## Phase 8: User Story 2 - Service Scaffolding Scripts (Priority: P2)

**Goal**: Run `.\new-service.ps1 -ServiceName "StudentManagement"` or `./new-service.sh StudentManagement` to generate service structure with AppHost registration

**Independent Test**: Execute scaffolding script with new service name, verify project created at Src/Foundation/services/<ServiceName>/ with AppHost Program.cs updated

### Implementation for User Story 2

- [ ] T062 [P] [US2] Create new-service.ps1 PowerShell script in .specify/scripts/powershell/new-service.ps1
- [ ] T063 [P] [US2] Create new-service.sh Bash script in .specify/scripts/bash/new-service.sh
- [ ] T064 [US2] Implement service name parameter validation in new-service.ps1 (requires -ServiceName)
- [ ] T065 [US2] Implement service name parameter validation in new-service.sh (requires positional argument)
- [ ] T066 [US2] Generate Domain layer folder structure (Entities, Events, ValueObjects) in scripts
- [ ] T067 [US2] Generate Application layer folder structure (Commands, Queries, Handlers, Validators) in scripts
- [ ] T068 [US2] Generate Infrastructure layer folder structure (Persistence, Messaging, Caching) in scripts
- [ ] T069 [US2] Generate API layer folder structure (Controllers, Program.cs, appsettings.json) in scripts
- [ ] T070 [US2] Add service project to solution file (.sln) in scripts
- [ ] T071 [US2] Update AppHost Program.cs to register new service with AddProject() in scripts
- [ ] T072 [US2] Add service references (ServiceDefaults, Domain, Application, Infrastructure) in scripts
- [ ] T073 [US2] Test PowerShell script: Run `.\new-service.ps1 -ServiceName "TestService"`, verify project created and AppHost updated
- [ ] T074 [US2] Test Bash script: Run `./new-service.sh TestService`, verify project created and AppHost updated

**Checkpoint**: At this point, scaffolding scripts should generate complete service structure with Clean Architecture layers

---

## Phase 9: User Story 7 - API Gateway with Legacy Routing (Priority: P2)

**Goal**: Configure YARP in API Gateway to route /legacy/* to old system, /api/* to new services (Strangler Fig pattern)

**Independent Test**: Send request to /legacy/health, verify forwarded to legacy system; send /api/health, verify routed to new service

### Implementation for User Story 7

- [ ] T075 [P] [US7] Create ApiGateway project at Src/Foundation/services/ApiGateway/ApiGateway.csproj
- [ ] T076 [US7] Install Yarp.ReverseProxy package in ApiGateway.csproj
- [ ] T077 [US7] Configure YARP routes in Src/Foundation/services/ApiGateway/appsettings.json (ReverseProxy section)
- [ ] T078 [US7] Add /legacy/* route cluster pointing to legacy system URL in appsettings.json
- [ ] T079 [US7] Add /api/* route clusters for new microservices in appsettings.json
- [ ] T080 [US7] Configure YARP middleware in Src/Foundation/services/ApiGateway/Program.cs (MapReverseProxy())
- [ ] T081 [US7] Register ApiGateway in AppHost Program.cs with AddProject() and WaitFor() dependencies
- [ ] T082 [US7] Test legacy routing: Send GET /legacy/health, verify response from legacy system
- [ ] T083 [US7] Test new service routing: Send GET /api/health, verify response from new service

**Checkpoint**: At this point, API Gateway should route requests to legacy and new systems based on path prefix

---

## Phase 10: User Story 8 - Resilient Messaging with Circuit Breaker (Priority: P2)

**Goal**: Implement circuit breaker pattern in MassTransit consumers to prevent cascading failures

**Independent Test**: Simulate downstream service failure, verify circuit opens after threshold; verify automatic recovery after timeout

### Implementation for User Story 8

- [ ] T084 [P] [US8] Add Polly package to Src/Foundation/shared/Infrastructure/Infrastructure.csproj
- [ ] T085 [US8] Configure circuit breaker policy in Src/Foundation/shared/Infrastructure/Messaging/MassTransitExtensions.cs
- [ ] T086 [US8] Set circuit breaker thresholds (5 failures, 30s timeout) in MassTransitExtensions.cs
- [ ] T087 [US8] Integrate circuit breaker with MassTransit retry policy in MassTransitExtensions.cs
- [ ] T088 [US8] Add fallback behavior (log error, notify monitoring) when circuit is open in MassTransitExtensions.cs
- [ ] T089 [US8] Test circuit breaker: Simulate 5 consecutive consumer failures, verify circuit opens
- [ ] T090 [US8] Test circuit recovery: Wait 30s after circuit opens, verify circuit half-opens and recovers on success

**Checkpoint**: At this point, messaging infrastructure should handle downstream failures gracefully with circuit breaker pattern

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T091 [P] Update quickstart.md with complete AppHost execution instructions
- [ ] T092 [P] Add troubleshooting section to quickstart.md for common container startup issues
- [ ] T093 Update README.md in Src/Foundation/AppHost/ with resource dependency diagram
- [ ] T094 [P] Create integration test suite in tests/integration/ to verify full stack boot
- [ ] T095 [P] Create BDD feature files in specs/000-aspire-scaffolding/features/ for tenant isolation, idempotency, observability scenarios
- [ ] T096 Performance testing: Measure AppHost startup time, verify <15s requirement
- [ ] T097 Performance testing: Measure API overhead with TenantInterceptor, verify <50ms P95 requirement
- [ ] T098 Performance testing: Measure event publication latency, verify <500ms P95 requirement
- [ ] T099 Security review: Verify no secrets in appsettings.json, all secrets use Aspire resource configuration
- [ ] T100 Run quickstart.md validation: Execute all quickstart commands, verify expected outputs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup (Phase 1) completion - BLOCKS all user stories
- **User Stories (Phase 3-10)**: All depend on Foundational (Phase 2) completion
  - Priority P1 stories (US1, US3, US4) should complete first as they establish core infrastructure
  - Priority P2 stories (US2, US5, US6, US7, US8) can proceed after P1 stories or in parallel
- **Polish (Phase 11)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 - AppHost Boot (P1)**: Can start after Foundational - No dependencies on other stories
- **US3 - Tenant Isolation (P1)**: Can start after Foundational - No dependencies on other stories
- **US4 - Event Publication (P1)**: Depends on US1 (requires RabbitMQ resource from AppHost)
- **US2 - Scaffolding (P2)**: Can start after Foundational - Should reference US1 (AppHost registration pattern)
- **US5 - Caching/Idempotency (P2)**: Depends on US1 (requires Redis resource from AppHost)
- **US6 - Observability (P2)**: Can start after Foundational - No dependencies on other stories
- **US7 - API Gateway (P2)**: Can start after Foundational - Should reference US1 (AppHost registration pattern)
- **US8 - Resilient Messaging (P2)**: Depends on US4 (extends MassTransit configuration)

### Within Each User Story

- Tasks within a story should be executed in sequential order unless marked [P] for parallel
- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before middleware/extensions
- Core implementation before integration testing
- Story complete before moving to next priority

### Parallel Opportunities

**Phase 1 (Setup)**: All tasks except T008 can run in parallel after AppHost project is created
**Phase 2 (Foundational)**: T016, T017 can run in parallel after T015 is complete
**Phase 3 (US1)**: T019, T020, T021 can run in parallel (different resources)
**Phase 4 (US3)**: T026, T027 can run in parallel (different files)
**Phase 5 (US4)**: T035, T040 can run in parallel (different files)
**Phase 6 (US5)**: T044 and T048 can run in parallel (different concerns)
**Phase 7 (US6)**: T053, T054, T055 can run in parallel (different OTEL providers)
**Phase 8 (US2)**: T062, T063 can run in parallel (different script languages)
**Phase 9 (US7)**: T075, T076 can run in parallel (project creation + package install)
**Phase 10 (US8)**: T084 and T085 can run in parallel (package install + config start)
**Phase 11 (Polish)**: T091, T092, T094, T095 can all run in parallel (different files/concerns)

---

## Parallel Example: User Story 1

```bash
# Launch all resource definitions together in AppHost Program.cs:
Task T019: "Add PostgreSQL resource definition"
Task T020: "Add Redis resource definition"
Task T021: "Add RabbitMQ resource definition"
# These can be implemented in parallel as they are independent resource configurations
```

---

## Implementation Strategy

### MVP First (P1 Stories Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 - AppHost Boot
4. Complete Phase 4: User Story 3 - Tenant Isolation
5. Complete Phase 5: User Story 4 - Event Publication
6. **STOP and VALIDATE**: Test P1 stories independently
7. Deploy/demo if ready - this provides core infrastructure for all services

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (AppHost) → Test independently → Deploy (MVP: orchestration working!)
3. Add US3 (Tenant Isolation) → Test independently → Deploy (MVP: multi-tenancy secured!)
4. Add US4 (Event Publication) → Test independently → Deploy (MVP: event-driven ready!)
5. Add US2 (Scaffolding) → Test independently → Deploy (developers can scaffold new services!)
6. Add US5 (Caching/Idempotency) → Test independently → Deploy (production-grade reliability!)
7. Add US6 (Observability) → Test independently → Deploy (full monitoring in place!)
8. Add US7 (API Gateway) → Test independently → Deploy (legacy migration path ready!)
9. Add US8 (Resilient Messaging) → Test independently → Deploy (fault tolerance complete!)
10. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (AppHost Boot)
   - Developer B: US3 (Tenant Isolation)
   - Developer C: US6 (Observability) - independent of other stories
3. After US1 completes:
   - Developer A: US4 (Event Publication) - depends on US1
   - Developer D: US5 (Caching/Idempotency) - depends on US1
4. After US4 completes:
   - Developer A: US8 (Resilient Messaging) - depends on US4
5. Remaining stories (US2, US7) can proceed independently

---

## Notes

- [P] tasks = different files, no dependencies within same phase
- [Story] label maps task to specific user story for traceability (US1-US8)
- Each user story should be independently completable and testable
- Commit after each task or logical group of [P] tasks
- Stop at each checkpoint to validate story independently
- Priority P1 stories (US1, US3, US4) are CRITICAL for MVP - complete these first
- Priority P2 stories add production-grade features - implement after P1 validation
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
- **CRITICAL**: All tasks follow strict format: `- [ ] [TaskID] [P?] [Story?] Description with file path`
