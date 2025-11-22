# API Contract Specifications

## Overview

This document defines the API contracts for all NorthStar microservices, ensuring consistency, versioning, and interoperability across the platform.

**API Style**: RESTful HTTP/JSON  
**API Gateway**: YARP (Yet Another Reverse Proxy)  
**Authentication**: JWT Bearer Tokens (from Identity Service)  
**Versioning Strategy**: URI-based (`/api/v1/`, `/api/v2/`)

---

## Global API Standards

### Base URL Structure

```
https://{gateway-host}/api/v{version}/{service}/{resource}
```

**Examples**:
- `https://northstar.example.com/api/v1/students/12345`
- `https://northstar.example.com/api/v1/assessments/search`
- `https://northstar.example.com/api/v2/interventions/groups`

### HTTP Methods

| Method | Usage | Idempotent | Safe |
|--------|-------|------------|------|
| GET | Retrieve resource(s) | ✅ | ✅ |
| POST | Create resource | ❌ | ❌ |
| PUT | Full update (replace) | ✅ | ❌ |
| PATCH | Partial update | ❌ | ❌ |
| DELETE | Remove resource | ✅ | ❌ |

### Standard Response Codes

| Code | Meaning | When to Use |
|------|---------|-------------|
| 200 OK | Success | GET, PUT, PATCH successful |
| 201 Created | Resource created | POST successful |
| 204 No Content | Success, no body | DELETE successful |
| 400 Bad Request | Invalid input | Validation errors |
| 401 Unauthorized | Not authenticated | Missing/invalid JWT |
| 403 Forbidden | Not authorized | Valid JWT, insufficient permissions |
| 404 Not Found | Resource doesn't exist | GET/PUT/DELETE on non-existent ID |
| 409 Conflict | Resource conflict | Duplicate, concurrency issue |
| 422 Unprocessable Entity | Business rule violation | Valid format, invalid business logic |
| 429 Too Many Requests | Rate limit exceeded | Throttling |
| 500 Internal Server Error | Server error | Unhandled exception |
| 503 Service Unavailable | Service down | Maintenance, dependency failure |

### Standard Headers

**Request Headers**:
```http
Authorization: Bearer {jwt-token}
Content-Type: application/json
Accept: application/json
X-Correlation-Id: {guid}  # For distributed tracing
X-Request-Id: {guid}      # For request tracking
```

**Response Headers**:
```http
Content-Type: application/json
X-Correlation-Id: {guid}  # Echo from request
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1699999999
Link: <https://...>; rel="next"  # For pagination
```

### Error Response Format

```json
{
  "type": "https://northstar.example.com/errors/validation-error",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/students",
  "traceId": "00-abc123-def456-00",
  "errors": {
    "firstName": ["First name is required."],
    "dateOfBirth": ["Must be a past date."]
  }
}
```

### Pagination Format

**Request**:
```http
GET /api/v1/students?page=2&pageSize=20&sortBy=lastName&sortOrder=asc
```

**Response**:
```json
{
  "items": [...],
  "pagination": {
    "page": 2,
    "pageSize": 20,
    "totalCount": 1543,
    "totalPages": 78,
    "hasNext": true,
    "hasPrevious": true
  },
  "links": {
    "self": "/api/v1/students?page=2&pageSize=20",
    "first": "/api/v1/students?page=1&pageSize=20",
    "last": "/api/v1/students?page=78&pageSize=20",
    "next": "/api/v1/students?page=3&pageSize=20",
    "previous": "/api/v1/students?page=1&pageSize=20"
  }
}
```

---

## Service API Contracts

### 1. Identity & Authentication Service

**Base Path**: `/api/v1/auth`, `/api/v1/users`

#### POST /api/v1/auth/login

**Description**: Authenticate user with email/password or external provider

