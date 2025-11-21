# Azure DevOps Hierarchy Comparison

**Spec**: 000-aspire-scaffolding  
**Date**: 2025-11-20  

---

## Visual Comparison

### Current Structure (v5.0 - Phase-Based)

```
┌─────────────────────────────────────────────────────────────────────┐
│ Epic: Aspire Orchestration & Cross-Cutting Scaffolding (93 SP)     │
└─────────────────────────────────────────────────────────────────────┘
   │
   ├─ F1: Phase 1 - Setup (7 SP)
   │  └─ US: Project Setup & Infrastructure (7 SP)
   │
   ├─ F2: Phase 2 - Foundational (4 SP) 🚧 BLOCKING
   │  └─ US: Foundational Infrastructure (4 SP)
   │
   ├─ F3: Phase 3 - AppHost Boot (7 SP) ⭐ P1
   │  └─ US1: AppHost Boot with Resource Orchestration (7 SP)
   │
   ├─ F4: Phase 4 - Tenant Isolation (9 SP) ⭐ P1
   │  └─ US3: Tenant Isolation with TenantInterceptor (9 SP)
   │
   ├─ F5: Phase 5 - Event Publication (9 SP) ⭐ P1
   │  └─ US4: Event Publication with MassTransit (9 SP)
   │
   ├─ F6: Phase 6 - Redis Caching (9 SP) 🔷 P2
   │  └─ US5: Redis Caching & Idempotency (9 SP)
   │
   ├─ F7: Phase 7 - Observability (9 SP) 🔷 P2
   │  └─ US6: Observability with OpenTelemetry (9 SP)
   │
   ├─ F8: Phase 8 - Scaffolding (13 SP) 🔷 P2
   │  └─ US2: Service Scaffolding Scripts (13 SP)
   │
   ├─ F9: Phase 9 - API Gateway (9 SP) 🔷 P2
   │  └─ US7: API Gateway with Legacy Routing (9 SP)
   │
   ├─ F10: Phase 10 - Resilient Messaging (7 SP) 🔷 P2
   │  └─ US8: Resilient Messaging with Circuit Breaker (7 SP)
   │
   └─ F11: Phase 11 - Polish (10 SP) 🔷 P3
      └─ US: Polish & Cross-Cutting Concerns (10 SP)

Features: 11 | User Stories: 11 | Tasks: 100 | Story Points: 93
```

### Proposed Structure (v6.0 - Domain-Based)

```
┌─────────────────────────────────────────────────────────────────────┐
│ Epic: Aspire Orchestration & Cross-Cutting Scaffolding (93 SP)     │
└─────────────────────────────────────────────────────────────────────┘
   │
   ├─ Feature 1: Platform Foundation & Base Infrastructure (11 SP) 🚧 P1
   │  ├─ US 1.1: Initialize Aspire Orchestration Projects (7 SP)
   │  │   └─ T008-T014: AppHost, ServiceDefaults, packages, scripts
   │  └─ US 1.2: Establish Shared Domain Entities & Persistence (4 SP)
   │      └─ T015-T018: EntityBase, ITenantEntity, AuditLog, DbContext
   │
   ├─ Feature 2: Service Orchestration & Tenant Isolation (16 SP) ⭐ P1
   │  ├─ US 2.1: Boot Application Stack with Health Monitoring (7 SP)
   │  │   └─ T019-T025: PostgreSQL, Redis, RabbitMQ, health checks
   │  └─ US 2.2: Enforce Automatic Tenant Data Isolation (9 SP)
   │      └─ T026-T034: TenantInterceptor, RLS, audit logging
   │
   ├─ Feature 3: Asynchronous Messaging & Events (16 SP) 🔷 P2
   │  ├─ US 3.1: Publish Domain Events via Message Bus (9 SP)
   │  │   └─ T035-T043: MassTransit, RabbitMQ, retry, DLQ
   │  └─ US 3.2: Add Message Retry & Circuit Breaker Resilience (7 SP)
   │      └─ T084-T090: Polly, circuit breaker, fallback
   │
   ├─ Feature 4: Caching, Idempotency & Observability (18 SP) 🔷 P2
   │  ├─ US 4.1: Implement Distributed Caching & Request Idempotency (9 SP)
   │  │   └─ T044-T052: Redis, IdempotencyService, middleware
   │  └─ US 4.2: Enable Distributed Tracing & Metrics (9 SP)
   │      └─ T053-T061: OpenTelemetry, traces, metrics, logs
   │
   ├─ Feature 5: Developer Experience & Migration (22 SP) 🟢 P3
   │  ├─ US 5.1: Automate New Service Scaffolding (13 SP)
   │  │   └─ T062-T074: PowerShell/Bash scripts, Clean Architecture
   │  └─ US 5.2: Route Traffic via API Gateway for Migration (9 SP)
   │      └─ T075-T083: YARP, routing rules, feature flags
   │
   └─ Feature 6: Quality Assurance & Documentation (10 SP) 🟣 P4
      └─ US 6.1: Complete Integration Testing & Performance Validation (10 SP)
          └─ T091-T100: Tests, BDD, performance, docs

Features: 6 | User Stories: 11 | Tasks: 100 | Story Points: 93
```

