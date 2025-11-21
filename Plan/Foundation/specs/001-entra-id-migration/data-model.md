# Data Model: Legacy IdentityServer to Microsoft Entra ID Migration

**Feature ID**: `001-entra-id-migration`  
**Parent Specification**: [Identity Service Data Model](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model-enhanced.md)  
**Focus**: Migration-specific schema changes and data transformations  
**Database**: `NorthStar_Identity_DB` (PostgreSQL 17)  
**Last Updated**: 2025-11-20

---

## Overview

This document describes **migration-specific** data model changes. The complete Identity Service data model (users, sessions, roles, audit) is defined in the [parent specification](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model-enhanced.md).

**Migration Scope**:
- Add `identity.external_provider_links` table (new)
- Add `identity.users.auth_deprecated_at` column (new)
- Preserve existing tables: `users`, `roles`, `user_roles`, `sessions`, `audit_records`

---

## New Table: external_provider_links

### Purpose
Link NorthStar user accounts to external identity providers (Microsoft Entra ID). Supports:
- Multiple identity providers per user (future: Google, Okta)
- Soft delete for rollback scenarios
- Audit trail (who created the link, when)

### Schema

```sql
CREATE TABLE identity.external_provider_links (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL,
    provider_subject_id VARCHAR(255) NOT NULL,
    provider_metadata JSONB NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID NULL REFERENCES identity.users(id),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT uk_provider_subject UNIQUE (provider, provider_subject_id)
);

COMMENT ON TABLE identity.external_provider_links IS 
    'Links NorthStar users to external identity providers (Entra ID, Google, etc.)';

COMMENT ON COLUMN identity.external_provider_links.provider IS 
    'Identity provider name: EntraID, Google, Okta, etc.';

COMMENT ON COLUMN identity.external_provider_links.provider_subject_id IS 
    'Provider-specific user identifier (e.g., Entra ID Object ID from oid claim)';

COMMENT ON COLUMN identity.external_provider_links.provider_metadata IS 
    'Optional JSON metadata from provider: {displayName, email, lastSyncedAt}';

COMMENT ON COLUMN identity.external_provider_links.is_active IS 
    'Soft delete flag. Set to false during rollback to preserve data.';
```

### Indexes

```sql
-- Lookup user links by user ID
CREATE INDEX ix_external_provider_links_user_id 
    ON identity.external_provider_links(user_id);

-- Lookup user by provider + subject ID (authentication flow)
CREATE INDEX ix_external_provider_links_provider_subject 
    ON identity.external_provider_links(provider, provider_subject_id);

-- Find active links only
CREATE INDEX ix_external_provider_links_active 
    ON identity.external_provider_links(is_active) 
    WHERE is_active = TRUE;
```

### Sample Data (Post-Migration)

```sql
-- Teacher user linked to Entra ID
INSERT INTO identity.external_provider_links VALUES (
    '550e8400-e29b-41d4-a716-446655440001',  -- id
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890',  -- user_id (from identity.users)
    'EntraID',                                -- provider
    '12345678-abcd-1234-abcd-1234567890ab',  -- provider_subject_id (Entra oid)
    '{"displayName": "John Doe", "email": "john.doe@district.edu", "lastSyncedAt": "2025-11-20T10:30:00Z"}', -- provider_metadata
    '2025-11-20 10:30:00+00',                -- created_at
    NULL,                                     -- created_by (automated migration)
    TRUE                                      -- is_active
);

-- Admin user manually linked by administrator
INSERT INTO identity.external_provider_links VALUES (
    '550e8400-e29b-41d4-a716-446655440002',  -- id
    'b2c3d4e5-f6a7-8901-bcde-f12345678901',  -- user_id
    'EntraID',
    '23456789-bcde-2345-bcde-2345678901bc',
    '{"displayName": "Jane Smith", "email": "jsmith@district.edu", "note": "Manual link due to email mismatch"}',
    '2025-11-20 14:00:00+00',
    'c3d4e5f6-a7b8-9012-cdef-123456789012',  -- created_by (admin user ID)
    TRUE
);
```

