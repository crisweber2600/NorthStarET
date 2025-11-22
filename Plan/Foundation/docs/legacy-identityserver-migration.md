# Legacy IdentityServer to Microsoft Entra ID Migration Guide

**Version**: 1.0  
**Date**: 2025-11-20  
**Status**: Planning  
**Parallel Run Period**: 90 days

---

## Overview

This document provides the step-by-step migration path from the legacy .NET Framework 4.6 IdentityServer to Microsoft Entra ID with custom session authentication. The migration follows a phased approach with a 90-day parallel run period to ensure zero data loss and minimal disruption to users.

---

## Migration Objectives

1. **Zero Data Loss**: All user accounts, roles, and permissions successfully migrated
2. **Seamless Transition**: Users experience minimal disruption during cutover
3. **Audit Trail**: Complete logging of migration activities and validation results
4. **Rollback Capability**: Ability to revert to legacy system during parallel run period
5. **Security Enhancement**: Improved authentication security with Entra ID MFA and conditional access

---

## Pre-Migration Requirements

### Microsoft Entra ID Configuration

- [ ] **Entra ID Tenant Provisioned** - Dedicated tenant for NorthStar LMS or existing organization tenant
- [ ] **Application Registrations Created**:
  - Web application registration (for OIDC authentication)
  - API application registration (for token validation)
  - Delegated permissions configured (User.Read, email, profile, openid)
- [ ] **MFA Policies Configured** - Conditional access policies for administrator roles
- [ ] **User Provisioning Strategy** - Manual invite or automated sync from HR system

### Development Environment Setup

- [ ] **Microsoft.Identity.Web** NuGet packages (v3.x) installed
- [ ] **Redis Stack** configured for session caching (local dev + staging)
- [ ] **PostgreSQL database** with Identity schema deployed
- [ ] **Aspire orchestration** configured with Entra ID connection strings
- [ ] **Test Entra ID accounts** created for validation

### Data Analysis Completed

- [ ] **Legacy database backup** taken and verified
- [ ] **User count analysis** - Total users, active vs. inactive, email coverage
- [ ] **Role mapping** - Legacy roles mapped to new RBAC structure
- [ ] **Email uniqueness verified** - No duplicate emails in legacy system
- [ ] **Orphaned accounts identified** - Users without email addresses or inactive >2 years

---

## Legacy IdentityServer Database Schema

### Current Schema (OldNorthStar)

**Database**: `LoginContext` (SQL Server)

**Key Tables**:
```sql
-- Legacy schema (simplified)
AspNetUsers (
  Id NVARCHAR(128) PRIMARY KEY,
  Email NVARCHAR(256),
  EmailConfirmed BIT,
  PasswordHash NVARCHAR(MAX),
  SecurityStamp NVARCHAR(MAX),
  PhoneNumber NVARCHAR(MAX),
  LockoutEnabled BIT,
  AccessFailedCount INT
)

AspNetRoles (
  Id NVARCHAR(128) PRIMARY KEY,
  Name NVARCHAR(256)
)

AspNetUserRoles (
  UserId NVARCHAR(128) FOREIGN KEY REFERENCES AspNetUsers(Id),
  RoleId NVARCHAR(128) FOREIGN KEY REFERENCES AspNetRoles(Id)
)

AspNetUserClaims (
  Id INT PRIMARY KEY IDENTITY,
  UserId NVARCHAR(128),
  ClaimType NVARCHAR(MAX),
  ClaimValue NVARCHAR(MAX)
)

PersistedGrants (
  Key NVARCHAR(200) PRIMARY KEY,
  Type NVARCHAR(50),
  SubjectId NVARCHAR(200),
  ClientId NVARCHAR(200),
  CreationTime DATETIME,
  Expiration DATETIME,
  Data NVARCHAR(MAX)
)
```

### Target Schema (New Identity Service)

**Database**: `Identity_DB` (PostgreSQL)

