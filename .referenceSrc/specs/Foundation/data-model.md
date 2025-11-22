# Data Model: Phase 1 Foundation Services

**Date**: 2025-11-19  
**Services**: Identity & Authentication, Configuration  
**Storage**: PostgreSQL 16+ with Row-Level Security

---

## Overview

This document defines the domain entities, value objects, and database schema for Phase 1 Foundation Services. Each service maintains its own database with multi-tenant isolation enforced through `tenant_id` columns and PostgreSQL Row-Level Security policies.

---

## Identity & Authentication Service

**Database**: `northstar_identity`

### Domain Entities

#### User

Represents an authenticated user account in the system.

**Properties**:
- `Id` (Guid, PK): Unique user identifier
- `TenantId` (Guid, FK): District/tenant association
- `Email` (string, unique per tenant): User email address (username)
- `EmailConfirmed` (bool): Email verification status
- `PasswordHash` (string, nullable): BCrypt password hash (null for Entra ID-only users)
- `FirstName` (string): User first name
- `LastName` (string): User last name
- `PhoneNumber` (string, nullable): Contact phone number
- `IsActive` (bool): Account active status
- `IsLocked` (bool): Account locked due to failed login attempts
- `LockoutEnd` (DateTime?, nullable): When lockout expires
- `FailedLoginAttempts` (int): Counter for login failures
- `LastLoginAt` (DateTime?, nullable): Last successful login timestamp
- `CreatedAt` (DateTime): Account creation timestamp
- `UpdatedAt` (DateTime): Last update timestamp

**Relationships**:
- One-to-Many with `UserRole` (user has multiple roles)
- One-to-Many with `UserClaim` (user has multiple claims)
- One-to-Many with `RefreshToken` (user has multiple refresh tokens)
- One-to-One with `ExternalProviderLink` (optional Entra ID linkage)

**Validation Rules**:
- Email must be valid format and unique within tenant
- PasswordHash required unless ExternalProviderLink exists
- FirstName and LastName required, 2-100 characters
- LockoutEnd must be future date if set
- FailedLoginAttempts reset to 0 on successful login

**Indexes**:
- `idx_users_tenant_id` on (TenantId)
- `idx_users_email` on (Email, TenantId) UNIQUE
- `idx_users_is_active` on (IsActive, TenantId)

---

#### Role

Represents an authorization role with associated permissions.

**Properties**:
- `Id` (Guid, PK): Unique role identifier
- `TenantId` (Guid, FK, nullable): Tenant-specific role (null for system roles)
- `Name` (string): Role name (e.g., "District Admin", "School Admin", "Teacher")
- `NormalizedName` (string): Uppercase name for case-insensitive lookup
- `Description` (string): Role description
- `IsSystemRole` (bool): True for built-in roles (cannot be deleted)
- `CreatedAt` (DateTime): Role creation timestamp

**Relationships**:
- One-to-Many with `UserRole` (role assigned to multiple users)
- One-to-Many with `RoleClaim` (role has multiple claims/permissions)

**System Roles** (IsSystemRole = true, TenantId = null):
- `System Administrator` - Full system access
- `District Administrator` - District-wide management
- `School Administrator` - School-level management
- `Teacher` - Classroom management
- `Staff` - General staff access
- `Student` - Student portal access (Phase 2)
- `Parent` - Parent portal access (Phase 2)

**Validation Rules**:
- Name required, 2-100 characters, unique within tenant scope
- System roles cannot be modified or deleted
- NormalizedName auto-generated from Name

**Indexes**:
- `idx_roles_tenant_id` on (TenantId)
- `idx_roles_normalized_name` on (NormalizedName, TenantId) UNIQUE

---

#### UserRole

Junction entity linking users to roles.

**Properties**:
- `UserId` (Guid, FK): User identifier
- `RoleId` (Guid, FK): Role identifier
- `AssignedAt` (DateTime): When role was assigned
- `AssignedBy` (Guid, nullable): User who assigned the role

**Validation Rules**:
- UserId + RoleId must be unique (composite key)
- Both User and Role must exist

**Indexes**:
- `pk_user_roles` PRIMARY KEY (UserId, RoleId)
- `idx_user_roles_role_id` on (RoleId)

