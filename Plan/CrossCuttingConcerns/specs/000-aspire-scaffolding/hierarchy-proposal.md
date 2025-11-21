# Work Item Hierarchy Proposal

**Epic**: Aspire Orchestration & Cross-Cutting Scaffolding  
**Date**: 2025-11-20  
**Proposed Version**: 6.0.0  
**Status**: Pending Approval  

---

## Executive Summary

This document proposes a **restructured work item hierarchy** that groups the 100 tasks into **6 functional domain features** instead of the original 11 phase-based features. This improves:

- **Team parallelization** (4-5 features can run concurrently after foundation)
- **Business value alignment** (features map to capabilities, not phases)
- **Clear milestones** (4 delivery checkpoints with incremental value)
- **Balanced workload** (10-22 SP per feature vs fragmented 4-13 SP)

---

## Current vs Proposed Structure

### Current (v5.0) - Phase-Based
```
Epic (93 SP)
├─ Phase 1: Setup (7 SP)
├─ Phase 2: Foundational (4 SP)
├─ Phase 3: US1 - AppHost Boot (7 SP)
├─ Phase 4: US3 - Tenant Isolation (9 SP)
├─ Phase 5: US4 - Event Publication (9 SP)
├─ Phase 6: US5 - Redis Caching (9 SP)
├─ Phase 7: US6 - Observability (9 SP)
├─ Phase 8: US2 - Scaffolding (13 SP)
├─ Phase 9: US7 - API Gateway (9 SP)
├─ Phase 10: US8 - Resilient Messaging (7 SP)
└─ Phase 11: Polish (10 SP)
```

**Issues:**
- ❌ 11 features = excessive fragmentation
- ❌ Phase-based grouping doesn't reflect functional domains
- ❌ Harder to parallelize (phases imply sequence)
- ❌ User stories 1:1 with features (no grouping benefit)

### Proposed (v6.0) - Domain-Based

```
Epic (93 SP)
├─ Feature 1: Platform Foundation & Base Infrastructure (11 SP) 🚧 P1 BLOCKING
│  ├─ US 1.1: Initialize Aspire Orchestration Projects (7 SP)
│  └─ US 1.2: Establish Shared Domain Entities & Persistence (4 SP)
│
├─ Feature 2: Service Orchestration & Tenant Isolation (16 SP) ⭐ P1 MVP
│  ├─ US 2.1: Boot Application Stack with Health Monitoring (7 SP)
│  └─ US 2.2: Enforce Automatic Tenant Data Isolation (9 SP)
│
├─ Feature 3: Asynchronous Messaging & Events (16 SP) 🔷 P2 Production
│  ├─ US 3.1: Publish Domain Events via Message Bus (9 SP)
│  └─ US 3.2: Add Message Retry & Circuit Breaker Resilience (7 SP)
│
├─ Feature 4: Caching, Idempotency & Observability (18 SP) 🔷 P2 Production
│  ├─ US 4.1: Implement Distributed Caching & Request Idempotency (9 SP)
│  └─ US 4.2: Enable Distributed Tracing & Metrics (9 SP)
│
├─ Feature 5: Developer Experience & Migration (22 SP) 🟢 P3 DevEx
│  ├─ US 5.1: Automate New Service Scaffolding (13 SP)
│  └─ US 5.2: Route Traffic via API Gateway for Migration (9 SP)
│
└─ Feature 6: Quality Assurance & Documentation (10 SP) 🟣 P4 Polish
   └─ US 6.1: Complete Integration Testing & Performance Validation (10 SP)
```

**Benefits:**
- ✅ 6 features = better cohesion and focus
- ✅ Domain-based grouping (messaging, performance, devex)
- ✅ Clear parallelization opportunities
- ✅ Balanced feature sizes (10-22 SP)

---

## Feature Details

### Feature 1: Platform Foundation & Base Infrastructure (P1 - BLOCKING)
**Story Points**: 11  
**Priority**: P1  
**Dependencies**: None (blocks all others)  

**User Stories:**
- **US 1.1** (7 SP): Initialize Aspire Orchestration Projects
  - Tasks: T008-T014 (AppHost, ServiceDefaults, packages, scripts)