**New Tables**:
```sql
-- Target schema (PostgreSQL)
CREATE TABLE identity.users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL,
  email VARCHAR(256) NOT NULL,
  legacy_user_id VARCHAR(128), -- Maps to AspNetUsers.Id
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  deleted_at TIMESTAMP NULL,
  UNIQUE(email, tenant_id)
);

CREATE TABLE identity.roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id UUID NOT NULL,
  role_name VARCHAR(100) NOT NULL,
  permissions JSONB,
  description TEXT,
  legacy_role_id VARCHAR(128), -- Maps to AspNetRoles.Id
  UNIQUE(role_name, tenant_id)
);

CREATE TABLE identity.user_roles (
  user_id UUID REFERENCES identity.users(id),
  role_id UUID REFERENCES identity.roles(id),
  tenant_id UUID NOT NULL,
  assigned_at TIMESTAMP DEFAULT NOW(),
  PRIMARY KEY (user_id, role_id)
);

CREATE TABLE identity.sessions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID REFERENCES identity.users(id),
  entra_subject_id VARCHAR(256) NOT NULL,
  tenant_id UUID NOT NULL,
  expires_at TIMESTAMP NOT NULL,
  created_at TIMESTAMP DEFAULT NOW(),
  refreshed_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE identity.external_provider_links (
  user_id UUID REFERENCES identity.users(id),
  provider VARCHAR(50) NOT NULL, -- 'EntraID', 'Legacy'
  external_user_id VARCHAR(256) NOT NULL,
  email VARCHAR(256),
  last_sync TIMESTAMP DEFAULT NOW(),
  tenant_id UUID NOT NULL,
  PRIMARY KEY (user_id, provider)
);

CREATE TABLE identity.audit_records (
  id BIGSERIAL PRIMARY KEY,
  user_id UUID,
  event_type VARCHAR(100) NOT NULL, -- 'UserMigrated', 'LoginSuccess', 'SessionCreated'
  tenant_id UUID,
  ip_address INET,
  timestamp TIMESTAMP DEFAULT NOW(),
  details JSONB
);

-- Row-Level Security policies
ALTER TABLE identity.users ENABLE ROW LEVEL SECURITY;
CREATE POLICY tenant_isolation_users ON identity.users
  USING (tenant_id = current_setting('app.current_tenant_id')::UUID);
```

---

## Migration Phase 1: Analysis & Preparation (Week 1)

### 1.1 Data Discovery

**SQL Script: Analyze Legacy Users**
```sql
-- Run against OldNorthStar LoginContext database
-- User account summary
SELECT 
  COUNT(*) as total_users,
  COUNT(CASE WHEN EmailConfirmed = 1 THEN 1 END) as confirmed_emails,
  COUNT(CASE WHEN LockoutEnabled = 1 THEN 1 END) as lockout_enabled,
  COUNT(CASE WHEN Email IS NULL OR Email = '' THEN 1 END) as missing_emails
FROM AspNetUsers;

-- Email uniqueness check
SELECT Email, COUNT(*) as duplicate_count
FROM AspNetUsers
WHERE Email IS NOT NULL AND Email != ''
GROUP BY Email
HAVING COUNT(*) > 1
ORDER BY duplicate_count DESC;

-- Role distribution
SELECT r.Name as role_name, COUNT(ur.UserId) as user_count
FROM AspNetRoles r
LEFT JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
GROUP BY r.Name
ORDER BY user_count DESC;

-- Claims analysis
SELECT ClaimType, COUNT(*) as claim_count
FROM AspNetUserClaims
GROUP BY ClaimType
ORDER BY claim_count DESC;

-- Inactive accounts (no PersistedGrants in last 2 years)
SELECT u.Email, u.Id, MAX(pg.CreationTime) as last_activity
FROM AspNetUsers u
LEFT JOIN PersistedGrants pg ON u.Id = pg.SubjectId
GROUP BY u.Email, u.Id
HAVING MAX(pg.CreationTime) < DATEADD(YEAR, -2, GETDATE())
  OR MAX(pg.CreationTime) IS NULL
ORDER BY last_activity DESC;
```

**Expected Outputs** (save to files):
- `migration-analysis-summary.csv` - User counts, email coverage
- `migration-duplicate-emails.csv` - Email conflicts requiring manual resolution
- `migration-role-distribution.csv` - Role mapping reference
- `migration-inactive-accounts.csv` - Accounts to archive instead of migrate

### 1.2 Tenant Mapping

**SQL Script: Map Users to Tenants**
```sql
-- Determine tenant assignment based on custom claims or district associations
-- This query depends on your specific multi-district implementation in OldNorthStar
SELECT 
  u.Id as legacy_user_id,
  u.Email,
  -- Replace with your actual tenant/district identifier claim
  c.ClaimValue as district_id,
  r.Name as role_name
FROM AspNetUsers u
LEFT JOIN AspNetUserClaims c ON u.Id = c.UserId 
  AND c.ClaimType = 'http://northstaret.com/claims/districtid'
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email IS NOT NULL
ORDER BY c.ClaimValue, u.Email;
```

