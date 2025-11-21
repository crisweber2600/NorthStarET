# Feature Specification: Staff Management Service Migration

**Specification Branch**: `Foundation/008-staff-management-service-migration-spec`  
**Implementation Branch**: `Foundation/008-staff-management-service-migration`  
**Created**: 2025-11-20  
**Status**: Draft  
**Input**: User description: "Migrate staff management (profiles, assignments, teams, certifications, schedules, search, import, audit) to a dedicated service with multi-school assignments and event-driven integration."

---

## Layer Identification (MANDATORY)

**Target Layer**: Foundation

**Layer Validation Checklist**:
- [x] Layer explicitly identified (Foundation)
- [x] Layer exists in mono-repo structure (`Plan/Foundation/` and `Src/Foundation/`)
- [x] If new layer: Architecture Review documented in `Plan/{LayerName}/README.md` (Not a new layer)
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (ServiceDefaults for messaging, shared Domain/Application patterns).  
**Justification**: Staff data is foundational and needs only shared messaging and persistence patterns to integrate with adjacent services (Identity, Section, Configuration).

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and maintain staff profiles with identity linkage (Priority: P1)

District or school admins create staff profiles, link to identity accounts, and manage role/permission metadata with full audit.

**Why this priority**: Core CRUD flow that unlocks all downstream assignments and notifications.  
**Independent Test**: Create a staff member, verify identity linkage event, update profile fields, and confirm audit history and search visibility.

**Acceptance Scenarios**:

1. Given a new staff profile, when created with required fields (name, contact, role), then the profile is saved, an identity provisioning event is published, and the staff appears in search results.
2. Given an existing profile, when key demographic data is updated, then the change is audited with before/after values and downstream services receive the update notification.
3. Given missing mandatory fields, when attempting creation, then the request is rejected with validation errors and no partial record is stored.

---

### User Story 2 - Manage multi-school assignments and schedules (Priority: P2)

Administrators assign staff to multiple schools with FTE percentages, schedules, and availability while preventing conflicts.

**Why this priority**: Enables accurate rostering, scheduling, and resource allocation across campuses.  
**Independent Test**: Assign a staff member to two schools, set FTE totals, define availability blocks, and verify conflict detection and context switching.

**Acceptance Scenarios**:

1. Given two assignments that exceed 1.0 total FTE, when saving, then the system blocks the change and reports the over-allocation.
2. Given overlapping availability blocks for the same staff member, when saving the schedule, then the system flags the conflict and prevents publishing.
3. Given valid assignments, when switching context between schools, then permissions and visibility align to the selected school and are audited.

---

### User Story 3 - Track certifications, teams, and bulk import (Priority: P3)

Admins manage certifications with expiry alerts, organize teams/PLCs, and import staff data in bulk with validation and audit.

**Why this priority**: Reduces manual work and ensures compliance readiness.  
**Independent Test**: Create a certification with expiry, enroll staff to a team, run a CSV import for multiple staff, and verify alerts, team membership, and audit coverage.

**Acceptance Scenarios**:

1. Given a certification nearing expiration, when the reminder window is reached, then a notification is issued and the certification status is updated upon renewal.
2. Given a CSV import with duplicate emails, when processing, then duplicates are rejected with reasons while valid records are ingested and audited.
3. Given team membership updates, when a staff member is added or removed, then team rosters update and downstream consumers receive the event.

### Edge Cases

- What happens when identity provisioning fails after staff creation? -> Staff profile remains in pending-identity status with retry and manual resolution queue.  
- How does the system handle staff privacy preferences? -> Directory visibility honors opt-out flags; sensitive fields are masked in search results.  
- What if assignment history needs correction? -> Historical assignments remain immutable while new correction entries supersede prior records with clear effective dates.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support creation, update, and archival of staff profiles with validation of required fields.
- **FR-002**: System MUST emit identity provisioning events on creation and profile update events on subsequent changes.
- **FR-003**: System MUST enforce tenant isolation for all staff data access and modifications.
- **FR-004**: System MUST allow multiple school assignments per staff member with cumulative FTE validation not exceeding 1.0.
- **FR-005**: System MUST manage availability schedules and detect overlaps or conflicts before activation.
- **FR-006**: System MUST support team creation and membership management with notifications when membership changes.
- **FR-007**: System MUST track certifications with effective dates, expirations, and configurable reminder windows.
- **FR-008**: System MUST provide searchable directory and filtering by name, role, school, and certification status.
- **FR-009**: System MUST enforce role-based permissions so only authorized admins can create or modify staff and assignments.
- **FR-010**: System MUST support bulk import of staff data with validation, error reporting, and partial failure handling.
- **FR-011**: System MUST generate immutable audit records for create/update/delete actions including actor, timestamp, and before/after values.
- **FR-012**: System MUST provide status telemetry for integration events (published/failed) and allow retries for transient failures.

### Key Entities

- **Staff Profile**: Core identity and demographic information for a staff member, including roles and contact details.
- **Staff Assignment**: Association between a staff member and a school with FTE percentage, role, and effective dates.
- **Availability Schedule**: Time blocks indicating when staff are available or unavailable for scheduling.
- **Team/PLC**: Grouping of staff for collaboration and scheduling, with membership history.
- **Certification**: Credential record with issuing body, issue date, expiry, and reminders.
- **Import Job**: Bulk import request with status, validation outcomes, and per-row error reports.
- **Audit Record**: Immutable log of operations with actor, timestamp, entity, and before/after snapshot.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of staff search queries return results in under 100ms and respect tenant and privacy filters.
- **SC-002**: 100% of staff create/update/delete actions produce audit entries with actor and before/after details.
- **SC-003**: 99% of identity provisioning events are published within 30 seconds of staff creation.
- **SC-004**: 99% of certification reminders are sent at least 60 days before expiration with no missing recipients.
- **SC-005**: Bulk import processes at least 50 staff records per minute with an error rate below 2% for valid files.
