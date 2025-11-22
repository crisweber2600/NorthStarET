# Scenario Processing Report

**Date**: 2025-11-20T15:30:00Z  
**Total Scenarios Processed**: 1  
**Successful**: 1  
**Failed**: 0  
**Target Layer**: CrossCuttingConcerns

---

## Branch Strategy (Constitution v2.1.0)

Each scenario generates **two branches**:
1. **Specification Branch** (`CrossCuttingConcerns/[###]-feature-name-spec`): Contains planning artifacts (spec.md, plan.md, research.md, data-model.md, contracts/)
2. **Proposed Branch** (`CrossCuttingConcerns/[###]-feature-name-proposed`): Copy of spec branch for stakeholder review
3. **Implementation Branch** (`CrossCuttingConcerns/[###]-feature-name`): Created later by `/speckit.implement` when development begins

---

## Results by Scenario

### ✅ 01-identity-service.md

**Feature**: Identity Service: Microsoft Entra ID Authentication & Authorization

- **Layer**: CrossCuttingConcerns (Foundation Service)
- **Specification Branch**: `CrossCuttingConcerns/01-identity-service-entra-id-spec` ✅ (published)
- **Proposed Branch**: `CrossCuttingConcerns/01-identity-service-entra-id-proposed` ✅ (published)
- **Spec**: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/spec.md` (43KB, 820 lines)
- **Plan**: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/plan.md` (~13,000 lines)
- **Research**: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/research.md` (~8,000 lines)
- **Data Model**: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/data-model.md` (~6,500 lines)
- **API Contracts**: `Plan/CrossCuttingConcerns/specs/01-identity-service-entra-id/contracts/identity-api-contracts.md` (~5,000 lines)
- **Remote URLs**:
  - Spec branch: https://github.com/crisweber2600/NorthStarET/tree/CrossCuttingConcerns/01-identity-service-entra-id-spec
  - Proposed branch: https://github.com/crisweber2600/NorthStarET/tree/CrossCuttingConcerns/01-identity-service-entra-id-proposed
  - Pull Request (Proposed): https://github.com/crisweber2600/NorthStarET/pull/new/CrossCuttingConcerns/01-identity-service-entra-id-proposed
- **Status**: ✅ Complete

#### Generated Artifacts Summary

**Specification (spec.md)**:
- 10 comprehensive user scenarios with BDD acceptance criteria
- All scenarios prioritized (P1, P2, P3) with rationale
- Complete database schema (6 tables: Users, Roles, UserRoles, Sessions, ExternalProviderLinks, AuditRecords)
- Backend-for-Frontend authentication flow (token exchange pattern)
- Performance SLOs (token exchange <200ms, session validation <20ms cached)
- Security requirements (RS256, HTTP-only cookies, audit logging)
- 10 edge case scenarios with handling strategies
- Migration strategy from legacy IdentityServer
- Success metrics and open questions

**Plan (plan.md)**:
- Architecture overview with BFF pattern and Clean Architecture layers
- 8 implementation phases with detailed tasks:
  1. Entra ID Configuration & Infrastructure Setup
  2. Database Schema & Migrations
  3. Token Exchange & Session Management
  4. Authorization & Role Mapping
  5. Session Refresh & Background Services
  6. Audit Logging & Security Events
  7. API Gateway Integration
  8. Testing & Documentation
- Technology stack with specific package versions (Microsoft.Identity.Web 2.15.5, EF Core 9.0, etc.)
- Service structure (Domain, Application, Infrastructure, API layers)
- Testing strategy with Red→Green evidence requirements (≥80% coverage)
- Infrastructure setup (Aspire, Entra ID app registrations, PostgreSQL, Redis)
- Security considerations and threat model
- Performance optimization strategies
- Deployment strategy with migration plan

**Research (research.md)**:
- 5 major technology decisions with detailed rationale:
  1. Microsoft.Identity.Web vs alternatives (IdentityServer, Auth0, Duende)
  2. Custom SessionAuthenticationHandler justification
  3. Session storage strategy (PostgreSQL + Redis hybrid)
  4. Token refresh architecture (background service vs on-demand)
  5. Multi-tenancy strategy (separate schemas vs shared tables with RLS)
- Security patterns and token validation approaches
- Performance optimization techniques (Redis caching, connection pooling)
- Alternative approaches considered and rejected with reasoning
- Complete package selection with versions and licenses

