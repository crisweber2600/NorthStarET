# Identity Service: Microsoft Entra ID Authentication & Authorization

**Service**: Identity & Authentication Service  
**Pattern**: OAuth 2.0/OIDC with Microsoft Entra ID  
**Architecture Reference**: [Identity Service Architecture](../architecture/services/identity-service.md)  
**Business Value**: Modernize authentication, enable SSO, reduce maintenance overhead

---

## Scenario 1: Staff Member Logs In Using Microsoft Entra ID SSO

**Given** NorthStar is configured to use Microsoft Entra ID exclusively for authentication
**And** a staff member has an active Entra ID account
**And** the staff member is assigned to a school district tenant
**When** the staff member navigates to the NorthStar login page
**And** clicks "Sign in with Microsoft"
**Then** they are redirected to Microsoft Entra ID login
**And** after successful authentication, they are redirected back to NorthStar
**And** their session is established with proper tenant context
**And** they see their district-specific dashboard
**And** their role-based permissions are loaded from Entra ID claims and NorthStar role mapping

---

## Scenario 2: Administrator Logs In Using Entra ID with Multi-Factor Authentication

**Given** Microsoft Entra ID is configured to require MFA for administrator accounts
**And** an administrator has an Entra ID account with MFA enabled
**When** the administrator attempts to log in
**Then** they are prompted for their username and password
**And** they are prompted for their second factor (authenticator app or SMS)
**And** after successful MFA verification, they receive a JWT token
**And** the token includes admin claims and tenant context
**And** they can access administrative features across all assigned districts

---

## Scenario 3: Microsoft Entra ID Configuration and User Provisioning

**Given** NorthStar previously used IdentityServer for authentication
**And** Microsoft Entra ID tenant is configured and NorthStar applications are registered (Web + API)
**And** users exist in the legacy IdentityServer database
**When** the migration process runs
**Then** user accounts are matched by email to Entra ID accounts
**And** user profiles are created in the new NorthStar database with Entra ID subject references
**And** ExternalProviderLinks are established for each migrated user
**And** existing roles, permissions, and tenant associations are preserved
**And** users log in using Entra ID credentials only
**And** legacy passwords are marked as deprecated

---

## Scenario 4: Token Refresh and Session Management (Entra ID Only)

**Given** a staff member is logged in with an active session via Entra ID
**And** their JWT access token is nearing expiration (within 5 minutes)
**When** the client application detects the approaching expiration
**Then** the application automatically requests a token refresh from Entra ID
**And** NorthStar validates the refresh token using Microsoft.Identity.Web
**And** a new access token is issued with updated claims
**And** the user's session continues without interruption
**And** the refresh is logged for security audit purposes

---

## Scenario 5: Cross-District Access with Tenant Switching (Entra ID)

**Given** a staff member is assigned to multiple school districts
**And** they are authenticated via Entra ID
**And** they are currently in District A context
**When** they select District B from the tenant switcher
**Then** a new token is issued with District B tenant context
**And** their dashboard refreshes with District B data
**And** their permissions are re-evaluated for District B
**And** they can only access data belonging to District B
**And** the tenant switch is logged for audit purposes

---

## Scenario 6: Password Reset Flow via Microsoft Entra ID

**Given** a user has forgotten their password
**When** they click "Forgot Password" on the login page
**Then** they are redirected to Microsoft's password reset flow (Entra ID)
**And** they receive a password reset email from Microsoft
**And** after completing the reset in Entra ID, they can log in with new credentials
**And** NorthStar logs the password reset event

---

## Scenario 7: Role-Based Authorization Check (Entra ID)

**Given** a teacher is logged in with authenticated session via Entra ID
**And** they attempt to access the "Student Enrollment" feature
**When** the application checks their permissions via NorthStar role mapping
**Then** NorthStar validates their JWT token using Microsoft.Identity.Web
**And** retrieves their role assignments from the database
**And** checks if "Student Enrollment" permission is granted
**And** returns authorization decision within 50ms (P95 SLO)
**And** the decision is cached for subsequent requests in the same session

---

## Scenario 8: Session Termination and Logout (Entra ID)

**Given** a staff member has an active authenticated session via Entra ID
**When** they click the "Logout" button
**Then** their access token is marked as revoked in NorthStar
**And** their refresh token is invalidated
**And** they are redirected to the Entra ID logout endpoint
**And** their session cookie is cleared
**And** they are redirected to the public login page
**And** any subsequent requests with the old token are rejected

