API Contract Notes (Digital Ink Service)

Endpoints:
POST /api/v1/ink/sessions { entityId, entityType }
PUT  /api/v1/ink/sessions/{id}/strokes { pageNumber, strokes[] }
POST /api/v1/ink/sessions/{id}/audio (multipart)
GET  /api/v1/ink/sessions/{id} (full session)
GET  /api/v1/ink/sessions/{id}/playback
POST /api/v1/ink/sessions/{id}/feedback { strokes[] }
GET  /api/v1/ink/sessions/{id}/export/llm

Security: SAS tokens for asset access; scopes ink.read / ink.write.
Errors: 413 payload too large (stroke batch), 409 concurrent modification guard.
