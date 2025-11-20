API Contract Notes (Section & Roster)

Representative Endpoints:
POST /api/v1/sections
POST /api/v1/sections/{id}/roster { studentId }
DELETE /api/v1/sections/{id}/roster/{studentId} ?effectiveDate=
POST /api/v1/sections/rollover { fromYear, toYear, dryRun }
GET  /api/v1/sections/search?gradeLevel=&subject=&period=&hasSeats=true
GET  /api/v1/students/{id}/schedule
GET  /api/v1/sections/{id}/export

Errors: 409 conflict (teacher/room); 422 validation.
Headers: X-Correlation-Id required.
