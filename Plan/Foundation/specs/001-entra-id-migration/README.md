# Legacy IdentityServer to Microsoft Entra ID Migration

**Feature ID**: `001-entra-id-migration`  
**Target Layer**: Foundation  
**Parent Specification**: [Identity Service with Entra ID](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/README.md)  
**Status**: Draft  
**Created**: 2025-11-20

---

## Overview

This specification defines the **one-time migration** from legacy IdentityServer authentication to Microsoft Entra ID for all existing NorthStar LMS users. This is a **focused implementation guide** that corresponds to **Phase 6** of the comprehensive Identity Service specification.

**⚠️ IMPORTANT**: This spec is a **subset** of the parent Identity Service specification. For complete authentication architecture, session management, and authorization details, refer to the [parent specification](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/README.md).

---

## Quick Links

- **Main Specification**: [spec.md](./spec.md) - Migration scenarios and acceptance criteria
- **Implementation Plan**: [plan.md](./plan.md) - 5-phase migration execution plan
- **Data Model**: [data-model.md](./data-model.md) - ExternalProviderLinks schema and EF migrations
- **Research**: [research.md](./research.md) - Migration strategy decisions and tooling
- **Contracts**: [contracts/](./contracts/) - Migration API endpoints (if applicable)

---

## Relationship to Parent Specification

### Parent: Identity Service with Entra ID
**Location**: [`Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/`](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/)

The parent specification defines the **complete Identity Service architecture**:
- ✅ OAuth 2.0/OIDC authentication with Entra ID
- ✅ Session management (Redis caching, token refresh)
- ✅ Authorization and RBAC
- ✅ API Gateway integration
- ✅ Multi-tenant context switching
- ✅ Audit logging and observability

### This Specification: Migration Execution
**This spec focuses exclusively on**:
- 🎯 Legacy user data mapping (IdentityServer → Entra ID)
- 🎯 Email-based user matching algorithm
- 🎯 ExternalProviderLinks table population
- 🎯 Role and permission preservation
- 🎯 Legacy password deprecation
- 🎯 Migration rollback procedures

**Prerequisites**: The Identity Service (Phases 1-5 of parent spec) must be implemented and operational before executing this migration.

---

## Business Value

**Goal**: Execute a **reversible, one-time migration** of 1,000+ existing users from legacy IdentityServer to Microsoft Entra ID.

**Benefits**:
- **Zero User Impact**: Users authenticate with Entra ID using existing credentials (email + password managed by Microsoft)
- **Preserved Access**: All role assignments, permissions, and tenant associations migrate intact
- **Audit Trail**: Complete migration report with user-level success/failure tracking
- **Rollback Safety**: All legacy data preserved; migration reversible if issues arise

**Success Metrics**:
- ≥95% automated match rate (email-based linking)
- 100% role preservation for migrated users
- ≥99% post-migration login success rate
- Zero data loss

---

## Migration Strategy Summary

### Approach: Email-Based Matching

1. **Pre-Migration Audit**: Generate report comparing legacy users to Entra ID accounts
2. **Automated Matching**: Link users where `legacy.Email == entra.UserPrincipalName`
3. **ExternalProviderLink Creation**: Store Entra ID Object ID in new junction table
4. **Role Preservation**: Copy role assignments to new schema (no loss)
5. **Legacy Deprecation**: Mark passwords as deprecated (kept for audit/rollback)
6. **Manual Resolution**: Admin tool for unmatched users (fuzzy matching suggestions)

**Expected Match Rate**: ≥95% (based on data audit)

### Data Flow

```
Legacy IdentityServer            Microsoft Entra ID
─────────────────────            ───────────────────
User: john@district.edu    ←→    User: john@district.edu
Password: hashed                  Object ID: abc-123-def
Roles: Teacher, Admin             Roles: (optional)

                    ↓
            Migration Service
                    ↓
         NorthStar Identity DB
         ─────────────────────
         User: john@district.edu
         Password: deprecated (kept)
         ExternalProviderLink:
           Provider: EntraID
           SubjectId: abc-123-def
         Roles: Teacher, Admin (preserved)
```

---

## Key Documents

### [spec.md](./spec.md) - Feature Specification
- 7 migration scenarios with Given/When/Then acceptance criteria
- Automated migration, manual linking, post-migration validation
- Role preservation verification
- Multi-tenant user handling

