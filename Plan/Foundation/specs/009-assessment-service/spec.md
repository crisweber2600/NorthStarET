Feature: Assessment Service Migration (Foundation Layer)
Business Value: Modern, scalable assessment lifecycle (definition → assignment → scoring → analytics) enabling faster instructional decisions and benchmark tracking while decoupling legacy tight coupling and improving performance & compliance.

Scope:
- In Scope: Assessment definitions, custom fields (JSONB), assignments, result recording, scoring engine (points, weighted, rubric), benchmarks, templates, state test import, student trends, exports, audit trail.
- Out of Scope (Phase 1): Adaptive testing engine, cross-district benchmark sharing, ML predictive scoring.

Key Scenarios (Condensed):
1. Create assessment with custom numeric fields (AssessmentCreatedEvent).
2. Assign assessment to roster (AssessmentAssignedEvent per student).
3. Record multi-field results → aggregate + benchmark classification (AssessmentResultRecordedEvent).
4. Configure benchmarks → auto categorize results (BenchmarkCreatedEvent).
5. Calculate weighted/rubric score + letter grade via Configuration Service scale.
6. Search/filter assessments (p95 <100ms, tenant isolation).
7. Student trends (linear regression improving/declining/stable, real-time update).
8. Bulk export (authorized data only, audit log).
9. State test data import (StateTestResultsImportedEvent).
10. Template library (district scoped reusable definitions).
11. Scheduling + reminders (7d/1d/day-of; completion dashboard).
12. Immutable audit trail (before/after, FERPA compliance).

Non-Functional Requirements:
- Performance SLOs: Create/Record/Search <100ms p95; Trends <200ms p95; Export 1000 results <30s.
- Availability 99.9%; RPO < 15m; RTO < 30m.
- Security: FERPA, principle of least privilege, Key Vault secrets, RLS enforced.
- Observability: Structured logging (tenant, correlationId), traces for scoring & trend calc spans, metrics for p95 latencies & error rates.
- Multi-Tenancy: Row Level Security + session variable app.current_tenant.
- Idempotency: Assessment creation 10m window; result recording 5m.

Risks & Mitigations:
- Large historical result migration (15 yrs) → batch + COPY, chunked replay of events for analytics warm cache.
- JSONB custom fields complexity → Validation layer + generated projection tables for common queries.
- Trend calc performance → Precompute nightly materialized views; fallback on on-demand calculation with cache.

Audit & Compliance:
- All create/update actions produce immutable audit record (who/when/what before/after, reason).
- Score changes flagged for review queue (AssessmentScoreChangeReviewNeededEvent optional future).

Event Catalog (Publish v1.0): AssessmentCreatedEvent, AssessmentAssignedEvent, AssessmentResultRecordedEvent, BenchmarkCreatedEvent, StateTestResultsImportedEvent.
Subscribed: StudentEnrolledEvent, DistrictSettingsUpdatedEvent.

Initial Cutover Plan (Strangler): Phase 2a dual-write new definitions; Phase 2b migrate data; Phase 2c full routing; Phase 2d enhanced features; Phase 2e decommission legacy.

Open Questions:
1. Do we require percentile calculations inside service or delegated to Analytics? (Proposed: delegate.)
2. State test formats variance—common adapter or per-format strategy? (Proposed: strategy pattern.)
3. Benchmark overlap across grades—inheritance rules? (Proposed: explicit grade scoped rows.)

Acceptance Criteria (Representative):
- Creating assessment returns 201 with generated Id, custom fields persisted.
- Recording a result publishes AssessmentResultRecordedEvent with benchmarkLevel computed.
- Search returns only tenant-owned assessments, sorted by descending createdAt.
- Trend endpoint returns direction stable/improving/declining with slope numeric.
- Export produces CSV with UTF-8 BOM, audited.
