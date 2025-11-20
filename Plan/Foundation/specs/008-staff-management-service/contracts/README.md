# Contracts: Staff Management Service
Layer: Foundation

## REST Endpoints (Draft)
- POST /api/v1/staff
- GET /api/v1/staff/{id}
- GET /api/v1/staff?role=&subject=&schoolId=&status=
- PUT /api/v1/staff/{id}
- DELETE /api/v1/staff/{id}
- POST /api/v1/staff/{id}/assignments
- GET /api/v1/staff/{id}/assignments
- POST /api/v1/teams
- GET /api/v1/teams?name=
- POST /api/v1/teams/{id}/members
- GET /api/v1/staff/directory
- POST /api/v1/staff/import
- POST /api/v1/staff/{id}/certifications
- GET /api/v1/staff/{id}/certifications

## Event Contracts (v1)
StaffCreatedEvent:
```json
{ "type":"StaffCreatedEvent", "version":1, "staffId":"uuid", "tenantId":"uuid", "email":"e@example.com", "role":"Teacher", "timestamp":"2025-11-20T00:00:00Z" }
```
StaffAssignedToSchoolEvent:
```json
{ "type":"StaffAssignedToSchoolEvent", "version":1, "assignmentId":"uuid", "staffId":"uuid", "schoolId":"uuid", "fte":0.3, "tenantId":"uuid", "timestamp":"2025-11-20T00:00:00Z" }
```
CertificationExpiringEvent:
```json
{ "type":"CertificationExpiringEvent", "version":1, "staffId":"uuid", "tenantId":"uuid", "certificationType":"Math Credential", "expirationDate":"2026-02-01", "daysUntilExpiration":60, "timestamp":"2025-12-03T00:00:00Z" }
```

---