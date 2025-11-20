# Spec: Staff Management Service Migration

Short Name: staff-service-migration
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Migrate staff management (profiles, assignments, teams, certifications, schedules, search, import, audit) from monolith to dedicated microservice with event-driven integration and multi-school assignment support.

## Business Value
Improves scalability and maintainability of staff operations, enables richer collaboration models (PLC teams), reliable certification tracking, and multi-school FTE assignment, supporting district-wide resource optimization.

## Target Layer
Foundation

## Actors
- District Administrator
- School Administrator / Principal
- Staff Member / Teacher
- Identity Service (user account sync)
- Configuration Service (school validation)
- Section Service (consumes assignments)
- Reporting Service

## Assumptions
- Identity service can provision accounts asynchronously.
- Schools already provisioned via Configuration Service.
- Messaging infrastructure available (MassTransit).

## Constraints
- Search p95 <100ms.
- Bulk import 50 staff <60s.
- Certification expiration notifications 60 days prior.
- Tenant isolation on all staff data.

## Scenarios (Condensed)
1. Create staff member with role -> persist, publish StaffCreatedEvent, trigger identity account & welcome email.
2. Profile updates -> publish StaffProfileUpdatedEvent, audit before/after.
3. Team creation -> create team, notify members, event publication.
4. Multi-school assignment -> create StaffAssignment records with FTE percentages & context switching.
5. Role/permission management -> RBAC enforced (department chair privileges).
6. Staff search & filtering -> tenant-filtered, sort by last name.
7. Schedule & availability -> store structured availability, detect conflicts.
8. Certification tracking -> notification prior to expiration; status change on renewal.
9. Performance review workflow (initiate, store historical) – placeholder for future expansion.
10. Bulk import CSV -> validate; create; publish events; user accounts provisioned.
11. Staff directory listing -> privacy preferences respected.
12. Audit trail -> immutable tenant-isolated change log.

## Non-Functional Requirements
- Security: RBAC, FERPA compliance for staff-student context checks.
- Reliability: Idempotency window for staff creation prevents duplicates.
- Observability: Event publish metrics, schedule conflict detection logs.

## Acceptance Criteria Summary
All scenarios mapped to BDD; events consumed successfully; FTE multi-school assignment validated; certification warnings generated; import summary accurate.

## Out of Scope
- Real-time collaboration tools.
- Performance review analytics scoring.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Duplicate staff via import | Email uniqueness + idempotency cache |
| Credential monitoring failure | Scheduled job health checks + alerts |
| FTE total mismanagement | Validation ensuring total <=1.0 across assignments |
| Schedule conflict complexity | Incremental conflict detection algorithm first |

## Initial Roadmap
1. Scaffold service layers + Db schema.
2. Implement staff creation + assignment + events.
3. Add search & directory endpoints.
4. Implement teams & membership flows.
5. Add certification tracking job.
6. Bulk import + audit trail.

## Audit & Compliance
All create/update/delete operations log actor, timestamp, entity, before/after diff, reason (if provided).

---
Spec draft.