**Request**:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "provider": "local"  // or "entraid"
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "def50200...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Teacher", "Admin"]
  }
}
```

**Errors**:
- 400: Invalid email format
- 401: Invalid credentials
- 429: Too many login attempts (rate limited)

#### POST /api/v1/auth/refresh

**Description**: Refresh access token using refresh token

**Request**:
```json
{
  "refreshToken": "def50200..."
}
```

**Response** (200 OK):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "ghi78900...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

#### POST /api/v1/users

**Description**: Register new user (admin only)

**Request**:
```json
{
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "SecurePassword123!",
  "roles": ["Teacher"]
}
```

**Response** (201 Created):
```json
{
  "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "roles": ["Teacher"],
  "createdAt": "2025-11-15T10:30:00Z"
}
```

**Authorization**: Requires `Admin` role

---

### 2. Student Management Service

**Base Path**: `/api/v1/students`

#### GET /api/v1/students

**Description**: Search and list students with filtering

**Query Parameters**:
- `page` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `search` (string) - Search first/last name
- `gradeLevel` (int) - Filter by grade
- `schoolId` (guid) - Filter by school
- `sortBy` (string) - Field to sort by
- `sortOrder` (string) - `asc` or `desc`

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
      "firstName": "Emma",
      "lastName": "Johnson",
      "stateStudentId": "12345678",
      "dateOfBirth": "2010-05-15",
      "gradeLevel": 8,
      "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
      "schoolName": "Lincoln Middle School",
      "enrollmentStatus": "Active"
    }
  ],
  "pagination": { ... }
}
```

**Authorization**: Requires `Teacher`, `Admin`, or `Staff` role

#### GET /api/v1/students/{id}

**Description**: Get single student by ID

**Response** (200 OK):
```json
{
  "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "firstName": "Emma",
  "lastName": "Johnson",
  "middleName": "Marie",
  "stateStudentId": "12345678",
  "dateOfBirth": "2010-05-15",
  "gender": "Female",
  "gradeLevel": 8,
  "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
  "schoolName": "Lincoln Middle School",
  "districtId": "e5f6a7b8-c9d0-1234-ef12-345678901234",
  "enrollmentStatus": "Active",
  "enrollmentDate": "2020-08-15",
  "demographics": {
    "race": ["White", "Hispanic"],
    "ethnicity": "Hispanic",
    "languagePreference": "English",
    "freeReducedLunch": true
  },
  "contacts": [
    {
      "id": "f6a7b8c9-d0e1-2345-f123-456789012345",
      "type": "Guardian",
      "firstName": "Sarah",
      "lastName": "Johnson",
      "relationship": "Mother",
      "phone": "555-123-4567",
      "email": "sarah.johnson@example.com",
      "isPrimary": true
    }
  ],
  "createdAt": "2020-08-15T08:00:00Z",
  "updatedAt": "2025-09-01T14:30:00Z"
}
```

**Errors**:
- 404: Student not found
- 403: Insufficient permissions (teacher can only view assigned students)

#### POST /api/v1/students

**Description**: Create new student enrollment

**Request**:
```json
{
  "firstName": "Michael",
  "lastName": "Williams",
  "middleName": "James",
  "stateStudentId": "87654321",
  "dateOfBirth": "2012-03-20",
  "gender": "Male",
  "gradeLevel": 6,
  "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
  "enrollmentDate": "2025-08-15",
  "demographics": {
    "race": ["Black"],
    "ethnicity": "Not Hispanic",
    "languagePreference": "English",
    "freeReducedLunch": false
  },
  "contacts": [
    {
      "type": "Guardian",
      "firstName": "Lisa",
      "lastName": "Williams",
      "relationship": "Mother",
      "phone": "555-987-6543",
      "email": "lisa.williams@example.com",
      "isPrimary": true
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "id": "a7b8c9d0-e1f2-3456-a123-456789012346",
  "firstName": "Michael",
  "lastName": "Williams",
  "stateStudentId": "87654321",
  ...
  "createdAt": "2025-11-15T10:45:00Z"
}
```

**Validation Rules**:
- `firstName`, `lastName`: Required, max 100 chars
- `dateOfBirth`: Required, must be past date
- `gradeLevel`: 0-12
- `schoolId`: Must exist in Configuration Service
- `stateStudentId`: Unique within district

