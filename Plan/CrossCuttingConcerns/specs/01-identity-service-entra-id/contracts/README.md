# API Contracts: Identity Service

**Feature ID**: `01-identity-service-entra-id`  
**API Version**: v1  
**Base URL**: `/api/auth`  
**OpenAPI Version**: 3.0.3  
**Created**: 2025-11-20  
**Status**: Draft

---

## Table of Contents

- [Overview](#overview)
- [Authentication](#authentication)
- [Common Schemas](#common-schemas)
- [Endpoints](#endpoints)
  - [Token Exchange](#post-apiauthexchange-token)
  - [Session Info](#get-apiauthsession)
  - [Token Refresh](#post-apiauthrefresh)
  - [Logout](#post-apiauthlogout)
  - [User Claims](#get-apiauthclaims)
  - [Tenant Switch](#post-apiauthswitch-tenant)
- [Error Handling](#error-handling)
- [Rate Limiting](#rate-limiting)

---

## Overview

The Identity Service exposes RESTful endpoints for authentication, session management, and authorization. All endpoints follow RFC 7807 Problem Details for error responses and use standard HTTP status codes.

**Key Design Principles**:
- ✅ **RESTful**: Resource-oriented URLs, HTTP verbs
- ✅ **Stateful Sessions**: Session-based authentication (cookies)
- ✅ **Error Standards**: RFC 7807 Problem Details
- ✅ **Idempotency**: Safe retry for all POST operations
- ✅ **Versioning**: URL path versioning (`/api/auth/v1/...` future)

---

## Authentication

### Client Authentication

**Token Exchange Endpoint** (`POST /api/auth/exchange-token`):
- **Authentication**: Bearer token (Entra ID JWT)
- **Header**: `Authorization: Bearer {entra_jwt}`

**All Other Endpoints**:
- **Authentication**: Session cookie (HTTP-only, secure)
- **Cookie Name**: `lms_session`
- **Auto-sent by browser** (no manual header required)

### Session Cookie Configuration

```http
Set-Cookie: lms_session=lms_session_{guid}; 
            Path=/; 
            HttpOnly; 
            Secure; 
            SameSite=Strict; 
            Max-Age=28800
```

---

## Common Schemas

### SessionInfo

```json
{
  "sessionId": "lms_session_123e4567-e89b-12d3-a456-426614174000",
  "userId": "20000000-0000-0000-0000-000000000001",
  "tenantId": "00000000-0000-0000-0000-000000000001",
  "expiresAt": "2025-11-20T16:00:00Z",
  "remainingMinutes": 450
}
```

### UserClaimsResponse

```json
{
  "userId": "20000000-0000-0000-0000-000000000001",
  "email": "admin@demo.edu",
  "displayName": "Admin User",
  "roles": ["Administrator"],
  "permissions": ["*"],
  "tenantIds": ["00000000-0000-0000-0000-000000000001"]
}
```

### ProblemDetails (RFC 7807)

```json
{
  "type": "https://api.northstaret.com/errors/authentication-failed",
  "title": "Authentication Failed",
  "status": 401,
  "detail": "Invalid or expired token",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

---

## Endpoints

### POST /api/auth/exchange-token

**Purpose**: Exchange Entra ID JWT for LMS session.

**Authentication**: Bearer token (Entra ID JWT)

**Request**:
```http
POST /api/auth/exchange-token HTTP/1.1
Host: api.northstaret.com
Authorization: Bearer eyJ0eXAiOiJKV1QiLCJhbGciOi...
Content-Type: application/json
```

**Response** (200 OK):
```json
{
  "sessionId": "lms_session_123e4567-e89b-12d3-a456-426614174000",
  "userId": "20000000-0000-0000-0000-000000000001",
  "tenantId": "00000000-0000-0000-0000-000000000001",
  "expiresAt": "2025-11-20T16:00:00Z"
}
```

**Response Headers**:
```http
Set-Cookie: lms_session=lms_session_123e4567-e89b-12d3-a456-426614174000; Path=/; HttpOnly; Secure; SameSite=Strict; Max-Age=28800
```

**Error Responses**:

**401 Unauthorized** - Invalid or expired Entra ID token:
```json
{
  "type": "https://api.northstaret.com/errors/invalid-token",
  "title": "Invalid Token",
  "status": 401,
  "detail": "Token signature validation failed",
  "traceId": "00-..."
}
```

**403 Forbidden** - User has no tenant access:
```json
{
  "type": "https://api.northstaret.com/errors/no-tenant-access",
  "title": "No Tenant Access",
  "status": 403,
  "detail": "User is not assigned to any tenants",
  "traceId": "00-..."
}
```

**OpenAPI Schema**:
```yaml
/api/auth/exchange-token:
  post:
    summary: Exchange Entra ID JWT for LMS session
    operationId: exchangeToken
    security:
      - BearerAuth: []
    responses:
      '200':
        description: Session created successfully
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/AuthenticationResponse'
        headers:
          Set-Cookie:
            schema:
              type: string
              example: lms_session=...; Path=/; HttpOnly; Secure
      '401':
        $ref: '#/components/responses/Unauthorized'
      '403':
        $ref: '#/components/responses/Forbidden'
```

---

### GET /api/auth/session

**Purpose**: Get current session information.

**Authentication**: Session cookie

**Request**:
```http
GET /api/auth/session HTTP/1.1
Host: api.northstaret.com
Cookie: lms_session=lms_session_123e4567-e89b-12d3-a456-426614174000
```

**Response** (200 OK):
```json
{
  "sessionId": "lms_session_123e4567-e89b-12d3-a456-426614174000",
  "userId": "20000000-0000-0000-0000-000000000001",
  "tenantId": "00000000-0000-0000-0000-000000000001",
  "expiresAt": "2025-11-20T16:00:00Z",
  "remainingMinutes": 450
}
```

**Error Responses**:

**401 Unauthorized** - Session expired or invalid:
```json
{
  "type": "https://api.northstaret.com/errors/session-expired",
  "title": "Session Expired",
  "status": 401,
  "detail": "Session has expired, please log in again",
  "traceId": "00-..."
}
```

**OpenAPI Schema**:
```yaml
/api/auth/session:
  get:
    summary: Get current session information
    operationId: getSession
    security:
      - CookieAuth: []
    responses:
      '200':
        description: Session information retrieved
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/SessionInfo'
      '401':
        $ref: '#/components/responses/Unauthorized'
```

---

### POST /api/auth/refresh

**Purpose**: Manually trigger token refresh (background service handles automatic refresh).

**Authentication**: Session cookie

**Request**:
```http
POST /api/auth/refresh HTTP/1.1
Host: api.northstaret.com
Cookie: lms_session=lms_session_123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json
```

**Response** (200 OK):
```json
{
  "success": true,
  "newExpiresAt": "2025-11-20T20:00:00Z"
}
```

**Error Responses**:

**401 Unauthorized** - Session expired:
```json
{
  "type": "https://api.northstaret.com/errors/session-expired",
  "title": "Session Expired",
  "status": 401,
  "detail": "Cannot refresh expired session, please log in again",
  "traceId": "00-..."
}
```

**502 Bad Gateway** - Entra ID token refresh failed:
```json
{
  "type": "https://api.northstaret.com/errors/token-refresh-failed",
  "title": "Token Refresh Failed",
  "status": 502,
  "detail": "Failed to refresh token with Microsoft Entra ID",
  "traceId": "00-..."
}
```

**OpenAPI Schema**:
```yaml
/api/auth/refresh:
  post:
    summary: Manually trigger token refresh
    operationId: refreshSession
    security:
      - CookieAuth: []
    responses:
      '200':
        description: Session refreshed successfully
        content:
          application/json:
            schema:
              type: object
              properties:
                success:
                  type: boolean
                newExpiresAt:
                  type: string
                  format: date-time
      '401':
        $ref: '#/components/responses/Unauthorized'
      '502':
        $ref: '#/components/responses/BadGateway'
```

---

### POST /api/auth/logout

**Purpose**: Terminate LMS session and get Entra ID logout URL.

**Authentication**: Session cookie

**Request**:
```http
POST /api/auth/logout HTTP/1.1
Host: api.northstaret.com
Cookie: lms_session=lms_session_123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json
```

**Response** (200 OK):
```json
{
  "entraLogoutUrl": "https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/logout?post_logout_redirect_uri=https%3A%2F%2Flms.northstaret.com"
}
```

**Response Headers**:
```http
Set-Cookie: lms_session=; Path=/; HttpOnly; Secure; SameSite=Strict; Max-Age=0
```

**Side Effects**:
- Session removed from PostgreSQL and Redis
- `UserLoggedOut` event published
- Session cookie cleared (Max-Age=0)

**Error Responses**:

**401 Unauthorized** - No active session:
```json
{
  "type": "https://api.northstaret.com/errors/no-session",
  "title": "No Active Session",
  "status": 401,
  "detail": "No session to log out",
  "traceId": "00-..."
}
```

**OpenAPI Schema**:
```yaml
/api/auth/logout:
  post:
    summary: Terminate LMS session
    operationId: logout
    security:
      - CookieAuth: []
    responses:
      '200':
        description: Logout successful
        content:
          application/json:
            schema:
              type: object
              properties:
                entraLogoutUrl:
                  type: string
                  format: uri
        headers:
          Set-Cookie:
            schema:
              type: string
              example: lms_session=; Max-Age=0
      '401':
        $ref: '#/components/responses/Unauthorized'
```

---

### GET /api/auth/claims

**Purpose**: Get current user's roles, permissions, and tenant assignments.

**Authentication**: Session cookie

**Request**:
```http
GET /api/auth/claims HTTP/1.1
Host: api.northstaret.com
Cookie: lms_session=lms_session_123e4567-e89b-12d3-a456-426614174000
```

**Response** (200 OK):
```json
{
  "userId": "20000000-0000-0000-0000-000000000001",
  "email": "admin@demo.edu",
  "displayName": "Admin User",
  "roles": ["Administrator"],
  "permissions": ["*"],
  "tenantIds": ["00000000-0000-0000-0000-000000000001"]
}
```

**Caching**:
- Response cached in Redis with 1-hour TTL
- Cache key: `lms_permissions:{userId}:{tenantId}`
- Cache invalidated on `UserRoleAssigned` or `UserRoleRevoked` events

**Error Responses**:

**401 Unauthorized** - Session expired:
```json
{
  "type": "https://api.northstaret.com/errors/session-expired",
  "title": "Session Expired",
  "status": 401,
  "detail": "Session has expired, please log in again",
  "traceId": "00-..."
}
```

**OpenAPI Schema**:
```yaml
/api/auth/claims:
  get:
    summary: Get user claims and permissions
    operationId: getUserClaims
    security:
      - CookieAuth: []
    responses:
      '200':
        description: User claims retrieved
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UserClaimsResponse'
      '401':
        $ref: '#/components/responses/Unauthorized'
```

---

### POST /api/auth/switch-tenant

**Purpose**: Switch active tenant context for multi-district users.

**Authentication**: Session cookie

**Request**:
```http
POST /api/auth/switch-tenant HTTP/1.1
Host: api.northstaret.com
Cookie: lms_session=lms_session_123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json

{
  "targetTenantId": "00000000-0000-0000-0000-000000000002"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "newTenantId": "00000000-0000-0000-0000-000000000002"
}
```

**Side Effects**:
- `session.tenant_id` updated in PostgreSQL and Redis
- Permission cache invalidated for old tenant
- `TenantContextSwitched` event published

**Error Responses**:

**401 Unauthorized** - Session expired:
```json
{
  "type": "https://api.northstaret.com/errors/session-expired",
  "title": "Session Expired",
  "status": 401,
  "detail": "Session has expired, please log in again",
  "traceId": "00-..."
}
```

**403 Forbidden** - User has no access to target tenant:
```json
{
  "type": "https://api.northstaret.com/errors/tenant-access-denied",
  "title": "Tenant Access Denied",
  "status": 403,
  "detail": "User does not have access to tenant 00000000-0000-0000-0000-000000000002",
  "traceId": "00-..."
}
```

**400 Bad Request** - Invalid tenant ID:
```json
{
  "type": "https://api.northstaret.com/errors/validation-error",
  "title": "Validation Error",
  "status": 400,
  "detail": "targetTenantId must be a valid UUID",
  "traceId": "00-...",
  "errors": {
    "targetTenantId": ["Must be a valid UUID"]
  }
}
```

**OpenAPI Schema**:
```yaml
/api/auth/switch-tenant:
  post:
    summary: Switch active tenant context
    operationId: switchTenant
    security:
      - CookieAuth: []
    requestBody:
      required: true
      content:
        application/json:
          schema:
            type: object
            properties:
              targetTenantId:
                type: string
                format: uuid
            required:
              - targetTenantId
    responses:
      '200':
        description: Tenant context switched successfully
        content:
          application/json:
            schema:
              type: object
              properties:
                success:
                  type: boolean
                newTenantId:
                  type: string
                  format: uuid
      '400':
        $ref: '#/components/responses/BadRequest'
      '401':
        $ref: '#/components/responses/Unauthorized'
      '403':
        $ref: '#/components/responses/Forbidden'
```

---

## Error Handling

### Error Response Format (RFC 7807)

All errors follow [RFC 7807 Problem Details for HTTP APIs](https://tools.ietf.org/html/rfc7807).

**Standard Fields**:
```json
{
  "type": "https://api.northstaret.com/errors/{error-code}",
  "title": "Human-readable error title",
  "status": 400,
  "detail": "Detailed error description",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Validation Errors** (additional `errors` field):
```json
{
  "type": "https://api.northstaret.com/errors/validation-error",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "traceId": "00-...",
  "errors": {
    "targetTenantId": ["Must be a valid UUID"],
    "email": ["Email is required", "Email format invalid"]
  }
}
```

### HTTP Status Codes

| Status Code | Meaning | Use Case |
|-------------|---------|----------|
| **200 OK** | Success | Successful request with response body |
| **204 No Content** | Success (no body) | Successful DELETE or idempotent POST |
| **400 Bad Request** | Client error | Validation failure, malformed JSON |
| **401 Unauthorized** | Authentication failed | Missing/invalid/expired session or token |
| **403 Forbidden** | Authorization failed | User lacks permission for requested resource |
| **404 Not Found** | Resource not found | User/session/role not found |
| **429 Too Many Requests** | Rate limit exceeded | Too many login attempts, see [Rate Limiting](#rate-limiting) |
| **500 Internal Server Error** | Server error | Unhandled exception, database failure |
| **502 Bad Gateway** | Upstream failure | Entra ID API failure, Redis unavailable |
| **503 Service Unavailable** | Service down | Maintenance mode, circuit breaker open |

### Error Types Registry

| Error Type | Status | Description |
|------------|--------|-------------|
| `invalid-token` | 401 | Entra ID JWT validation failed |
| `session-expired` | 401 | Session has expired |
| `no-session` | 401 | No active session found |
| `no-tenant-access` | 403 | User not assigned to any tenants |
| `tenant-access-denied` | 403 | User lacks access to requested tenant |
| `validation-error` | 400 | Request validation failed |
| `token-refresh-failed` | 502 | Entra ID token refresh failed |
| `rate-limit-exceeded` | 429 | Too many requests |

---

## Rate Limiting

### Limits

| Endpoint | Limit | Window | Purpose |
|----------|-------|--------|---------|
| `POST /api/auth/exchange-token` | 10 requests | 1 minute | Prevent brute force |
| `POST /api/auth/refresh` | 5 requests | 1 minute | Prevent abuse |
| `POST /api/auth/logout` | 10 requests | 1 minute | Allow multiple devices |
| `POST /api/auth/switch-tenant` | 20 requests | 1 minute | Allow rapid switching |
| `GET /api/auth/session` | 60 requests | 1 minute | Dashboard polling |
| `GET /api/auth/claims` | 60 requests | 1 minute | UI permission checks |

### Rate Limit Headers

```http
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 5
X-RateLimit-Reset: 2025-11-20T08:30:00Z
```

### Rate Limit Exceeded Response

**429 Too Many Requests**:
```json
{
  "type": "https://api.northstaret.com/errors/rate-limit-exceeded",
  "title": "Rate Limit Exceeded",
  "status": 429,
  "detail": "Too many login attempts. Please try again in 60 seconds.",
  "traceId": "00-...",
  "retryAfter": 60
}
```

**Response Headers**:
```http
Retry-After: 60
```

---

## OpenAPI Specification (Complete)

See [openapi.yaml](./openapi.yaml) for the complete OpenAPI 3.0.3 specification (auto-generated from Swashbuckle in `Identity.API`).

**Swagger UI**: Available at `https://api.northstaret.com/swagger` (development only)

---

## Versioning Strategy

**Current**: No version prefix (`/api/auth/*`)  
**Future**: Add version prefix when breaking changes introduced (`/api/auth/v2/*`)

**Breaking Change Examples**:
- Renaming fields (e.g., `userId` → `id`)
- Removing endpoints
- Changing response structure (e.g., flat object → nested)

**Non-Breaking Changes** (same version):
- Adding new endpoints
- Adding optional request fields
- Adding new response fields
- Expanding enums

---

**Contract Version**: 1.0.0  
**Last Updated**: 2025-11-20  
**Next Review**: After Phase 1 implementation
