---
description: "Task list for Legacy IdentityServer to Microsoft Entra ID Migration"
---

# Tasks: Legacy IdentityServer to Microsoft Entra ID Migration

**Specification Branch**: `Foundation/001-entra-id-migration-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/001-entra-id-migration` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/001-entra-id-migration/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/services/Identity/Migration/`  
**Specification Path**: `Plan/Foundation/specs/001-entra-id-migration/`

### Layer Consistency Checklist

- [x] Target Layer matches spec.md Layer Identification (Foundation)
- [x] Target Layer matches plan.md Layer Identification (Foundation)
- [x] Implementation path follows layer structure (`Src/Foundation/services/Identity/Migration/`)
- [x] Specification path follows layer structure (`Plan/Foundation/specs/001-entra-id-migration/`)
- [x] Shared infrastructure dependencies: `Src/Foundation/shared/{Domain,Infrastructure}`
- [x] Cross-layer dependencies justified: Identity Service from parent spec (Phases 1-5 complete)

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] T001 Verify project references ONLY shared infrastructure (`Src/Foundation/shared/Domain`, `Src/Foundation/shared/Infrastructure`)
- [ ] T002 Verify NO direct service-to-service references (migration uses IdentityDbContext from shared Infrastructure)
- [ ] T003 Verify migration uses existing Identity Service entities (User, ExternalProviderLink from shared Domain)
- [ ] T004 Verify README.md documents layer position and dependencies on Identity Service

---

## Identity & Authentication Compliance

*MANDATORY: This migration modifies authentication infrastructure*

- [ ] T005 Verify NO references to Duende IdentityServer (migration deprecates legacy IdentityServer)
- [ ] T006 Verify Microsoft.Identity.Web used for Entra ID token validation (parent Identity Service)
- [ ] T007 Verify SessionAuthenticationHandler exists and registered (parent Identity Service Phase 2)
- [ ] T008 Verify Redis configured for session caching (Aspire.Hosting.Redis)
- [ ] T009 Verify identity.sessions table includes tenant_id for multi-tenancy
- [ ] T010 Verify TokenExchangeService implements BFF pattern (parent Identity Service Phase 3)
- [ ] T011 Verify authentication flow follows `docs/legacy-identityserver-migration.md` architecture

---

## Prerequisites Validation

**⚠️ CRITICAL**: This migration assumes Identity Service (Phases 1-5) is complete. Verify:

- [ ] T012 Confirm Entra ID authentication flow implemented and tested (Identity Service Phase 1)
- [ ] T013 Confirm session management (Redis + PostgreSQL) operational (Identity Service Phase 2)
- [ ] T014 Confirm authorization and RBAC functional (Identity Service Phase 3)
- [ ] T015 Confirm token refresh working (Identity Service Phase 4)
- [ ] T016 Confirm tenant switching works correctly (Identity Service Phase 5)
- [ ] T017 Review parent implementation plan: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/plan.md`

---

## Phase 1: Pre-Migration Preparation [Week 1]

**Goal**: Validate data, configure Entra ID, establish migration infrastructure

**Purpose**: Prepare systems and validate data quality before executing migration

### Entra ID Tenant Validation

- [ ] T018 Query Entra ID tenant to confirm all users exist (Microsoft Graph API)
- [ ] T019 Extract user emails from legacy IdentityServer database in `Src/Foundation/services/Identity/Migration/Scripts/extract_legacy_users.sql`
- [ ] T020 Generate email match report comparing legacy database to Entra ID in `Src/Foundation/services/Identity/Migration/Reports/PreMigrationAudit/email_match_analysis.csv`
- [ ] T021 Document exceptions (mismatched emails, missing accounts) in `Src/Foundation/services/Identity/Migration/Reports/PreMigrationAudit/exceptions_report.md`
- [ ] T022 Validate match rate and apply decision criteria: <90% = HALT migration and investigate data quality; 90-94% = escalate to stakeholder approval with documented exceptions; ≥95% = proceed to Phase 2

### Microsoft Graph API Configuration

