# Data Model: Identity Service with Entra ID

**Feature ID**: `01-identity-service-entra-id`  
**Target Layer**: Foundation (Implementation in `Src/Foundation/services/Identity/`)  
**Database**: `NorthStar_Identity_DB` (PostgreSQL 17)  
**Schema Version**: 1.0.0  
**Created**: 2025-11-20  
**Status**: Ready for Implementation

---

## Table of Contents

- [Overview](#overview)
- [Entity Relationship Diagram](#entity-relationship-diagram)
- [Domain Entities](#domain-entities)
- [Value Objects](#value-objects)
- [Database Schema](#database-schema)
- [Indexes & Performance](#indexes--performance)
- [Multi-Tenancy Strategy](#multi-tenancy-strategy)
- [Audit & Compliance](#audit--compliance)
- [Migration Scripts](#migration-scripts)

---

## Overview

The Identity Service data model supports Microsoft Entra ID-based authentication with the following key features:

- **User Management**: Core user accounts with tenant isolation
- **Session Management**: Active sessions linked to Entra ID subject IDs
- **Role-Based Access Control (RBAC)**: Roles with JSONB permissions
- **External Provider Links**: Mapping between NorthStar users and Entra ID accounts
- **Security Auditing**: Comprehensive logging of all authentication and authorization events

**⚠️ IMPORTANT: Authentication vs Session Management**
- **Authentication**: Handled entirely by Microsoft Entra ID - NO local password storage or validation
- **Session Management**: NorthStar stores session metadata (user ID, tenant context, expiration) to support server-side session invalidation and multi-tenant context switching
- **Access Token Hash**: Used for session validation and revocation, NOT password storage. Tokens are issued by Entra ID and hashed for secure session tracking.

**Design Principles**:
- ✅ **Clean Architecture**: Domain entities have no infrastructure dependencies
- ✅ **Multi-Tenancy**: All tables include `tenant_id` with global query filters
- ✅ **Soft Delete**: Users and roles support logical deletion (`deleted_at` timestamp)
- ✅ **Audit Trail**: All state changes logged to `audit_records` table
- ✅ **Performance**: Strategic indexing for <100ms query latency (P95)

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          Identity Domain Model                           │
└─────────────────────────────────────────────────────────────────────────┘

┌───────────────────┐
│   districts       │ (Foundation Layer - District/Tenant)
│  (tenant)         │
├───────────────────┤
│ id (PK)           │───┐
│ name              │   │
│ slug              │   │
│ created_at        │   │
└───────────────────┘   │
                        │
                        │ FK: tenant_id
                        │
┌───────────────────┐   │        ┌───────────────────┐
│   users           │◀──┘        │   roles           │
│                   │            │                   │
├───────────────────┤            ├───────────────────┤
│ id (PK)           │───┐    ┌──│ id (PK)           │
│ tenant_id (FK)    │   │    │  │ tenant_id (FK)    │
│ email             │   │    │  │ role_name         │
│ display_name      │   │    │  │ permissions       │ (JSONB)
│ created_at        │   │    │  │ description       │
│ updated_at        │   │    │  │ created_at        │
│ deleted_at        │   │    │  └───────────────────┘
└───────────────────┘   │    │
         │              │    │
         │ FK: user_id  │    │ FK: role_id
         │              │    │
         ▼              │    │
┌───────────────────┐   │    │  ┌───────────────────┐
│   sessions        │   │    │  │   user_roles      │
│                   │   │    │  │                   │
├───────────────────┤   │    │  ├───────────────────┤
│ id (PK)           │   │    ├─▶│ user_id (FK)      │
│ user_id (FK)      │◀──┘    └─▶│ role_id (FK)      │
│ entra_subject_id  │           │ tenant_id (FK)    │
│ tenant_id (FK)    │           │ assigned_at       │
│ access_token_hash │           │ assigned_by (FK)  │
│ expires_at        │           └───────────────────┘
│ created_at        │
│ refreshed_at      │
│ ip_address        │
│ user_agent        │
└───────────────────┘
         │
         │ FK: user_id
         │
         ▼
┌──────────────────────────┐
│ external_provider_links  │
│                          │
├──────────────────────────┤
│ user_id (FK, PK)         │
│ provider (PK)            │ ('EntraID')
│ external_user_id         │ (Entra 'sub' claim)
│ email                    │
│ last_sync                │
│ tenant_id (FK)           │
└──────────────────────────┘

         ┌───────────────────┐
         │   audit_records   │
         │                   │
         ├───────────────────┤
         │ id (PK)           │
         │ user_id (FK)      │
         │ event_type        │ ('UserAuthenticated', 'UserLoggedOut', etc.)
         │ tenant_id (FK)    │
         │ ip_address        │
         │ timestamp         │
         │ details           │ (JSONB)
         └───────────────────┘
```

---

## Domain Entities

### User (Aggregate Root)

**Purpose**: Represents a NorthStar LMS user (staff, admin, or future student).

**Properties**:
```csharp
public class User : Entity<Guid>, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } // Unique per tenant
    public string DisplayName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; } // Soft delete
    
    // Navigation properties
    public virtual ICollection<Session> Sessions { get; private set; }
    public virtual ICollection<UserRole> UserRoles { get; private set; }
    public virtual ExternalProviderLink? EntraIdLink { get; private set; }
    
    // Factory method
    public static User Create(Guid tenantId, string email, string displayName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email.ToLowerInvariant(),
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        user.AddDomainEvent(new UserCreatedEvent(user.Id, tenantId, email));
        return user;
    }
    
    // Business logic
    public void UpdateProfile(string displayName)
    {
        DisplayName = displayName;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserProfileUpdatedEvent(Id, displayName));
    }
    
    public void SoftDelete()
    {
        if (DeletedAt != null)
            throw new InvalidOperationException("User already deleted");
        
        DeletedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeletedEvent(Id, TenantId));
    }
    
    public bool IsActive() => DeletedAt == null;
}
```

**Business Rules**:
- Email must be unique within a tenant (enforced by unique index)
- Display name required (FluentValidation: 1-255 characters)
- Soft delete sets `deleted_at` timestamp (records retained for audit)
- Cannot delete user with active sessions (check before soft delete)

---

### Session

**Purpose**: Represents an active user session with Entra ID linkage.

**Properties**:
```csharp
public class Session : Entity<string>
{
    public Guid UserId { get; private set; }
    public string EntraSubjectId { get; private set; } // Entra ID 'sub' claim
    public Guid TenantId { get; private set; }
    public string AccessTokenHash { get; private set; } // SHA256 hash for validation
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime RefreshedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    
    // Navigation property
    public virtual User User { get; private set; }
    
    // Factory method
    public static Session Create(
        User user,
        string entraSubjectId,
        string accessToken,
        TimeSpan sessionDuration,
        string? ipAddress,
        string? userAgent)
    {
        var sessionId = $"lms_session_{Guid.NewGuid()}";
        var tokenHash = HashAccessToken(accessToken);
        
        var session = new Session
        {
            Id = sessionId,
            UserId = user.Id,
            EntraSubjectId = entraSubjectId,
            TenantId = user.TenantId,
            AccessTokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.Add(sessionDuration),
            CreatedAt = DateTime.UtcNow,
            RefreshedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        
        session.AddDomainEvent(new SessionCreatedEvent(sessionId, user.Id, user.TenantId));
        return session;
    }
    
    // Business logic
    public void Refresh(TimeSpan extensionDuration)
    {
        ExpiresAt = DateTime.UtcNow.Add(extensionDuration);
        RefreshedAt = DateTime.UtcNow;
        AddDomainEvent(new SessionRefreshedEvent(Id, UserId, ExpiresAt));
    }
    
    public void Terminate()
    {
        ExpiresAt = DateTime.UtcNow; // Immediate expiration
        AddDomainEvent(new SessionTerminatedEvent(Id, UserId));
    }
    
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    
    public void SwitchTenant(Guid newTenantId)
    {
        var oldTenantId = TenantId;
        TenantId = newTenantId;
        AddDomainEvent(new TenantContextSwitchedEvent(UserId, oldTenantId, newTenantId));
    }
    
    private static string HashAccessToken(string token)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}
```

**Business Rules**:
- Session ID format: `lms_session_{guid}` (validated by value object)
- **Access token hash**: SHA256 hash of Entra ID-issued access token for session validation and revocation (NOT password hashing - all authentication is via Entra ID)
- Expiration: 8 hours for staff, 1 hour for admins (sliding window)
- Cannot refresh session within 1 minute of last refresh (rate limiting)

---

### Role

**Purpose**: Defines a role with associated permissions for RBAC.

**Properties**:
```csharp
public class Role : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string RoleName { get; private set; } // "Teacher", "Administrator", "DistrictAdmin"
    public string Permissions { get; private set; } // JSONB: ["students.read", "students.write"]
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation property
    public virtual ICollection<UserRole> UserRoles { get; private set; }
    
    // Factory method
    public static Role Create(Guid tenantId, string roleName, string[] permissions, string? description = null)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RoleName = roleName,
            Permissions = System.Text.Json.JsonSerializer.Serialize(permissions),
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
        
        role.AddDomainEvent(new RoleCreatedEvent(role.Id, tenantId, roleName));
        return role;
    }
    
    // Business logic
    public string[] GetPermissions() =>
        System.Text.Json.JsonSerializer.Deserialize<string[]>(Permissions) ?? Array.Empty<string>();
    
    public void UpdatePermissions(string[] permissions)
    {
        Permissions = System.Text.Json.JsonSerializer.Serialize(permissions);
        AddDomainEvent(new RolePermissionsUpdatedEvent(Id, TenantId, permissions));
    }
    
    public bool HasPermission(string permission) =>
        GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
}
```

**Business Rules**:
- Role name must be unique within a tenant
- Permissions stored as JSONB array (PostgreSQL native support)
- Permission format: `{resource}.{action}` (e.g., `students.read`, `assessments.write`)
- Cannot delete role if users assigned (check `user_roles` before delete)

---

### UserRole (Link Table)

**Purpose**: Many-to-many relationship between Users and Roles (per tenant).

**Properties**:
```csharp
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedBy { get; set; } // User who assigned the role
    
    // Navigation properties
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
    public virtual User? Assigner { get; set; }
}
```

**Business Rules**:
- Composite primary key: (user_id, role_id, tenant_id)
- Cannot assign same role twice to user in same tenant
- Assigner must have `roles.assign` permission

---

### ExternalProviderLink

**Purpose**: Links NorthStar user to Entra ID account.

**Properties**:
```csharp
public class ExternalProviderLink : Entity<(Guid UserId, string Provider)>
{
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } // 'EntraID'
    public string ExternalUserId { get; private set; } // Entra 'sub' claim
    public string Email { get; private set; }
    public DateTime LastSync { get; private set; }
    public Guid TenantId { get; private set; }
    
    // Navigation property
    public virtual User User { get; private set; }
    
    // Factory method
    public static ExternalProviderLink CreateEntraIdLink(
        User user,
        string entraSubjectId,
        string entraEmail)
    {
        var link = new ExternalProviderLink
        {
            UserId = user.Id,
            Provider = "EntraID",
            ExternalUserId = entraSubjectId,
            Email = entraEmail.ToLowerInvariant(),
            LastSync = DateTime.UtcNow,
            TenantId = user.TenantId
        };
        
        link.AddDomainEvent(new ExternalProviderLinkedEvent(user.Id, "EntraID", entraSubjectId));
        return link;
    }
    
    // Business logic
    public void UpdateSync()
    {
        LastSync = DateTime.UtcNow;
    }
}
```

**Business Rules**:
- Composite primary key: (user_id, provider)
- One user can have multiple provider links (future: Google, Facebook)
- External user ID must be unique per provider (enforced by index)

---

### AuditRecord

**Purpose**: Security audit log for all authentication and authorization events.

**Properties**:
```csharp
public class AuditRecord : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } // 'UserAuthenticated', 'UserLoggedOut', etc.
    public Guid? TenantId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Details { get; private set; } // JSONB: additional event context
    
    // Factory method
    public static AuditRecord Create(
        string eventType,
        Guid? userId = null,
        Guid? tenantId = null,
        string? ipAddress = null,
        object? details = null)
    {
        return new AuditRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType,
            TenantId = tenantId,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            Details = details != null 
                ? System.Text.Json.JsonSerializer.Serialize(details) 
                : null
        };
    }
}
```

**Business Rules**:
- Immutable (insert-only, no updates)
- Retention: 90 days in PostgreSQL, 7 years in Azure Blob (cold storage)
- Anonymous events allowed (e.g., failed login before user identified)

---

## Value Objects

### SessionId

```csharp
public record SessionId
{
    private const string Prefix = "lms_session_";
    
