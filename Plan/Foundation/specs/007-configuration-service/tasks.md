---
description: "Task list for Configuration Service Migration (hierarchical settings, cache, audit)"
---

# Tasks: Configuration Service Migration

**Specification Branch**: `Foundation/007-configuration-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/007-configuration-service` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/007-configuration-service/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Configuration/`  
**Specification Path**: `Plan/Foundation/specs/007-configuration-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/services/Configuration/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/007-configuration-service/`)
- [ ] Shared dependencies (Redis cache, MassTransit events, RLS) align between plan and spec
- [ ] Cross-layer dependencies (Assessment/Section consumers) documented

---

## Layer Compliance Validation

- [ ] T001 Verify service references only Foundation shared libraries and multi-tenant infrastructure (`Src/Foundation/services/Configuration/Configuration.csproj`)
- [ ] T002 Verify no direct references to higher-layer services; outbound communication via events/contracts
- [ ] T003 Ensure AppHost registers Configuration service with proper isolation and cache dependency (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, endpoints, and events (`Src/Foundation/services/Configuration/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Configure Microsoft.Identity.Web JWT validation with tenant claim check (`Src/Foundation/services/Configuration/Program.cs`)
- [ ] T006 Ensure SessionAuthenticationHandler + Redis session caching registered (`Src/Foundation/services/Configuration/Program.cs`)
- [ ] T007 Enforce tenant_id filters in DbContext and queries (`Src/Foundation/services/Configuration/Infrastructure/ConfigDbContext.cs`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Scaffold projects, database migrations, cache wiring, telemetry

- [ ] T008 Scaffold solution projects (API, Application, Domain, Infrastructure, Tests) under `Src/Foundation/services/Configuration/`
- [ ] T009 Create initial migrations for District, School, Calendar, Settings, GradingScale, CustomAttribute, NotificationTemplate (`Src/Foundation/services/Configuration/Infrastructure/Migrations/`)
- [ ] T010 [P] Configure Redis cache provider + options (`Src/Foundation/services/Configuration/Infrastructure/Cache/RedisCacheProvider.cs`)
- [ ] T011 [P] Add MassTransit + Azure Service Bus setup for events (`Src/Foundation/services/Configuration/Program.cs`)
- [ ] T012 [P] Add OpenTelemetry tracing/metrics with tenant attributes (`Src/Foundation/services/Configuration/Telemetry/TelemetryConfig.cs`)
- [ ] T013 [P] Setup tests (unit/integration/BDD) with seeded tenants (`tests/Foundation/Configuration/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Domain entities, repositories, caching primitives, hierarchy resolver

- [ ] T014 Implement domain entities + events (District, School, Calendar, Settings, GradingScale, CustomAttribute, NotificationTemplate) in `Src/Foundation/services/Configuration/Domain/`
- [ ] T015 Implement repositories with tenant-aware filters (`Src/Foundation/services/Configuration/Infrastructure/Repositories/`)
- [ ] T016 Add hierarchy resolution service (system -> district -> school) with memoization in `Src/Foundation/services/Configuration/Application/Resolution/HierarchyResolver.cs`
- [ ] T017 Add cache key/versioning helpers (`Src/Foundation/services/Configuration/Infrastructure/Cache/CacheKeys.cs`)
- [ ] T018 Implement audit interceptor writing before/after diffs (`Src/Foundation/services/Configuration/Infrastructure/Audit/AuditInterceptor.cs`)

**Checkpoint**: Core model + cache/audit infrastructure ready

---

## Phase 3: User Story 1 - District Creation & Base Settings (Priority: P1) **MVP**

**Goal**: Create districts with tenant uniqueness, persist base settings, and publish events (scenario 1)

**Independent Test**: POST /districts returns unique TenantId, emits DistrictCreatedEvent, and seeds default settings

### Implementation for User Story 1

- [ ] T019 [P] [US1] Implement POST /api/v1/districts with validation + unique tenant constraint (`Src/Foundation/services/Configuration/API/DistrictsController.cs`)
- [ ] T020 [US1] Seed system defaults + tenant base settings upon district creation (`Src/Foundation/services/Configuration/Application/Districts/DistrictProvisioningService.cs`)
- [ ] T021 [P] [US1] Publish DistrictCreatedEvent to bus with minimal payload (`Src/Foundation/services/Configuration/Application/Districts/DistrictEventPublisher.cs`)
- [ ] T022 [US1] Integration tests for district creation, uniqueness, event emission, and cache seed (`tests/Foundation/Configuration/Integration/DistrictCreationTests.cs`)

**Checkpoint**: District provisioning complete with events

---

## Phase 4: User Story 2 - School & Calendar Management (Priority: P1)

**Goal**: Manage schools and academic calendars with events (scenarios 2, 3)

**Independent Test**: School creation publishes SchoolCreatedEvent; calendar persisted and returned with correct scope (district/school)

### Implementation for User Story 2

- [ ] T023 [P] [US2] Implement POST /api/v1/schools and GET endpoints with RLS enforcement (`Src/Foundation/services/Configuration/API/SchoolsController.cs`)
- [ ] T024 [US2] Publish SchoolCreatedEvent with tenant/school metadata (`Src/Foundation/services/Configuration/Application/Schools/SchoolEventPublisher.cs`)
- [ ] T025 [US2] Implement calendar GET/PUT endpoints with validation (`Src/Foundation/services/Configuration/API/CalendarsController.cs`)
- [ ] T026 [P] [US2] Add integration tests for school + calendar flows (`tests/Foundation/Configuration/Integration/SchoolCalendarTests.cs`)

**Checkpoint**: School + calendar APIs functional

---

## Phase 5: User Story 3 - Settings Hierarchy & Caching (Priority: P1)

**Goal**: Implement configuration settings with hierarchy resolution and caching (scenarios 4, 5, 6)

**Independent Test**: GET /settings resolves school > district > system default; cache hit p95 <50ms; cache invalidated on update

### Implementation for User Story 3

- [ ] T027 [P] [US3] Implement GET /api/v1/settings with hierarchy resolver and cache usage (`Src/Foundation/services/Configuration/API/SettingsController.cs`)
- [ ] T028 [US3] Implement PUT /api/v1/settings/{key} updating appropriate scope + version bump (`Src/Foundation/services/Configuration/Application/Settings/UpdateSettingHandler.cs`)
- [ ] T029 [US3] Implement cache invalidation + optional pre-warm on ConfigurationChangedEvent (`Src/Foundation/services/Configuration/Application/Settings/SettingsCacheInvalidator.cs`)
- [ ] T030 [US3] Add unit matrix tests for hierarchy combinations + cache correctness (`tests/Foundation/Configuration/Unit/HierarchyResolutionTests.cs`)

**Checkpoint**: Settings hierarchy + cache validated

---

## Phase 6: User Story 4 - Custom Attributes & Grading Scales (Priority: P2)

**Goal**: Manage custom attributes and grading scales for downstream services (scenarios 7, 8)

**Independent Test**: Custom attributes stored per tenant/entity type; grading scales saved and retrieved; events emitted; cache invalidated

### Implementation for User Story 4

- [ ] T031 [P] [US4] Implement custom attributes endpoints (GET/POST) with validation (`Src/Foundation/services/Configuration/API/CustomAttributesController.cs`)
- [ ] T032 [US4] Implement grading scale endpoints (GET/POST) with schema validation (`Src/Foundation/services/Configuration/API/GradingScalesController.cs`)
- [ ] T033 [US4] Publish ConfigurationChangedEvent for custom attributes and grading scale changes (`Src/Foundation/services/Configuration/Application/Events/ConfigurationEventPublisher.cs`)
- [ ] T034 [US4] Add unit/integration tests for custom attributes + grading scales (`tests/Foundation/Configuration/Integration/CustomAttributesGradingTests.cs`)

**Checkpoint**: Custom attributes + grading scales functional with events

---

## Phase 7: User Story 5 - Compliance, Navigation, Templates (Priority: P2)

**Goal**: Handle state-specific compliance flags, navigation customization, and notification templates (scenarios 9, 10, 11)

**Independent Test**: Compliance flags enforce required fields; navigation config returned per role; templates stored with merge fields and versioning

### Implementation for User Story 5

- [ ] T035 [P] [US5] Implement compliance flags + validation rules (state-specific) in `Src/Foundation/services/Configuration/Application/Compliance/ComplianceRules.cs`
- [ ] T036 [US5] Implement role-based navigation config endpoints (`Src/Foundation/services/Configuration/API/NavigationController.cs`)
- [ ] T037 [US5] Implement notification template endpoints with versioning (`Src/Foundation/services/Configuration/API/TemplatesController.cs`)
- [ ] T038 [US5] Add tests for compliance enforcement, navigation, and templates (`tests/Foundation/Configuration/Integration/ComplianceNavigationTemplateTests.cs`)

**Checkpoint**: Compliance/navigation/templates delivered

---

## Phase 8: User Story 6 - Audit Trail & Reporting (Priority: P3)

**Goal**: Provide comprehensive audit of configuration changes (scenario 12)

**Independent Test**: Audit records capture user, timestamp, key, before/after; reported via endpoint/export

### Implementation for User Story 6

- [ ] T039 [P] [US6] Persist audit records via interceptor for all write operations (`Src/Foundation/services/Configuration/Infrastructure/Audit/AuditInterceptor.cs`)
- [ ] T040 [US6] Implement audit query/export endpoint with filtering (`Src/Foundation/services/Configuration/API/AuditController.cs`)
- [ ] T041 [US6] Add tests verifying audit captured for settings/attributes/templates (`tests/Foundation/Configuration/Integration/AuditTests.cs`)

**Checkpoint**: Audit trail completed

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T042 [P] Add cache stampede protection (mutex + jitter TTL) (`Src/Foundation/services/Configuration/Infrastructure/Cache/CacheThrottling.cs`)
- [ ] T043 Harden resilience/policies for cache/services (timeouts/retries) (`Src/Foundation/services/Configuration/Infrastructure/Resilience/ResiliencePolicies.cs`)
- [ ] T044 [P] Documentation updates: hierarchy, cache keys, event catalog (`Src/Foundation/services/Configuration/docs/hierarchy.md`)
- [ ] T045 Final audit that all spec scenarios + acceptance criteria covered (`Plan/Foundation/specs/007-configuration-service/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1/US2/US3 (P1)  US4/US5 (P2)  US6 (P3)  Polish
- US1 depends on domain + cache scaffolding
- US2 depends on entities/migrations
- US3 depends on hierarchy resolver + cache and config events
- US4 relies on event publisher + cache invalidation
- US5 depends on custom attributes/templates infrastructure
- US6 depends on audit interceptor + endpoints

## Parallel Execution Examples

- T008-T013 in parallel (projects, migrations, cache, bus, telemetry, tests)
- US1 tasks T019-T021 parallelizable before tests
- US3 tasks T027-T029 parallelizable; T030 follows
- US4 tasks T031-T033 parallel; tests follow

## Implementation Strategy

- MVP = Phases 1-3 delivering district provisioning + settings hierarchy with cache + events
- Next add custom attributes + grading + compliance/navigation/templates, then audit/export
- Keep cache hit ratios monitored; invalidation tied to ConfigurationChangedEvent to avoid staleness
