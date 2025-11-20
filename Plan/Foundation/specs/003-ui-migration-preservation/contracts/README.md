# Contracts: UI Migration Preservation
Layer: Foundation

No new API endpoints; existing endpoints reused unchanged.

## Test Contract Snapshot
Baseline response schemas captured for critical endpoints:
- GET /api/v1/students
- GET /api/v1/students/{id}
- GET /api/v1/assessments/{id}

Snapshot tests ensure schema parity.

---