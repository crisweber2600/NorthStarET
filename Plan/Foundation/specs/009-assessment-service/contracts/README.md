API Contract Notes (Assessment Service)

Versioning: /api/v1; backward compatible changes only in minor versions; destructive changes require v2.
Error Model: RFC 7807 problem+json (type, title, status, traceId, errors[]).
Correlation: Require X-Correlation-Id echo in response headers.
Pagination: search endpoints use ?page=1&pageSize=50 with X-Total-Count header.
Security: JWT with tenant claim; scope assessment.read / assessment.write.

Representative Endpoints:
POST /api/v1/assessments { name, subject, fields[] }
POST /api/v1/assessments/{id}/assign { studentIds[], dueDate }
POST /api/v1/assessments/{id}/results { assignmentId, fieldValues[] }
GET  /api/v1/students/{id}/trends
POST /api/v1/assessments/import/state-test { fileReference, format }