**Authorization**: Requires `Admin` or `RegistrationClerk` role

#### PUT /api/v1/students/{id}

**Description**: Update student (full replacement)

**Request**: Same as POST (all fields)

**Response** (200 OK): Updated student object

**Authorization**: Requires `Admin` or `RegistrationClerk` role

#### GET /api/v1/students/{id}/dashboard

**Description**: Get student dashboard data (summary for homepage)

**Response** (200 OK):
```json
{
  "student": { ... },
  "recentAssessments": [
    {
      "assessmentId": "b8c9d0e1-f2a3-4567-b234-567890123456",
      "assessmentName": "Math Mid-Year",
      "score": 85.5,
      "dateTaken": "2025-11-10T14:00:00Z",
      "benchmark": 80.0,
      "meetsExpectation": true
    }
  ],
  "activeInterventions": [
    {
      "interventionId": "c9d0e1f2-a3b4-5678-c345-678901234567",
      "name": "Reading Support",
      "startDate": "2025-09-15",
      "meetingSchedule": "Mon/Wed 2:00 PM"
    }
  ],
  "upcomingAssessments": [...],
  "attendanceSummary": {
    "daysPresent": 45,
    "daysAbsent": 2,
    "attendanceRate": 95.7
  }
}
```

**Authorization**: Requires teacher assigned to student, or `Admin`/`Staff` role

---

### 3. Assessment Service

**Base Path**: `/api/v1/assessments`, `/api/v1/benchmarks`

#### GET /api/v1/assessments

**Description**: List available assessments

**Query Parameters**:
- `gradeLevel` (int) - Filter by grade
- `subject` (string) - Filter by subject
- `type` (string) - `Formative`, `Summative`, `Benchmark`

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "d0e1f2a3-b4c5-6789-d456-789012345678",
      "name": "Math Mid-Year Assessment",
      "description": "Comprehensive math assessment for grades 6-8",
      "type": "Summative",
      "subject": "Mathematics",
      "gradeLevel": 7,
      "fieldCount": 25,
      "estimatedMinutes": 45,
      "isActive": true
    }
  ],
  "pagination": { ... }
}
```

#### POST /api/v1/assessments/{assessmentId}/results

**Description**: Record assessment result for student

**Request**:
```json
{
  "studentId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "dateTaken": "2025-11-15T10:00:00Z",
  "fieldScores": [
    {
      "fieldId": "e1f2a3b4-c5d6-7890-e567-890123456789",
      "fieldName": "Addition",
      "value": "18",
      "maxValue": "20"
    },
    {
      "fieldId": "f2a3b4c5-d6e7-8901-f678-901234567890",
      "fieldName": "Subtraction",
      "value": "17",
      "maxValue": "20"
    }
  ],
  "totalScore": 87.5,
  "notes": "Student struggled with fractions"
}
```

**Response** (201 Created):
```json
{
  "id": "a3b4c5d6-e7f8-9012-a789-012345678901",
  "assessmentId": "d0e1f2a3-b4c5-6789-d456-789012345678",
  "studentId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "dateTaken": "2025-11-15T10:00:00Z",
  "totalScore": 87.5,
  "benchmark": 80.0,
  "meetsExpectation": true,
  "recordedBy": "teacher-user-id",
  "recordedAt": "2025-11-15T10:30:00Z"
}
```

**Idempotency**: 
- Idempotency key: `{assessmentId}:{studentId}:{dateTaken}:{recordedBy}`
- If duplicate detected within 24 hours, return existing result (200 OK)

**Events Published**:
- `AssessmentCompletedEvent`

**Authorization**: Requires `Teacher` role (for assigned students) or `Admin`

---

### 4. Staff Management Service

**Base Path**: `/api/v1/staff`, `/api/v1/teams`

#### GET /api/v1/staff

**Description**: List staff members

**Query Parameters**:
- `role` (string) - Filter by role (`Teacher`, `Administrator`, `Specialist`)
- `schoolId` (guid) - Filter by school
- `isActive` (bool) - Filter by active status

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "b4c5d6e7-f8a9-0123-b890-123456789012",
      "firstName": "Robert",
      "lastName": "Martinez",
      "email": "robert.martinez@school.edu",
      "roles": ["Teacher", "DepartmentHead"],
      "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
      "schoolName": "Lincoln Middle School",
      "department": "Mathematics",
      "isActive": true
    }
  ],
  "pagination": { ... }
}
```