---

#### Claim

Represents a specific permission or attribute.

**Properties**:
- `Id` (Guid, PK): Unique claim identifier
- `Type` (string): Claim type (e.g., "Permission", "Attribute")
- `Value` (string): Claim value (e.g., "CanCreateDistrict", "CanEditStudent")
- `Description` (string): Human-readable claim description

**Standard Permission Claims**:
- `CanManageDistricts` - Create/edit/delete districts
- `CanManageSchools` - Create/edit/delete schools
- `CanManageUsers` - Create/edit/delete users
- `CanManageRoles` - Assign/revoke roles
- `CanViewReports` - Access reporting features (Phase 4)
- `CanManageStudents` - Student management (Phase 2)
- `CanManageAssessments` - Assessment management (Phase 2)

**Indexes**:
- `idx_claims_type_value` on (Type, Value) UNIQUE

---

#### UserClaim / RoleClaim

Junction entities linking claims to users or roles.

**Properties**:
- `UserId` / `RoleId` (Guid, FK): User or Role identifier
- `ClaimId` (Guid, FK): Claim identifier
- `GrantedAt` (DateTime): When claim was granted

**Indexes**:
- `pk_user_claims` PRIMARY KEY (UserId, ClaimId)
- `pk_role_claims` PRIMARY KEY (RoleId, ClaimId)

---

#### RefreshToken

Represents a long-lived token for obtaining new access tokens.

**Properties**:
- `Id` (Guid, PK): Unique token identifier
- `UserId` (Guid, FK): Associated user
- `Token` (string): Secure random token value (hashed)
- `ExpiresAt` (DateTime): Token expiration timestamp
- `CreatedAt` (DateTime): Token creation timestamp
- `RevokedAt` (DateTime?, nullable): When token was revoked
- `ReplacedByToken` (Guid?, nullable): New token that replaced this one

**Validation Rules**:
- Token must be cryptographically secure (256-bit minimum)
- ExpiresAt must be future date
- Revoked tokens cannot be used

**Indexes**:
- `idx_refresh_tokens_user_id` on (UserId)
- `idx_refresh_tokens_token` on (Token) UNIQUE
- `idx_refresh_tokens_expires_at` on (ExpiresAt)

---

#### ExternalProviderLink

Represents linkage between local user and Microsoft Entra ID.

**Properties**:
- `Id` (Guid, PK): Unique link identifier
- `UserId` (Guid, FK): Local user identifier
- `Provider` (string): External provider name ("MicrosoftEntraId")
- `ProviderUserId` (string): External user identifier (Entra ID Object ID)
- `ProviderEmail` (string): Email from external provider
- `LastSyncAt` (DateTime): Last synchronization timestamp
- `CreatedAt` (DateTime): Link creation timestamp

**Validation Rules**:
- UserId + Provider must be unique (one Entra ID link per user)
- ProviderUserId required, unique across all tenants
- Provider must be valid provider name

**Indexes**:
- `idx_external_provider_links_user_id` on (UserId) UNIQUE
- `idx_external_provider_links_provider_user_id` on (ProviderUserId) UNIQUE

---

### Value Objects

#### Email
- Pattern: RFC 5322 compliant email format
- Max length: 254 characters
- Normalized to lowercase

#### PasswordHash
- Algorithm: BCrypt with work factor 12
- Minimum password requirements: 8 characters, 1 uppercase, 1 lowercase, 1 number

---

### Database Schema (PostgreSQL)