- **US 1.2** (4 SP): Establish Shared Domain Entities & Persistence
  - Tasks: T015-T018 (EntityBase, ITenantEntity, AuditLog, DbContext)

**Why Grouped**: Both are foundational blockers; setup without domain entities is incomplete.

---

### Feature 2: Service Orchestration & Tenant Isolation (P1 - MVP)
**Story Points**: 16  
**Priority**: P1  
**Dependencies**: Feature 1  

**User Stories:**
- **US 2.1** (7 SP): Boot Application Stack with Health Monitoring
  - Tasks: T019-T025 (PostgreSQL, Redis, RabbitMQ, health checks)
  - Acceptance: Startup < 15s, all resources healthy
- **US 2.2** (9 SP): Enforce Automatic Tenant Data Isolation
  - Tasks: T026-T034 (TenantInterceptor, RLS, audit logging)
  - Acceptance: 100% query filtering, audited opt-outs

**Why Grouped**: Core platform capabilities that define MVP - orchestration + security.

---

### Feature 3: Asynchronous Messaging & Events (P2 - Production)
**Story Points**: 16  
**Priority**: P2  
**Dependencies**: Feature 1, US 3.2 depends on US 3.1  

**User Stories:**
- **US 3.1** (9 SP): Publish Domain Events via Message Bus
  - Tasks: T035-T043 (MassTransit, RabbitMQ, retry, DLQ)
  - Acceptance: Events publish < 500ms P95, 3 retries, DLQ routing
- **US 3.2** (7 SP): Add Message Retry & Circuit Breaker Resilience
  - Tasks: T084-T090 (Polly, circuit breaker, fallback)
  - Acceptance: Opens after 5 failures, 30s timeout, logging

**Why Grouped**: Both are messaging concerns; circuit breaker extends base MassTransit config.

---

### Feature 4: Caching, Idempotency & Observability (P2 - Production)
**Story Points**: 18  
**Priority**: P2  
**Dependencies**: Feature 1  

**User Stories:**
- **US 4.1** (9 SP): Implement Distributed Caching & Request Idempotency
  - Tasks: T044-T052 (Redis, IdempotencyService, middleware)
  - Acceptance: Cache hits < 20ms, 10-min idempotency window, 202 on duplicates
- **US 4.2** (9 SP): Enable Distributed Tracing & Metrics
  - Tasks: T053-T061 (OpenTelemetry, traces, metrics, logs)
  - Acceptance: Single trace per request, correlation IDs, metrics exported

**Why Grouped**: Both are performance/reliability concerns that make system production-grade.

---

### Feature 5: Developer Experience & Migration (P3 - DevEx)
**Story Points**: 22  
**Priority**: P3  
**Dependencies**: Feature 1  

**User Stories:**
- **US 5.1** (13 SP): Automate New Service Scaffolding
  - Tasks: T062-T074 (PowerShell/Bash scripts, Clean Architecture layers)
  - Acceptance: Script completes < 2 min, generates 4 layers, AppHost registration