- [ ] T022a Document Azure prerequisites in `Src/Foundation/services/Identity/Migration/Docs/AZURE_SETUP.md` (tenant ID, subscription, RBAC permissions, resource group)
- [ ] T023 Register service principal in Entra ID tenant for migration script (Azure Portal) - see AZURE_SETUP.md for detailed steps
- [ ] T024 Grant `User.Read.All` and `Application.ReadWrite.All` application permissions to service principal (Azure Portal)
- [ ] T025 Create Azure Key Vault (Standard SKU, region matching Identity Service) for migration credentials in `Src/Foundation/services/Identity/Migration/Config/keyvault-setup.ps1`
- [ ] T026 Store Graph API client ID and secret in Azure Key Vault
- [ ] T027 Test Graph API query: `GET /users?$select=id,userPrincipalName,mail` in `Src/Foundation/services/Identity/Migration/Tests/Integration/GraphApiClientTests.cs`

### Database Schema Migration

- [ ] T028 Create EF Core migration: `AddExternalProviderLinksTable` in `Src/Foundation/shared/Infrastructure/Data/Migrations/`
- [ ] T029 Add `external_provider_links` table with columns: id, user_id, tenant_id, provider, provider_subject_id, provider_metadata, created_at, created_by, is_active
- [ ] T029a Add Row-Level Security (RLS) policy: `tenant_id = current_setting('app.current_tenant')::UUID`
- [ ] T030 [P] Create index `IX_external_provider_links_user_tenant` on (user_id, tenant_id)
- [ ] T031 [P] Create index `IX_external_provider_links_provider_subject` on (provider, provider_subject_id)
- [ ] T032 [P] Create index `IX_external_provider_links_tenant` on tenant_id
- [ ] T032a [P] Create index `IX_external_provider_links_active` on is_active WHERE is_active = TRUE
- [ ] T033 Add EF Core migration: `AddAuthDeprecatedAtColumn` for `identity.users.auth_deprecated_at` TIMESTAMPTZ column
- [ ] T034 Apply migrations to staging database: `dotnet ef database update --project Src/Foundation/shared/Infrastructure`

### Migration Service Infrastructure

- [ ] T035 Create `IGraphApiClient` interface in `Src/Foundation/services/Identity/Migration/Interfaces/IGraphApiClient.cs`
- [ ] T036 Create `GraphApiClient` implementation in `Src/Foundation/services/Identity/Migration/Services/GraphApiClient.cs`
- [ ] T037 Create `IMigrationReportGenerator` interface in `Src/Foundation/services/Identity/Migration/Interfaces/IMigrationReportGenerator.cs`
- [ ] T038 Create `MigrationReportGenerator` implementation in `Src/Foundation/services/Identity/Migration/Services/MigrationReportGenerator.cs`
- [ ] T039 Create `UserMigrationService` skeleton in `Src/Foundation/services/Identity/Migration/Services/UserMigrationService.cs`
- [ ] T040 Create migration configuration class in `Src/Foundation/services/Identity/Migration/Config/MigrationConfig.cs`
- [ ] T041 Register services in DI container in `Src/Foundation/services/Identity/Migration/DependencyInjection.cs`

### Pre-Migration Audit Report

- [ ] T042 Implement query for total legacy users in `UserMigrationService.GetLegacyUserStatsAsync()`
- [ ] T043 Implement query for users with emails in `UserMigrationService.GetUsersWithEmailsAsync()`
- [ ] T044 Implement query for users without emails in `UserMigrationService.GetUsersWithoutEmailsAsync()`
- [ ] T045 Implement Entra ID user count query in `GraphApiClient.GetTotalUserCountAsync()`
- [ ] T046 Generate CSV report: `pre-migration-audit-{timestamp}.csv` with columns: LegacyUserId, Email, EntraMatch, RoleCount
- [ ] T047 Execute pre-migration audit in staging environment
- [ ] T048 Manual review of pre-migration report (validate accuracy)

### Phase 1 Testing

- [ ] T049 [P] Unit test: `GraphApiClientTests` - mock Graph API responses in `Src/Foundation/services/Identity/Migration/Tests/Unit/GraphApiClientTests.cs`
- [ ] T050 [P] Unit test: `MigrationServiceTests` - test report generation logic in `Src/Foundation/services/Identity/Migration/Tests/Unit/MigrationServiceTests.cs`
- [ ] T051 Integration test: Query Entra ID test tenant with 10 sample users in `Src/Foundation/services/Identity/Migration/Tests/Integration/EntraIdIntegrationTests.cs`
- [ ] T052 Validation: Pre-migration report accuracy (manual review with stakeholder sign-off)

