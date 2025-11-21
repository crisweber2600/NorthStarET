# Implementation Plan: Legacy IdentityServer to Microsoft Entra ID Migration

**Feature ID**: `001-entra-id-migration`  
**Target Layer**: Foundation  
**Parent Specification**: [Identity Service with Entra ID](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/README.md)  
**Parent Plan Reference**: [Identity Service Implementation Plan](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/plan.md) (See Phase 6)  
**Plan Version**: 1.0.0  
**Created**: 2025-11-20  
**Status**: Ready for Implementation

---

## Executive Summary

This plan implements **Phase 6** of the Identity Service specification as a **standalone migration execution**. It assumes that:

1. ✅ **Phases 1-5 of the Identity Service are complete**:
   - Entra ID authentication flow is implemented and tested
   - Session management (Redis + PostgreSQL) is operational
   - Authorization and RBAC are functional
   - Token refresh and tenant switching work correctly
   
2. 🎯 **This migration focuses exclusively on**:
   - Mapping legacy IdentityServer users to Entra ID accounts
   - Creating ExternalProviderLinks for matched users
   - Preserving role and permission assignments
   - Deprecating legacy authentication credentials
   - Generating migration audit reports

**⚠️ PREREQUISITE**: Review the [parent implementation plan](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/plan.md) to understand the full Identity Service architecture before executing this migration.

---

## Layer Identification

- **Target Layer**: Foundation  
- **Implementation Location**: `Src/Foundation/services/Identity/Migration/`  
- **Database Context**: `IdentityDbContext` (defined in Identity Service)  
- **Justification**: Data migration is a one-time operational task within the Identity Service domain

**Cross-Layer Dependencies**:
- `Src/Foundation/services/Identity/` (Identity Service from parent spec)
- `Src/Foundation/shared/Infrastructure` (Data contexts, repositories)
- `Src/Foundation/shared/Domain` (User, Role, ExternalProviderLink entities)

---

## Technical Context

**Feature**: Legacy IdentityServer to Microsoft Entra ID User Migration  
**Goal**: Execute one-time data migration linking existing users to Entra ID accounts  
**Current State**: Legacy IdentityServer authentication active, users authenticated with local passwords  
**Target State**: All users linked to Entra ID, legacy passwords deprecated, Entra ID-only authentication  
**Architectural Impact**: High - Fundamental change to authentication source, but Identity Service architecture (from parent spec) handles all runtime auth logic

---

## Constitution Check

- [x] **Layer Compliance**: Migration executes in Foundation layer, modifies Foundation data
- [x] **Dependency Rule**: Only depends on shared infrastructure and Identity Service entities
- [x] **Pattern Compliance**: Follows established data migration patterns, uses EF Core migrations
- [x] **Testing Requirements**: Reqnroll BDD scenarios, integration tests with Testcontainers
- [x] **Reversibility**: Rollback plan documented (see Phase 4)

---

## Migration Architecture

### High-Level Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Migration Execution Flow                      │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────┐         ┌──────────────────┐
│ Legacy Identity  │         │ Microsoft Entra  │
│ Server Database  │         │ ID Tenant        │
│                  │         │                  │
│ - Users (1000)   │         │ - Users (1000)   │
│ - Email          │         │ - UPN/Email      │
│ - Password Hash  │         │ - Object ID      │
│ - Roles          │         │ - Roles          │
└─────────┬────────┘         └────────┬─────────┘
          │                           │
          │  1. Extract User Data     │  2. Query Entra ID
          │                           │     (Microsoft Graph API)
          │                           │
          └───────────┬───────────────┘
                      ▼
          ┌───────────────────────┐
          │  Migration Service    │
          │                       │
          │  - Email Matching     │
          │  - Link Creation      │
          │  - Role Preservation  │
          │  - Audit Logging      │
          └───────────┬───────────┘
                      ▼
          ┌───────────────────────┐
          │  NorthStar Identity   │
          │  Database             │
          │                       │
          │  Tables:              │
          │  - users              │
          │  - external_provider_ │
          │    links (NEW)        │
          │  - user_roles         │
          │  - audit_records      │
          └───────────────────────┘