- **US 5.2** (9 SP): Route Traffic via API Gateway for Migration
  - Tasks: T075-T083 (YARP, routing rules, feature flags)
  - Acceptance: /legacy/* routes to old, /api/* to new, zero downtime toggle

**Why Grouped**: Both improve developer experience - scaffolding for new services, gateway for migration.

---

### Feature 6: Quality Assurance & Documentation (P4 - Polish)
**Story Points**: 10  
**Priority**: P4  
**Dependencies**: All features  

**User Stories:**
- **US 6.1** (10 SP): Complete Integration Testing & Performance Validation
  - Tasks: T091-T100 (integration tests, BDD, performance validation, docs)
  - Acceptance: Full stack tests pass, performance budgets met, quickstart validated

**Why Grouped**: Final polish phase validating all previous work.

---

## Delivery Milestones

### Milestone 1: MVP - Working Multi-Tenant Platform (27 SP)
**Features**: 1 + 2  
**Value**: Core platform with orchestration, health monitoring, and tenant isolation  
**Demo**: Boot stack, show health dashboard, verify tenant isolation

### Milestone 2: Production-Ready - Events & Observability (61 SP)
**Features**: 1 + 2 + 3 + 4  
**Value**: Production-grade with async events, resilience, caching, observability  
**Demo**: Publish events with retry/DLQ, show traces in dashboard, cache performance

### Milestone 3: Developer-Optimized - Tooling & Migration (83 SP)
**Features**: 1 + 2 + 3 + 4 + 5  
**Value**: Full platform with service scaffolding automation and legacy migration path  
**Demo**: Scaffold new service in < 2 min, route traffic via gateway with toggle

### Milestone 4: Production-Hardened - Full Test Coverage (93 SP)
**Features**: All (1-6)  
**Value**: Production-hardened with comprehensive tests and validated performance  
**Demo**: Run full test suite, show coverage report, validate all performance budgets

---

## Parallelization Strategy

### After Feature 1 Completion (BLOCKING)
**Parallel Track A**: Feature 2 (Orchestration & Tenancy)  
**Parallel Track B**: Feature 4 (Caching & Observability)  
**Parallel Track C**: Feature 5 (DevEx & Gateway)  

**Sequential**: Feature 3 (Messaging) - US 3.1 must complete before US 3.2

**Final**: Feature 6 (Quality) - requires all features complete

### Team Assignment Example
- **Team Alpha**: Feature 1 → Feature 2 (infrastructure focus)
- **Team Beta**: Feature 3 (messaging expertise)
- **Team Gamma**: Feature 4 (performance/observability)
- **Team Delta**: Feature 5 (developer tooling)
- **Cross-Team**: Feature 6 (quality validation)

---

## Comparison Matrix

| Aspect | Phase-Based (v5.0) | Domain-Based (v6.0) |
|--------|-------------------|---------------------|
| Feature Count | 11 | 6 |
| User Stories | 11 | 11 |
| Story Points | 93 | 93 |
| Tasks | 100 | 100 |
| Parallelization | Limited (phases imply sequence) | High (4-5 features concurrent) |
| Business Alignment | Low (technical phases) | High (functional domains) |
| Team Autonomy | Low (phase dependencies) | High (domain ownership) |
| Milestone Clarity | Medium (phase completion) | High (value delivery) |
| Feature Balance | Poor (4-13 SP range) | Good (10-22 SP range) |

---

## Risk Analysis

### Risks of Current Phase-Based Structure
1. **Over-fragmentation**: 11 features create administrative overhead
2. **False sequence**: "Phase" naming implies serial execution when most work can be parallel
3. **Weak cohesion**: Related work (messaging + circuit breaker) split across phases
4. **Team silos**: Hard to assign clear feature ownership

### Risks of Proposed Domain-Based Structure
1. **Feature size variance**: Feature 5 (22 SP) is 2x Feature 1 (11 SP)
   - *Mitigation*: Feature 5 has 2 independent user stories that can be split
2. **US 3.2 dependency**: Circuit breaker depends on MassTransit config
   - *Mitigation*: Clearly documented, team can plan sequentially within feature
3. **Milestone pressure**: Larger features may delay milestone completion
   - *Mitigation*: Milestones stacked incrementally, each adds value independently

---

## Recommendation

✅ **Adopt the Domain-Based Structure (v6.0)**

**Rationale:**
1. Better aligns with Agile principles (functional increments over temporal phases)
2. Enables true parallel development after foundation
3. Clearer business value per feature
4. Reduces coordination overhead (6 vs 11 features)
5. Maintains all 93 SP and 100 tasks (no work lost)

**Next Steps:**
1. Review with stakeholders and adjust if needed
2. Update `.ado-hierarchy.json` from `.ado-hierarchy-proposed.json`
3. Sync to Azure DevOps to create Epic → 6 Features → 11 User Stories
4. Update `spec.md` and `tasks.md` frontmatter with new feature groupings
5. Communicate new structure to development teams

---

## Approval

- [ ] Product Owner Approval
- [ ] Engineering Lead Approval
- [ ] Architecture Review Board Approval
- [ ] Scrum Master Acknowledgment

**Approved By**: ___________________________  
**Date**: ___________________________
