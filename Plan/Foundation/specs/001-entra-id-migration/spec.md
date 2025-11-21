# Feature Specification: Legacy IdentityServer to Microsoft Entra ID Migration

**Feature ID**: `001-entra-id-migration`  
**Short Name**: entra-id-migration  
**Target Layer**: Foundation  
**Parent Specification**: [Identity Service with Entra ID](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/README.md) (`01-identity-service-entra-id`)  
**Business Value**: Execute one-time migration from legacy IdentityServer to Microsoft Entra ID, enabling enterprise SSO and eliminating local password management  
**Created**: 2025-11-20  
**Status**: Draft

---

## Relationship to Identity Service Specification

**⚠️ IMPORTANT**: This specification is a **focused implementation guide** for migrating from legacy IdentityServer to Microsoft Entra ID. It is a **subset** of the comprehensive Identity Service specification:

- **Parent Spec**: [`Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/`](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/)  
- **This Spec Corresponds To**: **Phase 6** ("IdentityServer User Migration") of the parent implementation plan  
- **Scope**: Migration execution only - authentication architecture, session management, and authorization are fully detailed in the parent spec

### What This Spec Covers

✅ **Migration-Specific Concerns**:
- Legacy user data mapping (IdentityServer → Entra ID)
- Email-based user matching and linking
- ExternalProviderLinks table population
- Role and permission preservation
- Legacy password deprecation
- Migration rollback procedures
- Data validation and error handling

### What This Spec Does NOT Cover (See Parent Spec)

❌ **Covered in Parent Specification**:
- Full authentication architecture and flow design
- Session management implementation (Redis caching, token refresh)
- Authorization and RBAC implementation
- API endpoint contracts and schemas
- Multi-tenant context switching
- Audit logging infrastructure
- Performance optimization and caching strategies
- Complete testing strategy (unit/integration/BDD)

**📖 For Complete Context**: Review the [parent specification](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/spec.md) before implementing this migration.

---

## Goal / Why

Execute a **one-time, reversible migration** from the legacy IdentityServer authentication system to Microsoft Entra ID for all existing NorthStar LMS users. This migration:

- **Preserves User Identity**: Matches existing users by email address to Entra ID accounts
- **Maintains Access**: Preserves all role assignments, permissions, and tenant associations
- **Zero Credential Loss**: Links legacy user records to Entra ID subjects without data loss
- **Enables Modern Auth**: Unlocks SSO, MFA, and enterprise identity features post-migration
- **Deprecates Legacy Auth**: Marks legacy passwords as deprecated, forcing Entra ID authentication

This is a **prerequisite** for decommissioning the legacy IdentityServer infrastructure and realizing the full benefits of the Identity Service architecture.

---

## Migration Strategy

### Approach: Email-Based User Matching

**Assumption**: All NorthStar users have email addresses that match their Entra ID User Principal Names (UPNs) or email attributes.

**Process**:
1. **Pre-Migration Audit**: Identify users without email matches in Entra ID
2. **Automated Matching**: Link users where `legacy.Email == entra.UserPrincipalName`
3. **Manual Review**: Flag unmatched users for administrator resolution
4. **ExternalProviderLink Creation**: Establish `Provider="EntraID"` links for all matched users
5. **Role Preservation**: Copy role assignments to new identity schema
6. **Legacy Deprecation**: Set `deleted_at` timestamp on legacy auth records

### Rollback Plan

If critical issues arise post-migration:
1. **Revert Authentication Configuration**: Re-enable IdentityServer endpoints
2. **Session Cleanup**: Invalidate all Entra ID-based sessions
3. **ExternalProviderLinks**: Mark as inactive (do NOT delete - preserve for retry)
4. **Audit**: Log rollback event with justification

---

## Migration Scenarios

### Scenario 1: Automated User Migration with Email Match

**Given** the legacy IdentityServer database contains 1,000 user accounts  
**And** the Entra ID tenant has 1,000 corresponding user accounts  
**And** 950 users have exact email matches between systems  
**When** the migration script executes  
**Then** 950 ExternalProviderLinks are created with `Provider="EntraID"` and `ProviderSubjectId={entra_oid}`  
**And** all 950 users' role assignments are preserved in the `identity.user_roles` table  
**And** legacy password hashes for these users are marked deprecated  
**And** the 50 unmatched users are flagged in a migration report for manual review

---

### Scenario 2: Manual User Linking for Edge Cases