```sql
-- Identity Service Database
CREATE DATABASE northstar_identity;

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    email VARCHAR(254) NOT NULL,
    email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    password_hash VARCHAR(255),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone_number VARCHAR(20),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_locked BOOLEAN NOT NULL DEFAULT FALSE,
    lockout_end TIMESTAMP WITH TIME ZONE,
    failed_login_attempts INT NOT NULL DEFAULT 0,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uk_users_email_tenant UNIQUE (email, tenant_id)
);

CREATE INDEX idx_users_tenant_id ON users(tenant_id);
CREATE INDEX idx_users_is_active ON users(is_active, tenant_id);

-- Row-Level Security
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON users
    USING (tenant_id = current_setting('app.tenant_id')::UUID);

-- Roles table
CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID,
    name VARCHAR(100) NOT NULL,
    normalized_name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    is_system_role BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_roles_tenant_id ON roles(tenant_id);
CREATE UNIQUE INDEX idx_roles_normalized_name ON roles(normalized_name, tenant_id);

ALTER TABLE roles ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON roles
    USING (tenant_id IS NULL OR tenant_id = current_setting('app.tenant_id')::UUID);

-- User-Role junction
CREATE TABLE user_roles (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    assigned_by UUID,
    PRIMARY KEY (user_id, role_id)
);

CREATE INDEX idx_user_roles_role_id ON user_roles(role_id);

-- Claims table
CREATE TABLE claims (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type VARCHAR(100) NOT NULL,
    value VARCHAR(255) NOT NULL,
    description VARCHAR(500),
    CONSTRAINT uk_claims_type_value UNIQUE (type, value)
);

-- User-Claim junction
CREATE TABLE user_claims (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    claim_id UUID NOT NULL REFERENCES claims(id) ON DELETE CASCADE,
    granted_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    PRIMARY KEY (user_id, claim_id)
);

-- Role-Claim junction
CREATE TABLE role_claims (
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    claim_id UUID NOT NULL REFERENCES claims(id) ON DELETE CASCADE,
    granted_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    PRIMARY KEY (role_id, claim_id)
);

-- Refresh tokens
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMP WITH TIME ZONE,
    replaced_by_token UUID
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);

-- External provider links
CREATE TABLE external_provider_links (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL,
    provider_user_id VARCHAR(255) NOT NULL,
    provider_email VARCHAR(254) NOT NULL,
    last_sync_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uk_external_provider_links_user_id UNIQUE (user_id),
    CONSTRAINT uk_external_provider_links_provider_user_id UNIQUE (provider_user_id)
);

CREATE INDEX idx_external_provider_links_user_id ON external_provider_links(user_id);
```

---

## Configuration Service

**Database**: `northstar_configuration`

### Domain Entities

#### District

Represents a school district (tenant).

**Properties**:
- `Id` (Guid, PK): Unique district identifier (also serves as TenantId)
- `Name` (string): District name
- `Code` (string): Unique district code (for external integrations)
- `State` (string): US state (2-letter code, e.g., "CA", "TX")
- `TimeZone` (string): IANA timezone (e.g., "America/Los_Angeles")
- `IsActive` (bool): District active status
- `ContactEmail` (string): Primary contact email
- `ContactPhone` (string): Primary contact phone
- `CreatedAt` (DateTime): District creation timestamp
- `UpdatedAt` (DateTime): Last update timestamp

**Relationships**:
- One-to-Many with `School` (district has multiple schools)
- One-to-Many with `Calendar` (district can have district-wide calendars)
- One-to-Many with `GradeLevel` (district defines grade levels)

**Validation Rules**:
- Name required, 2-200 characters
- Code required, 2-50 characters, unique across all districts
- State must be valid US state code
- TimeZone must be valid IANA timezone
- ContactEmail must be valid email format

**Indexes**:
- `idx_districts_code` on (Code) UNIQUE
- `idx_districts_is_active` on (IsActive)

---

#### School

Represents a school within a district.

**Properties**:
- `Id` (Guid, PK): Unique school identifier
- `TenantId` (Guid, FK): District identifier
- `DistrictId` (Guid, FK): Same as TenantId (for clarity)
- `Name` (string): School name
- `Code` (string): Unique school code within district
- `SchoolType` (enum): Elementary, Middle, High, K12, Charter
- `PrincipalName` (string, nullable): Principal full name
- `PrincipalEmail` (string, nullable): Principal email
- `AddressLine1` (string): Street address
- `AddressLine2` (string, nullable): Apartment/suite
- `City` (string): City
- `State` (string): US state (2-letter code)
- `ZipCode` (string): ZIP code
- `PhoneNumber` (string): School main phone
- `IsActive` (bool): School active status
- `EnrollmentCount` (int): Current enrollment (cached)
- `CreatedAt` (DateTime): School creation timestamp
- `UpdatedAt` (DateTime): Last update timestamp

**Relationships**:
- Many-to-One with `District` (school belongs to district)
- One-to-Many with `Calendar` (school-specific calendars)

