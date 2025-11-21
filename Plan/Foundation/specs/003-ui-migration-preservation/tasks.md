---
description: "Task list for UI Migration with Preservation Strategy (AngularJS to Angular 18 parity)"
---

# Tasks: UI Migration with Preservation Strategy

**Specification Branch**: `Foundation/003-ui-migration-preservation-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/003-ui-migration-preservation` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/003-ui-migration-preservation/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/ (snapshot tests)

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/frontend/angular/`  
**Specification Path**: `Plan/Foundation/specs/003-ui-migration-preservation/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/frontend/angular/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/003-ui-migration-preservation/`)
- [ ] Shared dependencies (API Gateway/YARP, Playwright, axe) align between plan and spec
- [ ] Coexistence strategy (legacy bridge) documented consistently

---

## Layer Compliance Validation

*MANDATORY: Mono-repo layer isolation checks*

- [ ] T001 Verify frontend package references stay within Foundation layer (`Src/Foundation/frontend/angular/`)
- [ ] T002 Verify no cross-layer leaks (UI consumes APIs via Gateway only)
- [ ] T003 Verify AppHost includes UI asset deployment wiring within Foundation boundary
- [ ] T004 Update UI README with layer placement and dependency map (`Src/Foundation/frontend/angular/README.md`)

---

## Identity & Authentication Compliance

*UI must honor existing Entra/BFF session model*

- [ ] T005 Verify BFF/session handler wiring uses Microsoft.Identity.Web tokens (no custom JWT) in `Src/Foundation/frontend/angular/src/app/auth/auth.config.ts`
- [ ] T006 Ensure session token exchange + Redis session cache integration documented in `Src/Foundation/frontend/angular/docs/auth-flow.md`
- [ ] T007 Confirm tenant_id propagated in API calls and not stored in client state (`Src/Foundation/frontend/angular/src/app/core/http/tenant-http-interceptor.ts`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Initialize Angular 18 workspace, testing, and parity baselines

- [ ] T008 Scaffold Angular 18 workspace + root app shell in `Src/Foundation/frontend/angular/` (Vite/Angular builder)
- [ ] T009 Configure ESLint/Prettier + path aliases for migrated modules (`Src/Foundation/frontend/angular/.eslintrc.cjs`)
- [ ] T010 [P] Add Playwright + visual regression setup with baseline folder (`tests/ui/baseline/` and `tests/ui/playwright.config.ts`)
- [ ] T011 [P] Add axe accessibility helper and keyboard shortcut test utilities (`tests/ui/utils/accessibility.ts`)
- [ ] T012 [P] Capture initial legacy screenshots for key pages (dashboard, student list, assessment form) in `tests/ui/baseline/legacy/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Coexistence shell, routing, data services, and contract baselines

- [ ] T013 Implement hybrid shell + router adapter for legacy + Angular 18 in `Src/Foundation/frontend/angular/src/app/shell/legacy-bridge.component.ts`
- [ ] T014 Add feature flag service to toggle legacy vs migrated components (`Src/Foundation/frontend/angular/src/app/core/feature-flags.ts`)
- [ ] T015 Implement shared layout frame + navigation wrapper (`Src/Foundation/frontend/angular/src/app/layout/app-layout.component.ts`)
- [ ] T016 Add legacy session propagation (postMessage/query token) adapter in `Src/Foundation/frontend/angular/src/app/legacy/session-adapter.ts`
- [ ] T017 Scaffold data services mirroring legacy endpoints (students/assessments) using same paths via Gateway (`Src/Foundation/frontend/angular/src/app/data/student.service.ts`)
- [ ] T018 [P] Add contract snapshot tests for GET students/student by id/assessment by id in `tests/ui/contracts/contract-snapshots.spec.ts`

**Checkpoint**: Shell + data services aligned; coexistence ready

---

## Phase 3: User Story 1 - Student Dashboard & List Parity (Priority: P1) **MVP**

**Goal**: Migrate student dashboard and list with pixel + workflow parity (scenarios 1, 2, 10, 8, 9)

**Independent Test**: Playwright comparison under desktop/tablet/mobile shows <1% diff; filtering/sorting/pagination match legacy behavior

### Implementation for User Story 1

- [ ] T019 [P] [US1] Scaffold Angular components for dashboard + student list with legacy markup/classes (`Src/Foundation/frontend/angular/src/app/students/dashboard/`)
- [ ] T020 [P] [US1] Implement list filters/sorting/pagination matching legacy algorithms (`Src/Foundation/frontend/angular/src/app/students/list/student-list.component.ts`)
- [ ] T021 [P] [US1] Wire data services to legacy endpoints via Gateway (no contract drift) in `Src/Foundation/frontend/angular/src/app/students/data/student.api.ts`
- [ ] T022 [US1] Add Playwright visual regression tests for dashboard/list at 1920/768/375 breakpoints (`tests/ui/specs/student-dashboard.spec.ts`)
- [ ] T023 [US1] Add accessibility + keyboard shortcut parity tests (tab order, shortcuts) in `tests/ui/specs/student-dashboard.a11y.spec.ts`
- [ ] T024 [US1] Add responsive layout guard rails + CSS isolation to avoid legacy cascade conflicts (`Src/Foundation/frontend/angular/src/styles/responsive.scss`)