    public string Value { get; }
    
    public SessionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Session ID cannot be empty", nameof(value));
        
        if (!value.StartsWith(Prefix))
            throw new ArgumentException($"Session ID must start with '{Prefix}'", nameof(value));
        
        if (!Guid.TryParse(value.Substring(Prefix.Length), out _))
            throw new ArgumentException("Session ID must contain valid GUID", nameof(value));
        
        Value = value;
    }
    
    public static SessionId Create() => new($"{Prefix}{Guid.NewGuid()}");
    
    public override string ToString() => Value;
}
```

### EntraSubjectId

```csharp
public record EntraSubjectId
{
    public string Value { get; }
    
    public EntraSubjectId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Entra subject ID cannot be empty", nameof(value));
        
        if (value.Length > 255)
            throw new ArgumentException("Entra subject ID too long", nameof(value));
        
        Value = value;
    }
    
    public override string ToString() => Value;
}
```

---

## Database Schema

### SQL Schema (PostgreSQL 17)

```sql
-- Schema creation
CREATE SCHEMA IF NOT EXISTS identity;

-- Users table
CREATE TABLE identity.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL, -- FK to tenants.districts(id)
    email VARCHAR(255) NOT NULL,
    display_name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMP NULL,
    
    CONSTRAINT uq_users_tenant_email UNIQUE (tenant_id, email)
);

