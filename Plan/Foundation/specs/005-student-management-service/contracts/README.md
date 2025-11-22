# Contracts: Student Management Service
Layer: Foundation

## REST APIs (proposed)
- `POST /api/students` — create student profile.
- `GET /api/students?search=...` — search with tenant scoping and privacy filters.
- `GET /api/students/{id}` — retrieve student with enrollments and photo metadata.
- `PUT /api/students/{id}` / `PATCH ...` — update demographics/flags.
- `POST /api/students/{id}/enrollments` — enroll student with effective dates.
- `POST /api/students/{id}/photos` — upload photo metadata; returns blob upload URL.
- `POST /api/students/{id}/merge` — propose/confirm merge; returns merged id.
- `POST /api/students/import` — start CSV import job; returns job id.

## Events
- `StudentCreated`, `StudentUpdated`, `StudentEnrolled`, `StudentWithdrawn`, `StudentMerged`, `StudentPhotoUploaded`, `StudentImported`.
- Headers: `tenant_id`, `correlation_id`, `causation_id`.

## Validation/Contracts
- Tenant context derived from gateway headers; all APIs require authenticated session.
- Import contract: CSV/Excel columns (student_number, first_name, last_name, dob, email, school_id, grade_level, status).

## Consumers
- Assessment, Section, Reporting, Intervention services subscribe to student lifecycle events for roster alignment and analytics.
