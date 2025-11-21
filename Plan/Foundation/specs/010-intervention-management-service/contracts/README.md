# Contracts: Intervention Management Service
Layer: Foundation

## REST APIs (proposed)
- `POST /api/interventions/templates` — create/update templates.
- `POST /api/interventions` — create intervention (uses template to generate sessions).
- `GET /api/interventions` — filter by status, owner, school.
- `POST /api/interventions/{id}/enrollments` — enroll students (manual/auto).
- `DELETE /api/interventions/{id}/enrollments/{enrollmentId}` — withdraw/exit.
- `POST /api/interventions/sessions/{sessionId}/attendance` — record attendance per student.
- `POST /api/interventions/{id}/progress` — add progress note/rating.
- `POST /api/interventions/{id}/communications` — send caregiver communication using template reference.
- `POST /api/interventions/{id}/exit` — confirm exit once criteria met.

## Events
- `InterventionCreated`, `InterventionEnrollmentChanged`, `InterventionSessionScheduled`, `AttendanceRecorded`, `ProgressUpdated`, `InterventionExited`, `CommunicationSent`.
- Headers: `tenant_id`, `correlation_id`, `actor`.

## Validation/Contracts
- Enrollment payload requires `studentIds` and source; idempotency key supported for auto-enrollment feeds.
- Attendance payload requires `sessionId`, `studentId`, `status`, optional `notes`.
- Communication payload references template id from Configuration service; audit tracked in `CommunicationLog`.

## Consumers
- Reporting/Analytics uses events for MTSS dashboards.
- Assessment and Section services may react to enrollment/attendance to adjust rosters or interventions triggers.
