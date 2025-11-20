# Contracts: Student Management Service
Layer: Foundation

## REST Endpoints (Draft)
- POST /api/v1/students
- GET /api/v1/students/{id}
- GET /api/v1/students?search=&grade=&status=
- POST /api/v1/students/{id}/enrollments
- POST /api/v1/students/import (multipart CSV)
- GET /api/v1/students/{id}/dashboard
- POST /api/v1/students/{id}/withdraw
- POST /api/v1/students/merge (body: primaryId, secondaryId)
- POST /api/v1/students/{id}/photo (multipart)
- GET /api/v1/students/export (generates CSV)

## Response Shape (Student)
```json
{
  "id": "uuid",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "2010-05-15",
  "gradeLevel": 5,
  "status": "Active",
  "photoUrl": "https://...",
  "enrollments": [ { "schoolId": "uuid", "gradeLevel": 5, "status": "Active" } ]
}
```

## Event Contract (StudentCreatedEvent v1)
```json
{
  "type": "StudentCreatedEvent",
  "version": 1,
  "studentId": "uuid",
  "tenantId": "uuid",
  "schoolId": "uuid",
  "gradeLevel": 5,
  "timestamp": "2025-11-20T12:00:00Z"
}
```

---