-- Sessions table
CREATE TABLE identity.sessions (
    id VARCHAR(255) PRIMARY KEY, -- Format: lms_session_{guid}
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    entra_subject_id VARCHAR(255) NOT NULL,
    tenant_id UUID NOT NULL,
    access_token_hash VARCHAR(64) NOT NULL, -- SHA256 hash
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    refreshed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ip_address INET,
    user_agent TEXT,
    
    CONSTRAINT chk_session_id_format CHECK (id ~ '^lms_session_[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$')
);

-- Roles table
CREATE TABLE identity.roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    role_name VARCHAR(100) NOT NULL,
    permissions JSONB NOT NULL DEFAULT '[]'::jsonb,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uq_roles_tenant_name UNIQUE (tenant_id, role_name)
);

-- User Roles link table
CREATE TABLE identity.user_roles (
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES identity.roles(id) ON DELETE CASCADE,
    tenant_id UUID NOT NULL,
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_by UUID REFERENCES identity.users(id),
    
    PRIMARY KEY (user_id, role_id, tenant_id)
);

-- External Provider Links table
CREATE TABLE identity.external_provider_links (
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL, -- 'EntraID'
    external_user_id VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    last_sync TIMESTAMP NOT NULL DEFAULT NOW(),
    tenant_id UUID NOT NULL,
    
    PRIMARY KEY (user_id, provider)
);

