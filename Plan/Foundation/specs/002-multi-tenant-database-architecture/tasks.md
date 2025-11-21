---
description: "Task list for Multi-Tenant Database Architecture (tenant isolation + RLS)"
---

# Tasks: Multi-Tenant Database Architecture

**Specification Branch**: `Foundation/002-multi-tenant-database-architecture-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/002-multi-tenant-database-architecture` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/002-multi-tenant-database-architecture/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ (placeholder)

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/shared/Infrastructure/MultiTenancy/`  
**Specification Path**: `Plan/Foundation/specs/002-multi-tenant-database-architecture/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/shared/Infrastructure/MultiTenancy/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/002-multi-tenant-database-architecture/`)
- [ ] Shared infrastructure dependencies match between spec and plan (EF Core, PostgreSQL RLS, OpenTelemetry)
- [ ] Cross-layer dependencies (Identity tenant claim, Gateway validation) justified in both spec and plan

---

## Layer Compliance Validation

*MANDATORY: Mono-repo layer isolation checks*

- [ ] T001 Verify project references stay within Foundation shared infrastructure (`Src/Foundation/shared/*`) and avoid higher layers
- [ ] T002 Verify NO direct service-to-service references; only shared multi-tenancy abstractions consumed via DI
- [ ] T003 Verify AppHost wiring keeps middleware/interceptors in Foundation layer (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer position and dependencies on Identity/Gateway tenant claim (`Src/Foundation/shared/Infrastructure/MultiTenancy/README.md`)

---

## Identity & Authentication Compliance

*Tenant context depends on Entra-issued JWT with tenant_id claim*

- [ ] T005 Confirm Microsoft.Identity.Web validates JWT and exposes `tenant_id` claim (Gateway/Host config in `Src/Foundation/AppHost/Program.cs`)
- [ ] T006 Verify SessionAuthenticationHandler & Redis session caching configured for tenant-aware sessions (`Src/Foundation/AppHost/Program.cs`)
- [ ] T007 Ensure identity.sessions table includes tenant_id for multi-tenancy audit (`Src/Foundation/shared/Infrastructure/Data/Migrations`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish multi-tenant infrastructure workspace and documentation baseline

- [ ] T008 Create multi-tenancy solution folder with README linking plan/spec (`Src/Foundation/shared/Infrastructure/MultiTenancy/README.md`)
- [ ] T009 Add solution + project references for multi-tenancy helpers (middleware, interceptors) (`Src/Foundation/shared/Infrastructure/MultiTenancy/MultiTenancy.csproj`)
- [ ] T010 [P] Bootstrap tests project for multi-tenancy integration (`tests/Foundation/MultiTenancy/Integration/MultiTenancy.Tests.csproj`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core abstractions and plumbing required before user stories

- [ ] T011 Define tenant contracts (`ITenantContext`, `ITenantEntity`, `IAuditable`, `ISoftDelete`) in `Src/Foundation/shared/Domain/Common/Tenancy.cs`
- [ ] T012 Implement ambient tenant context storage + accessor (`TenantContext`, `ITenantContextAccessor`) in `Src/Foundation/shared/Infrastructure/MultiTenancy/TenantContext.cs`
- [ ] T013 Add base `MultiTenantDbContext` with tenant shadow property and OnModelCreating hook in `Src/Foundation/shared/Infrastructure/MultiTenancy/MultiTenantDbContext.cs`
- [ ] T014 Add EF Core model builder extension to apply tenant filters to all `ITenantEntity` types in `Src/Foundation/shared/Infrastructure/MultiTenancy/ModelBuilderExtensions.cs`
- [ ] T015 Add DI registration extension for multi-tenancy services (`Src/Foundation/shared/Infrastructure/MultiTenancy/DependencyInjection.cs`)

**Checkpoint**: Foundation ready - tenant abstractions and DI wiring exist

---

## Phase 3: User Story 1 - Tenant Context Propagation (Priority: P1) **MVP**

**Goal**: Propagate tenant_id from JWT through middleware to EF Core so every request sets `app.current_tenant`

**Independent Test**: Request without tenant_id returns 401; request with tenant_id sets session variable and applies filter automatically

### Implementation for User Story 1

- [ ] T016 [P] [US1] Implement `TenantContextMiddleware` to read `tenant_id` claim and set ambient context in `Src/Foundation/shared/Infrastructure/MultiTenancy/TenantContextMiddleware.cs`
- [ ] T017 [US1] Register middleware and accessor in host startup (`Src/Foundation/AppHost/Program.cs`)
- [ ] T018 [P] [US1] Implement `TenantConnectionInterceptor` setting `SET app.current_tenant` per connection in `Src/Foundation/shared/Infrastructure/MultiTenancy/TenantConnectionInterceptor.cs`
- [ ] T019 [P] [US1] Add OpenTelemetry enrichment for `tenant.id` on spans/metrics in `Src/Foundation/shared/Infrastructure/Telemetry/TenantInstrumentation.cs`
- [ ] T020 [US1] Add integration tests for middleware/interceptor enforcing tenant claim and session variable (`tests/Foundation/MultiTenancy/Integration/TenantContextMiddlewareTests.cs`)

**Checkpoint**: Tenant context propagation operational with telemetry

---

## Phase 4: User Story 2 - RLS Policies & Schema Enforcement (Priority: P1)

**Goal**: Enable PostgreSQL RLS with tenant filters and indexes on all tenant entities

**Independent Test**: Cross-tenant query attempts return zero rows; insert/update/delete blocked when tenant_id mismatch; indexes used for tenant-scoped queries

### Implementation for User Story 2

- [ ] T021 [P] [US2] Create migration to add/validate TenantId columns per data model (student, enrollment, audit) in `Src/Foundation/shared/Infrastructure/Data/Migrations/` 
- [ ] T022 [P] [US2] Create migration enabling RLS and policies using template in `Src/Foundation/shared/Infrastructure/Data/Migrations/EnableRlsPolicies.cs`
- [ ] T023 [P] [US2] Add composite indexes `(tenant_id, last_name)`, `(tenant_id, created_at)`, `(tenant_id, student_id)` per data-model.md in `Src/Foundation/shared/Infrastructure/Data/Migrations/AddTenantIndexes.cs`
- [ ] T024 [US2] Add SQL script to verify RLS policies remain after backup/restore (`Src/Foundation/tools/rls/verify_rls.sql`)
- [ ] T025 [US2] Implement automated RLS validation test harness against seeded tenants (`tests/Foundation/MultiTenancy/Integration/RlsPolicyTests.cs`)
- [ ] T026 [US2] Document RLS policy template and exceptions (configuration.GradeLevels) in `Src/Foundation/shared/Infrastructure/MultiTenancy/docs/rls-policy.md`

**Checkpoint**: RLS enforced with indexes and validation in place

---

## Phase 5: User Story 3 - Migration & Mapping Support (Priority: P2)

**Goal**: Provide migration scaffolding to consolidate legacy per-district data with tenant tagging and resumability

**Independent Test**: Sample migration job maps legacy IDs to UUIDs, writes tenant_id, and can resume after interruption without duplicates

### Implementation for User Story 3

- [ ] T027 [P] [US3] Create `migration.LegacyIdMapping` and `migration.BatchState` tables with RLS in `Src/Foundation/shared/Infrastructure/Data/Migrations/CreateMigrationSupport.cs`
- [ ] T028 [US3] Implement mapping repository + services (`ILegacyIdMapper`, `IBatchCheckpointStore`) in `Src/Foundation/shared/Infrastructure/MultiTenancy/Migration/`
- [ ] T029 [P] [US3] Build sample migration harness for students using COPY ingest and checkpointing in `Src/Foundation/tools/migration/StudentMigrationHarness.cs`
- [ ] T030 [US3] Add validation scripts for count parity and orphan detection per tenant in `Src/Foundation/tools/migration/validation.sql`
- [ ] T031 [P] [US3] Add integration test to resume migration after simulated failure using batch state (`tests/Foundation/MultiTenancy/Integration/MigrationResumeTests.cs`)
- [ ] T032 [US3] Document migration sequencing + rollback procedure in `Src/Foundation/shared/Infrastructure/MultiTenancy/docs/migration-runbook.md`

**Checkpoint**: Migration support reusable across services with validation

---

## Phase 6: User Story 4 - Operational Safeguards & Performance (Priority: P3)

**Goal**: Validate performance, observability, and backup/restore behavior for multi-tenant operations

**Independent Test**: Synthetic dataset (500k rows, 100 tenants) meets P95 <100ms queries; backup/restore retains RLS; audit entries remain tenant-isolated

### Implementation for User Story 4

- [ ] T033 [P] [US4] Implement load test harness for tenant-filtered queries with synthetic data in `tests/Foundation/MultiTenancy/Load/TenantQueryBenchmarks.cs`
- [ ] T034 [US4] Create audit interceptor capturing tenant/user/action to `audit.AuditRecords` table in `Src/Foundation/shared/Infrastructure/MultiTenancy/Audit/TenantAuditInterceptor.cs`
- [ ] T035 [P] [US4] Add Grafana/Prometheus dashboard config for tenant metrics and RLS validation alerts in `observability/dashboards/multi-tenancy.json`
- [ ] T036 [US4] Add backup/restore validation script to re-check RLS policies post-restore in `Src/Foundation/tools/rls/restore_validation.sh`
- [ ] T037 [US4] Document new tenant onboarding runbook (UUID generation, configuration registration) in `Src/Foundation/shared/Infrastructure/MultiTenancy/docs/onboarding.md`

**Checkpoint**: Operational runbooks + performance + observability in place

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T038 [P] Update developer docs and quickstart for multi-tenancy validation (`docs/multi-tenancy/quickstart.md`)
- [ ] T039 Harden error handling + logging around tenant context loss (`Src/Foundation/shared/Infrastructure/MultiTenancy/TenantContextMiddleware.cs`)
- [ ] T040 [P] Add code analyzers/guards preventing queries without tenant filter (`Src/Foundation/shared/Infrastructure/MultiTenancy/Analyzers/TenantGuard.cs`)
- [ ] T041 Final review: reconcile implemented tasks with spec scenarios + acceptance criteria (`Plan/Foundation/specs/002-multi-tenant-database-architecture/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  User Stories (Phases 3-6)  Polish
- User Story 1 (P1) must complete before RLS policy validation and migration harnesses
- User Story 2 (P1) depends on tenant context plumbing and feeds migration + operations
- User Story 3 (P2) depends on RLS-enabled schema
- User Story 4 (P3) depends on prior stories to benchmark and validate

## Parallel Execution Examples

- Run T016/T018/T019 in parallel (middleware, interceptor, telemetry) once DI scaffolding exists
- Run migrations T021-T023 in parallel branches, merge after validation tests added
- Run T029 migration harness build in parallel with T030 validation script authoring

## Implementation Strategy

- MVP: Complete Phases 1-3 to deliver tenant context propagation with RLS-enabled schema and validation
- Incremental: Add migration/mapping support (Phase 5), then operational safeguards/performance (Phase 6)
- Testing-first where possible for middleware/RLS to lock isolation guarantees early