**Checkpoint**: Student dashboard + list parity validated across devices

---

## Phase 4: User Story 2 - Assessment Entry Form Migration (Priority: P2)

**Goal**: Migrate assessment entry form with validation and workflow parity (scenario 3)

**Independent Test**: Form validation messages, tab order, and submit flow identical; visual diff <1%

### Implementation for User Story 2

- [ ] T025 [P] [US2] Scaffold assessment entry form component mirroring field order + ARIA labels (`Src/Foundation/frontend/angular/src/app/assessments/assessment-form.component.ts`)
- [ ] T026 [US2] Implement validation rules + error messaging identical to legacy (`Src/Foundation/frontend/angular/src/app/assessments/validation.ts`)
- [ ] T027 [US2] Add Playwright functional + visual tests for assessment form (`tests/ui/specs/assessment-form.spec.ts`)
- [ ] T028 [US2] Add API contract snapshot for assessment post payload to ensure no schema drift (`tests/ui/contracts/assessment-contract.spec.ts`)

**Checkpoint**: Assessment entry migrated with validation + visual parity

---

## Phase 5: User Story 3 - Preferences, Navigation, and Responsive Behavior (Priority: P2)

**Goal**: Preserve user preferences, navigation structure, and responsive behavior (scenarios 7, 8, 9)

**Independent Test**: Preferences imported on first load; menu structure and breakpoints identical; navigation/active states preserved

### Implementation for User Story 3

- [ ] T029 [P] [US3] Implement preference ingestion from legacy local storage keys to centralized preference API (`Src/Foundation/frontend/angular/src/app/preferences/preferences.service.ts`)
- [ ] T030 [US3] Migrate navigation menu component with identical structure/order/icons (`Src/Foundation/frontend/angular/src/app/navigation/menu.component.ts`)
- [ ] T031 [P] [US3] Add responsive breakpoint styles matching legacy breakpoints (1920/768/375) in `Src/Foundation/frontend/angular/src/styles/breakpoints.scss`
- [ ] T032 [US3] Add Playwright navigation + responsive tests verifying preserved menu behavior (`tests/ui/specs/navigation-responsive.spec.ts`)

**Checkpoint**: Preferences + navigation parity validated

---

## Phase 6: User Story 4 - Quality Gates: Visual, Accessibility, Print/Export (Priority: P3)

**Goal**: Enforce visual regression, accessibility, and print/export parity across migrated components (scenarios 5, 11, 12)

**Independent Test**: Visual diffs <1%; axe scan shows no new violations; print/export outputs match legacy templates

### Implementation for User Story 4

- [ ] T033 [P] [US4] Configure CI visual regression gate with baseline approval workflow (`.github/workflows/ui-visual-regression.yml`)
- [ ] T034 [US4] Add axe accessibility scans to CI for migrated routes (`tests/ui/specs/accessibility-gate.spec.ts`)
- [ ] T035 [P] [US4] Implement print/export parity styles and scripts for roster/report pages (`Src/Foundation/frontend/angular/src/app/print/print-styles.scss`)
- [ ] T036 [US4] Add Playwright print preview and export snapshot tests (`tests/ui/specs/print-export.spec.ts`)

**Checkpoint**: Quality gates block regressions before rollout

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T037 [P] Clean up legacy bridge instrumentation and remove unused scripts (`Src/Foundation/frontend/angular/src/app/legacy/`)
- [ ] T038 Update documentation: migration runbook, coexistence cutoff date, troubleshooting (`Src/Foundation/frontend/angular/docs/migration-runbook.md`)
- [ ] T039 [P] Add bundle size monitoring + threshold alerts for migrated modules (`Src/Foundation/frontend/angular/tools/bundle-report.config.cjs`)
- [ ] T040 Final parity audit against spec scenarios; log deltas and approvals (`Plan/Foundation/specs/003-ui-migration-preservation/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1 (P1)  US2/US3 (P2)  US4 (P3)  Polish
- US1 depends on shell + data services; gates overall MVP
- US2 depends on shared services + feature flags; can run parallel with US3 after US1 stabilization
- US4 depends on migrated components to set baselines

## Parallel Execution Examples

- T008-T011 can run in parallel (workspace + tooling setup)
- T019-T021 parallelize component scaffolding while T022-T023 build tests
- T029/T031 can run parallel to T030; T033/T035 can run parallel with CI config

## Implementation Strategy

- MVP = Phases 1-3 to deliver dashboard/list parity with coexistence and visual tests
- Incrementally add assessment form (US2) and preferences/navigation (US3), then harden with quality gates (US4)
- Keep baselines under version control; require approvals for updates to avoid visual drift