-- Audit Records table
CREATE TABLE identity.audit_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES identity.users(id),
    event_type VARCHAR(100) NOT NULL,
    tenant_id UUID,
    ip_address INET,
    timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    details JSONB
);

-- Comments for documentation
COMMENT ON TABLE identity.users IS 'Core user accounts with tenant isolation';
COMMENT ON TABLE identity.sessions IS 'Active user sessions linked to Entra ID';
COMMENT ON TABLE identity.roles IS 'RBAC role definitions with JSONB permissions';
COMMENT ON TABLE identity.user_roles IS 'Many-to-many link between users and roles';
COMMENT ON TABLE identity.external_provider_links IS 'Mapping between NorthStar users and Entra ID accounts';
COMMENT ON TABLE identity.audit_records IS 'Security audit log for authentication and authorization events';
```

---

## Indexes & Performance

### Primary Indexes (Auto-Created)

- `users.id` (PRIMARY KEY) → B-tree
- `sessions.id` (PRIMARY KEY) → B-tree
- `roles.id` (PRIMARY KEY) → B-tree
- `user_roles.(user_id, role_id, tenant_id)` (PRIMARY KEY) → B-tree
- `external_provider_links.(user_id, provider)` (PRIMARY KEY) → B-tree
- `audit_records.id` (PRIMARY KEY) → B-tree

### Performance Indexes (Explicit)

```sql
-- Session validation (P95 <100ms from DB)
CREATE INDEX idx_sessions_id_active ON identity.sessions(id)
WHERE expires_at > NOW();

