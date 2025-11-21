# Contracts: Data Import & Integration Service
Layer: Foundation

## REST APIs (proposed)
- `POST /api/import/templates` — create/update template with column mappings and validation rules.
- `GET /api/import/templates` — list templates by type/tenant.
- `POST /api/import/jobs` — upload file and create job for a template; supports SFTP references.
- `POST /api/import/jobs/{jobId}/start` — start or resume processing.
- `GET /api/import/jobs/{jobId}` — status, progress, metrics, links to error reports.
- `POST /api/import/jobs/{jobId}/rollback` — rollback a completed job within retention window.
- `POST /api/import/schedules` — create recurring schedule; supports pause/resume.
- `GET /api/import/jobs/{jobId}/errors` — download error report CSV.

## Events
- `ImportStarted`, `ImportProgressed`, `ImportCompleted`, `ImportFailed`, `ImportRollbackExecuted`, `StateTestDataImported`.
- Headers: `tenant_id`, `job_id`, `template_version`, `correlation_id`, `file_checksum`.

## Validation/Contracts
- Upload payload requires template id/version; file checksum stored for idempotency.
- Duplicate resolution operations available via `PATCH /api/import/jobs/{jobId}/duplicates/{rowId}` with resolution decision (Accept|Merge|Reject).
- Scheduled jobs pin template version at schedule creation; warning emitted if new version exists.

## Consumers
- Student, Staff, Assessment services subscribe to relevant import completion events to trigger downstream reconciliation.
- Operations dashboard consumes progress events for monitoring and alerting.
