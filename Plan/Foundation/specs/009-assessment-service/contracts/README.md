# Contracts: Assessment Service
Layer: Foundation

## REST APIs (proposed)
- `POST /api/assessments` — create assessment definition (versioned).
- `GET /api/assessments` — list/filter by subject, grade, status.
- `POST /api/assessments/{id}/assignments` — assign to roster/student list.
- `POST /api/assessments/{id}/benchmarks` — configure benchmark ranges.
- `POST /api/assessments/{assignmentId}/results` — record result for a student.
- `PATCH /api/assessments/{assignmentId}/results/{resultId}` — correct a score with audit.
- `POST /api/assessments/imports` — start import job; returns job id.
- `POST /api/assessments/exports` — request export; returns job id and eventual download URI.
- `GET /api/assessments/trends` — retrieve trend metrics for roster/student group.

## Events
- `AssessmentCreated`, `AssessmentAssigned`, `AssessmentResultRecorded`, `AssessmentResultCorrected`, `AssessmentImportCompleted`, `AssessmentImportFailed`, `AssessmentExportCompleted`.
- Headers include `tenant_id`, `correlation_id`, `definition_version`.

## Validation/Contracts
- Benchmark payload: list of `{ level, minScore, maxScore, gradingScaleId }` with non-overlapping ranges.
- Assignment payload includes roster/student ids and due date; idempotency key supported.
- Import schema columns: `student_id`, `assessment_code`, `score`, `max_score`, `date_taken`, `source`.

## Consumers
- Intervention service uses results/benchmarks for risk triggers.
- Reporting/Analytics consumes events for dashboards and longitudinal analysis.