**Manual Steps**:
1. Export results to `migration-tenant-mapping.csv`
2. Validate district IDs match target tenant UUIDs in new system
3. Identify users without district assignment → assign to default tenant or flag for review
4. Create tenant mapping table: `legacy_district_id → new_tenant_uuid`

### 1.3 Entra ID User Provisioning Plan

**Decision Point**: Choose provisioning strategy

**Option A: Manual Invitations** (Recommended for <500 users)
1. Generate CSV with email addresses for each tenant
2. Use Entra ID bulk invite feature
3. Users receive email invitation to activate account
4. Users set up MFA during first login
5. Migration script links activated accounts after first successful login

**Option B: Automated Sync** (For >500 users or existing HR integration)
1. Configure Azure AD Connect or SCIM provisioning
2. Sync users from HR system or existing directory
3. Pre-create ExternalProviderLinks entries with expected Entra IDs
4. Validate sync before cutover

**Timeline**:
- Weeks 1-2: Send invitations or configure sync
- Week 3: Follow-up reminders for non-activated accounts
- Week 4: Manual outreach to VIP users (admins, principals)

---

## Migration Phase 2: Parallel Infrastructure (Weeks 2-4)

### 2.1 Deploy New Identity Service

**Deployment Checklist**:
- [ ] **PostgreSQL database** created with Identity schema
- [ ] **Row-Level Security policies** applied and tested
- [ ] **Redis Stack** configured for session caching
- [ ] **Microsoft.Identity.Web** configuration deployed:
  ```json
  {
    "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "<your-tenant-id>",
      "ClientId": "<web-app-client-id>",
      "ClientSecret": "<from-key-vault>",
      "CallbackPath": "/signin-oidc"
    },
    "ApiAudience": "<api-app-id-uri>"
  }
  ```
- [ ] **SessionAuthenticationHandler** registered in API service
- [ ] **TokenExchangeService** implemented and tested
- [ ] **Health checks** configured for Entra ID connectivity

### 2.2 Data Migration Scripts

**Script 1: Migrate Roles**
```sql
-- Insert legacy roles into new schema
INSERT INTO identity.roles (tenant_id, role_name, legacy_role_id, description)
SELECT 
  t.new_tenant_id,
  r.Name,
  r.Id,
  'Migrated from legacy role: ' || r.Name
FROM 
  legacy_db.AspNetRoles r
CROSS JOIN tenant_mapping t -- Roles are tenant-scoped
ON CONFLICT (role_name, tenant_id) DO NOTHING;
```

**Script 2: Migrate Users**
```sql
-- Insert legacy users into new schema
-- Note: PasswordHash NOT migrated - users must authenticate via Entra ID
INSERT INTO identity.users (tenant_id, email, legacy_user_id, created_at)
SELECT 
  tm.new_tenant_id,
  LOWER(TRIM(u.Email)), -- Normalize email
  u.Id,
  COALESCE(u.CreateDate, NOW())
FROM 
  legacy_db.AspNetUsers u
INNER JOIN tenant_mapping tm ON tm.legacy_district_id = (
    SELECT ClaimValue FROM legacy_db.AspNetUserClaims c
    WHERE c.UserId = u.Id AND c.ClaimType = 'http://northstaret.com/claims/districtid'
    LIMIT 1
  )
WHERE 
  u.Email IS NOT NULL 
  AND u.Email != ''
  AND u.Id NOT IN (SELECT legacy_user_id FROM migration_inactive_accounts)
ON CONFLICT (email, tenant_id) DO UPDATE
  SET legacy_user_id = EXCLUDED.legacy_user_id;

-- Create legacy provider links for historical reference
INSERT INTO identity.external_provider_links (user_id, provider, external_user_id, email, tenant_id)
SELECT 
  u.id,
  'Legacy',
  u.legacy_user_id,
  u.email,
  u.tenant_id
FROM identity.users u
WHERE u.legacy_user_id IS NOT NULL
ON CONFLICT (user_id, provider) DO NOTHING;
```