---

## Modified Table: users

### New Column: auth_deprecated_at

```sql
ALTER TABLE identity.users 
ADD COLUMN auth_deprecated_at TIMESTAMPTZ NULL;

COMMENT ON COLUMN identity.users.auth_deprecated_at IS 
    'Timestamp when legacy password authentication was deprecated. NULL = legacy auth still active. Set during Entra ID migration.';
```

**Purpose**: Mark users whose authentication has migrated to Entra ID. Legacy password hashes remain in the database (for audit/rollback) but are no longer valid for authentication.

**Usage in Authentication Handler**:
```csharp
public async Task<IActionResult> AuthenticateAsync(LoginRequest request)
{
    var user = await _userRepository.GetByEmailAsync(request.Email);
    
    // Block legacy password authentication for migrated users
    if (user.AuthDeprecatedAt.HasValue)
    {
        return Unauthorized(new {
            error = "authentication_modernized",
            message = "Password authentication is no longer supported. Please use 'Sign in with Microsoft'.",
            migratedAt = user.AuthDeprecatedAt
        });
    }
    
    // Legacy authentication logic (for non-migrated users)...
}
```

---

## Data Migration Flow

### Before Migration

```
identity.users
┌──────────┬────────────────────┬─────────────────────┬──────────────────────┐
│ id       │ email              │ password_hash       │ auth_deprecated_at   │
├──────────┼────────────────────┼─────────────────────┼──────────────────────┤
│ abc-123  │ john@district.edu  │ $2a$10$...          │ NULL                 │
│ def-456  │ jane@district.edu  │ $2a$10$...          │ NULL                 │
└──────────┴────────────────────┴─────────────────────┴──────────────────────┘

identity.external_provider_links
(empty table)
```

### After Migration

```
identity.users
┌──────────┬────────────────────┬─────────────────────┬──────────────────────┐
│ id       │ email              │ password_hash       │ auth_deprecated_at   │
├──────────┼────────────────────┼─────────────────────┼──────────────────────┤
│ abc-123  │ john@district.edu  │ $2a$10$... (kept!)  │ 2025-11-20T10:30:00Z │
│ def-456  │ jane@district.edu  │ $2a$10$... (kept!)  │ 2025-11-20T10:30:00Z │
└──────────┴────────────────────┴─────────────────────┴──────────────────────┘

identity.external_provider_links
┌──────────┬──────────┬──────────┬───────────────────────┬───────────┐
│ id       │ user_id  │ provider │ provider_subject_id   │ is_active │
├──────────┼──────────┼──────────┼───────────────────────┼───────────┤
│ xyz-001  │ abc-123  │ EntraID  │ entra-oid-1234...     │ TRUE      │
│ xyz-002  │ def-456  │ EntraID  │ entra-oid-5678...     │ TRUE      │
└──────────┴──────────┴──────────┴───────────────────────┴───────────┘
```

**Key Points**:
- ✅ Password hashes **preserved** (marked deprecated, not deleted)
- ✅ ExternalProviderLinks **created** (Entra ID Object ID stored)
- ✅ User IDs **unchanged** (preserves foreign key relationships)

---

## Entity Framework Core Entities

### ExternalProviderLink (New Entity)

```csharp
namespace NorthStar.Identity.Domain.Entities;

public class ExternalProviderLink : EntityBase
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = null!; // "EntraID", "Google", etc.
    public string ProviderSubjectId { get; set; } = null!; // Entra oid claim
    public JsonDocument? ProviderMetadata { get; set; } // Optional metadata
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; } // Admin ID for manual links
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User? Creator { get; set; }
}
```

### User Entity (Modified)

```csharp
namespace NorthStar.Identity.Domain.Entities;

public class User : EntityBase
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? PasswordHash { get; set; } // Nullable after migration
    public DateTime? AuthDeprecatedAt { get; set; } // NEW COLUMN
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<ExternalProviderLink> ExternalProviderLinks { get; set; } = new List<ExternalProviderLink>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
```

