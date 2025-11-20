# Identity Architecture Update: IdentityServer to Entra ID

**Date**: 2025-11-20  
**Status**: Documentation Updated  
**Impact**: All planning and architecture documentation

---

## Summary of Changes

This update removes all references to Duende IdentityServer and documents the modern Microsoft Entra ID architecture with custom session authentication that is actually implemented in the codebase.

### Key Architectural Decisions

1. **Identity Provider**: Microsoft Entra ID (Azure AD) - NOT Duende IdentityServer
2. **Authentication Pattern**: Session-based with custom SessionAuthenticationHandler
3. **Token Handling**: Validate Entra ID tokens using Microsoft.Identity.Web (NOT issue custom tokens)
4. **Session Storage**: PostgreSQL (identity.sessions table) + Redis caching
5. **Token Exchange**: Backend-for-Frontend (BFF) pattern - Entra tokens → LMS session IDs

### Migration Strategy

- **Parallel Run Period**: 90 days (legacy IdentityServer read-only, new Entra ID active)
- **User Account Migration**: Email-based matching with ExternalProviderLinks table
- **Cutover Date**: TBD (after 95% of users migrate to Entra ID)
- **Complete Migration Guide**: See `Plan/Foundation/Plans/docs/legacy-identityserver-migration.md`

---

## Files Updated

### Constitution & Governance

✅ **`.specify/memory/constitution.md`**
- Line 89: Updated Principle 6 shared infrastructure from "Duende IdentityServer, Microsoft Entra ID integration" to "Microsoft Entra ID with custom session authentication"

### Architecture Documentation

✅ **`docs/architecture/services/identity-service.md`**
- Updated technology stack to Microsoft Entra ID via Microsoft.Identity.Web
- Replaced Duende IdentityServer infrastructure with SessionAuthenticationHandler
- Updated database schema (removed PersistedGrants, added Sessions, ExternalProviderLinks, AuditRecords)
- Changed service responsibilities from "token issuance" to "token validation"

### Service Implementation Guides

✅ **`Src/Foundation/services/Identity/README.md`**
- Replaced Duende IdentityServer with Microsoft Entra ID integration
- Updated token management from issuance to validation and session management
- Added BFF token exchange pattern documentation

### Migration Plans

✅ **`Plan/Foundation/Plans/MASTER_MIGRATION_PLAN.md`**
- Updated technology stack from "Duende IdentityServer (OAuth 2.0/OIDC)" to "Microsoft Entra ID with Microsoft.Identity.Web"
- Added custom session authentication reference

✅ **`Plan/Foundation/Plans/MIGRATION_READINESS.md`**
- Replaced "Duende IdentityServer license acquired" checklist with "Microsoft Entra ID tenant configured"
- Added SessionAuthenticationHandler implementation checklist
- Added Redis Stack configuration requirement

✅ **`Plan/Foundation/Plans/MIGRATION_PLAN.md`**
- No changes needed (references MASTER_MIGRATION_PLAN)

✅ **`Plan/Foundation/Plans/SERVICE_CATALOG.md`**
- Updated shared authentication description from "supports both legacy IdentityServer tokens and new Entra ID tokens" to "Microsoft Entra ID with session-based authentication"
- Added 90-day parallel run period note

### Scenarios & Specifications

✅ **`Plan/Foundation/Plans/scenarios/01-identity-migration-entra-id.md`**
- Completely rewrote Technical Implementation Notes section
- Removed "Duende IdentityServer as proxy" approach
- Added detailed BFF token exchange flow documentation
- Updated database schema to reflect session-based approach
- Added performance SLOs for token exchange and session validation

✅ **`Plan/Foundation/specs/Foundation/002-identity-authentication/spec.md`**
- Updated Goal section from "modern Duende IdentityServer on .NET 8" to "Microsoft Entra ID integration"
- Changed Service Boundary from "token issuance" to "token validation"
- Updated Handoff section to remove Duende configuration tasks
- Added Microsoft.Identity.Web configuration and SessionAuthenticationHandler tasks

### New Documentation Created

✅ **`Plan/Foundation/Plans/docs/legacy-identityserver-migration.md`** (NEW)
- Complete 90-day migration guide
- Legacy database schema mapping
- Entra ID provisioning strategies
- Parallel run configuration
- Cutover execution plan with rollback procedures
- Post-cutover validation criteria
- Troubleshooting guide