---

## Side-by-Side Comparison

| Metric | Phase-Based (v5.0) | Domain-Based (v6.0) | Change |
|--------|-------------------|---------------------|--------|
| **Features** | 11 | 6 | ↓ 45% |
| **User Stories** | 11 | 11 | → Same |
| **Tasks** | 100 | 100 | → Same |
| **Story Points** | 93 | 93 | → Same |
| **Avg SP per Feature** | 8.5 | 15.5 | ↑ 82% |
| **Feature Size Range** | 4-13 SP | 10-22 SP | Better balance |
| **US per Feature** | 1.0 | 1.8 | ↑ 83% |
| **Blocking Features** | 1 (Phase 2) | 1 (Feature 1) | → Same |
| **P1 Features** | 4 (F1-F4) | 2 (F1-F2) | ↓ 50% |
| **Parallel Potential** | Low (phases) | High (domains) | ↑ Major |

---

## Dependency Graph

### Current (Phase-Based)

```
Phase 1 (Setup)
     ↓
Phase 2 (Foundational) ← BLOCKS ALL
     ↓
┌────┴────┬────┬────┬────┬────┬────┬────┬────┐
│         │    │    │    │    │    │    │    │
P3       P4   P5   P6   P7   P8   P9  P10  P11
(US1)   (US3)(US4)(US5)(US6)(US2)(US7)(US8)(Polish)
```

**Issues:**
- Sequential phase naming implies order
- Most phases actually independent after Phase 2
- US2, US7, US8 could start immediately after Phase 2

### Proposed (Domain-Based)

```
Feature 1: Foundation
    ↓
    ├─────────┬─────────┬─────────┬─────────┐
    │         │         │         │         │
Feature 2  Feature 3  Feature 4  Feature 5  Feature 6
(Orch &    (Messaging)(Cache &   (DevEx &   (Quality)
 Tenant)              Obs)      Migration)    ↑
    │         │         │         │         │
    │      US 3.1 →  US 3.2       │         │
    │         │         │         │         │
    └─────────┴─────────┴─────────┴─────────┘
```

**Benefits:**
- Clear: Only Feature 1 blocks
- Explicit: US 3.2 depends on US 3.1 documented
- Parallel: Features 2, 4, 5 fully independent after Feature 1
- Sequential: Feature 3 proceeds US 3.1 → US 3.2
- Final: Feature 6 requires all (integration testing)

---

## Milestone Progression

### Phase-Based Milestones (Unclear Value Increments)

```
Milestone 1: Phases 1-2 Complete (11 SP)
  → Foundation ready, but no business value yet

Milestone 2: P1 Complete - Phases 1-5 (36 SP)
  → AppHost boots, tenants isolated, events publish

Milestone 3: P2 Complete - Phases 1-10 (83 SP)
  → Added caching, observability, scaffolding, gateway, resilience

Milestone 4: All Complete - Phases 1-11 (93 SP)
  → Polish and tests done
```

### Domain-Based Milestones (Clear Value Delivery)

