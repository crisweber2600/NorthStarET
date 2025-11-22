# Data Model: Intervention Management Service

**Feature ID**: 010-intervention-management-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **Intervention**
  - Fields: `id`, `tenant_id`, `name`, `objective`, `owner_staff_id`, `schedule_template_id`, `start_date`, `end_date`, `status` (Draft|Active|Completed|Cancelled), `created_at`.
  - Notes: references template used to generate sessions.

- **InterventionTemplate**
  - Fields: `id`, `tenant_id`, `name`, `frequency` (Daily|Weekly|Custom), `duration_minutes`, `location_type`, `default_facilitator_role`, `progress_metric` (Likert|Percent), `version`.

- **InterventionSession**
  - Fields: `id`, `tenant_id`, `intervention_id`, `start_time`, `end_time`, `location`, `facilitator_id`, `status` (Scheduled|Completed|Cancelled).
  - Notes: unique constraint prevents overlapping sessions for facilitator/location within the same time range.

- **Enrollment**
  - Fields: `id`, `tenant_id`, `intervention_id`, `student_id`, `source` (Manual|Auto|RiskList), `status` (Active|Withdrawn|Exited), `enrolled_at`, `withdrawn_at`, `exit_reason`.
  - Notes: multiple enrollments per student allowed across different interventions; status drives attendance eligibility.

- **AttendanceRecord**
  - Fields: `id`, `tenant_id`, `session_id`, `student_id`, `status` (Present|Absent|Excused|Tardy), `notes`, `taken_by`, `recorded_at`.
  - Notes: unique per session/student; conflict detection ensures student not double-booked.

- **ProgressNote**
  - Fields: `id`, `tenant_id`, `intervention_id`, `student_id`, `note`, `rating` (nullable), `entered_by`, `entered_at`.

- **ExitCriteria**
  - Fields: `id`, `tenant_id`, `intervention_id`, `threshold_type` (Attendance|Progress|CustomMetric), `target_value`, `window`, `created_at`.

- **CommunicationLog**
  - Fields: `id`, `tenant_id`, `intervention_id`, `student_id`, `channel` (Email|SMS|Letter), `template_id`, `content_ref`, `status`, `sent_at`, `actor`.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor`, `before`, `after`, `created_at`.

## Events
- `InterventionCreated`, `InterventionEnrollmentChanged`, `InterventionSessionScheduled`, `AttendanceRecorded`, `ProgressUpdated`, `InterventionExited`, `CommunicationSent`.

## Access Patterns
- Session retrieval by facilitator/date: index on `(tenant_id, facilitator_id, start_time)`.
- Enrollment queries by student: `(tenant_id, student_id, status)`.
- Attendance rollups: materialized view summarizing attendance % per intervention and student.

## Validation Rules
- Sessions cannot overlap for the same facilitator or room; student cannot be scheduled into overlapping sessions within the same time block.
- Enrollment required before recording attendance or progress.
- Exit criteria evaluations run against rollups; exit recommendation logged before status changes.