```

### Data Mapping Strategy

**Primary Key**: Email address (case-insensitive match)  
**Entra ID Identifier**: `oid` claim (Object ID) from Entra ID user record  
**Linking Table**: `identity.external_provider_links`

| Legacy Field | Entra ID Field | NorthStar Field | Notes |
|--------------|----------------|-----------------|-------|
| `SubjectId` (GUID) | - | `users.id` | Preserved as primary key |
| `Email` | `userPrincipalName` or `mail` | `users.email` | Matching key |
| `FirstName` | `givenName` | `users.display_name` | Updated if different |
| `LastName` | `surname` | `users.display_name` | Updated if different |
| - | `id` (Object ID) | `external_provider_links.provider_subject_id` | New link |
| `PasswordHash` | - | - | Marked deprecated, not migrated |
| `Roles` (junction) | `appRoleAssignments` | `user_roles` | Preserved and validated |

---

## Implementation Phases

### Phase 1: Pre-Migration Preparation [Week 1]

**Goal**: Validate data, configure Entra ID, establish migration infrastructure

**Deliverables**:
- Entra ID tenant provisioned with all user accounts
- Microsoft Graph API client configured with `User.Read.All` permission
- ExternalProviderLinks table created via EF Core migration
- Migration service skeleton implemented
- Pre-migration audit report generated

**Tasks**:

1. **Validate Entra ID Tenant Readiness**
   - Confirm all users exist in Entra ID tenant
   - Verify email addresses match legacy database (≥95% match rate expected)
   - Document exceptions (mismatched emails, missing accounts)

2. **Configure Microsoft Graph API Access**
   - Register service principal in Entra ID for migration script
   - Grant `User.Read.All` application permission
   - Store client ID and secret in Azure Key Vault
   - Test Graph API query: `GET /users?$select=id,userPrincipalName,mail`

3. **Create Database Migration**
   - Add EF Core migration: `AddExternalProviderLinksTable`
   - Include indexes: `IX_external_provider_links_provider_subject_id`, `IX_external_provider_links_user_id`
   - Apply to staging database

4. **Build Migration Service**
   - Create `UserMigrationService` in `Src/Foundation/services/Identity/Migration/`
   - Implement `IGraphApiClient` abstraction (testable)
   - Implement `IMigrationReportGenerator` for audit logging

5. **Generate Pre-Migration Report**
   - Query legacy database: total users, users with emails, users without emails
   - Query Entra ID: total users, email distribution
   - Generate CSV report: `pre-migration-audit-{timestamp}.csv`
   - Expected columns: LegacyUserId, Email, EntraMatch (Yes/No/Ambiguous), RoleCount

**Testing**:
- Unit tests: `GraphApiClientTests`, `MigrationServiceTests`
- Integration test: Query Entra ID test tenant with 10 sample users
- Validation: Pre-migration report accuracy (manual review)

---

### Phase 2: Automated User Migration [Week 2]

**Goal**: Execute automated email-based user matching and link creation

**Deliverables**:
- `UserMigrationService.ExecuteAsync()` method
- ExternalProviderLinks created for all matched users (target: ≥95%)
- Role assignments preserved in `identity.user_roles`
- Legacy password deprecation flags set
- Migration execution report with success/failure counts

**Tasks**:

1. **Implement Email Matching Algorithm**
   ```csharp
   public async Task<MigrationResult> MatchUsersAsync()
   {
       // 1. Load all legacy users with emails
       var legacyUsers = await _identityDbContext.Users
           .Where(u => u.Email != null)
           .ToListAsync();
       
       // 2. Query Entra ID for matching users
       foreach (var legacyUser in legacyUsers)
       {
           var entraUser = await _graphClient.GetUserByEmailAsync(legacyUser.Email);
           if (entraUser != null)
           {
               yield return new UserMatch {
                   LegacyUserId = legacyUser.Id,
                   EntraObjectId = entraUser.Id,
                   MatchConfidence = MatchConfidence.Exact
               };
           }
       }
   }
   ```

2. **Create ExternalProviderLinks**
   - For each matched user pair:
     ```csharp
     var link = new ExternalProviderLink {
         Id = Guid.NewGuid(),
         UserId = match.LegacyUserId,
         Provider = "EntraID",
         ProviderSubjectId = match.EntraObjectId,
         CreatedAt = DateTime.UtcNow
     };
     await _identityDbContext.ExternalProviderLinks.AddAsync(link);
     ```

3. **Preserve Role Assignments**
   - Query existing `user_roles` for each migrated user
   - Validate role IDs still exist in target database
   - Log any orphaned role assignments for manual review

4. **Deprecate Legacy Passwords**
   - **DO NOT DELETE** password hashes (retain for audit)
   - Add `auth_deprecated_at` timestamp column via migration
   - Update migrated users: `SET auth_deprecated_at = NOW()`

5. **Generate Migration Report**
   - Summary: Total processed, matched, unmatched, links created, roles preserved
   - Detailed CSV: Per-user migration status
   - Error log: Users that failed matching with reason codes

**Testing**:
- Unit tests: `EmailMatchingAlgorithmTests`, `LinkCreationTests`
- Integration test: Migrate 100 test users (Testcontainers with seeded data)
- BDD: `08-user-migration.feature` scenarios
- Evidence: Capture terminal output (Red→Green for BDD tests)

**Evidence Capture**:
```bash
# Red phase (before implementation)
dotnet test --filter "Category=Migration" > phase2-red-migration-tests.txt

