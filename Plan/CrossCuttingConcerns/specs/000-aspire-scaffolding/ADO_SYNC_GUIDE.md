# Azure DevOps Sync Guide: Aspire Scaffolding

**Epic**: #1399 - Aspire Orchestration & Cross-Cutting Scaffolding  
**Spec Path**: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/`  
**Generated**: 2025-11-20  
**Sync Status**: ✅ Complete (all User Stories created with requirement keys)

---

## Current ADO Structure

### Epic #1399: Aspire Orchestration & Cross-Cutting Scaffolding
- **URL**: https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1399
- **Tags**: foundation, cross-cutting, aspire
- **Layer**: CrossCuttingConcerns (Foundation)

### Features (Phases)

| Phase | Feature ID | Title | Tasks | User Stories |
|-------|-----------|-------|-------|--------------|
| Phase 1 | #1403 | Phase 1: Setup | T008-T014 (7 tasks) | US: Priority 1 (#1429, 7 SP) |
| Phase 2 | #1410 | Phase 2: Foundational | T015-T018 (4 tasks) | US: Priority 1 (#1425, 4 SP) |
| Phase 3 | #1405 | Phase 3: User Story 1 - AppHost Boot | T019-T025 (7 tasks) | US1: AppHost Boot (#1427, 7 SP) |
| Phase 4 | #1404 | Phase 4: User Story 3 - Tenant Isolation | T026-T034 (9 tasks) | US3: Tenant Isolation (#1426, 9 SP) |
| Phase 5 | #1416 | Phase 5: Event Publication | T035-T043 (9 tasks) | US4: Event Publication (#1428, 9 SP) |
| Phase 6 | #1409 | Phase 6: Redis Caching | T044-T052 (9 tasks) | US5: Redis Caching (#1433, 9 SP) |
| Phase 7 | #1411 | Phase 7: Observability | T053-T061 (9 tasks) | US6: Observability (#1435, 9 SP) |
| Phase 8 | #1415 | Phase 8: Scaffolding | T062-T074 (13 tasks) | US2: Scaffolding (#1432, 13 SP) |
| Phase 9 | #1413 | Phase 9: API Gateway | T075-T083 (9 tasks) | US7: API Gateway (#1434, 9 SP) |
| Phase 10 | #1412 | Phase 10: Resilient Messaging | T084-T090 (7 tasks) | US8: Resilient Messaging (#1431, 7 SP) |
| Phase 11 | #1414 | Phase 11: Polish | T091-T100 (10 tasks) | Polish (#1430, 10 SP) |

### User Stories with Requirement Keys

| Requirement Key | User Story ID | Title | Priority | Story Points | Phase |
|----------------|---------------|-------|----------|--------------|-------|
| US1 | #1427 | Phase 3: US1 - AppHost Boot with Resource Orchestration | P1 | 7 | 3 |
| US3 | #1426 | Phase 4: US3 - Tenant Isolation with TenantInterceptor | P1 | 9 | 4 |
| US4 | #1428 | Phase 5: US4 - Event Publication with MassTransit | P1 | 9 | 5 |
| US5 | #1433 | Phase 6: US5 - Redis Caching & Idempotency | P2 | 9 | 6 |
| US6 | #1435 | Phase 7: US6 - Observability with OpenTelemetry | P2 | 9 | 7 |
| US2 | #1432 | Phase 8: US2 - Service Scaffolding Scripts | P2 | 13 | 8 |
| US7 | #1434 | Phase 9: US7 - API Gateway with Legacy Routing | P2 | 9 | 9 |
| US8 | #1431 | Phase 10: US8 - Resilient Messaging with Circuit Breaker | P2 | 7 | 10 |
| - | #1429 | Phase 1: Priority 1 Implementation | P1 | 7 | 1 (Setup) |
| - | #1425 | Phase 2: Priority 1 Implementation | P1 | 4 | 2 (Foundational) |
| - | #1430 | Phase 11: Priority 3 Implementation | P3 | 10 | 11 (Polish) |

---

## Requirement Key Mapping (tasks.md → User Stories)

### Priority 1 (P1) - MVP/Critical Path
- **US1 - AppHost Boot** (Phase 3): Tasks T019-T025 with `[US1]` marker
  - Goal: Boot PostgreSQL, Redis, RabbitMQ in <15s, verify Aspire dashboard
  - Independent Test: Run AppHost, check http://localhost:15000 shows healthy resources

- **US3 - Tenant Isolation** (Phase 4): Tasks T026-T034 with `[US3]` marker
  - Goal: Enforce TenantId on all queries/saves with `[IgnoreTenantFilter]` opt-out
  - Independent Test: Save entity without TenantId → exception; use attribute → audit log

- **US4 - Event Publication** (Phase 5): Tasks T035-T043 with `[US4]` marker
  - Goal: Publish domain events to RabbitMQ with retry/DLQ, <500ms P95
  - Independent Test: Trigger event, verify in RabbitMQ UI; simulate failure, verify DLQ

### Priority 2 (P2) - Important Features
- **US5 - Redis Caching & Idempotency** (Phase 6): Tasks T044-T052 with `[US5]` marker
  - Goal: 10-minute idempotency TTL, return 202 on duplicates
  - Independent Test: POST with X-Idempotency-Key twice, verify 202 with original ID

- **US6 - Observability** (Phase 7): Tasks T053-T061 with `[US6]` marker
  - Goal: Distributed tracing, metrics, logs with correlation IDs
  - Independent Test: Execute API request, verify trace with correlation ID in dashboard

- **US2 - Service Scaffolding** (Phase 8): Tasks T062-T074 with `[US2]` marker
  - Goal: Run `./new-service.ps1 -ServiceName "X"` to generate service structure
  - Independent Test: Execute script, verify project created with AppHost registration

- **US7 - API Gateway** (Phase 9): Tasks T075-T083 with `[US7]` marker
  - Goal: YARP routing /legacy/* to old system, /api/* to new services
  - Independent Test: GET /legacy/health → legacy; GET /api/health → new service

- **US8 - Resilient Messaging** (Phase 10): Tasks T084-T090 with `[US8]` marker
  - Goal: Circuit breaker in MassTransit consumers
  - Independent Test: Simulate 5 failures, verify circuit opens; wait 30s, verify recovery

### Priority 3 (P3) - Polish
- **Phase 11 - Polish**: Tasks T091-T100 (no specific US marker)
  - Cross-cutting improvements: docs, perf tuning, security, validation

### Foundational (No User Story)
- **Phase 1 - Setup**: Tasks T008-T014 (infrastructure setup)
- **Phase 2 - Foundational**: Tasks T015-T018 (blocking prerequisites)

---

## Task Distribution Analysis

**Total**: 100 tasks, 93 story points

**By Priority**:
- P1 (Phases 1-5): 36 tasks = 36 SP (Critical path - MVP)
- P2 (Phases 6-10): 47 tasks = 47 SP (Important features)
- P3 (Phase 11): 10 tasks = 10 SP (Polish/validation)
- Foundational (Phases 1-2): 11 tasks = 11 SP (Setup, blocks all user stories)

**Story Point Calculation**: 1 task = 1 story point (simplified from variable points)

---

## User Story Titles Updated (Requirement Keys)

The following User Story titles now include requirement keys for traceability:

### ✅ Already Updated in ADO

1. **#1429** → "Phase 1: Priority 1 Implementation" (Setup - no specific US)
2. **#1425** → "Phase 2: Priority 1 Implementation" (Foundational - no specific US)
3. **#1427** → "Phase 3: US1 - AppHost Boot with Resource Orchestration"
4. **#1426** → "Phase 4: US3 - Tenant Isolation with TenantInterceptor"
5. **#1428** → "Phase 5: US4 - Event Publication with MassTransit"
6. **#1433** → "Phase 6: US5 - Redis Caching & Idempotency"
7. **#1435** → "Phase 7: US6 - Observability with OpenTelemetry"
8. **#1432** → "Phase 8: US2 - Service Scaffolding Scripts"
9. **#1434** → "Phase 9: US7 - API Gateway with Legacy Routing"
10. **#1431** → "Phase 10: US8 - Resilient Messaging with Circuit Breaker"
11. **#1430** → "Phase 11: Priority 3 Implementation" (Polish - no specific US)

---

## Verification Checklist

Use this checklist to verify the ADO sync is complete and accurate:

### Epic Level
- [x] Epic #1399 exists with correct title
- [x] Epic has appropriate tags (foundation, cross-cutting, aspire)
- [x] Epic description includes business value and architecture context

### Feature Level (11 Features)
- [x] All 11 Features created and linked to Epic #1399
- [x] Feature titles match phase names from tasks.md
- [x] Feature work item IDs recorded in .ado-hierarchy.json

### User Story Level (11 User Stories)
- [x] All 11 User Stories created with requirement keys
- [x] User Story titles follow format "Phase N: US# - [Description]"
- [x] Story Points = count of tasks (1 point per task)
- [x] Priority field matches priority group (1, 2, or 3)
- [x] User Stories linked to parent Features
- [x] User Story descriptions include task checklists with markdown checkboxes

### Hierarchy Registry
- [x] .ado-hierarchy.json updated with all work item IDs
- [x] requirement_key field populated for each user story
- [x] user_stories object keys are descriptive (e.g., "us1-apphost-boot")
- [x] All task IDs listed in tasks array per phase

### Task Traceability
- [x] Tasks in tasks.md have [US#] markers where applicable
- [x] Task checklists embedded in User Story descriptions
- [x] Each task has 4 sub-checkboxes: Coded, Tested, Reviewed, Merged

---

## How to Update User Story Titles in ADO (Manual)

If the MCP tool isn't working, update titles manually:

1. Navigate to each User Story in ADO
2. Click "Edit" on the work item
3. Update the "Title" field to include requirement key
4. Save changes

Example updates:
- #1427: "Phase 3: Priority 1 Implementation" → "Phase 3: US1 - AppHost Boot with Resource Orchestration"
- #1426: "Phase 4: Priority 1 Implementation" → "Phase 4: US3 - Tenant Isolation with TenantInterceptor"

---

## Next Steps

1. **Sprint Planning**: Group User Stories by priority (P1 → P2 → P3)
2. **Task Breakdown**: Convert task checklists in User Story descriptions to ADO Task work items (optional)
3. **Iteration Assignment**: Assign P1 stories (US1, US3, US4) to Sprint 1
4. **Team Assignment**: Distribute stories across team members
5. **Start Implementation**: Begin with Phase 1 (Setup) and Phase 2 (Foundational)

---

## Related Documentation

- **ADO Sync Agent**: `.github/agents/ado-sync-agent.md`
- **Hierarchy Registry**: `Plan/CrossCuttingConcerns/specs/.ado-hierarchy.json`
- **Spec**: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/spec.md`
- **Tasks**: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/tasks.md`
- **Plan**: `Plan/CrossCuttingConcerns/specs/000-aspire-scaffolding/plan.md`

---

**Last Updated**: 2025-11-20  
**Sync Agent Version**: 1.0.0 (Priority-based User Story groupings with requirement keys)