**Checkpoint**: Pre-migration audit complete, infrastructure ready, ≥95% match rate validated

---

## Phase 2: Automated User Migration [Week 2]

**Goal**: Execute automated email-based user matching and link creation

**Purpose**: Create ExternalProviderLinks for all matched users, preserve roles, deprecate legacy passwords

### Email Matching Algorithm

- [ ] T053 Implement `MatchUsersAsync()` in `UserMigrationService` - load legacy users with emails
- [ ] T054 Implement case-insensitive email comparison logic
- [ ] T055 Query Entra ID for matching users via Graph API in `GraphApiClient.GetUserByEmailAsync(string email)`
- [ ] T056 Generate `UserMatch` records with MatchConfidence (Exact, Fuzzy, NoMatch) in `Src/Foundation/services/Identity/Migration/Models/UserMatch.cs`
- [ ] T057 Log unmatched users to migration report for Phase 3 manual review
- [ ] T058 Implement progress logging (every 100 users processed)

### ExternalProviderLink Creation

- [ ] T059 Create `ExternalProviderLink` entity factory in `UserMigrationService.CreateProviderLink(UserMatch match)`
- [ ] T060 Implement batch insert logic for ExternalProviderLinks (batch size: 100)
- [ ] T061 Set Provider = "EntraID", ProviderSubjectId = Entra Object ID (oid claim)
- [ ] T062 Populate provider_metadata JSONB with displayName, email, lastSyncedAt
- [ ] T063 Handle duplicate link attempts (check UNIQUE constraint, skip if exists)
- [ ] T064 Commit transaction after each batch with retry on transient errors

### Role Preservation

- [ ] T065 Query existing `identity.user_roles` for each migrated user in `UserMigrationService.PreserveRolesAsync(Guid userId)`
- [ ] T066 Validate role_id FK integrity (ensure roles exist in identity.roles table)
- [ ] T067 Validate tenant_id FK integrity (ensure districts exist in identity.tenants table)
- [ ] T068 Log orphaned role assignments (missing FK references) to exception report
- [ ] T069 Generate role preservation summary: Total roles preserved, orphaned roles count

### Legacy Password Deprecation

- [ ] T070 Update migrated users: `SET auth_deprecated_at = NOW()` in `UserMigrationService.DeprecateLegacyPasswordsAsync()`
- [ ] T071 **DO NOT DELETE** password_hash column (retain for audit/rollback)
- [ ] T072 Verify password hashes remain in database after deprecation (integration test)
- [ ] T073 Update authentication handler to reject deprecated passwords in `Src/Foundation/services/Identity/Api/Handlers/LegacyAuthHandler.cs`
- [ ] T074 Add deprecation message: "Authentication modernized. Use 'Sign in with Microsoft'"

### Migration Execution Report

- [ ] T075 Implement `GenerateMigrationReportAsync()` in `MigrationReportGenerator`
- [ ] T076 Summary statistics: Total processed, matched, unmatched, links created, roles preserved
- [ ] T077 Detailed CSV: `migration-execution-{timestamp}.csv` with per-user status
- [ ] T078 Error log: Users that failed matching with reason codes (EmailMismatch, EntraUserNotFound, etc.)
- [ ] T079 Generate report automatically at end of migration execution

### Phase 2 Testing

- [ ] T080 [P] Unit test: `EmailMatchingAlgorithmTests` - exact match, case sensitivity in `Src/Foundation/services/Identity/Migration/Tests/Unit/EmailMatchingTests.cs`
- [ ] T081 [P] Unit test: `LinkCreationTests` - entity factory, batch logic in `Src/Foundation/services/Identity/Migration/Tests/Unit/LinkCreationTests.cs`
- [ ] T082 [P] Unit test: `RolePreservationTests` - FK validation, orphaned roles in `Src/Foundation/services/Identity/Migration/Tests/Unit/RolePreservationTests.cs`
- [ ] T083 Integration test: Migrate 100 test users with Testcontainers (PostgreSQL + seeded data) in `Src/Foundation/services/Identity/Migration/Tests/Integration/MigrationServiceIntegrationTests.cs`
- [ ] T084 BDD test: Create `features/08-user-migration.feature` in `Src/Foundation/services/Identity/Migration/Tests/BDD/Features/`
- [ ] T085 BDD test: Implement step definitions in `Src/Foundation/services/Identity/Migration/Tests/BDD/Steps/UserMigrationSteps.cs`