#### POST /api/v1/teams

**Description**: Create staff team (for interventions, PLCs)

**Request**:
```json
{
  "name": "Math PLC Team",
  "description": "Professional Learning Community for Math Department",
  "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
  "leaderId": "b4c5d6e7-f8a9-0123-b890-123456789012",
  "memberIds": [
    "c5d6e7f8-a9b0-1234-c901-234567890123",
    "d6e7f8a9-b0c1-2345-d012-345678901234"
  ]
}
```

**Response** (201 Created):
```json
{
  "id": "e7f8a9b0-c1d2-3456-e123-456789012345",
  "name": "Math PLC Team",
  "description": "Professional Learning Community for Math Department",
  "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
  "leader": {
    "id": "b4c5d6e7-f8a9-0123-b890-123456789012",
    "name": "Robert Martinez"
  },
  "members": [ ... ],
  "createdAt": "2025-11-15T11:00:00Z"
}
```

---

### 5. Intervention Management Service

**Base Path**: `/api/v1/interventions`, `/api/v1/intervention-groups`

#### POST /api/v1/intervention-groups

**Description**: Create intervention group

**Request**:
```json
{
  "name": "Tier 2 Reading Support",
  "description": "Small group reading intervention for struggling readers",
  "interventionType": "Reading",
  "tier": 2,
  "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
  "facilitatorId": "b4c5d6e7-f8a9-0123-b890-123456789012",
  "startDate": "2025-09-15",
  "endDate": "2026-05-30",
  "meetingSchedule": {
    "days": ["Monday", "Wednesday"],
    "time": "14:00",
    "durationMinutes": 30
  },
  "studentIds": [
    "c3d4e5f6-a7b8-9012-cdef-123456789012",
    "a7b8c9d0-e1f2-3456-a123-456789012346"
  ]
}
```

**Response** (201 Created):
```json
{
  "id": "f8a9b0c1-d2e3-4567-f234-567890123456",
  "name": "Tier 2 Reading Support",
  "interventionType": "Reading",
  "tier": 2,
  "facilitator": {
    "id": "b4c5d6e7-f8a9-0123-b890-123456789012",
    "name": "Robert Martinez"
  },
  "studentCount": 2,
  "meetingSchedule": { ... },
  "createdAt": "2025-11-15T11:15:00Z"
}
```

**Events Published**:
- `InterventionGroupCreatedEvent`
- `StudentAddedToInterventionEvent` (for each student)

---

### 6. Section & Roster Service

**Base Path**: `/api/v1/sections`

#### GET /api/v1/sections

**Description**: List class sections

