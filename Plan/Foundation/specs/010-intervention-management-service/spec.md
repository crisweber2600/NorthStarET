# Feature Specification: Intervention Management Service Migration

**Specification Branch**: `Foundation/010-intervention-management-service-migration-spec`  
**Implementation Branch**: `Foundation/010-intervention-management-service-migration`  
**Created**: 2025-11-20  
**Status**: Draft  
**Input**: User description: "Migrate MTSS/RTI intervention management (groups, enrollment, scheduling, attendance, progress, templates, communications, audit) to a dedicated service with tenant isolation."

---

## Layer Identification (MANDATORY)

**Target Layer**: Foundation

**Layer Validation Checklist**:
- [x] Layer explicitly identified (Foundation)
- [x] Layer exists in mono-repo structure (`Plan/Foundation/` and `Src/Foundation/`)
- [x] If new layer: Architecture Review documented in `Plan/{LayerName}/README.md` (Not a new layer)
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (ServiceDefaults for messaging, shared Domain/Application patterns).  
**Justification**: Intervention workflows rely on tenant-scoped data and standard messaging patterns; no cross-layer service dependencies outside shared infrastructure.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create interventions and enroll students (Priority: P1)

Educators create intervention groups or individual plans, schedule sessions, and enroll students manually or via risk lists.

**Why this priority**: Establishes the core entity and enrollment needed for any MTSS workflow.  
**Independent Test**: Create an intervention with schedule, enroll students (manual and auto), and verify events and visibility for participants.

**Acceptance Scenarios**:

1. Given a new intervention definition with schedule, when saved, then sessions are generated, the intervention is persisted, and an InterventionCreated notification is emitted.
2. Given a risk list feed, when auto-enrollment criteria are met, then eligible students are enrolled with audit history and an enrollment event is published.
3. Given missing required fields (objective, owner, schedule), when creating an intervention, then the request is rejected with validation errors and no sessions are produced.

---

### User Story 2 - Record attendance, notes, and progress (Priority: P2)

Facilitators capture session attendance, notes, and progress ratings with conflict detection and audit.

**Why this priority**: Provides day-to-day execution evidence and compliance posture.  
**Independent Test**: Record attendance for a session, add progress notes, detect scheduling conflicts, and confirm audit entries and downstream notifications.

**Acceptance Scenarios**:

1. Given an active session, when attendance is recorded for all participants, then present/absent statuses and notes are stored and an attendance event is sent.
2. Given overlapping sessions for a facilitator or room, when saving attendance, then the system flags the conflict and prevents double booking.
3. Given a progress note update, when saved, then the note persists with rating, actor, and timestamp and is included in the audit trail.

---

### User Story 3 - Evaluate effectiveness and communicate outcomes (Priority: P3)

Educators monitor dashboards (attendance %, progress trajectory, exit criteria) and send communications to caregivers with auditability.

**Why this priority**: Demonstrates impact and keeps stakeholders informed.  
**Independent Test**: View effectiveness metrics for a group, trigger exit upon meeting criteria, and send a caregiver summary while confirming delivery and audit logging.

**Acceptance Scenarios**:

1. Given configured exit criteria, when a student meets the threshold, then a recommended exit status is generated and an exit event is recorded upon confirmation.
2. Given a caregiver communication, when sent, then the message uses the selected template, records delivery status, and attaches to the student's communication log.
3. Given a dashboard request, when data is aggregated, then metrics (attendance rate, progress trend) return within target latency and reflect latest attendance/results.

### Edge Cases

- What happens when a student is withdrawn mid-cycle? -> Enrollment is ended with effective date, future sessions are canceled, and history remains intact.  
- How does the system handle facilitator reassignment during an active schedule? -> Upcoming sessions update to the new facilitator while past attendance remains unchanged.  
- What if an intervention template is updated while active interventions are running? -> Existing interventions retain their original configuration unless explicitly re-applied with version acknowledgment.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support creation and update of interventions with objectives, target population, schedule templates, and ownership.
- **FR-002**: System MUST allow manual and rules-based enrollment of students with audit of enrollment source and timestamp.
- **FR-003**: System MUST generate session schedules from templates and allow adjustments with conflict detection for rooms, facilitators, and students.
- **FR-004**: System MUST capture attendance with statuses, notes, and optional reason codes and publish attendance events.
- **FR-005**: System MUST support progress notes and ratings per student with chronological history.
- **FR-006**: System MUST define and evaluate exit criteria, recommending exits and tracking confirmations.
- **FR-007**: System MUST provide dashboards summarizing attendance rates, progress trends, and projected outcomes for a group or student.
- **FR-008**: System MUST support intervention templates for common programs with versioning.
- **FR-009**: System MUST emit events for creation, enrollment, attendance, progress updates, and exits with retry for transient failures.
- **FR-010**: System MUST enforce tenant isolation and role-based permissions for all intervention operations.
- **FR-011**: System MUST maintain immutable audit logs for create/update/delete actions and communications sent.
- **FR-012**: System MUST allow caregiver communications using approved templates with delivery status tracking.

### Key Entities

- **Intervention**: Defined program with objectives, schedule, facilitator, and targeted student population.
- **Intervention Session**: Scheduled occurrence linked to an intervention, including time, location, and facilitator.
- **Enrollment**: Association between a student and an intervention with status, source, and effective dates.
- **Attendance Record**: Session-level attendance status with notes and reason codes.
- **Progress Note**: Narrative and rating capturing student progress at a point in time.
- **Intervention Template**: Reusable configuration for common intervention types with versioning.
- **Communication Log**: Record of caregiver communications with content reference and delivery status.
- **Audit Record**: Immutable record of changes and communications with actor and timestamps.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of intervention create and enrollment operations complete in under 100ms.
- **SC-002**: 99% of attendance submissions are processed within 2 seconds and generate corresponding events.
- **SC-003**: Scheduling conflict detection identifies >99% of overlapping facilitator or room bookings before publication.
- **SC-004**: Dashboards return attendance and progress metrics within 200ms for 95% of requests using current data.
- **SC-005**: 100% of caregiver communications and exit actions are logged with audit entries and delivery status where applicable.
