# Data Model: Unified SSO & Authorization via Entra (LMS Identity Module)

**Date**: 2025-10-20  
**Feature**: 001-unified-sso-auth  
**Phase**: 1 - Design & Contracts

## Overview

This document defines the domain entities, value objects, and their relationships for the authentication and authorization system. The model supports single sign-on via Microsoft Entra ID, multi-tenant authorization, and session management. Identity tables reside in the primary LMS PostgreSQL database within a dedicated `identity` schema to maintain separation from other functional areas.

---

## Domain Entities

### User

Represents a person who authenticates with the system. Linked to Microsoft Entra ID via subject ID.

**Aggregate Root**: Yes

**Properties**:
| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | Guid | Yes | Unique identifier | Auto-generated |
| EntraSubjectId | EntraSubjectId (Value Object) | Yes | Entra ID subject claim (sub) | Unique, immutable |
| Email | string | Yes | User's email address | Valid email format, max 256 chars |
| FirstName | string | Yes | User's first name | Max 100 chars |
| LastName | string | Yes | User's last name | Max 100 chars |
| DisplayName | string | No | Full display name | Auto-computed: `{FirstName} {LastName}` |
| IsActive | bool | Yes | Whether user account is active | Default: true |
| CreatedAt | DateTimeOffset | Yes | When user was created | Auto-set on creation |
| LastLoginAt | DateTimeOffset | No | Last successful authentication | Updated on each sign-in |

**Relationships**:
- `Memberships` (1:N with Membership): User can have multiple tenant memberships
- `Sessions` (1:N with Session): User can have multiple active sessions

**Business Rules**:
- EntraSubjectId must be unique across all users
- Email must be unique across all users
- Inactive users cannot authenticate (enforced at Application layer)
- DisplayName is always computed; cannot be set directly

**Persistence**:
- Table: `Users`
- Indexes:
  - `EntraSubjectId` (unique)
  - `Email` (unique)
  - `IsActive` (for active user queries)

---

### Tenant

Represents an organizational scope (district or school). Defines the boundary for authorization decisions.

**Aggregate Root**: Yes

**Properties**:
| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | TenantId (Value Object) | Yes | Unique identifier | Auto-generated |
| Name | string | Yes | Tenant display name | Max 200 chars |
| Type | TenantType (Enum) | Yes | District or School | Valid enum value |
| ParentTenantId | TenantId | No | Parent tenant (for schools in districts) | Must reference existing Tenant |
| ExternalId | string | No | External system identifier | Max 100 chars, unique if provided |
| IsActive | bool | Yes | Whether tenant is active | Default: true |
| CreatedAt | DateTimeOffset | Yes | When tenant was created | Auto-set on creation |

**Enumerations**:
```csharp
public enum TenantType
{
    District = 1,
    School = 2
}
```

**Relationships**:
- `ParentTenant` (N:1 with Tenant): School can belong to a District
- `ChildTenants` (1:N with Tenant): District can have multiple Schools
- `Memberships` (1:N with Membership): Tenant can have multiple user memberships

**Business Rules**:
- District tenants cannot have a ParentTenantId
- School tenants should have a ParentTenantId referencing a District (enforced at Application layer)
- Inactive tenants are not available for tenant switching (enforced at Application layer)
- Cannot delete a tenant with active memberships

**Persistence**:
- Table: `Tenants`
- Indexes:
  - `ParentTenantId` (for hierarchy queries)
  - `Type` (for filtering by tenant type)
  - `ExternalId` (unique, sparse)
  - `IsActive`

---

### Membership

Represents a user's association with a tenant and their role assignments within that tenant.

**Aggregate Root**: No (owned by User and Tenant aggregates)

**Properties**:
| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | Guid | Yes | Unique identifier | Auto-generated |
| UserId | Guid | Yes | Foreign key to User | Must reference existing User |
| TenantId | TenantId | Yes | Foreign key to Tenant | Must reference existing Tenant |
| RoleId | Guid | Yes | Foreign key to Role | Must reference existing Role |
| GrantedAt | DateTimeOffset | Yes | When membership was granted | Auto-set on creation |
| GrantedBy | Guid | No | UserId of who granted the membership | Must reference existing User |
| ExpiresAt | DateTimeOffset | No | When membership expires (optional) | Must be in future if set |
| IsActive | bool | Yes | Whether membership is active | Default: true |

