API Contract Notes (Data Import Service)

Endpoints:
POST /api/v1/imports (multipart upload)
POST /api/v1/imports/{id}/start
GET  /api/v1/imports/{id}
GET  /api/v1/imports/{id}/errors
POST /api/v1/imports/{id}/rollback
GET  /api/v1/imports/history?status=&entity=&page=
POST /api/v1/imports/templates
GET  /api/v1/imports/templates
POST /api/v1/imports/validate (dry-run)

Real-time progress via SignalR (future).
Problem+json error format.