### Phase 2 Evidence Capture

- [ ] T086 Red phase: Run tests BEFORE implementation - `dotnet test --filter "Category=Migration" > phase2-red-migration-tests.txt`
- [ ] T087 Implement all Phase 2 tasks (T053-T079)
- [ ] T088 Green phase: Run tests AFTER implementation - `dotnet test --filter "Category=Migration" > phase2-green-migration-tests.txt`
- [ ] T089 Attach both transcripts to phase review artifacts

**Checkpoint**: Automated migration complete, ≥95% users linked, roles preserved, report generated

---

## Phase 3: Manual Exception Handling [Week 3]

**Goal**: Provide tooling for administrators to resolve unmatched users

**Purpose**: Enable admin resolution of edge cases (email mismatches, missing accounts)

### Exception Report UI/CLI

- [ ] T090 Create admin CLI tool entry point in `Src/Foundation/services/Identity/Migration/Tools/MigrationAdmin/Program.cs`
- [ ] T091 Implement `list-exceptions` command to display unmatched users in `MigrationAdminTool.cs`
- [ ] T092 Load exceptions from Phase 2 migration report CSV
- [ ] T093 Display: LegacyUserId, Email, SuggestedEntraMatches (top 3 fuzzy matches)
- [ ] T094 Implement navigation: Next, Previous, Filter by reason code

### Fuzzy Email Matching

- [ ] T095 Implement Levenshtein distance algorithm in `Src/Foundation/services/Identity/Migration/Utils/StringSimilarity.cs`
- [ ] T096 Create `SuggestMatchesAsync(string legacyEmail)` in `UserMigrationService`
- [ ] T097 Query Entra ID users with partial email match via Graph API: `$filter=startswith(mail, '{prefix}')`
- [ ] T098 Calculate Levenshtein distance for each candidate match
- [ ] T099 Return top 3 matches where distance ≤ 3 (configurable threshold)
- [ ] T100 Display suggested matches in CLI tool with confidence scores

### Manual Link Creation

- [ ] T101 Create `link-user` command in admin CLI tool
- [ ] T102 Accept parameters: --legacy-user-id, --entra-object-id
- [ ] T103 Validate admin has permission to create links (require DistrictAdmin role)
- [ ] T104 Create ExternalProviderLink with `created_by = {admin_id}` (audit trail)
- [ ] T105 Log audit event: "Manual user link created" to `identity.audit_records`
- [ ] T106 Update migration report: Mark user as manually resolved
- [ ] T107 Implement `POST /api/admin/migration/create-link` endpoint in `Src/Foundation/services/Identity/Api/Controllers/AdminMigrationController.cs`
- [ ] T108 Add authentication: Require `[Authorize(Roles = "DistrictAdmin,SystemAdmin")]`
- [ ] T109 Validate request body schema: `{ legacyUserId: UUID, entraObjectId: string, notes: string }`
- [ ] T110 Return 201 Created with link details

### Post-Manual-Resolution Report

- [ ] T111 Implement `regenerate-report` command in admin CLI tool
- [ ] T112 Re-run migration statistics with updated ExternalProviderLinks data
- [ ] T113 Calculate updated match rate (should reach 98-100%)
- [ ] T114 Identify remaining unresolved users (target: ≤2%)
- [ ] T115 Generate `post-manual-resolution-{timestamp}.csv` report

### Phase 3 Testing

- [ ] T116 [P] Unit test: `FuzzyMatchingTests` - Levenshtein distance, threshold tuning in `Src/Foundation/services/Identity/Migration/Tests/Unit/FuzzyMatchingTests.cs`
- [ ] T117 [P] Unit test: `ManualLinkCreationTests` - audit logging, permission checks in `Src/Foundation/services/Identity/Migration/Tests/Unit/ManualLinkCreationTests.cs`
- [ ] T118 Integration test: Admin resolves 5 unmatched users in Testcontainers environment in `Src/Foundation/services/Identity/Migration/Tests/Integration/ManualResolutionTests.cs`
- [ ] T119 UAT: Admin reviews and links edge cases in staging environment (manual test)

**Checkpoint**: Manual resolution complete, match rate ≥98%, remaining exceptions documented

---

## Phase 4: Validation & Rollback Readiness [Week 4]

**Goal**: Verify migration success and prepare rollback procedure