**Script 3: Migrate User-Role Assignments**
```sql
-- Migrate role assignments
INSERT INTO identity.user_roles (user_id, role_id, tenant_id)
SELECT 
  u.id,
  r.id,
  u.tenant_id
FROM 
  legacy_db.AspNetUserRoles lur
INNER JOIN identity.users u ON lur.UserId = u.legacy_user_id
INNER JOIN identity.roles r ON lur.RoleId = r.legacy_role_id AND r.tenant_id = u.tenant_id
ON CONFLICT (user_id, role_id) DO NOTHING;
```

**Validation Queries**:
```sql
-- Verify migration counts match
SELECT 'Users' as entity, COUNT(*) as legacy_count 
FROM legacy_db.AspNetUsers WHERE Email IS NOT NULL
UNION ALL
SELECT 'Users' as entity, COUNT(*) as new_count 
FROM identity.users WHERE deleted_at IS NULL;

SELECT 'Roles' as entity, COUNT(*) as legacy_count 
FROM legacy_db.AspNetRoles
UNION ALL
SELECT 'Roles' as entity, COUNT(*) as new_count 
FROM identity.roles;

SELECT 'UserRoles' as entity, COUNT(*) as legacy_count 
FROM legacy_db.AspNetUserRoles
UNION ALL
SELECT 'UserRoles' as entity, COUNT(*) as new_count 
FROM identity.user_roles;
```

### 2.3 Entra ID Provider Link Synchronization

**After users activate Entra ID accounts**, run linking script:

```sql
-- Update ExternalProviderLinks with Entra subject IDs
-- This happens automatically during first login via TokenExchangeService
-- Manual verification query:
SELECT 
  u.email,
  u.legacy_user_id,
  epl_legacy.external_user_id as legacy_id,
  epl_entra.external_user_id as entra_subject_id,
  epl_entra.last_sync as entra_linked_at
FROM identity.users u
LEFT JOIN identity.external_provider_links epl_legacy 
  ON u.id = epl_legacy.user_id AND epl_legacy.provider = 'Legacy'
LEFT JOIN identity.external_provider_links epl_entra 
  ON u.id = epl_entra.user_id AND epl_entra.provider = 'EntraID'
WHERE u.deleted_at IS NULL
ORDER BY epl_entra.last_sync DESC NULLS LAST;

-- Identify users who haven't linked Entra ID accounts yet
SELECT email, created_at
FROM identity.users u
WHERE NOT EXISTS (
  SELECT 1 FROM identity.external_provider_links epl
  WHERE epl.user_id = u.id AND epl.provider = 'EntraID'
)
AND deleted_at IS NULL
ORDER BY created_at;
```

---

## Migration Phase 3: Parallel Run Period (Weeks 5-17, 90 days)

### 3.1 Dual Authentication Support

**Configuration**:
- Legacy IdentityServer remains **read-only** for existing sessions
- New users are **required** to use Entra ID
- Existing users can continue with legacy sessions but are **encouraged** to switch
- Admin dashboard shows migration progress dashboard

**API Gateway Configuration**:
```csharp
// Support both legacy tokens and new session authentication
services.AddAuthentication()
    .AddJwtBearer("Legacy", options => {
        // Legacy IdentityServer token validation (read-only)
        options.Authority = "https://legacy.northstaret.com/identityserver";
        options.Audience = "northstar-api";
        options.TokenValidationParameters.ValidateLifetime = false; // Allow existing sessions
    })
    .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>("LmsSession", options => {
        // New session authentication
        options.SessionIdHeader = "X-Lms-Session-Id";
    });

services.AddAuthorization(options => {
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("Legacy", "LmsSession")
        .RequireAuthenticatedUser()
        .Build();
});
```

### 3.2 User Communication Plan

**Week 5-6: Announcement**
- Email all users: "Upcoming authentication system upgrade"
- Benefits: Improved security, MFA, password reset through Microsoft
- Action required: Activate Entra ID account (link in email)
- Timeline: 90-day transition period

**Week 8-10: Reminders**
- Targeted emails to users still on legacy authentication
- In-app notification banner: "Switch to new login for enhanced security"
- Tutorial videos/help articles for Entra ID onboarding

**Week 12-14: Final Push**
- Admin reports showing users not yet migrated
- Phone outreach to VIP users (principals, district admins)
- Optional: Schedule webinar/office hours for migration support

**Week 16: Sunset Warning**
- Final notification: "Legacy login will be disabled in 2 weeks"
- Emergency contact information for users needing assistance

