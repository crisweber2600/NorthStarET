# Contracts: Configuration Service
Layer: Foundation

## REST Endpoints (Draft)
- POST /api/v1/districts
- GET /api/v1/districts/{id}
- POST /api/v1/schools
- GET /api/v1/schools/{id}
- GET /api/v1/districts/{id}/schools
- GET /api/v1/calendars?academicYear=2025
- PUT /api/v1/calendars/{id}
- GET /api/v1/settings
- PUT /api/v1/settings/{key}
- GET /api/v1/grading-scales
- POST /api/v1/grading-scales
- GET /api/v1/custom-attributes?entityType=STUDENT
- POST /api/v1/custom-attributes
- GET /api/v1/templates
- POST /api/v1/templates

## Event Contracts (v1)
DistrictCreatedEvent:
```json
{ "type": "DistrictCreatedEvent", "version":1, "tenantId":"uuid", "districtId":"uuid", "name":"Lincoln USD", "timestamp":"2025-11-20T00:00:00Z" }
```
ConfigurationChangedEvent:
```json
{ "type":"ConfigurationChangedEvent", "version":1, "tenantId":"uuid", "scope":"DISTRICT", "key":"grading.scale.K5", "timestamp":"2025-11-20T00:00:00Z" }
```

---