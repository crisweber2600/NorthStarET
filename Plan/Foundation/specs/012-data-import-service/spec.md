Feature: Data Import & Integration Service Migration (Foundation Layer)
Business Value: Reliable, asynchronous, auditable ingestion of heterogeneous external data (CSV, Excel, state test, SFTP) with transformation, validation, scheduling, rollback.

Scenario Summary:
1. CSV upload + schema validation (missing fields, types) pre-processing gate.
2. Excel multi-sheet mapping with transformations (merged cells, date serials).
3. Nightly SFTP state test import → StateTestDataImportedEvent.
4. Field mapping & transformation rules (configurable per template).
5. Large file error segregation report (failed CSV export + email summary).
6. Scheduled jobs (cron) + retry backoff, history tracking.
7. Import templates (reusable mappings + rules) tenant scoped.
8. Business validation rules (grade range, DOB, uniqueness).
9. Duplicate detection (exact, fuzzy) manual resolution.
10. Immutable audit log (who/what/when/source).
11. Rollback partial failure (ImportRollbackEvent coordination).
12. Real-time progress (processed/total, ETA, cancel capability).

NFRs: File upload p95 <2s (≤10MB); Processing 100 records/s target; Validation <5s for 1000 rows; Availability 99.9%; Security (malware scan, encryption, FERPA).
Idempotency: Job creation 10m; row processing keyed by natural IDs.
Events: ImportStartedEvent, ImportCompletedEvent, ImportFailedEvent, RowValidationFailedEvent, ImportRollbackEvent.
Risk: Large fuzzy duplicate sets → bounded Levenshtein comparisons, threshold tuning.