**Validation Rules**:
- Name required, 2-200 characters
- Code required, 2-50 characters, unique within district
- SchoolType must be valid enum value
- AddressLine1, City, State, ZipCode required
- PhoneNumber must be valid phone format

**Indexes**:
- `idx_schools_tenant_id` on (TenantId)
- `idx_schools_code` on (Code, TenantId) UNIQUE
- `idx_schools_is_active` on (IsActive, TenantId)

---

#### Calendar

Represents an academic calendar for a school year.

**Properties**:
- `Id` (Guid, PK): Unique calendar identifier
- `TenantId` (Guid, FK): District identifier
- `DistrictId` (Guid, FK): District identifier (null for school-specific)
- `SchoolId` (Guid, FK, nullable): School identifier (null for district-wide)
- `SchoolYear` (string): Academic year (e.g., "2024-2025")
- `StartDate` (DateOnly): First day of school year
- `EndDate` (DateOnly): Last day of school year
- `TotalInstructionalDays` (int): Expected instructional days
- `Name` (string): Calendar name (e.g., "Standard Calendar", "Year-Round")
- `IsDefault` (bool): Default calendar for district/school
- `CreatedAt` (DateTime): Calendar creation timestamp
- `UpdatedAt` (DateTime): Last update timestamp

**Relationships**:
- Many-to-One with `District`
- Many-to-One with `School` (optional)
- One-to-Many with `CalendarDay` (calendar has multiple days)

**Validation Rules**:
- SchoolYear required, format "YYYY-YYYY"
- StartDate must be before EndDate
- TotalInstructionalDays must be 1-200
- Either DistrictId or SchoolId must be set (not both)
- Only one IsDefault calendar per district/school

**Indexes**:
- `idx_calendars_tenant_id` on (TenantId)
- `idx_calendars_school_year` on (SchoolYear, TenantId)
- `idx_calendars_school_id` on (SchoolId)

---

#### CalendarDay

Represents a specific day within an academic calendar.

**Properties**:
- `Id` (Guid, PK): Unique day identifier
- `TenantId` (Guid, FK): District identifier
- `CalendarId` (Guid, FK): Calendar identifier
- `Date` (DateOnly): Specific date
- `DayType` (enum): Instructional, Holiday, EarlyDismissal, ProfessionalDevelopment, WeatherClosure
- `Description` (string, nullable): Holiday/event name
- `IsInstructional` (bool): Counts toward instructional days
- `DismissalTime` (TimeOnly, nullable): Early dismissal time

**Validation Rules**:
- Date must be within Calendar StartDate and EndDate
- DayType required
- IsInstructional = true only for Instructional or EarlyDismissal types
- DismissalTime required if DayType = EarlyDismissal

**Indexes**:
- `idx_calendar_days_tenant_id` on (TenantId)
- `idx_calendar_days_calendar_id` on (CalendarId)
- `idx_calendar_days_date` on (Date, CalendarId) UNIQUE

---

#### GradeLevel

Represents a grade level definition within a district.

**Properties**:
- `Id` (Guid, PK): Unique grade level identifier
- `TenantId` (Guid, FK): District identifier
- `Name` (string): Grade level name (e.g., "PK", "K", "1", "2", ..., "12")
- `Sequence` (int): Sort order (0 for PK, 1 for K, 2 for 1st, etc.)
- `IsActive` (bool): Active status
- `CreatedAt` (DateTime): Creation timestamp

**Standard Grade Levels**:
- PK (Pre-Kindergarten), K (Kindergarten), 1-12

**Validation Rules**:
- Name required, 1-20 characters
- Sequence must be unique within district
- Name must be unique within district

**Indexes**:
- `idx_grade_levels_tenant_id` on (TenantId)
- `idx_grade_levels_sequence` on (Sequence, TenantId) UNIQUE

---

#### SystemSetting

Represents configurable settings at district, school, or user level.

