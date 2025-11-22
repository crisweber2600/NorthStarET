# Contracts: Legacy IdentityServer to Microsoft Entra ID Migration
Layer: Foundation

## Events
- `MigrationRunStarted` — emitted when a migration job begins; includes run id, tenant list, and mode (DryRun|Execute).
- `MigrationRunProgress` — periodic progress with metrics (users processed, matches, created links, suppressed records).
- `MigrationRunCompleted` — final summary with success counts, failures, reconciliation artifact location.
- `MigrationRunFailed` — terminal failure with reason and remediation hint.
- `ExternalProviderLinkCreated` — optional audit event per user when a new Entra link is created.

## Reports & Artifacts
- Reconciliation CSVs: matches, missing emails, role/tenant mismatches, disabled accounts.
- Audit trail references: link to identity audit records for `auth_deprecated_at` updates and link creations.

## Admin Run Arguments (reference)
- `--mode` (`DryRun|Execute|Rollback`)
- `--tenant-filter` (comma-separated list or `All`)
- `--report-path` (output directory for CSV/JSON reports)
- `--resume-run` (optional run id checkpoint)

All endpoints are internal to the migration tool; consumer integrations rely on the events above.
