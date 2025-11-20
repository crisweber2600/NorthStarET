# Spec: Configuration Service Migration

Short Name: configuration-service-migration
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Centralize multi-tenant district, school, calendar, settings, grading scales, compliance, custom attributes, and notification template management with hierarchical override (system → district → school) and audit trail.

## Business Value
Provides authoritative configuration source reducing duplication, enabling consistent cross-service behavior (scheduling, grading, compliance), and simplifying onboarding & customization.

## Target Layer
Foundation

## Actors
- System Administrator
- District Administrator
- School Administrator
- Downstream Services (Assessment, Section, Identity, Reporting)

## Assumptions
- Tenant established during district creation; Identity & Student services available.
- RLS & multi-tenant architecture baseline complete.
- Redis available for config caching.

## Constraints
- Read latency p95 <50ms (cached).
- Write operations invalidate cache consistently.
- Unique tenant_id per district.

## Scenarios (Condensed)
1. Create district settings -> persist, publish DistrictCreatedEvent, trigger downstream provisioning.
2. Configure academic calendar -> store; Section & Attendance consume.
3. Create school -> persist with tenant_id; publish SchoolCreatedEvent.
4. Configure grade levels & subjects -> saved; used by Assessment & Section.
5. Multi-tenant isolation -> only tenant settings visible; enforced by RLS.
6. Hierarchy resolution -> district override > system default; fallback chain.
7. Custom attributes -> extend student field set, tenant-scoped.
8. Grading scale configuration -> per grade range; consumed by Assessment/Reporting.
9. State-specific compliance -> enable state-specific blocks conditionally.
10. Role-based navigation customization -> dynamic menu rendering.
11. Notification/email templates -> stored with branding, merge fields.
12. Audit trail -> logs changes with before/after values.

## Non-Functional Requirements
- Security: Role-based authorization for modification; audit mandatory.
- Performance: Heavy read caching; cache TTL 1h + event-driven invalidation.
- Reliability: Hierarchy resolution deterministic.

## Acceptance Criteria Summary
All scenarios represented by API endpoints + events; hierarchy resolution tests green; cache correctness validated; audit queries show changes.

## Out of Scope
- UI design for advanced conditional rule editors.
- Multi-locale template translation (future phase).

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Stale cache after update | Event-driven invalidation + ETag versioning |
| Complex override logic errors | Unit tests for resolution matrix |
| Large custom attribute proliferation | Soft limit + monitoring |

## Initial Roadmap
1. Scaffold service + entities + repositories.
2. Implement district & school creation + events.
3. Add calendar & grading scale modelling.
4. Implement settings hierarchy resolution & caching.
5. Add custom attributes & template endpoints.
6. Implement audit logging & reporting.

## Audit & Compliance
All configuration writes store user_id, timestamp, setting_key, previous_value, new_value.

---
Spec draft.