-- User lookup by email during token exchange
CREATE INDEX idx_users_tenant_email ON identity.users(tenant_id, email)
WHERE deleted_at IS NULL;

-- User roles lookup for authorization (P95 <50ms)
CREATE INDEX idx_user_roles_user_tenant ON identity.user_roles(user_id, tenant_id);

-- Session refresh query (background service runs every 4 minutes)
CREATE INDEX idx_sessions_expiring_soon ON identity.sessions(expires_at)
WHERE expires_at > NOW() AND expires_at < NOW() + INTERVAL '5 minutes';

-- External provider lookup (Entra ID subject → NorthStar user)
CREATE INDEX idx_external_links_provider_external_id ON identity.external_provider_links(provider, external_user_id);

-- Audit queries by user
CREATE INDEX idx_audit_records_user_timestamp ON identity.audit_records(user_id, timestamp DESC);

-- Audit queries by tenant
CREATE INDEX idx_audit_records_tenant_timestamp ON identity.audit_records(tenant_id, timestamp DESC);

-- Audit queries by event type
CREATE INDEX idx_audit_records_event_type ON identity.audit_records(event_type, timestamp DESC);
```

**Index Sizing Estimates**:
- 10,000 active users → ~2MB index size
- 10,000 active sessions → ~500KB index size
- 1M audit records (90 days) → ~50MB index size

**Query Performance Targets**:
- Session validation by ID: P95 <100ms (database), P95 <20ms (Redis cache)
- User lookup by email: P95 <50ms
- User roles lookup: P95 <50ms
- Session refresh query: <1 second (max 1000 expiring sessions)

---

## Multi-Tenancy Strategy

### Global Query Filter (EF Core)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply tenant filter to all tenant-scoped entities
    modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == CurrentTenantId);
    modelBuilder.Entity<Session>().HasQueryFilter(s => s.TenantId == CurrentTenantId);
    modelBuilder.Entity<Role>().HasQueryFilter(r => r.TenantId == CurrentTenantId);
    modelBuilder.Entity<UserRole>().HasQueryFilter(ur => ur.TenantId == CurrentTenantId);
    
    // Audit records: NO filter (cross-tenant queries for compliance)
}
```

### Tenant Interceptor

```csharp
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var currentTenantId = _tenantProvider.GetCurrentTenantId();
        
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry.Entity is ITenantScoped tenantScoped && entry.State == EntityState.Added)
            {
                tenantScoped.TenantId = currentTenantId;
            }
        }
        
        return base.SavingChanges(eventData, result);
    }
}
```

**Tenant Isolation Validation**:
- Unit tests verify global query filter applied
- Integration tests attempt cross-tenant access (expect failure)
- Regression tests run after each schema change

---

## Audit & Compliance