### Template Files Updated

✅ **`.specify/templates/spec-template.md`**
- Added Identity & Authentication Requirements guidance section
- Mandates Microsoft Entra ID (NOT Duende IdentityServer)
- References legacy-identityserver-migration.md for architecture

✅ **`.specify/templates/plan-template.md`**
- Added Identity & Authentication Guidance section in Technical Context
- Specifies session-based authentication pattern
- Lists required dependencies (Microsoft.Identity.Web, StackExchange.Redis)

✅ **`.specify/templates/tasks-template.md`**
- Added Identity & Authentication Compliance validation section
- Checklist to prevent Duende IdentityServer references
- Validates Microsoft.Identity.Web usage
- Ensures SessionAuthenticationHandler and Redis configuration

### Reference Implementation Cleanup

✅ **`.referenceSrc/Plans/architecture/services/identity-service.md`**
- Added OBSOLETE notice pointing to current documentation

✅ **`.referenceSrc/specs/Foundation/002-identity-authentication/spec.md`**
- Added OBSOLETE notice pointing to current specification

---

## Implementation Checklist

For teams implementing authentication features:

### DO Use

- ✅ Microsoft Entra ID as identity provider
- ✅ Microsoft.Identity.Web for JWT token validation
- ✅ SessionAuthenticationHandler for API authorization
- ✅ PostgreSQL sessions table + Redis caching
- ✅ BFF token exchange pattern (Web → API)
- ✅ HTTP-only, secure, SameSite cookies for session IDs
- ✅ ExternalProviderLinks table to map users to Entra subjects

### DO NOT Use

- ❌ Duende IdentityServer
- ❌ Custom JWT token generation/issuance
- ❌ Self-hosted OIDC provider
- ❌ PersistedGrants table (Duende-specific)
- ❌ Pure JWT bearer token authentication (use sessions instead)

---

## Code References

The actual implementation exists in the WIPNorthStar codebase:

**Authentication Handler**:
- `SessionAuthenticationHandler.cs` - Custom ASP.NET Core authentication scheme
- `SessionAuthenticationOptions.cs` - Configuration
- `SessionAuthenticationDefaults.cs` - Constants

**Token Exchange (BFF)**:
- `TokenExchangeService.cs` - Exchanges Entra tokens for LMS sessions
- `ExchangeEntraTokenCommand.cs` - MediatR command
- `ExchangeEntraTokenCommandHandler.cs` - Business logic

**Session Management**:
- `ISessionRepository.cs` - Session persistence interface
- `SessionRepository.cs` - EF Core implementation with Redis caching
- `SessionRefreshService.cs` - Background service for session cleanup

**Configuration**:
- `Program.cs` (API) - Microsoft.Identity.Web registration
- `Program.cs` (Web) - OIDC configuration with Entra ID

---

## Migration Timeline

| Phase | Duration | Activities |
|-------|----------|------------|
| **Phase 1: Preparation** | Week 1 | Data analysis, tenant mapping, Entra ID provisioning |
| **Phase 2: Parallel Infrastructure** | Weeks 2-4 | Deploy new Identity service, migrate data, test |
| **Phase 3: Parallel Run** | Weeks 5-17 (90 days) | Dual auth support, user migration, monitoring |
| **Phase 4: Cutover** | Week 18 | Disable legacy auth, validate, monitor |
| **Phase 5: Decommission** | Weeks 19-22 | Optimization, legacy system decommission |

---

## Support & Questions

- **Migration Guide**: `Plan/Foundation/Plans/docs/legacy-identityserver-migration.md`
- **Architecture Docs**: `docs/architecture/services/identity-service.md`
- **Scenario Testing**: `Plan/Foundation/Plans/scenarios/01-identity-migration-entra-id.md`
- **Constitution**: `.specify/memory/constitution.md` (Principle 6 - Shared Infrastructure)

---

## Validation

All documentation has been updated to ensure consistency. To verify:

```bash
# Search for any remaining IdentityServer references (should only find legacy/historical context)
grep -r "Duende\|IdentityServer" Plan/ docs/ Src/ .specify/ --include="*.md" | grep -v "legacy\|obsolete\|OBSOLETE\|historical\|replaced"

# Expected: Only legitimate references to "legacy IdentityServer" or obsolete markers
```

---

**Last Updated**: 2025-11-20  
**Next Review**: Before Phase 3 cutover (validate all docs align with implementation)
