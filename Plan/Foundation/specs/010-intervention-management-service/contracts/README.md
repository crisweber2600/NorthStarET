API Contract Notes (Intervention Service)

Endpoints (representative):
POST /api/v1/interventions { name, tierLevel, focusArea, startDate, frequency, facilitatorId }
POST /api/v1/interventions/{id}/enroll { studentId }
POST /api/v1/interventions/{id}/sessions { sessionDate }
POST /api/v1/sessions/{id}/attendance { studentId, status }
POST /api/v1/interventions/{id}/notes { studentId, observation, progressRating }
GET  /api/v1/interventions/{id}/effectiveness

Errors: 409 conflict on schedule overlap; 422 validation detail.
Security: scopes intervention.read / intervention.write.
