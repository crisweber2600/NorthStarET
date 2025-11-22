# Spec: Student Management Service Migration

Short Name: student-service-migration
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Migrate legacy monolith student management functionality into an independent event-driven Student Management Service providing CRUD, enrollment, search, import/export, dashboard aggregation, and compliance features.

## Business Value
Enables scalable, isolated student operations, improves performance and extensibility, supports cross-service orchestration via domain events, and establishes foundation for future analytics and intervention workflows.

## Target Layer
Foundation

## Actors
- District Administrator
- School Administrator
- Teacher
- Assessment Service (subscriber)
- Section Service (subscriber)
- Reporting Service (subscriber)

## Assumptions
- Multi-tenant architecture & RLS already implemented.
- Identity & Configuration services available for user + school validation.
- Message bus (MassTransit + Azure Service Bus) provisioned.

## Constraints
- Tenant isolation mandatory for all queries.
- Event publishing within <50ms overhead.
- Search p95 latency <100ms.
- Bulk import 500 students <2 minutes.

## Scenarios (Condensed)
1. Create new student -> validate, persist, publish StudentCreatedEvent, return 201.
2. Downstream services (Assessment, Section, Reporting) react to StudentCreatedEvent within 5s.
3. Update demographics -> persist delta, publish StudentDemographicsChangedEvent, audit.
4. Search students -> tenant-filtered, wildcard support, <100ms.
5. Enroll student -> create enrollment, publish StudentEnrolledEvent, update status.
6. Bulk CSV import -> validate all rows, transactional insert, publish per student, summary report.
7. Student dashboard -> aggregate multi-service data via orchestrated calls, return <200ms.
8. Withdraw student -> update enrollment, publish StudentWithdrawnEvent, hide from active lists.
9. Privacy enforcement -> authorization ensures legitimate educational interest; auditable.
10. Merge duplicates -> reassign relationships, soft-delete secondary, publish StudentMergedEvent.
11. Photo upload -> blob storage per tenant path, resize, secure access.
12. State export -> generate CSV with required fields, exclude confidential, 24h link.

## Non-Functional Requirements
- Security: FERPA compliance, audit trail for all access mutations.
- Performance: SLOs as above; dashboard aggregated concurrently.
- Reliability: Idempotency for creation & import operations.
- Observability: Correlation IDs and tenant attributes in spans.

## Acceptance Criteria Summary
All scenarios covered by integration + BDD tests; domain events validated via consumer harness; performance benchmarks meet SLOs; audit + privacy checks enforced.

## Out of Scope
- Advanced predictive analytics.
- Real-time collaborative editing.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Duplicate creation during import | Idempotency + email uniqueness checks |
| Dashboard performance degradation | Parallel service calls + caching |
| Eventual consistency delays | Retry + monitoring of subscriber lag |
| Merge data loss risk | Pre-merge validation and backup snapshot |

## Initial Roadmap
1. Scaffold service (layers, DbContext, events).
2. Implement create/search/enroll + events.
3. Add bulk import & photo storage.
4. Implement merge + withdrawal workflows.
5. Add dashboard aggregation.
6. Build export pipeline + audit reporting.

## Audit & Compliance
All CRUD and read access logged with tenant_id, user_id, action, entity_id, timestamp.

---
Generated manually (spec draft).