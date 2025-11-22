# Data Model: Assessment Service

**Feature ID**: 009-assessment-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **AssessmentDefinition**
  - Fields: `id`, `tenant_id`, `name`, `subject`, `grade_band`, `custom_fields (jsonb)`, `version`, `status` (Draft|Published|Archived), `created_at`, `published_at`, `template_id (optional)`.
  - Notes: version increments on breaking changes; archived definitions cannot be assigned.

- **Benchmark**
  - Fields: `id`, `assessment_definition_id`, `level`, `min_score`, `max_score`, `grading_scale_id`, `created_at`.
  - Notes: constraint prevents overlapping ranges per assessment definition.

- **AssessmentAssignment**
  - Fields: `id`, `tenant_id`, `assessment_definition_id`, `roster_id` (or `student_id`), `due_date`, `status` (Assigned|InProgress|Submitted|Closed), `created_at`.
  - Notes: uniqueness on (`assessment_definition_id`, `roster_id`, `status in active`) to enforce idempotency.

- **AssessmentResult**
  - Fields: `id`, `tenant_id`, `assignment_id`, `student_id`, `score`, `max_score`, `weighted_breakdown (jsonb)`, `benchmark_level`, `submitted_at`, `submitted_by`, `updated_at`.
  - Notes: audit triggers recorded for corrections; benchmark_level computed by domain logic.

- **AssessmentTemplateLibrary**
  - Fields: `id`, `tenant_id`, `name`, `payload (jsonb)`, `version`, `created_at`.

- **ImportJob**
  - Fields: `id`, `tenant_id`, `source` (StateTest|CSV), `status`, `total_rows`, `succeeded_rows`, `failed_rows`, `error_report_uri`, `started_at`, `completed_at`.

- **ExportJob**
  - Fields: `id`, `tenant_id`, `filter_payload (jsonb)`, `status`, `requested_by`, `file_uri`, `created_at`, `completed_at`.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor`, `before`, `after`, `created_at`.

## Events
- `AssessmentCreated`, `AssessmentAssigned`, `AssessmentResultRecorded`, `AssessmentResultCorrected`, `AssessmentImportCompleted`, `AssessmentImportFailed`, `AssessmentExportCompleted`.
- Payloads include `tenant_id`, `correlation_id`, `definition_version`, and benchmark metadata when applicable.

## Access Patterns
- Definitions by subject/grade: indexes on `(tenant_id, subject, grade_band, status)`.
- Results by student: `(tenant_id, student_id, submitted_at DESC)`.
- Trend queries: materialized view summarizing recent results per student/assessment with `trend` column.

## Validation Rules
- Benchmarks must fully cover score range with no overlaps.
- Assignments cannot be duplicated for the same roster/student in active status.
- Results must reference active assignment and use the current definition version unless explicitly allowing historical scoring.