**Query Parameters**:
- `schoolId` (guid)
- `academicYear` (string) - e.g., "2025-2026"
- `teacherId` (guid)
- `gradeLevel` (int)

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "a9b0c1d2-e3f4-5678-a345-678901234567",
      "name": "7th Grade Math - Period 3",
      "subject": "Mathematics",
      "gradeLevel": 7,
      "academicYear": "2025-2026",
      "teacherId": "b4c5d6e7-f8a9-0123-b890-123456789012",
      "teacherName": "Robert Martinez",
      "studentCount": 28,
      "maxCapacity": 30,
      "schedule": {
        "period": 3,
        "days": ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"],
        "startTime": "10:15",
        "endTime": "11:00"
      }
    }
  ],
  "pagination": { ... }
}
```

#### POST /api/v1/sections/{sectionId}/roster

**Description**: Add student to section roster

**Request**:
```json
{
  "studentId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "enrollmentDate": "2025-08-15"
}
```

**Response** (201 Created):
```json
{
  "id": "b0c1d2e3-f4a5-6789-b456-789012345678",
  "sectionId": "a9b0c1d2-e3f4-5678-a345-678901234567",
  "studentId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "enrollmentDate": "2025-08-15",
  "enrollmentStatus": "Active"
}
```

**Events Published**:
- `StudentRosteredEvent`

---

### 7. Data Import Service

**Base Path**: `/api/v1/imports`

#### POST /api/v1/imports/students

**Description**: Import students from CSV file

**Request** (multipart/form-data):
```
file: student_import.csv
districtId: d4e5f6a7-b8c9-0123-def1-234567890123
schoolId: e5f6a7b8-c9d0-1234-ef12-345678901234
validateOnly: false
```

**Response** (202 Accepted):
```json
{
  "importJobId": "c1d2e3f4-a5b6-7890-c567-890123456789",
  "status": "Processing",
  "statusUrl": "/api/v1/imports/c1d2e3f4-a5b6-7890-c567-890123456789/status",
  "estimatedCompletionTime": "2025-11-15T11:30:00Z"
}
```

#### GET /api/v1/imports/{importJobId}/status

**Description**: Get import job status

**Response** (200 OK):
```json
{
  "importJobId": "c1d2e3f4-a5b6-7890-c567-890123456789",
  "status": "Completed",
  "fileName": "student_import.csv",
  "totalRows": 150,
  "successCount": 145,
  "errorCount": 5,
  "startedAt": "2025-11-15T11:20:00Z",
  "completedAt": "2025-11-15T11:28:00Z",
  "errors": [
    {
      "row": 12,
      "field": "dateOfBirth",
      "message": "Invalid date format"
    },
    {
      "row": 45,
      "field": "gradeLevel",
      "message": "Grade level must be 0-12"
    }
  ]
}
```

---

### 8. Reporting & Analytics Service

**Base Path**: `/api/v1/reports`

#### POST /api/v1/reports/generate

**Description**: Generate report (async job)

**Request**:
```json
{
  "reportType": "StudentAssessmentSummary",
  "parameters": {
    "schoolId": "d4e5f6a7-b8c9-0123-def1-234567890123",
    "gradeLevel": 7,
    "academicYear": "2025-2026",
    "assessmentIds": ["d0e1f2a3-b4c5-6789-d456-789012345678"]
  },
  "format": "PDF",
  "deliveryMethod": "Download"
}
```

**Response** (202 Accepted):
```json
{
  "reportJobId": "d2e3f4a5-b6c7-8901-d678-901234567890",
  "status": "Generating",
  "statusUrl": "/api/v1/reports/d2e3f4a5-b6c7-8901-d678-901234567890/status",
  "estimatedCompletionTime": "2025-11-15T11:35:00Z"
}
```

#### GET /api/v1/reports/{reportJobId}/download

**Description**: Download generated report

**Response** (200 OK, Content-Type: application/pdf):
Binary PDF file

---

### 9. Content & Media Service

**Base Path**: `/api/v1/files`, `/api/v1/videos`

#### POST /api/v1/files/upload

**Description**: Upload file to Azure Blob Storage

**Request** (multipart/form-data):
```
file: document.pdf
category: StudentPortfolio
studentId: c3d4e5f6-a7b8-9012-cdef-123456789012  # optional
```

**Response** (201 Created):
```json
{
  "id": "e3f4a5b6-c7d8-9012-e789-012345678901",
  "fileName": "document.pdf",
  "fileSize": 245678,
  "mimeType": "application/pdf",
  "category": "StudentPortfolio",
  "uploadedBy": "teacher-user-id",
  "uploadedAt": "2025-11-15T11:40:00Z",
  "downloadUrl": "/api/v1/files/e3f4a5b6-c7d8-9012-e789-012345678901/download",
  "expiresAt": "2025-11-15T17:40:00Z"  # 6-hour expiry for security
}
```

**Limits**:
- Max file size: 100 MB
- Allowed types: PDF, DOCX, XLSX, PNG, JPG, MP4

---

### 10. Configuration Service

**Base Path**: `/api/v1/configuration`

#### GET /api/v1/configuration/schools

**Description**: List schools in district

**Query Parameters**:
- `districtId` (guid)
- `isActive` (bool)

**Response** (200 OK):
```json
{
  "items": [
    {
      "id": "d4e5f6a7-b8c9-0123-def1-234567890123",
      "name": "Lincoln Middle School",
      "districtId": "e5f6a7b8-c9d0-1234-ef12-345678901234",
      "address": "123 Education St, Springfield",
      "principalId": "f6a7b8c9-d0e1-2345-f123-456789012345",
      "gradeRange": "6-8",
      "studentCapacity": 750,
      "isActive": true
    }
  ]
}
```

#### GET /api/v1/configuration/calendars/{academicYear}

**Description**: Get academic calendar

**Response** (200 OK):
```json
{
  "academicYear": "2025-2026",
  "districtId": "e5f6a7b8-c9d0-1234-ef12-345678901234",
  "startDate": "2025-08-15",
  "endDate": "2026-05-30",
  "events": [
    {
      "id": "a7b8c9d0-e1f2-3456-a123-456789012347",
      "name": "First Day of School",
      "date": "2025-08-15",
      "type": "Instructional"
    },
    {
      "id": "b8c9d0e1-f2a3-4567-b234-567890123457",
      "name": "Thanksgiving Break",
      "startDate": "2025-11-27",
      "endDate": "2025-11-29",
      "type": "Holiday"
    }
  ]
}
```

---

## API Gateway Configuration (YARP)

**Gateway Routes**:

```json
{
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/v1/auth/{**catch-all}" }
      },
      "users-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/v1/users/{**catch-all}" },
        "AuthorizationPolicy": "AdminOnly"
      },
      "students-route": {
        "ClusterId": "students-cluster",
        "Match": { "Path": "/api/v1/students/{**catch-all}" },
        "AuthorizationPolicy": "AuthenticatedUsers"
      },
      "assessments-route": {
        "ClusterId": "assessments-cluster",
        "Match": { "Path": "/api/v1/assessments/{**catch-all}" },
        "AuthorizationPolicy": "AuthenticatedUsers"
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "identity-service": { "Address": "http://identity-api" }
        }
      },
      "students-cluster": {
        "Destinations": {
          "students-service": { "Address": "http://students-api" }
        }
      },
      "assessments-cluster": {
        "Destinations": {
          "assessments-service": { "Address": "http://assessments-api" }
        }
      }
    }
  }
}
```

**Rate Limiting**:
```json
{
  "RateLimiting": {
    "Global": {
      "PermitLimit": 1000,
      "Window": "1m",
      "QueueLimit": 10
    },
    "PerUser": {
      "PermitLimit": 100,
      "Window": "1m"
    }
  }
}
```

---

## OpenAPI/Swagger Documentation

Each service must provide OpenAPI 3.0 specification:

```csharp
// Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NorthStar Students API",
        Version = "v1",
        Description = "Student Management Service for NorthStar LMS",
        Contact = new OpenApiContact
        {
            Name = "NorthStar Support",
            Email = "support@northstar.example.com"
        }
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

**Swagger UI Access**: `https://{service-host}/swagger`

---

## Versioning Strategy

**URI Versioning** (Recommended):
- `v1` = Current production version
- `v2` = New version with breaking changes
- Both versions supported during transition period (6 months)

**Breaking Changes**:
- Removing fields
- Changing field types
- Removing endpoints
- Changing URL structure

**Non-Breaking Changes** (can be in same version):
- Adding optional fields
- Adding new endpoints
- Relaxing validation

---

## API Testing

**Postman Collections**:
- Maintain Postman collections per service
- Include example requests for all endpoints
- Environment variables for dev/staging/prod

**Contract Testing**:
- Use Pact for consumer-driven contract tests
- Ensure API changes don't break consumers

---

**Version**: 1.0  
**Last Updated**: November 15, 2025  
**Owner**: API Architecture Team  
**Status**: Specification Complete - Ready for Implementation
