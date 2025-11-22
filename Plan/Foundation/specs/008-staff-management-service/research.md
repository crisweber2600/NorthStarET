# Research: Staff Management Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Use EF Core 9 + Npgsql with tenant_id and RLS**  
  - Rationale: aligns with multi-tenant architecture (feature 002); supports complex relations (assignments, availability) with constraints.  
  - Alternatives: Split DB per tenant (ops overhead), document DB (complex joins for schedules).

- **Event-first integration (StaffCreated/Updated, AssignmentChanged, CertificationExpiring)**  
  - Rationale: downstream services (Section, Assessment) rely on timely staff updates; eventing fits constitution Principle 4.  
  - Alternatives: synchronous callbacks to consumers (tight coupling).

- **Availability + assignment validation in domain layer**  
  - Rationale: enforce FTE caps and overlap detection before persistence to avoid bad state.  
  - Alternatives: DB-only constraints (harder to express multi-school overlap logic).

- **Imports via shared Data Import Service**  
  - Rationale: reuse templates/pipeline from feature 012; Staff service exposes schema + validation rules.  
  - Alternatives: bespoke importer (duplicated effort).

## Open Questions
1. Required identity provisioning flow: does Staff creation automatically trigger Identity user creation or just link when available? (Assume create identity when missing.)
2. Notification channels for certification expiry—email only or in-app alerts? Confirm template sources (Configuration service).
3. Do teams/PLCs require hierarchy (team within school) or cross-school memberships allowed? Specs imply cross-school allowed; confirm.
