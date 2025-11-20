---
description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: TDD is mandatory. Include tasks for unit tests, Reqnroll Features + step definitions (committed before implementation), Aspire integration tests, and Playwright journeys. Execute every suite Red → Green before writing production code, capturing failing and passing transcripts for `dotnet test` (unit + Reqnroll + Aspire) and `pwsh tests/ui/playwright.ps1` (Playwright).

**Tool-Assisted Implementation** (for AI agents):

- Begin each session with structured thinking: `#think` or `#mcp_sequentialthi_sequentialthinking` to plan approach
- Query official documentation for .NET/Azure decisions: `#microsoft.docs.mcp` for API contracts and best practices
- UI tasks follow tool-assisted pipeline:
  1. Extract design: `#figma/dev-mode-mcp-server` to gather component specs from Figma frames
  2. Implement UI: Build Razor Pages/components per specifications
  3. Test automation: `#mcp_playwright_browser_navigate`, `#mcp_playwright_browser_click`, `#mcp_playwright_browser_fill_form` for journeys
  4. Debug live: `#mcp_chromedevtool_take_snapshot`, `#mcp_chromedevtool_list_console_messages`, `#mcp_chromedevtool_list_network_requests`

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions
- NEW UI feature tasks MUST include the exact Figma frame/flow link; if unavailable, label the work "Skipped — No Figma", create the supporting `figma-prompts/` collateral using `#figma/dev-mode-mcp-server`, and omit implementation tasks until the design artifact arrives.
- UI MIGRATION tasks (preserving existing layouts) do NOT require Figma - document via Playwright tests for functional parity.
- Reference owning Reqnroll Feature, test assets (unit, BDD, Playwright), Aspire touchpoints, and multi-tenancy validation (if applicable) so traceability back to the constitution is obvious.

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

<!--
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.

  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/

  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment

  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting and formatting tools

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

Examples of foundational tasks (adjust based on your project):

- [ ] T004 Setup database schema and migrations framework
- [ ] T005 [P] Implement authentication/authorization framework
- [ ] T006 [P] Setup API routing and middleware structure
- [ ] T007 Create base models/entities that all stories depend on
- [ ] T008 Configure error handling and logging infrastructure
- [ ] T009 Setup environment configuration management

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (MANDATORY — execute before implementation) ⚠️

**NOTE: Commit the Reqnroll Feature and step definitions before production code; run `dotnet test` Red → Green and capture both outputs.**

- [ ] T010 [US1] Add `specs/[###-feature]/features/[story1].feature` with scenarios covering the Figma journey.
- [ ] T011 [US1] Implement step definitions in `tests/bdd/steps/test_[story1]_steps.cs` (ensure they fail before implementation).
- [ ] T012 [P] [US1] Create Playwright journey test in `tests/playwright/[story1].spec.ts`.
- [ ] T013 [P] [US1] Build Aspire integration test project in `tests/integration/[story1]/` to validate hosting/client wiring.

### Implementation for User Story 1

- [ ] T014 [P] [US1] Create [Entity1] model in src/models/[entity1].cs (respect Domain boundaries)
- [ ] T015 [P] [US1] Create [Entity2] model in src/models/[entity2].cs
- [ ] T016 [US1] Implement [Service] in src/application/services/[service].cs (depends on T014, T015)
- [ ] T017 [US1] Implement [endpoint/feature] in src/ui/[location]/[file].cs
- [ ] T018 [US1] Add validation and error handling
- [ ] T019 [US1] Add structured logging for user story 1 operations

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (MANDATORY — execute before implementation) ⚠️

- [ ] T020 [US2] Update `specs/[###-feature]/features/[story2].feature` with new scenarios.
- [ ] T021 [US2] Implement/extend step definitions in `tests/bdd/steps/test_[story2]_steps.cs`.
- [ ] T022 [P] [US2] Extend Playwright coverage in `tests/playwright/[story2].spec.ts`.
- [ ] T023 [P] [US2] Add Aspire integration tests in `tests/integration/[story2]/`.

### Implementation for User Story 2

- [ ] T024 [P] [US2] Create [Entity] model in src/models/[entity].cs
- [ ] T025 [US2] Implement [Service] in src/application/services/[service].cs
- [ ] T026 [US2] Implement [endpoint/feature] in src/ui/[location]/[file].cs
- [ ] T027 [US2] Integrate with User Story 1 components (if needed) while preserving async boundaries

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (MANDATORY — execute before implementation) ⚠️

- [ ] T028 [US3] Add or refine `specs/[###-feature]/features/[story3].feature`.
- [ ] T029 [US3] Implement step definitions in `tests/bdd/steps/test_[story3]_steps.cs`.
- [ ] T030 [P] [US3] Create Playwright journey test in `tests/playwright/[story3].spec.ts`.
- [ ] T031 [P] [US3] Add Aspire integration validation in `tests/integration/[story3]/`.

### Implementation for User Story 3

- [ ] T032 [P] [US3] Create [Entity] model in src/models/[entity].cs
- [ ] T033 [US3] Implement [Service] in src/application/services/[service].cs
- [ ] T034 [US3] Implement [endpoint/feature] in src/ui/[location]/[file].cs

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in docs/
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX [P] Extend unit test suite in tests/unit/ to maintain ≥ 80% coverage
- [ ] TXXX Security hardening
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests MUST be written and FAIL before implementation (unit/TDD, Reqnroll, Playwright, Aspire integration)
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (Red phase):
Task: "Add specs/[###-feature]/features/[story1].feature"
Task: "Implement step definitions in tests/bdd/steps/test_[story1]_steps.cs"
Task: "Create tests/playwright/[story1].spec.ts"
Task: "Build Aspire integration suite in tests/integration/[story1]/"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].cs"
Task: "Create [Entity2] model in src/models/[entity2].cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- **Red→Green Evidence (MANDATORY)**: Capture terminal output BEFORE implementation: `dotnet test --configuration Debug --verbosity normal > phase-red-dotnet-test.txt` and `pwsh tests/ui/playwright.ps1 > phase-red-playwright.txt`. After implementation, capture Green transcripts with same commands. Attach all 4 transcripts to phase review artifacts.
- After each task completes: commit, pull, and push to phase review branch using pattern `git push origin HEAD:[feature-number]review-Phase[phase-number]`
- Maintain Clean Architecture boundaries (UI → Application → Domain → Infrastructure) in every task
- **Multi-tenancy**: For services with tenant-scoped data, validate tenant_id columns and Row-Level Security policies
- **AI agents**: Use tool-assisted patterns (structured thinking, official docs queries, Figma extraction for new features, Playwright automation, DevTools debugging) as mandated by constitution v1.7.0
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence, cross-tenant data leakage