**Purpose**: Ensure migration can be validated and reversed if issues arise

### Post-Migration Validation Tests

- [ ] T120 Create `POST /api/admin/migration/validate` endpoint in `AdminMigrationController`
- [ ] T121 Test 1: All migrated users can authenticate via Entra ID - implement in `Src/Foundation/services/Identity/Migration/Tests/Integration/PostMigrationAuthTests.cs`
- [ ] T122 Test 2: Role assignments preserved - validate user_roles FK integrity
- [ ] T123 Test 3: Legacy auth blocked - attempt login with deprecated password, expect 401
- [ ] T124 Test 4: Session creation works - verify Redis session cache populated
- [ ] T125 Test 5: Tenant switching works - validate multi-district user context
- [ ] T126 Execute validation suite: `dotnet test --filter "Category=PostMigration"`
- [ ] T127 Generate validation report: Pass/Fail for each test, overall health score

### Rollback Script Development

- [ ] T128 Create rollback SQL script in `Src/Foundation/services/Identity/Migration/Scripts/rollback_migration.sql`
- [ ] T129 Clear deprecation timestamps: `UPDATE identity.users SET auth_deprecated_at = NULL`
- [ ] T130 Mark ExternalProviderLinks inactive: `UPDATE identity.external_provider_links SET is_active = FALSE`
- [ ] T131 Insert rollback audit record to `identity.audit_records`
- [ ] T132 Add transaction wrapper with rollback on error
- [ ] T133 Document rollback prerequisites in `Src/Foundation/services/Identity/Migration/Docs/ROLLBACK.md`

### Rollback Testing

- [ ] T134 Execute migration in staging environment
- [ ] T135 Run rollback script: `psql -f rollback_migration.sql`
- [ ] T136 Verify users can authenticate with legacy credentials post-rollback
- [ ] T137 Verify ExternalProviderLinks marked inactive (not deleted)
- [ ] T138 Re-execute migration (test idempotency) - should skip existing links
- [ ] T139 Document rollback success in `Src/Foundation/services/Identity/Migration/Reports/Rollback/rollback_test_results.md`

### Production Runbook Creation

- [ ] T140 Create production runbook in `Src/Foundation/services/Identity/Migration/Docs/PRODUCTION_RUNBOOK.md`
- [ ] T141 Document maintenance window: Recommended 2-4 hours, off-peak hours
- [ ] T142 Pre-migration checklist: Database backups, admin access, rollback script tested
- [ ] T143 Execution steps: Command to run migration, expected duration (2 hours for 1000 users)
- [ ] T144 Monitoring instructions: Check Aspire dashboard, Redis cache hit rate, auth success rate
- [ ] T145 Post-migration checklist: Validate login success rate, check error logs, verify role access
- [ ] T146 Rollback triggers: >5% auth failure rate, critical errors, user complaints
- [ ] T147 Communication templates: User notification email, rollback announcement

### Phase 4 Testing

- [ ] T148 Integration test: Full migration + rollback cycle in Testcontainers in `Src/Foundation/services/Identity/Migration/Tests/Integration/RollbackIntegrationTests.cs`
- [ ] T149 Load test: Simulate 100 concurrent Entra ID logins post-migration - create `Src/Foundation/services/Identity/Migration/Tests/Load/entra-auth-load.js` (k6) with acceptance: p95 latency <500ms, 0% error rate
- [ ] T150 Evidence: Capture validation test results - `dotnet test --filter "Category=PostMigration" > phase4-validation-results.txt`

**Checkpoint**: Rollback tested, validation passing, production runbook ready

---

## Phase 5: Production Execution [Week 5]

**Goal**: Execute migration in production with monitoring and validation

**Purpose**: Safely migrate production users with real-time monitoring and rollback capability

### Pre-Production Checklist

- [ ] T151 Execute database backup: `pg_dump NorthStar_Identity_DB > backup-pre-migration-{timestamp}.sql`
- [ ] T152 Verify rollback script tested and accessible in production environment
- [ ] T153 Schedule maintenance window: Coordinate with stakeholders (recommended Friday 8 PM - Sunday 2 AM local district time, 4-hour execution window)
- [ ] T154 Configure monitoring alerts: Aspire dashboard, auth failure rate (threshold: >5% = rollback trigger)
- [ ] T155 Notify users: Send email "Sign in with Microsoft" launch announcement 48 hours before
- [ ] T156 Assign on-call admin team: 2 admins required during maintenance window (1 DistrictAdmin with RBAC knowledge + 1 SystemAdmin with database/Azure access)
- [ ] T157 Test Azure Key Vault access: Verify migration service can retrieve Graph API credentials