### 3.3 Monitoring & Validation

**Daily Monitoring Queries**:
```sql
-- Migration progress dashboard
WITH migration_stats AS (
  SELECT 
    COUNT(*) as total_users,
    COUNT(CASE WHEN EXISTS (
      SELECT 1 FROM identity.external_provider_links epl
      WHERE epl.user_id = u.id AND epl.provider = 'EntraID'
    ) THEN 1 END) as entra_linked,
    COUNT(CASE WHEN EXISTS (
      SELECT 1 FROM identity.sessions s
      WHERE s.user_id = u.id AND s.expires_at > NOW()
    ) THEN 1 END) as active_sessions
  FROM identity.users u
  WHERE u.deleted_at IS NULL
)
SELECT 
  total_users,
  entra_linked,
  active_sessions,
  ROUND(100.0 * entra_linked / NULLIF(total_users, 0), 2) as pct_migrated,
  ROUND(100.0 * active_sessions / NULLIF(total_users, 0), 2) as pct_active
FROM migration_stats;

-- Recent authentication activity
SELECT 
  DATE(timestamp) as activity_date,
  event_type,
  COUNT(*) as event_count
FROM identity.audit_records
WHERE timestamp >= NOW() - INTERVAL '7 days'
GROUP BY DATE(timestamp), event_type
ORDER BY activity_date DESC, event_count DESC;
```

**Validation Checklist** (Run weekly):
- [ ] No data loss - User count matches legacy system
- [ ] Role assignments preserved - Spot check 10 random users
- [ ] Sessions functioning - Login success rate >98%
- [ ] Performance SLOs met - Token exchange <200ms P95
- [ ] No security incidents - Review audit logs for suspicious activity

---

## Migration Phase 4: Cutover (Week 18)

### 4.1 Pre-Cutover Validation

**72 Hours Before Cutover**:
- [ ] **Migration progress ≥95%** - At least 95% of active users linked to Entra ID
- [ ] **All administrators migrated** - Zero legacy sessions for admin roles
- [ ] **Final data sync** completed and validated
- [ ] **Rollback plan tested** - Ability to revert to legacy within 1 hour
- [ ] **On-call team briefed** - 24/7 coverage for cutover weekend
- [ ] **Communication sent** - Final notice to all users

### 4.2 Cutover Execution

**Cutover Window: Saturday 2:00 AM - 6:00 AM**

**Step 1: Enable Maintenance Mode** (2:00 AM)
```bash
# Display maintenance page
kubectl scale deployment/web-app --replicas=0
kubectl scale deployment/maintenance-page --replicas=3
```

**Step 2: Disable Legacy Authentication** (2:15 AM)
```csharp
// Update API Gateway configuration
services.AddAuthentication()
    .AddScheme<SessionAuthenticationOptions, SessionAuthenticationHandler>("LmsSession", options => {
        options.SessionIdHeader = "X-Lms-Session-Id";
    });
// Remove "Legacy" JWT Bearer scheme

services.AddAuthorization(options => {
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("LmsSession") // Only new auth
        .RequireAuthenticatedUser()
        .Build();
});
```

**Step 3: Final Data Verification** (2:30 AM)
```sql
-- Confirm no orphaned data
SELECT 'Orphaned UserRoles' as issue, COUNT(*) as count
FROM identity.user_roles ur
WHERE NOT EXISTS (SELECT 1 FROM identity.users u WHERE u.id = ur.user_id)
UNION ALL
SELECT 'Orphaned Sessions', COUNT(*)
FROM identity.sessions s
WHERE NOT EXISTS (SELECT 1 FROM identity.users u WHERE u.id = s.user_id);
-- Expected: 0 orphaned records
```

**Step 4: Archive Legacy Database** (3:00 AM)
```sql
-- Take final backup
-- Mark legacy database as read-only
ALTER DATABASE LoginContext SET READ_ONLY;

-- Archive to cold storage (Azure Blob Storage)
-- Keep backup accessible for 90 days post-cutover
```

**Step 5: Deploy Updated Configuration** (3:30 AM)
```bash
# Deploy new authentication configuration
kubectl apply -f k8s/identity-service-prod.yaml
kubectl apply -f k8s/api-gateway-prod.yaml
kubectl rollout status deployment/identity-service
kubectl rollout status deployment/api-gateway
```