```
Milestone 1: MVP - Features 1-2 (27 SP)
  ✅ Working multi-tenant platform with orchestration
  Demo: Boot stack, health dashboard, tenant isolation

Milestone 2: Production - Features 1-4 (61 SP)
  ✅ Production-grade with events, resilience, observability
  Demo: Publish events with DLQ, distributed traces, cache hits

Milestone 3: DevEx - Features 1-5 (83 SP)
  ✅ Developer-optimized with scaffolding and migration
  Demo: Scaffold service < 2 min, gateway routing toggle

Milestone 4: Hardened - All Features (93 SP)
  ✅ Production-hardened with full test coverage
  Demo: Test suite passes, performance budgets validated
```

---

## Team Parallelization

### Phase-Based (Limited)

```
Sprint 1-2: Everyone on Phase 1-2 (blocking)
Sprint 3:   Split teams across P3-P5 (but phases still suggest order)
Sprint 4:   P6-P10 (still feels sequential)
Sprint 5:   P11 (everyone waits for polish)
```

**Coordination Overhead:** High (11 features to track)  
**Team Autonomy:** Low (phase dependencies unclear)

### Domain-Based (Optimized)

```
Sprint 1-2: Everyone on Feature 1 (blocking)
Sprint 3-4: 
  - Team Alpha: Feature 2 (Orchestration & Tenancy)
  - Team Beta:  Feature 3 (Messaging) - US 3.1 first
  - Team Gamma: Feature 4 (Caching & Observability)
  - Team Delta: Feature 5 (DevEx & Gateway)
  
Sprint 5:
  - Team Beta continues Feature 3 - US 3.2 (circuit breaker)
  - Other teams finish their features or help Feature 6 prep

Sprint 6: All teams on Feature 6 (Quality & testing)
```

**Coordination Overhead:** Low (6 features, clear domains)  
**Team Autonomy:** High (feature ownership, clear dependencies)

---

## Migration Path

### If Adopting v6.0 Structure

**Step 1: Mapping**
- Map all 100 tasks to new feature groupings (already done in proposal)
- Verify no tasks lost (✅ all 100 accounted for)

**Step 2: Communication**
- Share `hierarchy-proposal.md` with stakeholders
- Explain rationale and benefits

**Step 3: ADO Update**
- Delete existing work items (not yet created, so easy)
- Create Epic with 6 Features
- Create 11 User Stories under correct features
- Link tasks to user stories

**Step 4: Artifact Updates**
- Replace `.ado-hierarchy.json` with `.ado-hierarchy-proposed.json`
- Update `spec.md` frontmatter to reference Feature 1 (foundation)
- Update `tasks.md` to add feature context to each phase

**Step 5: Team Alignment**
- Assign feature ownership to teams
- Review dependency graph together
- Plan Sprint 1-2 (Feature 1) as all-hands effort

---

## Decision Matrix

| Criteria | Weight | Phase-Based Score | Domain-Based Score |
|----------|--------|------------------|-------------------|
| Business Value Clarity | 20% | 5/10 | 9/10 |
| Team Parallelization | 20% | 4/10 | 9/10 |
| Coordination Overhead | 15% | 3/10 | 8/10 |
| Feature Cohesion | 15% | 5/10 | 9/10 |
| Milestone Clarity | 15% | 6/10 | 9/10 |
| Risk (change cost) | 10% | 9/10 (no change) | 7/10 (restructure) |
| Team Autonomy | 5% | 4/10 | 9/10 |
| **Weighted Total** | | **5.15/10** | **8.55/10** |

**Recommendation:** ✅ Adopt Domain-Based (v6.0)

---

## Summary

The domain-based structure (v6.0) offers:
- **45% fewer features** to track (11 → 6)
- **Better parallelization** (4-5 features concurrent)
- **Clearer milestones** (value delivery per milestone)
- **Balanced workload** (10-22 SP range vs 4-13 SP)
- **Same deliverables** (93 SP, 100 tasks preserved)

Trade-off: Requires restructuring work items (low risk since not yet created in ADO).

**Decision**: Adopt v6.0 and update all artifacts accordingly.