**Data Model (data-model.md)**:
- Entity Relationship Diagram with Mermaid syntax
- 6 domain entities with complete property specifications:
  - User (id, tenant_id, email, entra_subject_id, created_at, updated_at, deleted_at)
  - Session (id, user_id, entra_subject_id, tenant_id, access_token_expires_at, refresh_token_expires_at)
  - Role (id, tenant_id, role_name, permissions_json, description)
  - UserRole (user_id, role_id, tenant_id, assigned_at, assigned_by)
  - ExternalProviderLink (user_id, provider, external_user_id, email, last_sync_at, tenant_id)
  - AuditRecord (id, user_id, event_type, tenant_id, ip_address, user_agent, timestamp, details_json)
- Value objects (SessionId, EntraSubjectId, TenantId, RolePermissions)
- Complete PostgreSQL schema with constraints, foreign keys, and indexes
- Strategic indexes for <100ms query performance (tenant_id + email, tenant_id + created_at)
- Multi-tenancy strategy with global query filters (`HasQueryFilter(e => e.TenantId == _tenantContext.CurrentTenantId)`)
- Soft delete support (`HasQueryFilter(e => e.DeletedAt == null)`)
- Audit and compliance requirements (FERPA/COPPA)
- EF Core migration scripts with up/down operations

**API Contracts (contracts/identity-api-contracts.md)**:
- 6 REST API endpoints with complete OpenAPI 3.0.3 specifications:
  - POST /api/auth/exchange-token (Entra token → LMS session)
  - POST /api/auth/logout (Session termination)
  - GET /api/auth/session (Session validation)
  - POST /api/auth/switch-tenant (Cross-district access)
  - POST /api/auth/refresh (Token refresh)
  - GET /api/auth/permissions (Role-based authorization check)
- RFC 7807 Problem Details error handling with standard error codes
- Rate limiting specifications (10 req/min for token exchange, 60 req/min for session validation)
- Authentication patterns (Bearer tokens + Session Cookie hybrid)
- Request/response examples with cURL commands
- HTTP status code mappings (200, 400, 401, 403, 429, 500)
- Security headers (X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security)

#### Layer Consistency Validation

**Target Layer**: CrossCuttingConcerns (Foundation Service) ✅
- Identity is correctly identified as a foundation service
- No dependencies on application-specific services (Student Management, Assessment, etc.)
- Only uses shared infrastructure (ServiceDefaults, Domain, Application, Infrastructure)

**Shared Infrastructure Usage**:
- ✅ ServiceDefaults: Aspire orchestration, health checks, telemetry, service discovery
- ✅ Domain: Base entity classes (Entity<TId>, IAggregateRoot, IDomainEvent)
- ✅ Application: ICommand, IQuery<T>, Result/Result<T>, ValidationBehavior, AbstractValidator
- ✅ Infrastructure: TenantInterceptor, AuditInterceptor, IdempotencyService, PostgreSQL connection

**Dependency Flow** (Constitution Principle 6):
```
Identity.API → Identity.Application → Identity.Domain ✅
Identity.Application → Shared.Application ✅
Identity.Infrastructure → Shared.Infrastructure ✅
Identity.Domain → Shared.Domain ✅
Identity.AppHost → Shared.ServiceDefaults ✅
```

No reverse dependencies or cross-layer violations detected ✅

#### Constitution Compliance (v2.1.0)

**Principle 1: Clean Architecture with Aspire** ✅
- Domain → Application → Infrastructure → API layers strictly enforced
- No reverse dependencies (e.g., Domain does NOT reference Application)
- Aspire orchestration with PostgreSQL (per-service DB), Redis Stack, Azure Service Bus

**Principle 2: Test-Driven Quality Gates** ✅
- Red→Green evidence capture mandated for all 8 phases
- ≥80% coverage target (Unit 70% + Integration 15% + BDD 10% + UI 5%)
- Testing strategy includes:
  - Unit tests for domain logic (User, Session, Role validation)
  - Integration tests for token exchange, session management
  - BDD tests (Reqnroll) for all 10 scenarios in spec.md
  - Load tests for performance SLOs (<200ms token exchange, <20ms session validation)

**Principle 3: UX Traceability** ❌ (Not applicable - API-only service, no UI components)

**Principle 4: Event-Driven Data Discipline** ✅
- 6 domain events published via MassTransit + Azure Service Bus:
  - UserAuthenticated (user_id, tenant_id, entra_subject_id, timestamp)
  - UserLoggedOut (user_id, tenant_id, session_id, timestamp)
  - SessionRefreshed (session_id, user_id, tenant_id, new_expiration)
  - TenantSwitched (user_id, from_tenant_id, to_tenant_id, timestamp)
  - AuthenticationFailed (email, tenant_id, reason, ip_address, timestamp)
  - PasswordResetRequested (user_id, email, entra_subject_id, timestamp)