### Migration Execution

- [ ] T158 Set maintenance mode: Display banner "System maintenance in progress" on login page
- [ ] T159 Execute production migration: `dotnet run --project Src/Foundation/services/Identity/Migration/ --environment Production --audit-log production-migration-{timestamp}.log`
- [ ] T160 Monitor execution progress: Log every 100 users processed (expected: ~7 seconds per user including Graph API latency and database writes)
- [ ] T161 Watch for errors: Alert on exception rate >1% (threshold for investigation)
- [ ] T162 Expected duration: ~2 hours for 1000 users (batch processing overhead: Graph API calls, DB transactions, audit logging)

### Real-Time Monitoring

- [ ] T163 Monitor Entra ID authentication success rate in Aspire dashboard (target: >99%)
- [ ] T164 Track failed authentication attempts in `identity.audit_records` table
- [ ] T165 Check Redis cache hit rate for session lookups (target: >95%)
- [ ] T166 Watch application logs for token validation errors (filter: "TokenValidationFailed")
- [ ] T167 Monitor Graph API rate limits: Stay under 2000 requests/minute (Microsoft Graph throttling)
- [ ] T168 Track migration progress: Current user count / total users (percentage complete)

### Post-Migration Validation

- [ ] T169 Sample 50 random users: Attempt Entra ID login for each
- [ ] T170 Verify role-based access: Test admin, teacher, student roles
- [ ] T171 Test tenant switching for multi-district users
- [ ] T172 Confirm legacy auth endpoints return 401 with deprecation message
- [ ] T173 Validate ExternalProviderLinks created: `SELECT COUNT(*) FROM identity.external_provider_links WHERE is_active = TRUE` (expect ≥950)
- [ ] T174 Check auth_deprecated_at timestamps: `SELECT COUNT(*) FROM identity.users WHERE auth_deprecated_at IS NOT NULL` (expect ≥950)

### Final Report Generation

- [ ] T175 Generate production migration report: `GET /api/admin/migration/summary`
- [ ] T176 Total users migrated: ____ (target: ≥950)
- [ ] T177 Match rate: ____% (target: ≥95%)
- [ ] T178 Post-migration auth success rate: ____% (target: ≥99%)
- [ ] T179 Unresolved users: ____ (list with resolution plan)
- [ ] T180 Rollback executed: Yes/No (if yes, attach rollback report)
- [ ] T181 Save report: `Src/Foundation/services/Identity/Migration/Reports/Production/production-migration-final-{timestamp}.json`
- [ ] T182 Stakeholder communication: Email final report to leadership

### Maintenance Mode Exit

- [ ] T183 Remove maintenance banner from login page
- [ ] T184 Monitor first 100 logins: Check for errors, user complaints
- [ ] T185 Update status page: "Migration complete - Use 'Sign in with Microsoft'"

### Phase 5 Success Criteria

- [x] ≥99% of migrated users authenticate successfully via Entra ID
- [x] Zero critical errors during migration execution
- [x] Role preservation: 100% (all role assignments intact)
- [x] Audit trail complete (migration events logged to audit_records)
- [x] Production runbook followed without deviations

**Checkpoint**: Production migration complete, users authenticating with Entra ID, legacy auth deprecated

---

## Phase 6: Legacy Cleanup & Documentation [Week 6]

**Goal**: Decommission legacy IdentityServer infrastructure, finalize documentation

**Purpose**: Complete migration by removing obsolete systems and documenting lessons learned

### Legacy IdentityServer Decommissioning

- [ ] T186 Disable legacy IdentityServer endpoints in `Src/Foundation/services/Identity/Api/Program.cs`
- [ ] T187 Remove IdentityServer NuGet packages from project files (schedule for 90 days post-migration)
- [ ] T188 Archive legacy authentication code to `Src/Foundation/services/Identity/Legacy/` (do not delete - preserve for reference)
- [ ] T189 Update API Gateway routing: Remove IdentityServer fallback routes
- [ ] T190 Schedule password hash purge: After 90 days, `UPDATE identity.users SET password_hash = NULL WHERE auth_deprecated_at < NOW() - INTERVAL '90 days'`