**Relationships**:
- `User` (N:1 with User): Membership belongs to a User
- `Tenant` (N:1 with Tenant): Membership is within a Tenant
- `Role` (N:1 with Role): Membership includes a Role assignment

**Business Rules**:
- A user can have only one membership per tenant (unique constraint on UserId + TenantId)
- Expired memberships (ExpiresAt < now) are treated as inactive (enforced at query level)
- Inactive memberships do not grant permissions
- Cannot delete a membership for a user's last active tenant (must have at least one)

**Persistence**:
- Table: `Memberships`
- Indexes:
  - `UserId` (for user membership lookups)
  - `TenantId` (for tenant member queries)
  - `UserId, TenantId` (unique composite)
  - `ExpiresAt` (for cleanup jobs)

---

### Role

Represents a named set of permissions. Owned by the LMS Identity module but cached locally.

**Aggregate Root**: No (reference data from the LMS Identity module)

**Properties**:
| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | Guid | Yes | Unique identifier | From LMS Identity module |
| Name | string | Yes | Role name | Max 100 chars, unique |
| DisplayName | string | Yes | User-friendly role name | Max 200 chars |
| Description | string | No | Role description | Max 500 chars |
| Permissions | List<Permission> | Yes | Permissions granted by role | Serialized JSON |
| IsSystemRole | bool | Yes | Whether role is system-defined | System roles cannot be deleted |
| CreatedAt | DateTimeOffset | Yes | When role was created | From LMS Identity module |

**Note**: This is a read-only cache of roles from the LMS Identity module. Modifications happen in that module, and this system receives updates via domain events.

**Relationships**:
- `Memberships` (1:N with Membership): Role can be assigned to multiple memberships

**Business Rules**:
- Role names must be unique
- System roles (System Admin, District Admin, School Admin, Teacher, Parent) cannot be deleted or modified locally
- Permissions are validated against a known schema (defined by the LMS Identity module)

**Persistence**:
- Table: `Roles`
- Indexes:
  - `Name` (unique)
  - `IsSystemRole`

---

### Session

Represents an authenticated user's active access period. Manages tokens and expiration.

**Aggregate Root**: No (owned by User aggregate)

**Properties**:
| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | Guid | Yes | Unique identifier | Auto-generated |
| UserId | Guid | Yes | Foreign key to User | Must reference existing User |
| EntraTokenHash | string | Yes | SHA256 hash of Entra JWT | Hashed for security |
| LmsAccessToken | string | Yes | LMS-issued access token (JWT) | Encrypted at rest |
| ActiveTenantId | TenantId | Yes | Currently selected tenant context | Must reference user's membership tenant |
| CreatedAt | DateTimeOffset | Yes | When session was created | Auto-set on creation |
| ExpiresAt | DateTimeOffset | Yes | When session expires | Must be in future |
| LastActivityAt | DateTimeOffset | Yes | Last user activity (for sliding expiration) | Updated on each request |
| IsRevoked | bool | Yes | Whether session was manually revoked | Default: false |
| IpAddress | string | No | Client IP address | Max 45 chars (IPv6) |
| UserAgent | string | No | Client user agent | Max 500 chars |

**Relationships**:
- `User` (N:1 with User): Session belongs to a User
- `ActiveTenant` (N:1 with Tenant): Session has an active tenant context

**Business Rules**:
- ActiveTenantId must reference a tenant where user has active membership
- Expired sessions (ExpiresAt < now) cannot be used (enforced at validation)
- Revoked sessions cannot be used
- Session expiration extends on each activity (sliding expiration: +30 minutes)
- Token hashes allow checking if Entra token was already exchanged (idempotency)