**Step 6: Smoke Tests** (4:00 AM)
```bash
# Run automated smoke test suite
./scripts/smoke-test-identity.sh
# Expected: 100% pass rate

# Manual verification:
# 1. Admin login via Entra ID → Success
# 2. Teacher login via Entra ID → Success
# 3. View student roster → Data loads correctly
# 4. Attempt legacy login → Redirected to Entra ID
# 5. Session refresh → Works without re-authentication
```

**Step 7: Disable Maintenance Mode** (5:00 AM)
```bash
# Restore web application
kubectl scale deployment/maintenance-page --replicas=0
kubectl scale deployment/web-app --replicas=5
```

**Step 8: Monitor for Issues** (5:00 AM - 6:00 AM)
- Watch Aspire dashboard for errors
- Monitor login success rates (target >98%)
- Check support email/helpdesk for user reports
- Review session creation rates

### 4.3 Rollback Procedure (If Needed)

**Trigger Criteria**:
- Login success rate drops below 90%
- More than 10 critical bug reports within 1 hour
- Data integrity issue discovered
- Security incident detected

**Rollback Steps**:
1. **Enable maintenance mode** - 5 minutes
2. **Revert API Gateway configuration** to include "Legacy" auth - 10 minutes
3. **Restore legacy IdentityServer** from read-only to read-write - 10 minutes
4. **Redeploy previous API version** - 15 minutes
5. **Smoke test legacy authentication** - 10 minutes
6. **Disable maintenance mode** - 5 minutes
7. **Post-mortem** - Document root cause and remediation plan

---

## Post-Cutover Activities (Weeks 19-22)

### Week 19: Immediate Post-Cutover

**Monitoring** (Daily):
- Login success rates (target: >99%)
- Session creation latency (P95 <200ms)
- Token validation errors
- Support ticket volume

**Validation Queries**:
```sql
-- Daily active users (should match pre-migration levels)
SELECT DATE(created_at) as login_date, COUNT(DISTINCT user_id) as daily_active_users
FROM identity.sessions
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY DATE(created_at)
ORDER BY login_date;

-- Authentication errors
SELECT event_type, COUNT(*) as error_count, details->>'error_message' as error_msg
FROM identity.audit_records
WHERE event_type LIKE '%Error%' 
  AND timestamp >= NOW() - INTERVAL '24 hours'
GROUP BY event_type, details->>'error_message'
ORDER BY error_count DESC;
```

### Week 20-22: Optimization & Cleanup

**Performance Tuning**:
- Review Redis cache hit rates (target >95%)
- Optimize session query performance
- Tune Row-Level Security policies if needed
- Scale Redis/PostgreSQL if needed

**Legacy System Decommission**:
- [ ] **Day 30**: Verify no rollback needed - Migration success confirmed
- [ ] **Day 60**: Decommission legacy IdentityServer infrastructure
- [ ] **Day 90**: Archive legacy database to cold storage
- [ ] **Day 120**: Remove legacy authentication code from API Gateway

**Documentation Updates**:
- Update architecture diagrams to remove legacy components
- Update developer onboarding guides with Entra ID authentication
- Create runbook for common session authentication issues
- Document lessons learned and migration retrospective

---

## Validation Criteria Summary

### Data Migration Success Criteria

| Metric | Target | Validation Method |
|--------|--------|-------------------|
| User migration completeness | 100% of active users | SQL count comparison |
| Role assignment accuracy | 100% match | Spot check 50 random users |
| Email uniqueness | No duplicates | Email duplicate query returns 0 |
| Tenant isolation | Zero cross-tenant data leaks | RLS policy tests |
| Entra ID linkage | ≥95% before cutover | ExternalProviderLinks query |

### Performance Success Criteria

| Metric | Target | Measurement |
|--------|--------|-------------|
| Token exchange latency | <200ms P95 | Aspire metrics |
| Session validation latency | <20ms P95 | Redis cache hit rate >95% |
| Login success rate | >98% | Audit logs analysis |
| Session creation errors | <0.1% | Error rate monitoring |

### Security Success Criteria

| Metric | Target | Validation |
|--------|--------|------------|
| MFA adoption (admins) | 100% | Entra ID conditional access reports |
| Audit logging coverage | 100% auth events | Sample audit_records table |
| Session timeout enforcement | Correct expiration | Manual session expiry test |
| Cross-tenant isolation | Zero violations | RLS penetration testing |

---