- Event schema documented in `Plan/CrossCuttingConcerns/architecture/domain-events-schema.md`

**Principle 5: Security & Compliance** ✅
- JWT RS256 token validation using Microsoft.Identity.Web
- Session security: HTTP-only, secure, SameSite=Strict cookies
- Session IDs are cryptographically random GUIDs (not sequential or predictable)
- Comprehensive audit logging to AuditRecords table (user_id, event_type, tenant_id, ip_address, timestamp, details_json)
- MFA configured in Entra ID tenant (not LMS responsibility)
- FERPA/COPPA compliance: No PII in logs, audit retention 7 years
- Rate limiting: Token exchange 10 req/min, session validation 60 req/min

**Principle 6: Mono-Repo Layer Isolation** ✅
- CrossCuttingConcerns layer correctly identified (Foundation Service)
- Shared infrastructure dependencies documented and validated:
  - Src/Foundation/shared/ServiceDefaults (Aspire, health checks)
  - Src/Foundation/shared/Domain (base entities, domain events)
  - Src/Foundation/shared/Application (CQRS, validation, results)
  - Src/Foundation/shared/Infrastructure (tenant isolation, audit interceptors)
- No dependencies on sibling services (Student Management, Assessment, etc.)

**Principle 7: Tool-Assisted Development** ✅
- Plan includes MCP tool usage instructions:
  - #microsoft.docs.mcp for official .NET/Azure documentation
  - #think or #sequential-thinking for complex problem-solving
  - #figma/dev-mode-mcp-server (not applicable - API only)

#### Technical Highlights

**Authentication Flow (Backend-for-Frontend)**:
1. User authenticates with Entra ID → receives JWT access token
2. Web app calls POST /api/auth/exchange-token with Entra Bearer token
3. API validates Entra token using Microsoft.Identity.Web (RS256 signature, issuer, audience)
4. API creates LMS session in PostgreSQL + caches in Redis (10-minute TTL)
5. API returns session ID (format: `lms_session_{guid}`)
6. Web stores session ID in HTTP-only, secure, SameSite=Strict cookie
7. Subsequent API calls include X-Lms-Session-Id header
8. SessionAuthenticationHandler validates session from Redis (cache hit <20ms) or PostgreSQL (cache miss <100ms)
9. Claims populated with user/tenant context for authorization

**Performance SLOs**:
- Token exchange: <200ms (P95) - includes Entra validation + DB write + Redis cache
- Session validation: <20ms (P95) - Redis cached (99% cache hit rate)
- Entra ID login redirect: <200ms (P95) - controlled by Microsoft
- Session refresh: <50ms (P95) - background service updates Redis + DB

**Security Measures**:
- Entra ID tokens use RS256 signing (validated by Microsoft.Identity.Web, no custom crypto)
- LMS session IDs are cryptographically random GUIDs (System.Security.Cryptography.RandomNumberGenerator)
- Session timeout: 8 hours sliding window for staff, 1 hour for admins (configurable per role)
- Sessions stored in HTTP-only, secure, SameSite=Strict cookies (not accessible to JavaScript)
- MFA configured in Entra ID tenant (Conditional Access Policies, not LMS responsibility)
- All authentication events logged to AuditRecords table (login, logout, failed attempts, tenant switches)
- Redis session cache includes TTL matching database expiration (no stale sessions)
- Account lockout after 5 failed attempts (Entra ID policy, not LMS)
- HTTPS enforced in production (certificate pinning for API Gateway)

**Migration Strategy**:
- User accounts matched by email to Entra ID accounts (case-insensitive)
- User profiles created in new NorthStar database with Entra ID subject references
- ExternalProviderLinks table establishes mapping (user_id ↔ entra_subject_id)
- Existing roles, permissions, and tenant associations preserved
- Legacy passwords marked as deprecated (deleted_at timestamp)
- Migration is idempotent (can be re-run safely)
- Rollback plan: Keep legacy IdentityServer running in parallel for 30 days

#### Open Questions (Resolved During Planning)

1. **Entra ID Tenant Strategy**: ✅ Single Entra ID tenant per school district (cost: ~$6/user/month for Premium P1)
2. **Role Synchronization**: ✅ Hybrid approach: Entra ID provides user identity + group membership, NorthStar manages fine-grained permissions
3. **SMTP Provider**: ✅ Azure Communication Services for email notifications (password resets, account lockouts)
4. **Session Storage**: ✅ PostgreSQL (source of truth) + Redis (cache) hybrid
5. **Token Refresh**: ✅ Background service (TokenRefreshHostedService) runs every 5 minutes

