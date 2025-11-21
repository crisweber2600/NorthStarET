# Tasks: Configuration Service (Multi-Tenant Settings)

**Specification Branch**: `CrossCuttingConcerns/003-configuration-service-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `CrossCuttingConcerns/003-configuration-service` *(created by `/speckit.implement`)*  
**Feature**: 003-configuration-service  
**Input**: plan.md, spec.md, data-model.md, research.md  

---

## Layer Context (MANDATORY)

**Target Layer**: CrossCuttingConcerns  
**Implementation Path**: `Src/Foundation/services/Configuration/`  
**Specification Path**: `Plan/CrossCuttingConcerns/specs/003-configuration-service/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/{TargetLayer}/...`)
- [ ] Specification path follows layer structure (`Plan/{TargetLayer}/specs/...`)
- [ ] Shared infrastructure dependencies match between spec and plan
- [ ] Cross-layer dependencies (if any) justified in both spec and plan

---

## Layer Compliance Validation

- [ ] T001 Verify Configuration service projects reference only Foundation/shared infrastructure (no upstream layers) in `Src/Foundation/services/Configuration/*.csproj`
- [ ] T002 Confirm AppHost registration wires Postgres, Redis, and messaging resources for Configuration service in `Src/Foundation/AppHost/Program.cs`
- [ ] T003 Document layer boundaries and shared dependencies in `Src/Foundation/services/Configuration/README.md`
- [ ] T004 Check for circular dependencies between Configuration and other services via solution graph review

---

## Format

Tasks use `- [ ] T### [P?] [Story] Description with file path`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create project scaffolding and host wiring.

- [ ] T005 Run `.specify/scripts/bash/check-prerequisites.sh --json` and capture FEATURE_DIR/LAYER for this spec
- [ ] T006 Scaffold Configuration service projects (API, Application, Domain, Infrastructure) in `Src/Foundation/services/Configuration/`
- [ ] T007 [P] Register Configuration service with Postgres, Redis, MassTransit in Aspire AppHost in `Src/Foundation/AppHost/Program.cs`
- [ ] T008 [P] Add launchSettings and CI configuration for Configuration.API in `Src/Foundation/services/Configuration/Properties/launchSettings.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish domain, data access, messaging, and multi-tenant guards.

- [ ] T009 [P] Implement domain entities (District, School, ConfigurationEntry, AcademicCalendar, GradeLevel, Subject) in `Configuration.Domain/Entities`
- [ ] T010 Configure EF Core `ConfigurationDbContext` with RLS/global tenant filters and migrations in `Configuration.Infrastructure/Data`
- [ ] T011 [P] Implement hierarchy resolution services (System -> District -> School) in `Configuration.Application/Services/ConfigurationResolver.cs`
- [ ] T012 [P] Add Redis caching for configuration lookups with per-tenant keys in `Configuration.Infrastructure/Caching/ConfigurationCacheService.cs`
- [ ] T013 Configure MassTransit event publishing/subscription for `DistrictCreated`, `SchoolCreated`, `ConfigurationChanged` in `Configuration.Infrastructure/Messaging`
- [ ] T014 Add validation, exception handling, and logging middleware in `Configuration.API/Program.cs`
- [ ] T015 Seed protected keys/non-overridable settings initial data in `Configuration.Infrastructure/Data/Migrations/*`

---

## Phase 3: User Story 1 - District Provisioning (Priority: P1) ✔ MVP

**Goal**: Create districts with tenant_id, emit events, and bootstrap admin provisioning.  
**Independent Test**: POST /districts creates district, publishes `DistrictCreatedEvent`, and appears in list.

### Tests (write-first)
- [ ] T016 [P] [US1] Integration test for `POST /districts` creating tenant-scoped record and emitting event in `Src/Foundation/services/Configuration/tests/Configuration.IntegrationTests/Districts/CreateDistrictTests.cs`
- [ ] T017 [P] [US1] Contract test validating payload/response schema for district creation in `tests/Configuration.ContractTests/Districts/CreateDistrictContractTests.cs`

### Implementation
- [ ] T018 [US1] Implement `POST /districts` endpoint with validation and event publishing in `Configuration.API/Controllers/DistrictsController.cs`
- [ ] T019 [US1] Persist District entity with tenant isolation in `Configuration.Infrastructure/Data/Repositories/DistrictRepository.cs`
- [ ] T020 [US1] Publish `DistrictCreatedEvent` to MassTransit and document payload in `Configuration.Infrastructure/Messaging/Events/DistrictCreatedEvent.cs`

---

## Phase 4: User Story 2 - Academic Calendar Management (Priority: P1)

**Goal**: Manage academic calendars per district with inheritance to schools.  
**Independent Test**: Calendar stored with tenant isolation; downstream services can consume; retrieval <50ms P95 via cache.

### Tests (write-first)
- [ ] T021 [P] [US2] Integration test for `POST /calendars` and `GET /calendars` honoring tenant context in `tests/Configuration.IntegrationTests/Calendars/CalendarTests.cs`
- [ ] T022 [P] [US2] Cache test verifying calendar reads served from Redis after first fetch in `tests/Configuration.UnitTests/Caching/CalendarCacheTests.cs`

### Implementation
- [ ] T023 [US2] Implement calendar endpoints and inheritance logic in `Configuration.API/Controllers/CalendarsController.cs`
- [ ] T024 [US2] Store calendar events JSON with tenant filters in `Configuration.Infrastructure/Data/Repositories/CalendarRepository.cs`
- [ ] T025 [US2] Publish `CalendarUpdatedEvent` for downstream consumers in `Configuration.Infrastructure/Messaging/Events/CalendarUpdatedEvent.cs`

---

## Phase 5: User Story 3 - School Provisioning (Priority: P1)

**Goal**: Create schools within districts and emit events for provisioning.  
**Independent Test**: School created with district tenant_id; `SchoolCreatedEvent` published; school appears in directory.

### Tests (write-first)
- [ ] T026 [P] [US3] Integration test for `POST /schools` creating schools with tenant validation in `tests/Configuration.IntegrationTests/Schools/CreateSchoolTests.cs`
- [ ] T027 [P] [US3] Contract test verifying school payload schema and event emission in `tests/Configuration.ContractTests/Schools/CreateSchoolContractTests.cs`

### Implementation
- [ ] T028 [US3] Implement school creation endpoint with validation in `Configuration.API/Controllers/SchoolsController.cs`
- [ ] T029 [US3] Persist School entity and publish `SchoolCreatedEvent` in `Configuration.Infrastructure/Data/Repositories/SchoolRepository.cs`
- [ ] T030 [US3] Cache school directory responses per tenant in `Configuration.Infrastructure/Caching/ConfigurationCacheService.cs`

---

## Phase 6: User Story 4 - Grade Levels and Subjects (Priority: P1)

**Goal**: Configure grade levels and subjects per school/district.  
**Independent Test**: Admin can set grade levels/subjects; data retrieved per tenant and consumed by downstream services.

### Tests (write-first)
- [ ] T031 [P] [US4] Integration test for configuring grade levels/subjects endpoints in `tests/Configuration.IntegrationTests/Metadata/GradeSubjectTests.cs`
- [ ] T032 [P] [US4] Unit test for metadata inheritance (district defaults to schools) in `tests/Configuration.UnitTests/Metadata/MetadataInheritanceTests.cs`

### Implementation
- [ ] T033 [US4] Implement grade/subject endpoints and validation in `Configuration.API/Controllers/MetadataController.cs`
- [ ] T034 [US4] Persist metadata with scope (System/District/School) in `Configuration.Infrastructure/Data/Repositories/MetadataRepository.cs`
- [ ] T035 [US4] Expose read endpoints with caching and query filters in `Configuration.API/Controllers/MetadataController.cs`

---

## Phase 7: User Story 5 - Isolation & Hierarchical Overrides (Priority: P1)

**Goal**: Enforce tenant isolation and override resolution rules.  
**Independent Test**: Tenant A cannot see Tenant B settings; override order System < District < School respected; protected keys blocked.

### Tests (write-first)
- [ ] T036 [P] [US5] RLS/integration test ensuring tenant isolation on configuration queries in `tests/Configuration.IntegrationTests/Security/TenantIsolationTests.cs`
- [ ] T037 [P] [US5] Unit test for override resolution rules in `tests/Configuration.UnitTests/Resolution/OverrideResolutionTests.cs`

### Implementation
- [ ] T038 [US5] Apply RLS/global filters and tenant checks in `Configuration.Infrastructure/Data/Configurations/*`
- [ ] T039 [US5] Implement override resolution service enforcing protected keys in `Configuration.Application/Services/ConfigurationResolver.cs`
- [ ] T040 [US5] Add middleware rejecting missing tenant context in `Configuration.API/Program.cs`

---

## Phase 8: User Story 6 - Custom Attributes (Priority: P2)

**Goal**: Allow tenant-scoped custom attributes without cross-tenant leakage.  
**Independent Test**: Admin creates custom attributes; values stored per tenant; downstream can extend records.

### Tests (write-first)
- [ ] T041 [P] [US6] Integration test for creating/listing custom attributes with tenant isolation in `tests/Configuration.IntegrationTests/Metadata/CustomAttributesTests.cs`
- [ ] T042 [P] [US6] Unit test for schema validation of custom attribute definitions in `tests/Configuration.UnitTests/Metadata/CustomAttributeValidationTests.cs`

### Implementation
- [ ] T043 [US6] Implement custom attributes endpoints and persistence in `Configuration.API/Controllers/CustomAttributesController.cs`
- [ ] T044 [US6] Add validation rules and JSON schema storage for custom attributes in `Configuration.Application/Validators/CustomAttributeValidator.cs`
- [ ] T045 [US6] Cache custom attributes per tenant in `Configuration.Infrastructure/Caching/ConfigurationCacheService.cs`

---

## Phase 9: User Story 7 - State Compliance Settings (Priority: P2)

**Goal**: Enable state-specific compliance flags and fields.  
**Independent Test**: State-specific settings appear per state; hidden for other states; data isolated per tenant.

### Tests (write-first)
- [ ] T046 [P] [US7] Integration test for state-specific configuration retrieval (CA/TX) in `tests/Configuration.IntegrationTests/Compliance/StateSettingsTests.cs`
- [ ] T047 [P] [US7] Unit test for allowed/protected compliance keys in `tests/Configuration.UnitTests/Compliance/ComplianceKeyRulesTests.cs`

### Implementation
- [ ] T048 [US7] Implement compliance configuration endpoints and validation in `Configuration.API/Controllers/ComplianceController.cs`
- [ ] T049 [US7] Seed state-specific defaults and protection rules in `Configuration.Infrastructure/Data/Migrations/*`
- [ ] T050 [US7] Document compliance key usage and restrictions in `Src/Foundation/services/Configuration/README.md`

---

## Phase 10: User Story 8 - Navigation Customization (Priority: P2)

**Goal**: Provide tenant/role-based navigation menus.  
**Independent Test**: Teacher/admin menus return correct items per tenant; UI renders dynamically from config.

### Tests (write-first)
- [ ] T051 [P] [US8] Integration test for navigation menu retrieval per role/tenant in `tests/Configuration.IntegrationTests/Menus/NavigationMenuTests.cs`
- [ ] T052 [P] [US8] Contract test for menu payload format and role filters in `tests/Configuration.ContractTests/Menus/NavigationMenuContractTests.cs`

### Implementation
- [ ] T053 [US8] Implement navigation menu endpoints with role-based filtering in `Configuration.API/Controllers/MenusController.cs`
- [ ] T054 [US8] Store menu definitions with tenant/role scope in `Configuration.Infrastructure/Data/Repositories/ConfigurationRepository.cs`
- [ ] T055 [US8] Cache menus per role/tenant for fast retrieval in `Configuration.Infrastructure/Caching/ConfigurationCacheService.cs`

---

## Phase 11: User Story 9 - Notification & Email Templates (Priority: P2)

**Goal**: Manage tenant-specific notification/email templates with branding.  
**Independent Test**: Templates stored with tenant isolation; retrieved with merge fields; emails use tenant SMTP settings.

### Tests (write-first)
- [ ] T056 [P] [US9] Integration test for template CRUD and retrieval with tenant context in `tests/Configuration.IntegrationTests/Templates/TemplateTests.cs`
- [ ] T057 [P] [US9] Unit test for template merge field validation in `tests/Configuration.UnitTests/Templates/TemplateValidationTests.cs`

### Implementation
- [ ] T058 [US9] Implement template endpoints and validation in `Configuration.API/Controllers/TemplatesController.cs`
- [ ] T059 [US9] Persist templates with tenant-specific SMTP/branding metadata in `Configuration.Infrastructure/Data/Repositories/TemplateRepository.cs`
- [ ] T060 [US9] Document template usage and merge field conventions in `Src/Foundation/services/Configuration/README.md`

---

## Phase 12: User Story 10 - Audit Trail (Priority: P1)

**Goal**: Immutable audit logging for configuration changes with history and revert support.  
**Independent Test**: Change writes audit record (user, timestamp, old/new); history available; revert allowed where permitted.

### Tests (write-first)
- [ ] T061 [P] [US10] Integration test for audit record creation on config updates in `tests/Configuration.IntegrationTests/Audit/AuditTrailTests.cs`
- [ ] T062 [P] [US10] Unit test for revert logic respecting protected keys in `tests/Configuration.UnitTests/Audit/AuditRevertTests.cs`

### Implementation
- [ ] T063 [US10] Implement audit logging decorator for write operations in `Configuration.Application/Behaviors/AuditLoggingBehavior.cs`
- [ ] T064 [US10] Persist audit records with immutability constraints in `Configuration.Infrastructure/Data/Repositories/AuditRepository.cs`
- [ ] T065 [US10] Add audit history/revert endpoint in `Configuration.API/Controllers/AuditController.cs`

---

## Phase 13: Polish & Cross-Cutting Concerns

- [ ] T066 [P] Optimize Redis/Postgres indexes to meet <50ms P95 read target in `Configuration.Infrastructure/Data/Configurations/*`
- [ ] T067 Harden security (authorization policies, input validation, CORS) in `Configuration.API/Program.cs`
- [ ] T068 [P] Final regression suite across integration/contract/unit tests in `Src/Foundation/services/Configuration/tests/`
- [ ] T069 Update operational runbook and diagrams in `Plan/CrossCuttingConcerns/specs/003-configuration-service/plan.md`

---

## Dependencies & Execution Order

- Setup → Foundational → User Stories (US1, US2, US3, US4, US10 are P1; US6-US9 are P2) → Polish
- US1 (districts) precedes schools and calendars; US4 overrides depend on domain/caching; compliance/custom attributes/menus/templates follow core config APIs.

## Parallel Execution Examples

- After Phase 2, US1 district provisioning and US2 calendars can proceed in parallel (different controllers/repos).  
- Tests marked [P] can run concurrently; caching and messaging tasks are independent of UI payload tasks.

## Implementation Strategy

- MVP: Phases 1-7 (through tenant isolation/override) deliver core configuration platform.  
- Next: Add custom attributes, compliance, menus, templates, and finalize audit polish.

---