### Documentation Finalization

- [ ] T191 Update `docs/legacy-identityserver-migration.md` with actual migration results
- [ ] T192 Document lessons learned in `Src/Foundation/services/Identity/Migration/Docs/LESSONS_LEARNED.md`
- [ ] T193 Update Identity Service README: Reflect Entra ID-only authentication
- [ ] T194 Create migration troubleshooting guide: Common issues + resolutions
- [ ] T195 Archive all migration reports to `Src/Foundation/services/Identity/Migration/Reports/Archive/`

### Knowledge Transfer

- [ ] T196 Conduct team training: "Authenticating with Entra ID" (1-hour session)
- [ ] T197 Update developer onboarding docs: Remove IdentityServer setup instructions
- [ ] T198 Create runbook for future identity provider additions (Google, Okta)

**Checkpoint**: Legacy infrastructure decommissioned, documentation complete, team trained

---

## Dependencies & Execution Order

### Phase Dependencies

- **Prerequisites Validation**: No dependencies - MUST complete BEFORE starting Phase 1
- **Phase 1 (Pre-Migration Preparation)**: Depends on Prerequisites Validation
- **Phase 2 (Automated User Migration)**: Depends on Phase 1 completion
- **Phase 3 (Manual Exception Handling)**: Depends on Phase 2 completion (needs unmatched user list)
- **Phase 4 (Validation & Rollback)**: Depends on Phase 3 completion (all users resolved or documented)
- **Phase 5 (Production Execution)**: Depends on Phase 4 completion (rollback tested, validation passing)
- **Phase 6 (Legacy Cleanup)**: Depends on Phase 5 completion + 30-day stabilization period

### Critical Path

```
Prerequisites → Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6
     (1 day)      (1 week)  (1 week)  (1 week)  (1 week)  (2 days)  (1 week)
```

### Parallel Opportunities

**Phase 1 (Pre-Migration):**
- T030, T031, T032 (database indexes) can run in parallel
- T049, T050 (unit tests) can run in parallel with T051 (integration test)

**Phase 2 (Automated Migration):**
- T080, T081, T082 (unit tests) can run in parallel
- T084, T085 (BDD tests) can run in parallel with unit tests

**Phase 3 (Manual Exception Handling):**
- T116, T117 (unit tests) can run in parallel

**Phase 4 (Validation & Rollback):**
- T121-T125 (validation tests) can be run concurrently (different test suites)

### Within Each Phase

- **Phase 1**: Setup tasks (T018-T027) → Database tasks (T028-T034) → Service tasks (T035-T041) → Report tasks (T042-T048) → Testing (T049-T052)
- **Phase 2**: Matching (T053-T058) → Linking (T059-T064) → Roles (T065-T069) → Deprecation (T070-T074) → Reporting (T075-T079) → Testing (T080-T089)
- **Phase 3**: UI/CLI (T090-T094) → Fuzzy Matching (T095-T100) → Manual Linking (T101-T110) → Reporting (T111-T115) → Testing (T116-T119)
- **Phase 4**: Validation (T120-T127) → Rollback Script (T128-T133) → Rollback Testing (T134-T139) → Runbook (T140-T147) → Testing (T148-T150)
- **Phase 5**: Pre-Production (T151-T157) → Execution (T158-T162) → Monitoring (T163-T168) → Validation (T169-T174) → Reporting (T175-T182) → Exit (T183-T185)

---

## Implementation Strategy

### Sequential Execution (Recommended)

This migration MUST be executed sequentially due to data dependencies:

1. **Prerequisites Validation** (T001-T017): Confirm Identity Service operational
2. **Phase 1** (T018-T052): Build infrastructure, validate data quality
3. **Phase 2** (T053-T089): Execute automated migration in staging
4. **Phase 3** (T090-T119): Resolve exceptions with admin tooling
5. **Phase 4** (T120-T150): Test rollback, validate migration success
6. **Phase 5** (T151-T185): Execute in production with monitoring
7. **Phase 6** (T186-T198): Decommission legacy infrastructure

**Critical Decision Points:**

- **After Phase 1**: If match rate <95%, STOP and investigate data quality issues
- **After Phase 2**: If automated migration fails >5% of users, STOP and review matching algorithm
- **After Phase 4**: If rollback test fails, DO NOT proceed to production
- **During Phase 5**: If auth failure rate >5%, execute rollback immediately

