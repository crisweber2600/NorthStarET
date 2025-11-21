# Data Model: Data Import & Integration Service

**Feature ID**: 012-data-import-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **ImportTemplate**
  - Fields: `id`, `tenant_id`, `name`, `type` (Student|Staff|StateTest|Custom), `version`, `column_mappings (jsonb)`, `validation_rules (jsonb)`, `created_at`, `published_at`.
  - Notes: version pinned on scheduled jobs; schema drift detected before runs.

- **ImportJob**
  - Fields: `id`, `tenant_id`, `template_id`, `source` (Upload|SFTP|API), `status` (Created|Validating|Processing|Completed|Failed|RolledBack), `processed_rows`, `total_rows`, `duplicate_count`, `error_count`, `started_at`, `completed_at`.
  - Notes: `resume_token` stored for resumable processing.

- **FileArtifact**
  - Fields: `id`, `tenant_id`, `job_id`, `uri`, `checksum`, `size_bytes`, `content_type`, `source_reference`, `uploaded_at`.
  - Notes: references blob storage location; checksum used to detect replays.

- **RowResult**
  - Fields: `id`, `tenant_id`, `job_id`, `row_number`, `status` (Valid|Failed|Duplicate|Skipped), `error_codes (jsonb)`, `resolved_value (jsonb)`, `processed_at`.

- **DuplicateCandidate**
  - Fields: `id`, `tenant_id`, `job_id`, `row_number`, `resolution` (Pending|Accept|Merge|Reject), `resolved_at`, `resolved_by`.

- **Schedule**
  - Fields: `id`, `tenant_id`, `template_id`, `cron_expression`, `timezone`, `last_run_at`, `next_run_at`, `status` (Active|Paused), `sftp_source (jsonb)`.

- **ErrorReport**
  - Fields: `id`, `tenant_id`, `job_id`, `report_uri`, `generated_at`.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor`, `before`, `after`, `created_at`.

## Events
- `ImportStarted`, `ImportProgressed`, `ImportCompleted`, `ImportFailed`, `ImportRollbackExecuted`, `StateTestDataImported`.
- All events include `tenant_id`, `job_id`, `template_version`, `correlation_id`, `file_checksum`.

## Access Patterns
- Job status: index `(tenant_id, status, started_at DESC)` for dashboards.
- RowResult lookups by job and status for error export generation.
- Schedule polling by `next_run_at` to trigger due jobs.

## Validation Rules
- Template version must be immutable for a running job; new version requires new job.
- Duplicate detection keys configured per template; resolution required before finalizing job.
- Rollback allowed only within retention window and when dependent services expose compensating actions.
