# Digital Ink API Specification

**Version:** 1.0  
**Last Updated:** November 20, 2025  
**Base URL:** `https://api.northstar.edu/api/v1/ink`

---

## Overview

RESTful API for digital ink session management, stroke capture, audio recording, and playback. All endpoints require JWT authentication via Identity Service.

**Authentication:** Bearer token in `Authorization` header  
**Content-Type:** `application/json` (except file uploads: `multipart/form-data`)  
**Rate Limiting:** 100 requests/min per user

---

## Endpoints

### Session Management

#### Create Ink Session

**Request:**
```http
POST /api/v1/ink/sessions
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "assignmentId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "backgroundAssetUrl": "https://storage.blob.core.windows.net/pdfs/worksheet.pdf"
}
```

**Response (201 Created):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "assignmentId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "backgroundAssetSasUrl": "https://storage.blob.core.windows.net/pdfs/worksheet.pdf?sas=...",
  "createdAt": "2025-11-20T14:30:00Z"
}
```

---

#### Get Session Details

**Request:**
```http
GET /api/v1/ink/sessions/{sessionId}
Authorization: Bearer {jwt}
```

**Response (200 OK):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "ownerId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "assignmentId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "backgroundAssetUrl": "https://storage.blob.core.windows.net/pdfs/worksheet.pdf?sas=...",
  "audioAssetUrl": "https://storage.blob.core.windows.net/audio/session-550e8400.webm?sas=...",
  "strokeData": {
    "pages": [
      {
        "pageNumber": 1,
        "strokes": [
          {
            "strokeId": "uuid",
            "tool": "pen",
            "color": "#000000",
            "width": 2.0,
            "points": [[10.5, 20.3, 0.75, 0], [11.2, 21.1, 0.80, 16]]
          }
        ]
      }
    ]
  },
  "createdAt": "2025-11-20T14:30:00Z",
  "updatedAt": "2025-11-20T14:35:00Z"
}
```

---

#### Archive Session

**Request:**
```http
DELETE /api/v1/ink/sessions/{sessionId}
Authorization: Bearer {jwt}
```

**Response (204 No Content)**

---

### Stroke Operations

#### Save Stroke Batch

**Request:**
```http
PUT /api/v1/ink/sessions/{sessionId}/strokes
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "pageNumber": 1,
  "strokes": [
    {
      "strokeId": "uuid",
      "tool": "pen",
      "color": "#000000",
      "width": 2.0,
      "points": [[10.5, 20.3, 0.75, 0], [11.2, 21.1, 0.80, 16], [12.8, 22.5, 0.78, 32]]
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "strokesSaved": 1,
  "totalStrokes": 42
}
```

---

#### Delete Stroke (Undo)

**Request:**
```http
DELETE /api/v1/ink/sessions/{sessionId}/strokes/{strokeId}
Authorization: Bearer {jwt}
```

**Response (204 No Content)**

---

### Audio Operations

#### Upload Audio

**Request:**
```http
POST /api/v1/ink/sessions/{sessionId}/audio
Authorization: Bearer {jwt}
Content-Type: multipart/form-data

{
  "file": <binary>,
  "durationMs": 45000,
  "sampleRate": 44100,
  "format": "webm"
}
```

**Response (201 Created):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "audioAssetUrl": "https://storage.blob.core.windows.net/audio/session-550e8400.webm?sas=...",
  "durationMs": 45000
}
```

---

#### Get Audio URL

**Request:**
```http
GET /api/v1/ink/sessions/{sessionId}/audio
Authorization: Bearer {jwt}
```

**Response (200 OK):**
```json
{
  "audioAssetUrl": "https://storage.blob.core.windows.net/audio/session-550e8400.webm?sas=...",
  "durationMs": 45000,
  "format": "webm",
  "expiresAt": "2025-11-20T15:00:00Z"
}
```

---

### Playback & Export

#### Get Playback Data

**Request:**
```http
GET /api/v1/ink/sessions/{sessionId}/playback
Authorization: Bearer {jwt}
```

**Response (200 OK):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "audioUrl": "https://storage.blob.core.windows.net/audio/session-550e8400.webm?sas=...",
  "audioDurationMs": 45000,
  "timeline": [
    {
      "timestampMs": 0,
      "strokeId": "uuid1",
      "pageNumber": 1,
      "points": [[10.5, 20.3, 0.75, 0]]
    },
    {
      "timestampMs": 1500,
      "strokeId": "uuid2",
      "pageNumber": 1,
      "points": [[15.0, 25.0, 0.60, 1500]]
    }
  ]
}
```

---

#### Export LLM Format

**Request:**
```http
GET /api/v1/ink/sessions/{sessionId}/export
Authorization: Bearer {jwt}
```

**Response (200 OK):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "format": "llm-optimized-v1",
  "audioUrl": "https://storage.blob.core.windows.net/audio/session-550e8400.webm?sas=...",
  "strokes": [
    {
      "strokeId": "uuid",
      "tool": "pen",
      "timeSeriesVectors": [
        [10.5, 20.3, 0.75, 0],
        [11.2, 21.1, 0.80, 16],
        [12.8, 22.5, 0.78, 32]
      ]
    }
  ],
  "metadata": {
    "totalStrokes": 42,
    "totalPages": 3,
    "audioDurationMs": 45000
  }
}
```

---

### Teacher Feedback

#### Add Feedback Annotation

**Request:**
```http
POST /api/v1/ink/sessions/{sessionId}/feedback
Authorization: Bearer {jwt}
Content-Type: application/json

{
  "pageNumber": 1,
  "feedbackStrokes": [
    {
      "strokeId": "uuid",
      "tool": "pen",
      "color": "#FF0000",
      "width": 3.0,
      "points": [[50.0, 100.0, 0.85, 0]]
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "feedbackId": "uuid",
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "teacherId": "teacher-uuid",
  "createdAt": "2025-11-20T15:00:00Z"
}
```

---

#### Get Feedback Overlay

**Request:**
```http
GET /api/v1/ink/sessions/{sessionId}/feedback
Authorization: Bearer {jwt}
```

**Response (200 OK):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "feedbackAnnotations": [
    {
      "feedbackId": "uuid",
      "teacherId": "teacher-uuid",
      "teacherName": "Ms. Johnson",
      "pageNumber": 1,
      "strokes": [
        {
          "strokeId": "uuid",
          "tool": "pen",
          "color": "#FF0000",
          "width": 3.0,
          "points": [[50.0, 100.0, 0.85, 0]]
        }
      ],
      "createdAt": "2025-11-20T15:00:00Z"
    }
  ]
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "Invalid stroke data format",
  "details": {
    "field": "strokes[0].points",
    "error": "Must be array of [x, y, pressure, timestamp]"
  }
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "message": "Invalid or expired JWT token"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "message": "You do not have access to this ink session"
}
```

### 404 Not Found
```json
{
  "error": "NotFound",
  "message": "Ink session not found"
}
```

### 429 Too Many Requests
```json
{
  "error": "RateLimitExceeded",
  "message": "Rate limit of 100 requests/min exceeded",
  "retryAfter": 60
}
```

---

## Related Documentation

- **Domain Events:** [./domain-events.md](./domain-events.md)
- **Service Overview:** [../SERVICE_OVERVIEW.md](../SERVICE_OVERVIEW.md)
- **Technology Stack:** [./technology-stack.md](./technology-stack.md)

---

**Last Updated:** November 20, 2025  
**Version:** 1.0
