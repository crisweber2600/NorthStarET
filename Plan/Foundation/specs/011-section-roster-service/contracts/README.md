# Contracts: Section & Roster Service
Layer: Foundation

## REST APIs (proposed)
- `POST /api/sections` — create section with capacity/period/room.
- `GET /api/sections` — search/filter by school, period, subject, available seats.
- `POST /api/sections/{id}/teachers` — assign teachers/co-teachers.
- `POST /api/sections/{id}/roster` — add students; idempotent on student_id.
- `DELETE /api/sections/{id}/roster/{rosterEntryId}` — drop student (effective end).
- `POST /api/sections/{id}/waitlist` — join waitlist; FIFO enforced.
- `POST /api/sections/{id}/rollover` — start rollover (dry-run or execute) for next year.
- `GET /api/sections/{id}/export` — export roster (CSV/PDF) with audit.

## Events
- `SectionCreated`, `TeacherAssigned`, `StudentAddedToRoster`, `StudentDroppedFromRoster`, `CapacityReached`, `WaitlistPromoted`, `RolloverCompleted`.
- Attendance integration: `AttendanceRecordedEvent` consumed to validate roster membership.

## Validation/Contracts
- Payloads require tenant context; roster operations validate capacity and conflicts.
- Rollover payload includes `sourceYear`, `targetYear`, optional filters; dry-run required before execute.
- Waitlist promotions emit `WaitlistPromoted` with previous and new positions.

## Consumers
- Attendance service and Gradebook consume roster events for validation and feeds.
- Reporting/Analytics consumes RolloverCompleted for longitudinal tracking.