**Given** a user exists in legacy IdentityServer with email `john.smith@district.edu`  
**And** the corresponding Entra ID account uses email `jsmith@district.edu` (mismatch)  
**And** the user was flagged during automated migration  
**When** an administrator reviews the migration report  
**And** confirms the accounts represent the same person  
**And** manually creates the ExternalProviderLink via admin tool  
**Then** the user can authenticate via Entra ID with the linked account  
**And** their historical data (roles, permissions, activity logs) is preserved

---

### Scenario 3: Post-Migration Authentication Validation

**Given** the migration has completed successfully  
**And** a migrated user navigates to the NorthStar login page  
**When** they click "Sign in with Microsoft"  
**And** authenticate with their Entra ID credentials  
**Then** the system locates their ExternalProviderLink by `ProviderSubjectId`  
**And** loads their User record with preserved roles and tenant associations  
**And** establishes a session with the correct district context  
**And** the user sees their historical dashboard data without interruption

---

### Scenario 4: Legacy Password Deprecation Enforcement

**Given** a user was successfully migrated to Entra ID  
**And** their legacy password hash still exists in the database  
**When** they attempt to authenticate using the legacy IdentityServer endpoint (if still accessible)  
**Then** the system rejects the authentication  
**And** displays a message: "Authentication has been modernized. Please use 'Sign in with Microsoft'"  
**And** logs the deprecated authentication attempt for security audit

---

### Scenario 5: Role and Permission Preservation Verification

**Given** a teacher user had the following roles in legacy IdentityServer:  
- `Teacher` (tenant: District A)  
- `GradeBookAdmin` (tenant: District A)  
**When** the migration script processes this user  
**Then** two records are created in `identity.user_roles`:  
- `role_id = {Teacher_role_id}`, `tenant_id = {District_A_id}`  
- `role_id = {GradeBookAdmin_role_id}`, `tenant_id = {District_A_id}`  
**And** when the user logs in via Entra ID post-migration  
**And** accesses the GradeBook module  
**Then** their permissions are correctly evaluated based on preserved roles

---

### Scenario 6: Migration Audit and Reporting

**Given** the migration script has completed execution  
**When** an administrator reviews the migration summary report  
**Then** the report includes:  
- Total users in legacy system: 1,000  
- Successfully migrated: 950  
- Flagged for manual review: 50  
- ExternalProviderLinks created: 950  
- Role assignments preserved: 2,300  
- Legacy passwords deprecated: 950  
**And** detailed CSV export of flagged users with:  
- Legacy email  
- Suggested Entra ID matches (fuzzy search)  
- Manual resolution instructions

---

### Scenario 7: Multi-Tenant User Migration

**Given** a staff member exists in legacy IdentityServer with access to 3 districts:  
- District A (role: `DistrictAdmin`)  
- District B (role: `Teacher`)  
- District C (role: `Teacher`)  
**When** the migration script processes this user  
**Then** one ExternalProviderLink is created linking to their Entra ID account  
**And** three `identity.user_roles` records are created (one per district+role)  
**And** when the user authenticates post-migration  
**And** selects District A from the tenant switcher  
**Then** they have `DistrictAdmin` permissions for District A  
**And** when they switch to District B or C  
**Then** they have `Teacher` permissions for those districts

---

## Layer Identification

**Target Layer**: Foundation  
**Layer Validation Checklist**:  
- [x] Layer exists in mono-repo (`Src/Foundation/`)  
- [x] Meets constitutional requirements (migration is a one-time Foundation service task)  

**Cross-Layer Dependencies**:  
- `Src/Foundation/shared/Infrastructure` (Auth handlers, data contexts)  
- `Src/Foundation/services/Identity/` (Identity Service implementation from parent spec)  

**Justification**: Migration is a **data transformation task** within the Identity Service domain. It modifies Foundation layer data (users, roles, sessions) and must execute in the context of the Identity Service database.

---

## Acceptance Criteria (Migration Success)

- ✅ **≥95% automated match rate** (email-based linking)  
- ✅ **100% role preservation** for migrated users  
- ✅ **Zero data loss** (all legacy user records retained with ExternalProviderLinks)  
- ✅ **Post-migration login success rate ≥99%** (excluding unmatched users)  
- ✅ **Complete audit trail** (migration report with user-level details)  
- ✅ **Rollback tested** in staging environment before production execution

---

## References

- **Parent Specification**: [Identity Service with Entra ID](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/spec.md)  
- **Data Model**: [Identity Service Data Model](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model-enhanced.md)  
- **API Contracts**: [Authentication API](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/contracts/auth-api.md)  
- **Architecture Reference**: [Identity Service Architecture](../../../architecture/services/identity-service.md)  

---

**Specification Branch**: `Foundation/001-entra-id-migration-spec`  
**Implementation Branch**: `Foundation/001-entra-id-migration`