### Logged Events

| Event Type | Logged Fields | Retention |
|------------|---------------|-----------|
| `UserAuthenticated` | userId, tenantId, ipAddress, timestamp | 90 days hot, 7 years cold |
| `UserLoggedOut` | userId, timestamp, reason (explicit/timeout) | 90 days hot, 7 years cold |
| `SessionRefreshed` | sessionId, userId, newExpiresAt | 90 days hot |
| `TenantContextSwitched` | userId, fromTenantId, toTenantId, timestamp | 90 days hot, 7 years cold |
| `AuthenticationFailed` | email, ipAddress, timestamp, reason | 90 days hot, 7 years cold |
| `UnauthorizedTenantAccess` | userId, targetTenantId, timestamp | 90 days hot, 7 years cold |
| `UserRoleAssigned` | userId, roleId, tenantId, assignedBy | 7 years cold |
| `UserRoleRevoked` | userId, roleId, tenantId, revokedBy | 7 years cold |

### Compliance Requirements

- **FERPA**: All student data access logged (extends to staff authentication)
- **COPPA**: Student authentication via school-issued accounts only (no self-registration)
- **SOC 2 Type II**: Entra ID provides compliance, NorthStar maintains audit trail
- **GDPR** (if applicable): User data deletion via soft delete, audit log retention

### Cold Storage Strategy

```sql
-- Scheduled job (runs monthly)
-- Move audit records older than 90 days to Azure Blob Storage
INSERT INTO azure_blob_archive.audit_records
SELECT * FROM identity.audit_records
WHERE timestamp < NOW() - INTERVAL '90 days';

DELETE FROM identity.audit_records
WHERE timestamp < NOW() - INTERVAL '90 days';
```

---

## Migration Scripts

### EF Core Migration Commands

```bash
# Create initial migration
cd Src/Foundation/services/Identity/Identity.Infrastructure
dotnet ef migrations add InitialCreate \
  --startup-project ../Identity.API \
  --context IdentityDbContext \
  --output-dir Data/Migrations

# Apply migration (dev)
dotnet ef database update \
  --startup-project ../Identity.API \
  --context IdentityDbContext

# Generate SQL script (prod)
dotnet ef migrations script \
  --startup-project ../Identity.API \
  --context IdentityDbContext \
  --output migrations.sql
```

### Seed Data (Development)

```sql
-- Seed tenant
INSERT INTO tenants.districts (id, name, slug, created_at)
VALUES ('00000000-0000-0000-0000-000000000001', 'Demo District', 'demo', NOW());

-- Seed roles
INSERT INTO identity.roles (id, tenant_id, role_name, permissions, description)
VALUES
  ('10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 'Administrator', 
   '["*"]', 'Full system access'),
  ('10000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001', 'Teacher', 
   '["students.read", "students.write", "assessments.read", "assessments.write"]', 'Classroom teacher'),
  ('10000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000001', 'ReadOnly', 
   '["*.read"]', 'Read-only access to all resources');

-- Seed test user
INSERT INTO identity.users (id, tenant_id, email, display_name, created_at, updated_at)
VALUES ('20000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001', 
        'admin@demo.edu', 'Admin User', NOW(), NOW());

-- Assign admin role
INSERT INTO identity.user_roles (user_id, role_id, tenant_id, assigned_at)
VALUES ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 
        '00000000-0000-0000-0000-000000000001', NOW());
```

---

## Appendix

### EF Core Entity Configurations

**UserConfiguration.cs**:
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(u => u.DisplayName)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasFilter("deleted_at IS NULL");
        
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
```

**SessionConfiguration.cs**:
```csharp
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions", "identity");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(s => s.EntraSubjectId)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(s => s.AccessTokenHash)
            .HasMaxLength(64)
            .IsRequired();
        
        builder.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(s => s.Id)
            .HasFilter("expires_at > NOW()");
    }
}
```

---

**Data Model Version**: 1.0.0  
**Last Updated**: 2025-11-20  
**Next Review**: After Phase 1 implementation