## Troubleshooting Guide

### Issue 1: User Cannot Login After Migration

**Symptoms**: User sees "Authentication failed" after Entra ID login

**Root Causes**:
1. User hasn't activated Entra ID account
2. Email mismatch between legacy and Entra ID
3. User not assigned to correct tenant

**Resolution**:
```sql
-- Check user status
SELECT u.email, u.tenant_id, 
       epl_legacy.external_user_id as legacy_id,
       epl_entra.external_user_id as entra_id,
       epl_entra.last_sync
FROM identity.users u
LEFT JOIN identity.external_provider_links epl_legacy ON u.id = epl_legacy.user_id AND epl_legacy.provider = 'Legacy'
LEFT JOIN identity.external_provider_links epl_entra ON u.id = epl_entra.user_id AND epl_entra.provider = 'EntraID'
WHERE u.email = '<user-email>';

-- If Entra ID link missing:
-- 1. Verify user has activated Entra ID account (check Entra ID portal)
-- 2. Have user attempt login again to trigger automatic linking
-- 3. Manual link (last resort):
INSERT INTO identity.external_provider_links (user_id, provider, external_user_id, email, tenant_id)
VALUES ('<user-uuid>', 'EntraID', '<entra-subject-id>', '<email>', '<tenant-uuid>');
```

### Issue 2: Session Validation Failures

**Symptoms**: API returns 401 Unauthorized despite valid session cookie

**Root Causes**:
1. Redis cache miss + database connection issue
2. Session expired but cookie not cleared
3. Tenant context mismatch

**Resolution**:
```bash
# Check Redis connectivity
redis-cli PING
# Expected: PONG

# Check session in Redis
redis-cli GET "session:<session-id>"
# Expected: JSON with user/tenant context

# Check session in database
SELECT * FROM identity.sessions WHERE id = '<session-id>';
# Verify expires_at > NOW()

# Clear stale session
DELETE FROM identity.sessions WHERE expires_at < NOW();
redis-cli FLUSHDB # Clear cache (regenerates from DB)
```

### Issue 3: Duplicate Email Conflicts

**Symptoms**: User migration script fails with unique constraint violation

**Resolution**:
```sql
-- Identify duplicates
SELECT email, COUNT(*) as dup_count, STRING_AGG(legacy_user_id, ', ') as legacy_ids
FROM (
  SELECT LOWER(TRIM(Email)) as email, Id as legacy_user_id
  FROM legacy_db.AspNetUsers
  WHERE Email IS NOT NULL
) sub
GROUP BY email
HAVING COUNT(*) > 1;

-- Manual resolution strategy:
-- 1. Keep most recently active account
-- 2. Archive others with email suffix: user+archived1@domain.com
-- 3. Notify affected users via email
```

### Issue 4: Performance Degradation

**Symptoms**: Session validation taking >100ms P95

**Resolution**:
```sql
-- Check missing indexes
SELECT tablename, indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'identity'
ORDER BY tablename;

-- Add missing indexes if needed
CREATE INDEX CONCURRENTLY idx_sessions_user_expires 
ON identity.sessions(user_id, expires_at) 
WHERE expires_at > NOW();

CREATE INDEX CONCURRENTLY idx_sessions_entra_subject 
ON identity.sessions(entra_subject_id);

-- Check Redis cache hit rate
INFO stats # Look for keyspace_hits vs keyspace_misses
# Target: >95% hit rate
```

---

## Contact Information

**Migration Team**:
- **Migration Lead**: [Name] - [email]
- **Database Engineer**: [Name] - [email]
- **Security Lead**: [Name] - [email]
- **On-Call Support**: [phone/slack channel]

**Escalation Path**:
1. Check this runbook first
2. Post in #identity-migration Slack channel
3. Page on-call engineer (Critical issues only)
4. Rollback authorization: Migration Lead or CTO

---

## Appendix A: SQL Migration Scripts

All migration scripts are available in:
- `scripts/migration/001-create-target-schema.sql`
- `scripts/migration/002-migrate-roles.sql`
- `scripts/migration/003-migrate-users.sql`
- `scripts/migration/004-migrate-user-roles.sql`
- `scripts/migration/005-validation-queries.sql`

---

## Appendix B: Rollback Checklist

Complete rollback checklist stored in:
- `docs/runbooks/identity-migration-rollback.md`

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-11-20 | Migration Team | Initial migration guide |