### [plan.md](./plan.md) - Implementation Plan
**5 Phases** (5 weeks total):
1. **Pre-Migration Preparation** (Week 1): Entra ID setup, Graph API config, pre-migration audit
2. **Automated User Migration** (Week 2): Email matching, link creation, role preservation
3. **Manual Exception Handling** (Week 3): Admin tool for unmatched users, fuzzy matching
4. **Validation & Rollback Readiness** (Week 4): Post-migration tests, rollback script testing
5. **Production Execution** (Week 5): Production migration, monitoring, final report

### [data-model.md](./data-model.md) - Database Schema
- New table: `identity.external_provider_links` (Entra ID linkage)
- New column: `identity.users.auth_deprecated_at` (legacy auth deprecation flag)
- EF Core migration scripts
- Rollback data state

### [research.md](./research.md) - Technical Decisions
- Email matching vs alternatives (name+DOB, SSN)
- Legacy password handling (deprecate, not delete)
- Role preservation strategy (NorthStar roles take precedence)
- Fuzzy matching algorithm (Levenshtein distance)
- Microsoft Graph API integration
- Rollback strategy

---

## Implementation Prerequisites

**Before starting this migration**:

1. ✅ **Identity Service Phases 1-5 complete** (from parent spec):
   - Entra ID authentication flow operational
   - Session management working (Redis + PostgreSQL)
   - Authorization and RBAC functional
   - Token refresh background service running

2. ✅ **Entra ID Tenant Provisioned**:
   - All users exist in Entra ID with email addresses
   - App registrations created (Web + API)
   - Service principal for Graph API access configured

3. ✅ **Staging Environment Ready**:
   - Test migration executed successfully
   - Rollback tested and validated
   - Performance benchmarks met (≥99% auth success)

---

## Acceptance Criteria

- ✅ **≥95% automated match rate** (email-based linking)
- ✅ **100% role preservation** for migrated users
- ✅ **Zero data loss** (all legacy user records retained)
- ✅ **Post-migration login success rate ≥99%**
- ✅ **Complete audit trail** (migration report with user-level details)
- ✅ **Rollback tested** in staging before production
- ✅ **Red→Green evidence** for all test phases (unit, integration, BDD)

---

## Testing Strategy

### Unit Tests
- Email matching algorithm (exact + fuzzy)
- Microsoft Graph API client (mocked)
- ExternalProviderLink creation logic
- Role preservation validation

### Integration Tests (Testcontainers)
- Full migration with seeded test data (100+ users)
- Post-migration authentication flow
- Rollback and re-migration cycle

### BDD Tests (Reqnroll)
See parent spec: [features/08-user-migration.feature](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/features/08-user-migration.feature)

**Scenarios**:
- Automated user migration with email match
- Manual user linking for edge cases
- Post-migration authentication validation
- Legacy password deprecation enforcement
- Role preservation verification
- Migration audit and reporting
- Multi-tenant user migration

---

## Migration Timeline

| Phase | Duration | Key Deliverables |
|-------|----------|------------------|
| **Phase 1**: Preparation | 1 week | Entra ID setup, Graph API config, pre-migration report |
| **Phase 2**: Automated Migration | 1 week | Email matching, link creation, role preservation |
| **Phase 3**: Manual Exceptions | 1 week | Admin tool, fuzzy matching, manual linking |
| **Phase 4**: Validation | 1 week | Post-migration tests, rollback script, runbook |
| **Phase 5**: Production | 1 week | Production execution, monitoring, final report |

**Total**: 5 weeks (with 1-week buffer for contingencies)

---

## Rollback Plan

**Triggers**: Auth failure rate >5%, critical bugs, data integrity issues

**Procedure**:
1. Set `external_provider_links.is_active = FALSE` (preserve data)
2. Clear `users.auth_deprecated_at` (re-enable legacy auth)
3. Revert authentication config (re-enable IdentityServer)
4. Invalidate Entra ID sessions (clear Redis cache)
5. Log rollback event in audit records

**Testing**: Rollback validated in staging (Phase 4)

---

## References

### Parent Specification
- [Identity Service with Entra ID](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/README.md) - Complete authentication architecture
- [Identity Service Implementation Plan](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/plan.md) - Phase 6 corresponds to this migration
- [Identity Service Data Model](../../../CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model-enhanced.md) - Full schema

### Microsoft Documentation
- [Microsoft Graph Users API](https://learn.microsoft.com/en-us/graph/api/resources/user)
- [Entra ID Authentication](https://learn.microsoft.com/en-us/entra/identity-platform/v2-protocols-oidc)

### Project Standards
- [Constitution](../../../../.specify/memory/constitution.md) - Testing requirements, Red→Green evidence
- [LAYERS.md](../../../../LAYERS.md) - Mono-repo layer architecture

---

**Status**: Ready for review and approval  
**Next Step**: Execute Phase 1 in development environment