---

## Scenario 9: Service-to-Service Authentication (Entra ID)

**Given** the Student Management Service needs to call the Assessment Service
**When** Student Management Service makes an API request
**Then** it includes its service principal token from Entra ID
**And** the API Gateway validates the token using Microsoft.Identity.Web
**And** NorthStar confirms the service identity
**And** the service-to-service call is authorized
**And** the request includes tenant context for data isolation

---

## Scenario 10: Failed Authentication Handling (Entra ID)

**Given** a user attempts to log in with invalid credentials
**When** Entra ID rejects the authentication
**Then** the user is shown an appropriate error message
**And** the failed attempt is logged in NorthStar
**And** after 5 failed attempts, the account is temporarily locked (Entra ID policy)
**And** the user receives notification about the locked account
**And** an administrator can unlock the account via Entra ID admin portal

---

## Related Architecture

**Service Architecture**: [Identity Service Technical Specification](../architecture/services/identity-service.md)  
**Authentication Pattern**: [Multi-Tenancy Pattern](../patterns/multi-tenancy.md)  
**Caching Strategy**: [Caching & Performance](../patterns/caching-performance.md)  
**Testing Approach**: [Testing Strategy](../standards/TESTING_STRATEGY.md)

---

## Technical Implementation Notes (Microsoft Entra ID Only)

**Entra ID Configuration Steps**:
1. Configure Microsoft Entra ID tenant and register NorthStar applications (Web + API with delegated permissions)
2. Install Microsoft.Identity.Web NuGet packages in NorthStar services
3. Implement OAuth 2.0/OIDC token validation using Microsoft.Identity.Web (no custom token issuance)
4. Create custom SessionAuthenticationHandler for API authorization
5. Implement TokenExchangeService for BFF pattern (Entra tokens → LMS session IDs)
6. Create ExternalProviderLinks table to map legacy users to Entra ID subjects
7. Migrate user accounts with email matching strategy (see identity migration documentation)
8. Configure Redis Stack for session caching with sliding expiration windows
9. Implement session management with PostgreSQL persistence + Redis cache
10. Setup Row-Level Security policies for tenant isolation in PostgreSQL
11. Configure MFA policies in Entra ID admin portal (not in LMS)
12. Implement session refresh mechanism via background service
13. Setup audit logging for security events to AuditRecords table

**Database Schema**:
- `identity.Users` (id, tenant_id, email, created_at, updated_at, deleted_at)
- `identity.Roles` (id, tenant_id, role_name, permissions, description)
- `identity.UserRoles` (user_id, role_id, tenant_id, assigned_at)
- `identity.Sessions` (id, user_id, entra_subject_id, tenant_id, expires_at, created_at, refreshed_at)
- `identity.ExternalProviderLinks` (user_id, provider, external_user_id, email, last_sync, tenant_id)
- `identity.AuditRecords` (user_id, event_type, tenant_id, ip_address, timestamp, details)

**Authentication Flow (Backend-for-Frontend, Entra ID Only)**:
1. User authenticates with Entra ID → receives JWT access token
2. Web app calls POST /api/auth/exchange-token with Entra Bearer token
3. API validates Entra token using Microsoft.Identity.Web
4. API creates LMS session in PostgreSQL + caches in Redis
5. API returns session ID (format: `lms_session_{guid}`)
6. Web stores session ID in HTTP-only cookie
7. Subsequent API calls include X-Lms-Session-Id header
8. SessionAuthenticationHandler validates session from cache/DB
9. Claims populated with user/tenant context for authorization

**Performance SLOs**:
- Token exchange: <200ms (P95)
- Session validation: <20ms (P95) - Redis cached
- Entra ID login redirect: <200ms (P95)
- Session refresh: <50ms (P95)

**Security Requirements**:
- Entra ID tokens use RS256 signing (validated by Microsoft.Identity.Web)
- LMS session IDs are cryptographically random GUIDs
- Session timeout: 8 hours sliding window for staff, 1 hour for admins
- Sessions stored in HTTP-only, secure, SameSite=Strict cookies
- MFA configured in Entra ID tenant (not LMS responsibility)
- All authentication events logged to AuditRecords table
- Redis session cache includes TTL matching database expiration
