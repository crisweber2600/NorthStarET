API Contract Notes (Data Import Service)

Endpoints:
POST /api/v1/imports (multipart upload)
POST /api/v1/imports/{id}/start
GET  /api/v1/imports/{id}
GET  /api/v1/imports/{id}/errors
POST /api/v1/imports/{id}/rollback
POST /api/v1/imports/{id}/cancel
POST /api/v1/imports/{id}/resolve-duplicate
GET  /api/v1/imports/history?status=&entity=&page=
POST /api/v1/imports/templates
GET  /api/v1/imports/templates
GET  /api/v1/imports/templates/{id}
PUT  /api/v1/imports/templates/{id}
DELETE /api/v1/imports/templates/{id}
POST /api/v1/imports/validate (dry-run)
POST /api/v1/validation-rules

Real-time progress via SignalR (future).
Problem+json error format.