**Persistence**:
- Table: `Sessions`
- Primary storage: **Redis** (for fast lookups during authorization)
- Secondary storage: **PostgreSQL** (for audit and revocation checks)
- Indexes (PostgreSQL):
  - `UserId` (for user session queries)
  - `ExpiresAt` (for cleanup jobs)
  - `IsRevoked`

**Redis Storage**:
- Key pattern: `session:{sessionId}`
- Value: JSON serialized session data
- TTL: Sliding expiration (30 minutes from last activity)

---

## Value Objects

### EntraSubjectId

Encapsulates Microsoft Entra ID subject identifier (from `sub` claim).

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| Value | string | Entra subject ID (GUID or opaque string) |

**Validation**:
- Must not be null or whitespace
- Max 256 characters
- Immutable after creation

**Equality**: Based on `Value` property

---

### TenantId

Encapsulates tenant identifier.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| Value | Guid | Tenant unique identifier |

**Validation**:
- Must not be empty GUID
- Immutable after creation

**Equality**: Based on `Value` property

---

### Permission

Encapsulates a single permission within a role.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| Resource | string | Protected resource (e.g., "Students", "Grades") |
| Action | string | Action allowed (e.g., "Read", "Write", "Delete") |

**Validation**:
- Resource and Action must not be null or whitespace
- Max 100 characters each
- Action must be one of: Read, Write, Delete, Manage (extensible)

**Equality**: Based on `Resource` and `Action` properties

---

## Domain Events

### UserAuthenticatedEvent

Raised when a user successfully authenticates via Entra.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| UserId | Guid | Authenticated user ID |
| SessionId | Guid | Created session ID |
| TenantId | TenantId | Initial active tenant |
| AuthenticatedAt | DateTimeOffset | When authentication occurred |

**Consumers**:
- Audit logging
- Analytics (track login patterns)

---

### TenantSwitchedEvent

Raised when a user switches their active tenant context.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| UserId | Guid | User ID |
| SessionId | Guid | Session ID |
| PreviousTenantId | TenantId | Previous active tenant |
| NewTenantId | TenantId | New active tenant |
| SwitchedAt | DateTimeOffset | When switch occurred |

**Consumers**:
- Audit logging
- Cache invalidation (clear authorization cache for old tenant)

---

### SessionExpiredEvent

Raised when a session expires (time-based or manual revocation).

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| SessionId | Guid | Expired session ID |
| UserId | Guid | User ID |
| Reason | SessionExpirationReason (Enum) | Why session expired |
| ExpiredAt | DateTimeOffset | When expiration occurred |

**Enumerations**:
```csharp
public enum SessionExpirationReason
{
    Timeout = 1,
    ManualRevocation = 2,
    SecurityPolicy = 3
}
```

**Consumers**:
- Audit logging
- Notification to user (via UI)

---

### RoleMembershipChangedEvent

**Source**: LMS Identity module (consumed, not produced by this system)

Raised when a user's role or tenant membership changes in the LMS Identity module.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| UserId | Guid | Affected user ID |
| TenantId | TenantId | Affected tenant ID |
| RoleId | Guid | Affected role ID |
| ChangeType | MembershipChangeType (Enum) | Type of change |
| ChangedAt | DateTimeOffset | When change occurred |

**Enumerations**:
```csharp
public enum MembershipChangeType
{
    Granted = 1,
    Revoked = 2,
    Updated = 3
}
```

**Consumers**:
- Cache invalidation (clear authorization cache for affected user/tenant)
- Local membership sync (update Membership entity)

---

## Entity Relationships Diagram

