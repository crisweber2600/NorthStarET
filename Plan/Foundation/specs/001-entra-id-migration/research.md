# Research: Legacy IdentityServer to Microsoft Entra ID Migration

**Feature ID**: `001-entra-id-migration`  
**Parent Specification**: [Identity Service with Entra ID](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/README.md)  
**Research Focus**: Migration-specific technical decisions and data mapping strategies  
**Last Updated**: 2025-11-20

---

## Relationship to Parent Specification

**⚠️ IMPORTANT**: Core authentication architecture research is documented in the [parent specification's research file](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/research.md). This document focuses exclusively on **migration-specific** decisions:

- User data mapping strategies
- Email matching algorithms
- Legacy data preservation
- Migration tooling selection
- Rollback procedures

**For authentication architecture research** (OAuth flows, token validation, session management), refer to the parent specification.

---

## Migration Strategy Research

### 1. User Matching Approach

**Decision**: Email-based matching as primary strategy

**Alternatives Considered**:

| Approach | Pros | Cons | Selected |
|----------|------|------|----------|
| **Email matching** | Simple, high accuracy (95%+), deterministic | Fails for email mismatches | ✅ **Yes** (Primary) |
| **Name + DOB matching** | Works without email | Low accuracy, ambiguous matches | ❌ No |
| **Manual mapping** | 100% accurate | Does not scale, requires admin effort | ✅ **Yes** (Fallback) |
| **SSN matching** | High accuracy | Privacy concerns, not all users have SSN | ❌ No |

**Rationale**: Email addresses are the de facto unique identifier in both systems. NorthStar requires email for account creation, and Entra ID uses email (UserPrincipalName) as the primary login identifier. Expected match rate: ≥95% based on data audit.

**Edge Cases**:
- Email mismatch (e.g., `john.doe@district.edu` vs `jdoe@district.edu`): Handle via fuzzy matching + manual review
- No email in legacy system: Flag for manual review, require admin to create Entra ID account
- Duplicate emails: Log error, require admin to resolve data integrity issue before migration

---

### 2. Legacy Password Handling

**Decision**: Deprecate passwords, do NOT delete or migrate

**Rationale**:
- **Security**: Migrating password hashes defeats the purpose of delegating auth to Entra ID
- **Compliance**: Retaining hashes (marked deprecated) maintains audit trail for security reviews
- **Reversibility**: If rollback needed, hashes remain available for legacy auth re-enablement

**Implementation**:
```sql
-- Add deprecation timestamp (migration Phase 2)
ALTER TABLE identity.users ADD COLUMN auth_deprecated_at TIMESTAMPTZ;

-- Mark migrated users
UPDATE identity.users 
SET auth_deprecated_at = NOW() 
WHERE id IN (SELECT user_id FROM identity.external_provider_links);

-- Enforce deprecation in authentication handler
IF user.AuthDeprecatedAt != null THEN
    RETURN Unauthorized("Authentication modernized. Use 'Sign in with Microsoft'");
END IF;
```

**Future Cleanup**: After 90 days post-migration (assuming no rollback), schedule password hash purge:
```sql
-- CAUTION: Irreversible operation
UPDATE identity.users SET password_hash = NULL 
WHERE auth_deprecated_at < NOW() - INTERVAL '90 days';
```

---

### 3. Role Preservation Strategy

**Decision**: Preserve existing role assignments in `identity.user_roles`, validate against Entra ID app roles post-migration

**Data Flow**:
```
Legacy IdentityServer              NorthStar Identity DB
─────────────────────              ─────────────────────
UserRoles                          identity.user_roles
  UserId: abc-123          →          user_id: abc-123
  Role: "Teacher"                     role_id: {Teacher_role_guid}
  TenantId: District_A               tenant_id: {District_A_guid}
                                      assigned_at: {migration_timestamp}
                                      assigned_by: NULL (system migration)

Entra ID (Validation)
─────────────────────
App Role Assignment
  User: john.doe@district.edu
  App Role: "Teacher" (optional match)
```

**Validation Logic**:
1. **Preserve legacy roles** in `identity.user_roles` table (guaranteed)
2. **Optionally sync Entra ID app roles** if configured:
   - Query Entra ID: `GET /users/{id}/appRoleAssignments`
   - If Entra role exists but not in NorthStar: Add role (additive sync)
   - If NorthStar role exists but not in Entra: Keep NorthStar role (preserve legacy)
3. **Authorization priority**: NorthStar roles take precedence (Entra roles are supplementary)

**Rationale**: Entra ID app roles are optional. Not all districts will configure them. NorthStar must function with role assignments defined in the application database.

---

### 4. ExternalProviderLinks Schema Design

**Decision**: Single table for all external identity providers (future-proof)

**Schema**:
```sql
CREATE TABLE identity.external_provider_links (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL, -- 'EntraID', 'Google', 'Okta' (future)
    provider_subject_id VARCHAR(255) NOT NULL, -- Provider's user identifier
    provider_metadata JSONB NULL, -- Optional: store displayName, email from provider
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID NULL REFERENCES identity.users(id), -- Manual link creator
    is_active BOOLEAN NOT NULL DEFAULT TRUE, -- Soft delete for rollback
    CONSTRAINT uk_provider_subject UNIQUE (provider, provider_subject_id)
);

CREATE INDEX ix_external_provider_links_user_id 
    ON identity.external_provider_links(user_id);

CREATE INDEX ix_external_provider_links_provider_subject 
    ON identity.external_provider_links(provider, provider_subject_id);
```

**Key Design Decisions**:
- `provider_subject_id`: Stores Entra ID's `oid` (Object ID) claim, which is immutable even if email changes
- `provider_metadata`: Optional JSONB for storing snapshot of user info from provider (useful for debugging mismatches)
- `is_active`: Enables soft delete for rollback (set to `false` instead of deleting record)
- `UNIQUE (provider, provider_subject_id)`: Prevents duplicate links, enforces one link per provider identity

**Alternative Considered**: Separate table per provider (`entra_id_links`, `google_links`)  
**Rejected**: Does not scale, violates DRY principle, complicates queries for users with multiple providers

---

### 5. Microsoft Graph API Integration

**Purpose**: Query Entra ID during migration to retrieve user Object IDs

**API Endpoint**: `GET https://graph.microsoft.com/v1.0/users?$filter=mail eq '{email}'`

**Authentication**: Service Principal (Application Permission)
- Permission required: `User.Read.All` (application permission, not delegated)
- Register app in Entra ID: "NorthStar Migration Service"
- Store client secret in Azure Key Vault

**Rate Limiting**:
- Microsoft Graph throttling: ~2,000 requests/minute for `User.Read.All`
- Migration strategy: Batch requests in groups of 20 users (using `$batch` endpoint)
- Estimated migration time: 1,000 users = ~50 batches = ~2 minutes

**Sample Code**:
```csharp
public async Task<EntraUser?> GetUserByEmailAsync(string email)
{
    var request = _graphClient.Users
        .Request()
        .Filter($"mail eq '{email}' or userPrincipalName eq '{email}'")
        .Select("id,displayName,mail,userPrincipalName")
        .Top(1);
    
    var result = await request.GetAsync();
    return result.FirstOrDefault();
}
```

**Fallback**: If Graph API unavailable, use cached export of Entra ID users (CSV export from Azure Portal)

---

### 6. Rollback Strategy

**Decision**: Preserve all legacy data, use soft delete pattern for reversibility

**Rollback Triggers**:
- Authentication failure rate >5% post-migration
- Critical bug in session management
- Data integrity issue (e.g., mass role deletion)

**Rollback Procedure** (See [plan.md](./plan.md) Phase 4 for detailed script):
1. Set `identity.external_provider_links.is_active = FALSE` (do NOT delete)
2. Clear `identity.users.auth_deprecated_at` (re-enable legacy auth)
3. Revert authentication configuration (re-enable IdentityServer endpoints)
4. Invalidate all Entra ID-based sessions (clear Redis cache)
5. Log rollback event in `audit_records`

**Testing**: Rollback tested in staging as part of Phase 4 (see plan.md)

---

### 7. Fuzzy Email Matching Algorithm

**Purpose**: Suggest potential matches for users with slight email mismatches

**Algorithm**: Levenshtein Distance (edit distance)

**Implementation**:
```csharp
public static int LevenshteinDistance(string s1, string s2)
{
    var matrix = new int[s1.Length + 1, s2.Length + 1];
    
    for (int i = 0; i <= s1.Length; i++) matrix[i, 0] = i;
    for (int j = 0; j <= s2.Length; j++) matrix[0, j] = j;
    
    for (int i = 1; i <= s1.Length; i++)
    {
        for (int j = 1; j <= s2.Length; j++)
        {
            int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + cost
            );
        }
    }
    
    return matrix[s1.Length, s2.Length];
}

// Suggest matches with distance ≤ 3
var suggestions = entraUsers
    .Where(eu => LevenshteinDistance(legacyEmail, eu.Mail) <= 3)
    .OrderBy(eu => LevenshteinDistance(legacyEmail, eu.Mail));
```

**Example Matches**:
- `john.smith@district.edu` → `jsmith@district.edu` (distance: 5, **not matched**)
- `john.smith@district.edu` → `john.smth@district.edu` (distance: 2, **suggested**)
- `teacher@district.edu` → `techer@district.edu` (distance: 1, **suggested**)

**Threshold**: Distance ≤ 3 for suggestions (admin must confirm)

---

## Migration Tooling

### Selected Tools

| Tool | Purpose | Justification |
|------|---------|---------------|
| **Entity Framework Core** | Database migrations | Consistent with project standards, type-safe schema changes |
| **Microsoft.Graph SDK** | Entra ID API queries | Official SDK, handles auth and throttling |
| **Serilog** | Migration logging | Structured logging, output to file and console |
| **CsvHelper** | Report generation | Export migration results to CSV for auditing |
| **Polly** | Retry logic | Handle transient Graph API failures |

### CLI Migration Tool

**Command Structure**:
```bash
dotnet run --project Src/Foundation/services/Identity/Migration/ \
    --command migrate-users \
    --environment Staging \
    --dry-run true \
    --output-report migration-report.csv

# Options:
# --command: migrate-users | rollback | validate
# --environment: Development | Staging | Production
# --dry-run: true (preview) | false (execute)
# --output-report: Path to CSV report
```

**Output Example**:
```
[INFO] Starting user migration (dry-run: true)
[INFO] Loading legacy users from database: 1,000 found
[INFO] Querying Entra ID via Microsoft Graph API...
[INFO] Matched users: 950 (95.0%)
[WARN] Unmatched users: 50 (5.0%)
[INFO] Generating report: migration-report.csv
[INFO] Migration complete (dry-run, no changes committed)
```

---

## Key Decisions Summary

1. ✅ **Email-based matching** as primary strategy (95%+ accuracy expected)
2. ✅ **Preserve legacy passwords** (mark deprecated, do NOT delete)
3. ✅ **Keep NorthStar role assignments** (Entra ID app roles are supplementary)
4. ✅ **Single ExternalProviderLinks table** for all providers (future-proof)
5. ✅ **Microsoft Graph API** for Entra ID queries (with retry logic)
6. ✅ **Soft delete pattern** for rollback safety (set `is_active = false`)
7. ✅ **Levenshtein distance** for fuzzy email matching (threshold: ≤3)
8. ✅ **CLI migration tool** with dry-run mode for safety

---

## References

### Parent Specification
- [Identity Service Research](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/research.md) - Core authentication architecture research

### Microsoft Documentation
- [Microsoft Graph Users API](https://learn.microsoft.com/en-us/graph/api/resources/user)
- [Entra ID Object ID (oid claim)](https://learn.microsoft.com/en-us/entra/identity-platform/id-token-claims-reference#payload-claims)
- [Application Permissions vs Delegated](https://learn.microsoft.com/en-us/entra/identity-platform/permissions-consent-overview)

### Technical References
- [Levenshtein Distance Algorithm](https://en.wikipedia.org/wiki/Levenshtein_distance)
- [CsvHelper Library](https://joshclose.github.io/CsvHelper/)
- [Polly Retry Policies](https://github.com/App-vNext/Polly)

---

**Next Steps**:  
1. Review and approve research decisions  
2. Prototype Graph API integration in development environment  
3. Generate pre-migration report with actual data (Phase 1 of plan.md)