### EF Core Configuration

```csharp
public class ExternalProviderLinkConfiguration : IEntityTypeConfiguration<ExternalProviderLink>
{
    public void Configure(EntityTypeBuilder<ExternalProviderLink> builder)
    {
        builder.ToTable("external_provider_links", "identity");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(e => e.ProviderSubjectId)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(e => e.ProviderMetadata)
            .HasColumnType("jsonb");
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        // Unique constraint: one link per provider + subject
        builder.HasIndex(e => new { e.Provider, e.ProviderSubjectId })
            .IsUnique()
            .HasDatabaseName("uk_provider_subject");
        
        // Foreign key to User
        builder.HasOne(e => e.User)
            .WithMany(u => u.ExternalProviderLinks)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Optional foreign key to Creator (admin)
        builder.HasOne(e => e.Creator)
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

---

## EF Core Migration Script

```bash
# Create migration
dotnet ef migrations add AddEntraIdMigrationSupport \
    --project Src/Foundation/services/Identity/Identity.Infrastructure \
    --startup-project Src/Foundation/services/Identity/Identity.API \
    --context IdentityDbContext

# Apply to database
dotnet ef database update \
    --project Src/Foundation/services/Identity/Identity.Infrastructure \
    --startup-project Src/Foundation/services/Identity/Identity.API \
    --context IdentityDbContext
```

**Generated Migration**:
```csharp
public partial class AddEntraIdMigrationSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "auth_deprecated_at",
            schema: "identity",
            table: "users",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "external_provider_links",
            schema: "identity",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                provider_subject_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                provider_metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_external_provider_links", x => x.id);
                table.ForeignKey(
                    name: "FK_external_provider_links_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "identity",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_external_provider_links_users_created_by",
                    column: x => x.created_by,
                    principalSchema: "identity",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "ix_external_provider_links_user_id",
            schema: "identity",
            table: "external_provider_links",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "uk_provider_subject",
            schema: "identity",
            table: "external_provider_links",
            columns: new[] { "provider", "provider_subject_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "external_provider_links",
            schema: "identity");

        migrationBuilder.DropColumn(
            name: "auth_deprecated_at",
            schema: "identity",
            table: "users");
    }
}
```

---

## Rollback Data State

### During Rollback

```sql
-- Mark links as inactive (preserve data)
UPDATE identity.external_provider_links 
SET is_active = FALSE;

-- Re-enable legacy authentication
UPDATE identity.users 
SET auth_deprecated_at = NULL;
```

**Result**: Users can authenticate with legacy passwords again. ExternalProviderLinks remain in database (inactive) for potential retry.

---

## Performance Considerations

### Query Patterns

**Authentication Lookup** (P95 < 50ms):
```sql
-- Find user by Entra ID Object ID
SELECT u.* 
FROM identity.users u
INNER JOIN identity.external_provider_links epl ON u.id = epl.user_id
WHERE epl.provider = 'EntraID' 
  AND epl.provider_subject_id = 'entra-oid-12345...'
  AND epl.is_active = TRUE
LIMIT 1;

-- Uses index: ix_external_provider_links_provider_subject
```

**User Profile with Links**:
```sql
-- Get user with all provider links
SELECT u.*, 
       jsonb_agg(jsonb_build_object(
           'provider', epl.provider,
           'subjectId', epl.provider_subject_id,
           'createdAt', epl.created_at
       )) as provider_links
FROM identity.users u
LEFT JOIN identity.external_provider_links epl ON u.id = epl.user_id AND epl.is_active = TRUE
WHERE u.id = 'user-guid-here'
GROUP BY u.id;
```

---

## References

- **Parent Data Model**: [Identity Service Data Model](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model-enhanced.md)  
- **Migration Plan**: [Migration Implementation Plan](./plan.md)  
- **Research**: [Migration Strategy Research](./research.md)