```
┌─────────────────────┐
│       User          │
│ (Aggregate Root)    │
├─────────────────────┤
│ Id (PK)             │
│ EntraSubjectId (UQ) │◄──────────┐
│ Email (UQ)          │           │
│ FirstName           │           │
│ LastName            │           │
│ DisplayName         │           │
│ IsActive            │           │
│ CreatedAt           │           │
│ LastLoginAt         │           │
└──────────┬──────────┘           │
           │                      │
           │ 1                    │
           │                      │
           │ N                    │
┌──────────▼──────────┐           │
│    Membership       │           │
├─────────────────────┤           │
│ Id (PK)             │           │
│ UserId (FK)         │───────────┘
│ TenantId (FK)       │───────────┐
│ RoleId (FK)         │──────┐    │
│ GrantedAt           │      │    │
│ GrantedBy           │      │    │
│ ExpiresAt           │      │    │
│ IsActive            │      │    │
└─────────────────────┘      │    │
                             │    │
┌─────────────────────┐      │    │
│       Role          │      │    │
├─────────────────────┤      │    │
│ Id (PK)             │◄─────┘    │
│ Name (UQ)           │           │
│ DisplayName         │           │
│ Description         │           │
│ Permissions (JSON)  │           │
│ IsSystemRole        │           │
│ CreatedAt           │           │
└─────────────────────┘           │
                                  │
┌─────────────────────┐           │
│      Tenant         │           │
│ (Aggregate Root)    │           │
├─────────────────────┤           │
│ Id (PK)             │◄──────────┘
│ Name                │
│ Type                │
│ ParentTenantId (FK) │───┐
│ ExternalId (UQ)     │   │
│ IsActive            │   │
│ CreatedAt           │   │
└──────────┬──────────┘   │
           │              │
           │ 1            │
           │              │
           └──────────────┘
           Self-reference (hierarchy)

┌─────────────────────┐
│      Session        │
├─────────────────────┤
│ Id (PK)             │
│ UserId (FK)         │───────► User
│ EntraTokenHash      │
│ LmsAccessToken      │
│ ActiveTenantId (FK) │───────► Tenant
│ CreatedAt           │
│ ExpiresAt           │
│ LastActivityAt      │
│ IsRevoked           │
│ IpAddress           │
│ UserAgent           │
└─────────────────────┘
```

---

## Database Schema (PostgreSQL)

### Users Table

```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    EntraSubjectId VARCHAR(256) NOT NULL UNIQUE,
    Email VARCHAR(256) NOT NULL UNIQUE,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    DisplayName VARCHAR(201) GENERATED ALWAYS AS (FirstName || ' ' || LastName) STORED,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    LastLoginAt TIMESTAMPTZ
);

CREATE INDEX IX_Users_IsActive ON Users(IsActive);
```

### Tenants Table

```sql
CREATE TABLE Tenants (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(200) NOT NULL,
    Type INTEGER NOT NULL, -- 1=District, 2=School
    ParentTenantId UUID REFERENCES Tenants(Id) ON DELETE SET NULL,
    ExternalId VARCHAR(100) UNIQUE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_Tenants_ParentTenantId ON Tenants(ParentTenantId);
CREATE INDEX IX_Tenants_Type ON Tenants(Type);
CREATE INDEX IX_Tenants_IsActive ON Tenants(IsActive);
```

### Roles Table

```sql
CREATE TABLE Roles (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE,
    DisplayName VARCHAR(200) NOT NULL,
    Description VARCHAR(500),
    Permissions JSONB NOT NULL,
    IsSystemRole BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IX_Roles_IsSystemRole ON Roles(IsSystemRole);
```

### Memberships Table

```sql
CREATE TABLE Memberships (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    TenantId UUID NOT NULL REFERENCES Tenants(Id) ON DELETE CASCADE,
    RoleId UUID NOT NULL REFERENCES Roles(Id) ON DELETE RESTRICT,
    GrantedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    GrantedBy UUID REFERENCES Users(Id) ON DELETE SET NULL,
    ExpiresAt TIMESTAMPTZ,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT UQ_Memberships_User_Tenant UNIQUE (UserId, TenantId)
);

CREATE INDEX IX_Memberships_UserId ON Memberships(UserId);
CREATE INDEX IX_Memberships_TenantId ON Memberships(TenantId);
CREATE INDEX IX_Memberships_ExpiresAt ON Memberships(ExpiresAt) WHERE ExpiresAt IS NOT NULL;
```

### Sessions Table (PostgreSQL - audit only; Redis is primary)

