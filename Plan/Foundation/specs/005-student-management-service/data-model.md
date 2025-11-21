# Data Model: Student Management Service

**Feature ID**: 005-student-management-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **Student**
  - Fields: `id (uuid)`, `tenant_id`, `student_number`, `state_id`, `first_name`, `last_name`, `birthdate`, `email`, `phone`, `status`, `privacy_flags`, `created_at`, `updated_at`.
  - Notes: `student_number` unique per tenant; supports soft flags for FERPA/privacy.

- **Enrollment**
  - Fields: `id`, `student_id`, `tenant_id`, `school_id`, `grade_level`, `start_date`, `end_date`, `status`, `source` (Manual|Import|Auto), `audit`.
  - Notes: historical rows preserved; overlaps blocked by constraint on active range.

- **StudentPhoto**
  - Fields: `id`, `student_id`, `tenant_id`, `blob_uri`, `content_type`, `hash`, `uploaded_at`, `uploaded_by`.
  - Notes: blob_uri points to tenant-prefixed path; hash used for duplicate detection.

- **StudentMergeCandidate**
  - Fields: `id`, `tenant_id`, `primary_student_id`, `duplicate_student_id`, `reason`, `status` (Proposed|Approved|Rejected|Completed), `created_at`, `resolved_at`.
  - Notes: records merge workflow; upon completion, relationships re-point to primary student.

- **DashboardAggregate (read model)**
  - Fields: `tenant_id`, `student_id`, `attendance_pct`, `assessment_trend`, `latest_intervention_status`, `refreshed_at`.
  - Notes: populated from downstream events; cached in Redis for p95 <200ms dashboard calls.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor_id`, `actor_role`, `payload_before`, `payload_after`, `created_at`.
  - Notes: immutable; used to satisfy FERPA and constitutional audit requirements.

## Events
- `StudentCreated`, `StudentUpdated`, `StudentEnrolled`, `StudentWithdrawn`, `StudentMerged`, `StudentPhotoUploaded`, `StudentImported`.
- All events include `tenant_id`, `correlation_id`, `actor`, and versioned payload schema.

## Access Patterns
- **Search**: composite index `(tenant_id, last_name, first_name)` plus trigram for name search.
- **Enrollment lookups**: `(tenant_id, student_id, status, start_date)` to efficiently fetch active enrollments.
- **Dashboard**: Redis key `dashboard:{tenant}:{studentId}` for cached aggregates with TTL + event-based invalidation.

## Validation Rules
- Student uniqueness: `(tenant_id, student_number)` unique; state_id unique when provided.
- Enrollment overlap prevented for the same `student_id` + `school_id` when status is Active.
- Merge operations require non-overlapping enrollments after consolidation and must preserve audit history.