---

## Next Steps

### For Stakeholder Review:

1. **Review proposed branch**: https://github.com/crisweber2600/NorthStarET/tree/CrossCuttingConcerns/01-identity-service-entra-id-proposed
2. **Create Pull Request**: https://github.com/crisweber2600/NorthStarET/pull/new/CrossCuttingConcerns/01-identity-service-entra-id-proposed
3. **Provide feedback** via PR comments on:
   - Specification completeness (10 scenarios, edge cases, success metrics)
   - Technical approach (BFF pattern, session storage strategy)
   - Security requirements (session timeout, audit logging, rate limiting)
   - Migration strategy (user matching, rollback plan)
4. **Approve or request changes** to specifications

### For Implementation (after approval):

1. **Checkout specification branch**:
   ```bash
   git checkout CrossCuttingConcerns/01-identity-service-entra-id-spec
   ```

2. **Generate task breakdown** (if needed):
   ```bash
   /speckit.tasks
   ```

3. **Begin implementation** (creates implementation branch):
   ```bash
   /speckit.implement
   ```
   - Implementation branch (`CrossCuttingConcerns/01-identity-service-entra-id`) created automatically
   - Follow 8-phase plan in plan.md
   - Capture Red→Green evidence for each phase (4 transcript files: dotnet-test-red.txt, playwright-red.txt, dotnet-test-green.txt, playwright-green.txt)

4. **Phase-by-phase workflow**:
   ```bash
   # BEFORE implementation (Red state)
   dotnet test --configuration Debug --verbosity normal > phase1-red-dotnet-test.txt
   pwsh tests/ui/playwright.ps1 > phase1-red-playwright.txt

   # Implement Phase 1 tasks...

   # AFTER implementation (Green state)
   dotnet test --configuration Debug --verbosity normal > phase1-green-dotnet-test.txt
   pwsh tests/ui/playwright.ps1 > phase1-green-playwright.txt

   # Push phase review branch
   git add .
   git commit -m "feat(identity): complete Phase 1 - Entra ID configuration"
   git push origin HEAD:CrossCuttingConcerns/01-identity-service-entra-id-review-Phase1
   ```

5. **Quality gates** (before merging):
   - ≥80% code coverage (dotnet test --collect:"XPlat Code Coverage")
   - All tests passing (unit, integration, BDD, load)
   - Red→Green evidence attached to phase review
   - Security scan passed (no critical/high vulnerabilities)
   - Architecture review (no layer violations)

### To work on this feature:

**Checkout specification branch** (for planning/documentation updates):
```bash
git checkout CrossCuttingConcerns/01-identity-service-entra-id-spec
```

**Review proposed branch** (for stakeholder feedback):
```bash
git checkout CrossCuttingConcerns/01-identity-service-entra-id-proposed
```

**Implementation branch** (created later by /speckit.implement):
```bash
git checkout CrossCuttingConcerns/01-identity-service-entra-id
```

---

## Summary Statistics

- **Total Documentation Generated**: ~32,500 lines
  - Specification: 820 lines
  - Plan: ~13,000 lines
  - Research: ~8,000 lines
  - Data Model: ~6,500 lines
  - API Contracts: ~5,000 lines

- **Implementation Effort Estimate**:
  - 8 phases, ~40 tasks
  - Estimated: 120-160 developer hours (3-4 weeks for 1 developer)
  - Recommended: 2 developers (back-end + infrastructure) for parallel work

- **Testing Requirements**:
  - Unit tests: ~60 test cases (domain logic, token validation, session management)
  - Integration tests: ~30 test cases (API endpoints, database operations, Redis caching)
  - BDD tests: 10 feature files (1 per scenario in spec.md)
  - Load tests: 4 performance scenarios (token exchange, session validation, session refresh, concurrent users)

- **Dependencies**:
  - Microsoft.Identity.Web 2.15.5 (Entra ID integration)
  - Microsoft.EntityFrameworkCore 9.0.0 (PostgreSQL ORM)
  - StackExchange.Redis 2.7.0 (session caching)
  - MassTransit 8.1.0 (event publishing)
  - Aspire.Hosting 9.0.0 (orchestration)

---

**Report Generated**: 2025-11-20T15:30:00Z  
**Generated By**: GitHub Copilot (speckit.scenario mode)  
**Constitution Version**: v2.1.0