**Properties**:
- `Id` (Guid, PK): Unique setting identifier
- `TenantId` (Guid, FK): District identifier
- `Scope` (enum): District, School, User
- `ScopeId` (Guid, nullable): District/School/User identifier
- `Category` (string): Setting category (e.g., "Notifications", "Display")
- `Key` (string): Setting key (e.g., "EmailNotificationsEnabled")
- `Value` (string): Setting value (JSON serialized)
- `DataType` (enum): Boolean, String, Integer, Decimal, Json
- `UpdatedBy` (Guid): User who last updated
- `UpdatedAt` (DateTime): Last update timestamp

**Validation Rules**:
- Key required, unique within Scope + ScopeId + Category
- Value must match DataType
- ScopeId required if Scope = School or User

**Indexes**:
- `idx_system_settings_tenant_id` on (TenantId)
- `idx_system_settings_scope` on (Scope, ScopeId, Category, Key) UNIQUE

---

### Database Schema (PostgreSQL)

```sql
-- Configuration Service Database
CREATE DATABASE northstar_configuration;

-- Districts table
CREATE TABLE districts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    code VARCHAR(50) NOT NULL UNIQUE,
    state VARCHAR(2) NOT NULL,
    time_zone VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    contact_email VARCHAR(254) NOT NULL,
    contact_phone VARCHAR(20),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_districts_is_active ON districts(is_active);

-- Note: Districts table does NOT use RLS (it IS the tenant definition)

-- Schools table
CREATE TABLE schools (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES districts(id),
    district_id UUID NOT NULL REFERENCES districts(id),
    name VARCHAR(200) NOT NULL,
    code VARCHAR(50) NOT NULL,
    school_type VARCHAR(20) NOT NULL,
    principal_name VARCHAR(200),
    principal_email VARCHAR(254),
    address_line1 VARCHAR(200) NOT NULL,
    address_line2 VARCHAR(200),
    city VARCHAR(100) NOT NULL,
    state VARCHAR(2) NOT NULL,
    zip_code VARCHAR(10) NOT NULL,
    phone_number VARCHAR(20) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    enrollment_count INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uk_schools_code_tenant UNIQUE (code, tenant_id)
);

CREATE INDEX idx_schools_tenant_id ON schools(tenant_id);
CREATE INDEX idx_schools_is_active ON schools(is_active, tenant_id);

ALTER TABLE schools ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON schools
    USING (tenant_id = current_setting('app.tenant_id')::UUID);

-- Calendars table
CREATE TABLE calendars (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES districts(id),
    district_id UUID NOT NULL REFERENCES districts(id),
    school_id UUID REFERENCES schools(id),
    school_year VARCHAR(10) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_instructional_days INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_calendars_tenant_id ON calendars(tenant_id);
CREATE INDEX idx_calendars_school_year ON calendars(school_year, tenant_id);
CREATE INDEX idx_calendars_school_id ON calendars(school_id);

ALTER TABLE calendars ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON calendars
    USING (tenant_id = current_setting('app.tenant_id')::UUID);

-- Calendar days table
CREATE TABLE calendar_days (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    calendar_id UUID NOT NULL REFERENCES calendars(id) ON DELETE CASCADE,
    date DATE NOT NULL,
    day_type VARCHAR(50) NOT NULL,
    description VARCHAR(200),
    is_instructional BOOLEAN NOT NULL DEFAULT FALSE,
    dismissal_time TIME,
    CONSTRAINT uk_calendar_days_date UNIQUE (date, calendar_id)
);

CREATE INDEX idx_calendar_days_tenant_id ON calendar_days(tenant_id);
CREATE INDEX idx_calendar_days_calendar_id ON calendar_days(calendar_id);

ALTER TABLE calendar_days ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON calendar_days
    USING (tenant_id = current_setting('app.tenant_id')::UUID);

-- Grade levels table
CREATE TABLE grade_levels (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES districts(id),
    name VARCHAR(20) NOT NULL,
    sequence INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uk_grade_levels_sequence UNIQUE (sequence, tenant_id)
);

CREATE INDEX idx_grade_levels_tenant_id ON grade_levels(tenant_id);

ALTER TABLE grade_levels ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON grade_levels
    USING (tenant_id = current_setting('app.tenant_id')::UUID);

-- System settings table
CREATE TABLE system_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES districts(id),
    scope VARCHAR(20) NOT NULL,
    scope_id UUID,
    category VARCHAR(100) NOT NULL,
    key VARCHAR(100) NOT NULL,
    value TEXT NOT NULL,
    data_type VARCHAR(20) NOT NULL,
    updated_by UUID NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    CONSTRAINT uk_system_settings_key UNIQUE (scope, scope_id, category, key)
);

CREATE INDEX idx_system_settings_tenant_id ON system_settings(tenant_id);

ALTER TABLE system_settings ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_policy ON system_settings
    USING (tenant_id = current_setting('app.tenant_id')::UUID);
```