```sql
CREATE TABLE Sessions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    EntraTokenHash VARCHAR(64) NOT NULL, -- SHA256 hash
    LmsAccessToken TEXT NOT NULL, -- Encrypted
    ActiveTenantId UUID NOT NULL REFERENCES Tenants(Id) ON DELETE RESTRICT,
    CreatedAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ExpiresAt TIMESTAMPTZ NOT NULL,
    LastActivityAt TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    IsRevoked BOOLEAN NOT NULL DEFAULT FALSE,
    IpAddress VARCHAR(45),
    UserAgent VARCHAR(500)
);

CREATE INDEX IX_Sessions_UserId ON Sessions(UserId);
CREATE INDEX IX_Sessions_ExpiresAt ON Sessions(ExpiresAt);
CREATE INDEX IX_Sessions_IsRevoked ON Sessions(IsRevoked);
```

---

## Cache Schema (Redis)

### Session Cache

**Key Pattern**: `session:{sessionId}`

**Value**: JSON
```json
{
  "id": "guid",
  "userId": "guid",
  "activeTenantId": "guid",
  "expiresAt": "ISO8601 timestamp",
  "lastActivityAt": "ISO8601 timestamp",
  "isRevoked": false
}
```

**TTL**: Sliding 30 minutes (updated on each access)

---

### Authorization Decision Cache

**Key Pattern**: `authz:{userId}:{tenantId}:{resource}:{action}`

**Value**: JSON
```json
{
  "allowed": true,
  "checkedAt": "ISO8601 timestamp",
  "roleId": "guid"
}
```

**TTL**: 10 minutes (absolute)

---

### Tenant Facts Cache

**Key Pattern**: `tenant:{tenantId}`

**Value**: Redis Hash
```
Field: name          Value: "Lincoln High School"
Field: type          Value: "2"
Field: parentTenantId Value: "parent-guid"
Field: isActive      Value: "true"
```

**TTL**: 10 minutes (absolute)

---

## Validation Rules Summary

### User
- ✅ EntraSubjectId is unique and immutable
- ✅ Email is unique and valid format
- ✅ FirstName, LastName required, max lengths enforced
- ✅ Inactive users cannot authenticate

### Tenant
- ✅ Name required, max 200 chars
- ✅ Type must be valid enum value
- ✅ Districts cannot have ParentTenantId
- ✅ Schools should have ParentTenantId (warning if missing)
- ✅ Cannot delete tenants with active memberships

### Membership
- ✅ UserId + TenantId must be unique
- ✅ ExpiresAt must be in future if set
- ✅ User must have at least one active membership (cannot delete last)
- ✅ Expired memberships treated as inactive

### Session
- ✅ ActiveTenantId must reference tenant where user has membership
- ✅ ExpiresAt must be in future at creation
- ✅ Revoked sessions cannot be used
- ✅ Session extends on activity (sliding expiration)

---

## Migration Strategy

### Initial Seed Data

**System Roles** (from LMS Identity module):
1. System Admin - Full access across all tenants
2. District Admin - Full access within district tenant
3. School Admin - Full access within school tenant
4. Teacher - Read/Write access to assigned classes, students, grades
5. Parent - Read access to own children's data

**Sample Tenant Hierarchy**:
```
District: Springfield School District
  ├─ School: Lincoln High School
  ├─ School: Washington Middle School
  └─ School: Roosevelt Elementary
```

**Test Users** (development only):
- System Admin: `admin@example.com` (Entra test account)
- District Admin: `district.admin@springfield.edu` (Entra test account)
- School Admin: `school.admin@lincoln.edu` (Entra test account)

### EF Core Migrations

1. **Migration 1**: Create Users, Tenants, Roles, Memberships tables
2. **Migration 2**: Create Sessions table
3. **Migration 3**: Seed system roles and test tenants (development only)

---

## Appendix: Permission Examples

**System Admin Permissions**:
```json
[
  {"resource": "*", "action": "*"}
]
```