# Green phase (after implementation)
dotnet test --filter "Category=Migration" > phase2-green-migration-tests.txt
```

---

### Phase 3: Manual Exception Handling [Week 3]

**Goal**: Provide tooling for administrators to resolve unmatched users

**Deliverables**:
- Admin UI or CLI tool for manual user linking
- Fuzzy email matching suggestions (Levenshtein distance)
- Manual link creation with audit trail
- Updated migration report showing manual resolutions

**Tasks**:

1. **Build Exception Report UI**
   - Display unmatched users (from Phase 2 report)
   - Show suggested Entra ID matches (fuzzy email search)
   - Allow admin to:
     - Confirm suggested match → create link
     - Manually enter Entra ID Object ID → create link
     - Mark as "No Match" → flag for account creation

2. **Implement Fuzzy Matching**
   ```csharp
   public async Task<List<EntraUser>> SuggestMatchesAsync(string legacyEmail)
   {
       var entraUsers = await _graphClient.SearchUsersAsync(legacyEmail);
       return entraUsers
           .Select(eu => new {
               User = eu,
               Distance = LevenshteinDistance(legacyEmail, eu.Mail)
           })
           .Where(x => x.Distance <= 3)
           .OrderBy(x => x.Distance)
           .Select(x => x.User)
           .ToList();
   }
   ```

3. **Manual Link Creation Endpoint**
   - `POST /api/admin/migration/create-link`
   - Validate admin permissions
   - Create ExternalProviderLink with `CreatedBy = {admin_id}`
   - Log to `audit_records`: "Manual user link created"

4. **Post-Manual-Resolution Report**
   - Re-run migration report generator
   - Show updated match rate (should reach 98-100%)
   - Identify remaining unresolved users

**Testing**:
- Unit tests: `FuzzyMatchingTests`, `ManualLinkCreationTests`
- Integration test: Admin resolves 5 unmatched users
- UAT: Admin reviews and links edge cases in staging

---

### Phase 4: Validation & Rollback Readiness [Week 4]

**Goal**: Verify migration success and prepare rollback procedure

**Deliverables**:
- Post-migration validation suite (all tests pass)
- Rollback script tested in staging
- Production migration runbook
- Go/No-Go checklist for production execution

**Tasks**:

1. **Post-Migration Validation Tests**
   ```bash
   # Test 1: All migrated users can authenticate via Entra ID
   dotnet test --filter "Category=PostMigration&Test=EntraIdAuth"
   
   # Test 2: Role assignments preserved
   dotnet test --filter "Category=PostMigration&Test=RolePreservation"
   
   # Test 3: Legacy auth blocked
   dotnet test --filter "Category=PostMigration&Test=LegacyAuthDeprecated"
   ```

2. **Build Rollback Script**
   ```sql
   -- Rollback: Re-enable legacy auth (do NOT delete ExternalProviderLinks)
   BEGIN TRANSACTION;
   
   -- Clear deprecation timestamps
   UPDATE identity.users 
   SET auth_deprecated_at = NULL 
   WHERE auth_deprecated_at IS NOT NULL;
   
   -- Mark ExternalProviderLinks as inactive (preserve data)
   ALTER TABLE identity.external_provider_links 
   ADD COLUMN IF NOT EXISTS is_active BOOLEAN DEFAULT TRUE;
   
   UPDATE identity.external_provider_links 
   SET is_active = FALSE;
   
   -- Audit rollback
   INSERT INTO identity.audit_records (event_type, details, created_at)
   VALUES ('MigrationRollback', '{"reason": "..."}', NOW());
   
   COMMIT;
   ```

3. **Test Rollback in Staging**
   - Execute migration in staging
   - Perform rollback
   - Verify users can authenticate with legacy credentials
   - Re-execute migration (test idempotency)

4. **Create Production Runbook**
   - Maintenance window schedule (recommended: 2 hours)
   - Pre-migration checklist: backups, admin access, rollback script ready
   - Execution steps: run migration, validate, monitor
   - Post-migration checklist: verify login success rate, check error logs
   - Rollback triggers: >5% auth failure rate, critical errors

**Testing**:
- Integration test: Full migration + rollback cycle
- Load test: Simulate 100 concurrent Entra ID logins post-migration
- Evidence: Attach validation test results to Phase 4 review

---

### Phase 5: Production Execution [Week 5]

**Goal**: Execute migration in production with monitoring and validation

**Deliverables**:
- Production migration executed during maintenance window
- Real-time monitoring dashboard
- Post-migration validation report (target: 99% auth success)
- Legacy IdentityServer decommissioning plan

**Tasks**:

1. **Pre-Production Checklist**
   - [ ] Database backup completed (PostgreSQL pg_dump)
   - [ ] Rollback script tested and ready
   - [ ] Admin team on-call during maintenance window
   - [ ] Monitoring alerts configured (auth failure rate)
   - [ ] Communication sent to users ("Sign in with Microsoft" launch)

2. **Execute Migration**
   ```bash
   # Production migration command
   dotnet run --project Src/Foundation/services/Identity/Migration/ \
       --environment Production \
       --dry-run false \
       --audit-log production-migration-{timestamp}.log
   ```

3. **Real-Time Monitoring**
   - Track Entra ID authentication success rate (target: >99%)
   - Monitor `audit_records` for failed authentications
   - Check Redis cache hit rate for session lookups
   - Watch application logs for token validation errors

4. **Post-Migration Validation**
   - Sample 50 random users: attempt Entra ID login
   - Verify role-based access: admin, teacher, student roles
   - Test tenant switching for multi-district users
   - Confirm legacy auth endpoints return 401 with deprecation message

5. **Generate Final Report**
   - Total users migrated: ____
   - Match rate: ____%
   - Post-migration auth success rate: ____%
   - Unresolved users: ____ (list with resolution plan)
   - Rollback executed: Yes/No

**Success Criteria**:
- ✅ ≥99% of migrated users authenticate successfully via Entra ID
- ✅ Zero critical errors during migration execution
- ✅ Role preservation: 100% (all role assignments intact)
- ✅ Audit trail complete (migration events logged)

---

## Data Model Changes

See [data-model.md](./data-model.md) for complete schema.

**Key Addition**: `identity.external_provider_links` table

```sql
CREATE TABLE identity.external_provider_links (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES identity.users(id),
    provider VARCHAR(50) NOT NULL, -- 'EntraID'
    provider_subject_id VARCHAR(255) NOT NULL, -- Entra Object ID
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by UUID NULL, -- Admin ID for manual links
    CONSTRAINT uk_provider_subject UNIQUE (provider, provider_subject_id)
);