---

## API Gateway

**Note**: The API Gateway (YARP) does not have its own database. It relies on Identity service for authentication validation and routes requests to backend services.

**In-Memory State**:
- Circuit breaker states (per destination)
- Rate limiting counters (per client)
- Route configuration (loaded from appsettings.json)

---

## Entity Relationship Diagram

```
Identity Service:
┌─────────────┐
│   District  │────────┐
│ (Config DB) │        │
└─────────────┘        │ tenant_id
                       ↓
┌──────────┐     ┌──────────┐     ┌──────────────────────┐
│   User   │────→│ UserRole │←────│        Role          │
└──────────┘     └──────────┘     └──────────────────────┘
     │                                     │
     │                                     │
     ↓                                     ↓
┌────────────────┐               ┌──────────────┐
│  RefreshToken  │               │  RoleClaim   │
└────────────────┘               └──────────────┘
     │                                     │
     │                                     ↓
     │                             ┌──────────────┐
     │                             │    Claim     │
     │                             └──────────────┘
     │                                     ↑
     ↓                                     │
┌──────────────────────┐          ┌──────────────┐
│ExternalProviderLink  │          │  UserClaim   │
└──────────────────────┘          └──────────────┘

Configuration Service:
┌──────────────┐
│   District   │────────┬──────────────────────────┐
└──────────────┘        │                          │
     │                  │                          │
     │ 1:N              │ 1:N                      │ 1:N
     ↓                  ↓                          ↓
┌──────────────┐  ┌──────────────┐        ┌──────────────┐
│   School     │  │   Calendar   │        │  GradeLevel  │
└──────────────┘  └──────────────┘        └──────────────┘
     │                  │                          
     │ 1:N              │ 1:N                      
     ↓                  ↓                          
┌──────────────┐  ┌──────────────┐               
│   Calendar   │  │ CalendarDay  │               
│  (School)    │  └──────────────┘               
└──────────────┘                                  
     │                                            
     │ 1:N                                        
     ↓                                            
┌──────────────┐                                 
│ CalendarDay  │                                 
└──────────────┘                                 

┌──────────────┐
│SystemSetting │ (standalone, scoped to District/School/User)
└──────────────┘
```

---

## Data Migration Considerations

### From Legacy LoginContext (SQL Server)

**Tables to Migrate**:
- `AspNetUsers` → `users`
- `AspNetRoles` → `roles`
- `AspNetUserRoles` → `user_roles`
- `AspNetUserClaims` → `user_claims`
- `AspNetRoleClaims` → `role_claims`

**Transformations**:
- Add `tenant_id` column (derive from district association)
- Merge IdentityServer-specific columns into `users` table
- Convert ASP.NET Identity GUID format to standard UUID
- Migrate password hashes (already BCrypt compatible)

### From Legacy DistrictContext (SQL Server)

**Tables to Migrate**:
- `Districts` → `districts` (identity preserved)
- `Schools` → `schools` (add `tenant_id`)
- `Calendars` → `calendars` (add `tenant_id`)
- `Grades` → `grade_levels` (add `tenant_id`)

**Data Consolidation**:
- Multiple district databases → Single multi-tenant configuration database
- Preserve all historical data
- Maintain referential integrity with composite keys including `tenant_id`

---

## Summary

- **2 databases**: `northstar_identity`, `northstar_configuration`
- **15 tables total**: 9 Identity + 6 Configuration
- **Multi-tenant isolation**: Row-Level Security + application filtering
- **Foreign key relationships**: Enforced at database level
- **Audit trails**: `created_at`, `updated_at`, `updated_by` columns
- **Performance**: Indexed on tenant_id, primary keys, unique constraints

**Ready for Phase 1: API Contracts Generation**