**District Admin Permissions** (within district tenant):
```json
[
  {"resource": "Schools", "action": "Manage"},
  {"resource": "Users", "action": "Manage"},
  {"resource": "Grades", "action": "Read"},
  {"resource": "Reports", "action": "Read"}
]
```

**Teacher Permissions** (within school tenant):
```json
[
  {"resource": "Students", "action": "Read"},
  {"resource": "Grades", "action": "Write"},
  {"resource": "Assignments", "action": "Manage"}
]
```

**Parent Permissions** (scoped to own children only):
```json
[
  {"resource": "Students", "action": "Read"},
  {"resource": "Grades", "action": "Read"},
  {"resource": "Assignments", "action": "Read"}
]
```

---

## Authorization Audit Log

### AuthorizationAuditLog

Tracks all authorization decisions for compliance, debugging, and security monitoring.

**Aggregate Root**: No (supporting entity)

**Properties**:
| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | Guid | Yes | Unique identifier | Auto-generated |
| UserId | Guid | Yes | User who requested authorization | FK to Users |
| TenantId | Guid | Yes | Tenant context for the decision | FK to Tenants |
| Resource | string | Yes | Resource being accessed | Max 100 chars |
| Action | string | Yes | Action being performed | Max 50 chars |
| Allowed | bool | Yes | Whether access was granted | - |
| Reason | string | No | Explanation for denial (if applicable) | Max 500 chars |
| Context | JSON | No | Additional context data | Stored as JSONB |
| Timestamp | DateTimeOffset | Yes | When decision was made | Auto-set on creation |
| DecisionLatencyMs | int | No | Time taken for decision in milliseconds | For performance monitoring |

**Relationships**:
- `User` (N:1 with User): Links to the user making the request
- `Tenant` (N:1 with Tenant): Links to the tenant context

**Business Rules**:
- Audit logs are immutable once created
- Logs should be retained according to compliance requirements (default: 90 days)
- High-volume logging may require partitioning or separate storage
- Sensitive context data must be redacted or encrypted

**Persistence**:
- Table: `AuthorizationAuditLogs`
- Indexes:
  - `UserId, Timestamp DESC` (for user audit trails)
  - `TenantId, Timestamp DESC` (for tenant audit trails)
  - `Timestamp DESC` (for time-based queries and retention)
  - `Resource, Action, Timestamp DESC` (for permission analysis)
- Partitioning: Consider monthly partitions for large-scale deployments

**Retention Policy**:
- **Default Retention**: 90 days
- **Compliance Override**: Can be extended to 365+ days for regulated environments
- **Archival**: Older records should be archived to cold storage (e.g., Azure Blob Storage)
- **Automated Cleanup**: Background job runs daily to purge records older than retention period

**Performance Considerations**:
- Audit logging is async to avoid blocking authorization decisions
- Batch inserts are used when possible to reduce database load
- Indexes are optimized for read queries (reporting, investigation)
- For high-traffic systems, consider using a dedicated audit database or event streaming

**Querying Examples**:
```sql
-- Find all denied access attempts for a user
SELECT * FROM identity.AuthorizationAuditLogs
WHERE UserId = 'user-guid' AND Allowed = false
ORDER BY Timestamp DESC
LIMIT 100;

-- Analyze permission usage by resource
SELECT Resource, Action, COUNT(*) as RequestCount, 
       AVG(DecisionLatencyMs) as AvgLatency
FROM identity.AuthorizationAuditLogs
WHERE Timestamp >= NOW() - INTERVAL '7 days'
GROUP BY Resource, Action
ORDER BY RequestCount DESC;

-- Identify slow authorization decisions
SELECT UserId, TenantId, Resource, Action, DecisionLatencyMs, Timestamp
FROM identity.AuthorizationAuditLogs
WHERE DecisionLatencyMs > 50
AND Timestamp >= NOW() - INTERVAL '1 day'
ORDER BY DecisionLatencyMs DESC;
```

---

*Note: Actual permission enforcement includes additional context (e.g., parent can only read their own children's data), handled by Application layer services.*