CREATE INDEX ix_external_provider_links_user_id 
    ON identity.external_provider_links(user_id);
```

---

## Testing Strategy

### Unit Tests
- `EmailMatchingAlgorithmTests` - Exact and fuzzy matching logic
- `GraphApiClientTests` - Entra ID API queries (mocked)
- `LinkCreationTests` - ExternalProviderLink entity creation
- `RolePreservationTests` - User role migration accuracy

### Integration Tests (Testcontainers)
- `MigrationServiceIntegrationTests` - Full migration with seeded data
- `PostMigrationAuthenticationTests` - Entra ID login after migration
- `RollbackIntegrationTests` - Rollback and re-migration

### BDD Tests (Reqnroll)
- See parent spec: [features/08-user-migration.feature](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/features/08-user-migration.feature)
- Scenarios covered:
  - Automated user migration with email match
  - Manual user linking for edge cases
  - Post-migration authentication validation
  - Role preservation verification

### Evidence Requirements
- ✅ Red→Green terminal outputs for all test phases
- ✅ Migration execution logs (staging + production)
- ✅ Validation reports (pre-migration, post-migration)
- ✅ Rollback test results

---

## References

- **Parent Specification**: [Identity Service Implementation Plan](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/plan.md)  
- **Data Model**: [Identity Service Data Model](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model-enhanced.md)  
- **BDD Features**: [User Migration Feature](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/features/08-user-migration.feature)  
- **Microsoft Graph API**: [Users Resource Type](https://learn.microsoft.com/en-us/graph/api/resources/user)  
- **Constitution**: [Testing Requirements](../../../../.specify/memory/constitution.md)

---

**Next Steps**:  
1. Review and approve this migration plan  
2. Execute Phase 1 in development environment  
3. Schedule production migration window (recommended: Friday evening, 4-hour window)
