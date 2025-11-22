# Data Model: Section & Roster Service

**Feature ID**: 011-section-roster-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **Section**
  - Fields: `id`, `tenant_id`, `school_id`, `course_code`, `name`, `period_id`, `room`, `capacity`, `status` (Active|Inactive|Archived), `co_teaching_enabled`, `created_at`.
  - Notes: `period_id` references configuration/period catalog; capacity used for waitlist logic.

- **TeacherAssignment**
  - Fields: `id`, `tenant_id`, `section_id`, `staff_id`, `role` (Primary|CoTeacher), `start_date`, `end_date`.
  - Notes: multiple active co-teachers allowed when `co_teaching_enabled = true`.

- **RosterEntry**
  - Fields: `id`, `tenant_id`, `section_id`, `student_id`, `status` (Active|Waitlisted|Dropped), `effective_start`, `effective_end`, `added_by`, `added_at`.
  - Notes: unique constraint prevents duplicate active entries for a student/section; history preserved.

- **WaitlistEntry**
  - Fields: `id`, `tenant_id`, `section_id`, `student_id`, `position`, `joined_at`.
  - Notes: FIFO ordering; auto-fill promotes position 1 into roster on seat open.

- **RolloverTemplate**
  - Fields: `id`, `tenant_id`, `source_year`, `target_year`, `filters (jsonb)`, `created_at`.
  - Notes: used to generate next-year sections; ties to `RolloverJob`.

- **RolloverJob**
  - Fields: `id`, `tenant_id`, `template_id`, `status` (Draft|DryRun|Executing|Completed|Failed), `created_at`, `completed_at`, `summary_uri`.

- **AttendanceSubscription**
  - Fields: `id`, `tenant_id`, `section_id`, `attendance_service_key`, `status`.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor`, `before`, `after`, `created_at`.

## Events
- `SectionCreated`, `StudentAddedToRoster`, `StudentDroppedFromRoster`, `CapacityReached`, `WaitlistPromoted`, `RolloverCompleted`, `TeacherAssigned`, `TeacherUnassigned`.

## Access Patterns
- Section search: index `(tenant_id, school_id, period_id, status, capacity)`.
- Roster lookups: `(tenant_id, section_id, status, effective_start)` for active rosters.
- Waitlist ordering: `(tenant_id, section_id, position)` to fetch next promotion candidate.

## Validation Rules
- Capacity must be >= active roster count; auto-fill triggered when `capacity > active_count` and waitlist not empty.
- Period conflicts checked using cached schedule map keyed by `teacher_id` and `student_id`.
- Rollover dry-run must complete without conflicts before execution is allowed.