### Team Strategy

**Minimum Team Size**: 2 developers + 1 admin

- **Developer 1**: Phases 1-2 (Migration infrastructure, automated matching)
- **Developer 2**: Phases 3-4 (Admin tooling, validation tests)
- **Admin**: Phase 3 (Manual exception resolution), Phase 5 (Production monitoring)

### Timeline

**Total Duration**: 6 weeks + 30-day stabilization period

- **Week 1**: Phase 1 (Preparation)
- **Week 2**: Phase 2 (Automated Migration in Staging)
- **Week 3**: Phase 3 (Manual Exception Resolution)
- **Week 4**: Phase 4 (Validation & Rollback Testing)
- **Week 5**: Phase 5 (Production Execution)
- **Week 6**: Phase 6 (Legacy Cleanup)
- **Weeks 7-10**: Stabilization period (monitor for issues before decommissioning legacy systems)

---

## Testing Strategy

### Test Types

1. **Unit Tests**: Business logic (email matching, link creation) - mock external dependencies
2. **Integration Tests**: Database operations, Graph API queries - use Testcontainers
3. **BDD Tests (Reqnroll)**: Migration scenarios from spec.md - full end-to-end
4. **Load Tests**: Concurrent authentication load (k6/NBomber) - production readiness
5. **UAT**: Admin reviews manual exception handling - staging environment

### Evidence Requirements (Constitution Compliance)

**Red → Green Evidence REQUIRED for each phase:**

- **Phase 1**: `phase1-red-dotnet-test.txt` + `phase1-green-dotnet-test.txt`
- **Phase 2**: `phase2-red-migration-tests.txt` + `phase2-green-migration-tests.txt`
- **Phase 3**: Manual UAT screenshots (admin resolving exceptions)
- **Phase 4**: `phase4-validation-results.txt` + rollback test logs
- **Phase 5**: Production migration log + final validation report

### Test Execution Commands

```bash
# Phase 1: Pre-Migration Tests
dotnet test --filter "Category=PreMigration" --verbosity normal

# Phase 2: Migration Tests
dotnet test --filter "Category=Migration" --verbosity normal

# Phase 4: Post-Migration Validation
dotnet test --filter "Category=PostMigration" --verbosity normal

# Phase 4: Rollback Tests
dotnet test --filter "Category=Rollback" --verbosity normal

# Full Test Suite
dotnet test --configuration Debug --verbosity normal
```

---

## Notes

### Important Reminders

- **DO NOT DELETE** password hashes during migration (retain for audit/rollback)
- **DO NOT PROCEED** to Phase 5 if rollback test fails in Phase 4
- **EXECUTE ROLLBACK IMMEDIATELY** if production auth failure rate >5%
- **WAIT 90 DAYS** before purging legacy password hashes (stabilization period)

### Migration Principles

- **Reversibility First**: Always able to rollback to legacy auth
- **Data Preservation**: Never delete user data, only deprecate
- **Audit Everything**: Log all migration actions to audit_records
- **Validate Continuously**: Test at every phase boundary
- **Monitor Aggressively**: Real-time dashboards during production execution

### File Path Conventions

This feature follows Foundation layer structure:

- **Implementation**: `Src/Foundation/services/Identity/Migration/`
- **Tests**: `Src/Foundation/services/Identity/Migration/Tests/`
- **Documentation**: `Src/Foundation/services/Identity/Migration/Docs/`
- **Reports**: `Src/Foundation/services/Identity/Migration/Reports/`

### Related Documentation

- **Parent Spec**: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/`
- **Architecture**: `docs/legacy-identityserver-migration.md`
- **Layer Guidelines**: `Plan/LAYERS.md`
- **Constitution**: `.specify/memory/constitution.md`

---

**Total Tasks**: 198 tasks across 6 phases + prerequisites  
**Parallelizable Tasks**: 11 tasks (marked with [P])  
**Critical Path Duration**: 6 weeks + 30-day stabilization  
**Estimated Effort**: 2 developers × 6 weeks = 12 developer-weeks

**Next Steps**:
1. Review and approve this task list
2. Create implementation branch: `Foundation/001-entra-id-migration`
3. Start with Prerequisites Validation (T001-T017)
4. Execute Phase 1 in development environment
