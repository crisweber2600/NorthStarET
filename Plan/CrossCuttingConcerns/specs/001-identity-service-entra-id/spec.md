# Feature Specification: Identity Service with Microsoft Entra ID Authentication & Authorization

**Feature ID**: `01-identity-service-entra-id`  
**Target Layer**: CrossCuttingConcerns  
**Service**: Identity & Authentication Service  
**Pattern**: OAuth 2.0/OIDC with Microsoft Entra ID  
**Architecture Reference**: [Identity Service Architecture](../../architecture/services/identity-service.md)  
**Business Value**: Modernize authentication, enable enterprise SSO, reduce maintenance overhead, enhance security posture  
**Created**: 2025-11-20  
**Status**: Draft

---

## Goal / Why

Enable secure, modern authentication for all NorthStar LMS users through Microsoft Entra ID integration. This replaces the legacy IdentityServer implementation with a cloud-native identity provider, providing:

- **Enterprise SSO**: Single sign-on across all district applications
- **Enhanced Security**: Multi-factor authentication, conditional access policies, and enterprise-grade security managed by Microsoft
- **Reduced Maintenance**: Eliminate custom password management, MFA implementation, and security patching overhead
- **Compliance**: Built-in audit logging, compliance certifications (SOC 2, ISO 27001), and data residency controls
- **Scalability**: Cloud-scale identity infrastructure supporting thousands of concurrent authentications

This establishes the security foundation for all NorthStar services using modern OAuth 2.0/OIDC standards with Microsoft's enterprise identity platform.

---

## Intended Experience / What

### Primary User Flow

A staff member opens their browser and navigates to the NorthStar LMS. They click "Sign in with Microsoft" and are seamlessly redirected to the familiar Microsoft Entra ID login page. After entering their district email and password (and completing MFA if required), they are returned to NorthStar with an established session. Their district-specific dashboard loads automatically, showing personalized data based on their role and assigned schools. The entire process feels native and secure - no usernames to remember, no passwords to reset in NorthStar.

### Session Management

The system maintains an 8-hour sliding session window for staff (1 hour for administrators). As users actively work, their session refreshes automatically in the background. When a token approaches expiration, the system requests a new access token from Entra ID transparently. Users never see interruptions unless they've been inactive beyond the timeout period. When they log out, both NorthStar and Entra ID sessions terminate cleanly.
Session state is persisted first to the `identity.sessions` table in PostgreSQL (source of truth) and then cached in Redis with matching TTL for P95 < 20ms validation.

### Multi-Tenant Context

Administrators and staff with access to multiple districts see a tenant selector in the navigation. Switching districts refreshes their session context, dashboard data, and permissions without requiring re-authentication. Each switch is instant (<200ms), leveraging Redis-cached tenant information. A single Microsoft Entra ID tenant backs all districts; tenant context is encoded via custom claims (`district_id`, `school_ids[]`, `northstar_role`) issued by Entra app roles and pulled into the session.

---

## Service Boundary Outcomes

The Identity Service owns:
- **Authentication orchestration** with Microsoft Entra ID
- **Session lifecycle management** (create, refresh, validate, terminate)
- **Token exchange and validation** (Entra JWT → NorthStar session)
- **User profile synchronization** from Entra ID to local database
- **Tenant context management** and multi-tenant authorization
- **Audit logging** for all authentication and authorization events

The service **does not**:
- Store passwords (delegated to Entra ID)
- Implement MFA logic (handled by Entra ID policies)
- Manage application-specific permissions (handled by each service via claims/roles)

**Role Model**: Base access is determined by Entra ID App Roles; fine-grained permissions and feature toggles live in NorthStar's database and are merged into the session claims during exchange.

**Event Contracts**:
- Publishes: `UserAuthenticated`, `UserLoggedOut`, `SessionRefreshed`, `TenantContextSwitched`
- Consumes: `UserRoleAssigned`, `UserRoleRevoked` (from Admin Service)

**Performance SLOs**:
- Token exchange (Entra → NorthStar session): P95 < 200ms
- Session validation (cached): P95 < 20ms
- Session validation (database fallback): P95 < 100ms
- Session refresh: P95 < 50ms
- Entra ID login redirect: P95 < 200ms

**Availability**: 99.9% uptime (dependent on Entra ID availability at 99.99%)

---

## User Scenarios & Testing *(mandatory)*

### User Story 1: Staff Member Logs In Using Microsoft Entra ID SSO (Priority: P1)

**Why this priority**: This is the foundational authentication flow. Without this working, users cannot access the system at all. This delivers immediate security value by leveraging enterprise identity infrastructure and establishes the baseline for all subsequent features.

**Independent Test**: A test user with valid Entra ID credentials can complete the full login flow from the NorthStar login page, through Entra ID authentication, and back to their dashboard with an established session. Success is measured by receiving a valid session cookie and seeing their role-appropriate dashboard within 2 seconds.

**Acceptance Scenarios**:

1. **Given** NorthStar is configured to use Microsoft Entra ID exclusively for authentication  
   **And** a staff member has an active Entra ID account  
   **And** the staff member is assigned to a school district tenant  
   **When** the staff member navigates to the NorthStar login page  
   **And** clicks "Sign in with Microsoft"  
   **Then** they are redirected to Microsoft Entra ID login  
   **And** after successful authentication, they are redirected back to NorthStar  
   **And** their session is established with proper tenant context  
   **And** they see their district-specific dashboard  
   **And** their role-based permissions are loaded from Entra ID claims and NorthStar role mapping

2. **Given** a staff member has an active Entra ID session in their browser  
   **When** they navigate directly to a protected NorthStar page  
   **Then** they are authenticated automatically via Entra ID session  
   **And** redirected to their requested page without manual login

3. **Given** a staff member completes authentication  
   **When** the session is established  
   **Then** a `UserAuthenticated` event is published with userId, tenantId, timestamp, and IP address  
   **And** the event is logged to the AuditRecords table

---

### User Story 2: Administrator Logs In Using Entra ID with Multi-Factor Authentication (Priority: P1)

**Why this priority**: Administrators have elevated privileges and require additional security. MFA is a critical security control for privileged accounts. This demonstrates that NorthStar correctly integrates with Entra ID's security policies without implementing custom MFA logic.

**Independent Test**: An administrator account configured with MFA in Entra ID attempts to log in. The test verifies that the MFA prompt appears from Entra ID (not NorthStar), that successful MFA completion grants access, and that admin claims are correctly propagated to NorthStar.

**Acceptance Scenarios**:

1. **Given** Microsoft Entra ID is configured to require MFA for administrator accounts  
   **And** an administrator has an Entra ID account with MFA enabled  
   **When** the administrator attempts to log in  
   **Then** they are prompted for their username and password  
   **And** they are prompted for their second factor (authenticator app or SMS)  
   **And** after successful MFA verification, they receive a JWT token from Entra ID  
   **And** the token includes admin claims and tenant context  
   **And** they can access administrative features across all assigned districts

2. **Given** an administrator session is established  
   **When** the session timeout is checked  
   **Then** administrator sessions expire after 1 hour of inactivity  
   **And** staff sessions expire after 8 hours of inactivity

3. **Given** an administrator attempts to access a protected admin endpoint  
   **When** the authorization check occurs  
   **Then** the system validates both the admin role claim from Entra ID and NorthStar's role mapping  
   **And** access is granted only if both checks pass

---

### User Story 3: Microsoft Entra ID Configuration and User Provisioning (Priority: P2)

**Why this priority**: Migration from IdentityServer to Entra ID is a one-time operation that enables the authentication system but doesn't block initial feature delivery. This can be executed during a planned maintenance window after the authentication flow is fully tested.

**Independent Test**: Run the user migration script against a test database with sample IdentityServer users. Verify that users are matched by email, ExternalProviderLinks are created, roles are preserved, and migrated users can log in with Entra ID credentials. Success is measured by 100% of test users successfully authenticating via Entra ID post-migration.

**Acceptance Scenarios**:

1. **Given** NorthStar previously used IdentityServer for authentication  
   **And** Microsoft Entra ID tenant is configured and NorthStar applications are registered (Web + API)  
   **And** users exist in the legacy IdentityServer database  
   **When** the migration process runs  
   **Then** user accounts are matched by email to Entra ID accounts  
   **And** user profiles are created in the new NorthStar database with Entra ID subject references  
   **And** ExternalProviderLinks are established for each migrated user  
   **And** existing roles, permissions, and tenant associations are preserved  
   **And** users log in using Entra ID credentials only  
   **And** legacy passwords are marked as deprecated

2. **Given** a user does not have a matching Entra ID account  
   **When** the migration process runs  
   **Then** the user is flagged for manual review  
   **And** an administrator can create the Entra ID account or manually link an existing one

3. **Given** the migration completes successfully  
   **When** a migrated user attempts to log in with old IdentityServer credentials  
   **Then** they are redirected to Entra ID authentication  
   **And** shown a message that authentication has been modernized

---

### User Story 4: Token Refresh and Session Management (Priority: P1)

**Why this priority**: Token expiration is inevitable in any OAuth 2.0 system. Graceful token refresh is essential to prevent users from being interrupted during active work. Without this, users would be logged out every 15 minutes (typical JWT lifetime), creating a poor experience.

**Independent Test**: Establish a user session, manipulate the token expiration time to trigger a refresh, and verify that the session continues seamlessly without user intervention. Success is measured by zero visible interruptions and correct logging of the refresh event.

**Acceptance Scenarios**:

1. **Given** a staff member is logged in with an active session via Entra ID  
   **And** their JWT access token is nearing expiration (within 5 minutes)  
   **When** the client application detects the approaching expiration  
   **Then** the application automatically requests a token refresh from Entra ID  
   **And** NorthStar validates the refresh token using Microsoft.Identity.Web  
   **And** a new access token is issued with updated claims  
   **And** the user's session continues without interruption  
   **And** the refresh is logged for security audit purposes

2. **Given** a user's refresh token has expired  
   **When** the system attempts to refresh the access token  
   **Then** the refresh fails  
   **And** the user sees a "Session expired" message  
   **And** they are redirected to Entra ID for re-authentication

3. **Given** a user is actively working (making requests every 2-3 minutes)  
   **When** their session window is sliding (8 hours for staff)  
   **Then** the session expiration is extended with each activity  
   **And** Redis cache TTL is updated to match the new expiration

---

### User Story 5: Cross-District Access with Tenant Switching (Priority: P2)

**Why this priority**: Multi-tenant support is critical for district administrators and support staff who need to work across multiple districts. This isn't needed for single-tenant users (most teachers) but is essential for administrative efficiency. It can be delivered after basic authentication (P1) but before production rollout.

**Independent Test**: Log in as a user with access to multiple districts (e.g., District A and District B). Switch from District A to District B using the tenant selector. Verify that the dashboard updates, permissions are re-evaluated, and data isolation is maintained. Success is measured by completing the switch in under 200ms and confirming that only District B data is visible.

**Acceptance Scenarios**:

1. **Given** a staff member is assigned to multiple school districts  
   **And** they are authenticated via Entra ID  
   **And** they are currently in District A context  
   **When** they select District B from the tenant switcher  
   **Then** a new token is issued with District B tenant context  
   **And** their dashboard refreshes with District B data  
   **And** their permissions are re-evaluated for District B  
   **And** they can only access data belonging to District B  
   **And** the tenant switch is logged for audit purposes

2. **Given** a user switches tenant context  
   **When** the switch completes  
   **Then** a `TenantContextSwitched` event is published  
   **And** the Redis session cache is updated with the new tenant context

3. **Given** a user attempts to switch to a tenant they don't have access to  
   **When** the switch is attempted  
   **Then** the request is denied  
   **And** an appropriate error message is shown  
   **And** the failed attempt is logged

---

### User Story 6: Password Reset Flow via Microsoft Entra ID (Priority: P3)

**Why this priority**: Password management is delegated to Entra ID, reducing NorthStar's security surface area. This is a convenience feature that improves user experience but isn't critical for initial system access. Users can always contact IT support for password resets.

**Independent Test**: Click "Forgot Password" on the NorthStar login page and verify redirection to Microsoft's password reset flow. Complete the reset and confirm that the user can log in with new credentials. Success is measured by successful authentication after password change.

**Acceptance Scenarios**:

1. **Given** a user has forgotten their password  
   **When** they click "Forgot Password" on the login page  
   **Then** they are redirected to Microsoft's password reset flow (Entra ID)  
   **And** they receive a password reset email from Microsoft  
   **And** after completing the reset in Entra ID, they can log in with new credentials  
   **And** NorthStar logs the password reset event

2. **Given** a user completes a password reset  
   **When** they attempt to log in  
   **Then** their existing sessions are invalidated  
   **And** they must complete a fresh authentication

---

### User Story 7: Role-Based Authorization Check (Priority: P1)

**Why this priority**: Authentication without authorization is useless. Users need to perform their actual work, which requires permission checks on every protected action. Fast authorization is critical for system usability - slow checks make the system feel unresponsive.

**Independent Test**: Log in as a teacher and attempt to access the "Student Enrollment" feature. Verify that the authorization check completes in under 50ms and returns the correct decision (allowed/denied). Test with multiple roles to confirm correct permission mapping.

**Acceptance Scenarios**:

1. **Given** a teacher is logged in with authenticated session via Entra ID  
   **And** they attempt to access the "Student Enrollment" feature  
   **When** the application checks their permissions via NorthStar role mapping  
   **Then** NorthStar validates their JWT token using Microsoft.Identity.Web  
   **And** retrieves their role assignments from the database  
   **And** checks if "Student Enrollment" permission is granted  
   **And** returns authorization decision within 50ms (P95 SLO)  
   **And** the decision is cached for subsequent requests in the same session

2. **Given** a user's role assignments change while they have an active session  
   **When** a `UserRoleAssigned` or `UserRoleRevoked` event is received  
   **Then** the user's cached permissions are invalidated  
   **And** the next authorization check fetches fresh permissions from the database

3. **Given** a user lacks the required permission  
   **When** they attempt a protected action  
   **Then** they receive a clear error message indicating missing permission  
   **And** the denial is logged to AuditRecords

---

### User Story 8: Session Termination and Logout (Priority: P1)

**Why this priority**: Secure logout is a fundamental security requirement, especially on shared computers in schools. Users must be able to confidently end their session and ensure no one else can access their account.

**Independent Test**: Log in as a user, perform some actions, then click Logout. Verify that the session cookie is cleared, the Redis cache entry is removed, the database session is marked as terminated, and subsequent requests with the old session ID are rejected. Confirm redirection to Entra ID logout endpoint.

**Acceptance Scenarios**:

1. **Given** a staff member has an active authenticated session via Entra ID  
   **When** they click the "Logout" button  
   **Then** their access token is marked as revoked in NorthStar  
   **And** their refresh token is invalidated  
   **And** they are redirected to the Entra ID logout endpoint  
   **And** their session cookie is cleared  
   **And** they are redirected to the public login page  
   **And** any subsequent requests with the old token are rejected

2. **Given** a user logs out  
   **When** the logout completes  
   **Then** a `UserLoggedOut` event is published  
   **And** the session is removed from Redis cache  
   **And** the database session record is updated with logout timestamp

3. **Given** a user's session has been terminated  
   **When** they attempt to use a cached session cookie  
   **Then** the request is rejected with 401 Unauthorized  
   **And** they are redirected to the login page

---

### User Story 9: Service-to-Service Authentication (Priority: P2)

**Why this priority**: Microservices need to securely communicate with each other. Service principals enable zero-trust architecture where every service authenticates itself. This is essential for production security but can be implemented after user authentication flows are stable.

**Independent Test**: Configure the Student Management Service with a service principal. Have it make an API call to the Assessment Service. Verify that the service principal token is validated, the service identity is confirmed, and tenant context is correctly propagated. Success is measured by the API call completing with proper authorization.

**Acceptance Scenarios**:

1. **Given** the Student Management Service needs to call the Assessment Service  
   **When** Student Management Service makes an API request  
   **Then** it includes its service principal token from Entra ID  
   **And** the API Gateway validates the token using Microsoft.Identity.Web  
   **And** NorthStar confirms the service identity  
   **And** the service-to-service call is authorized  
   **And** the request includes tenant context for data isolation

2. **Given** a service-to-service call includes a tenant context  
   **When** the called service processes the request  
   **Then** it enforces tenant isolation on all data access  
   **And** logs the service call with both service identities and tenant context

3. **Given** a service principal token is invalid or expired  
   **When** a service-to-service call is attempted  
   **Then** the call is rejected with 401 Unauthorized  
   **And** the calling service receives a clear error message to refresh its credentials

---

### User Story 10: Failed Authentication Handling (Priority: P1)

**Why this priority**: Security depends on proper handling of authentication failures. Account lockout prevents brute force attacks, and clear error messages help legitimate users recover from mistakes. This must be in place from day one to maintain security posture.

**Independent Test**: Attempt to log in with invalid credentials repeatedly. Verify that after 5 failed attempts, the account is locked (enforced by Entra ID policy). Confirm that failed attempts are logged with timestamps and IP addresses. Test that error messages don't reveal whether the email or password was wrong.

**Acceptance Scenarios**:

1. **Given** a user attempts to log in with invalid credentials  
   **When** Entra ID rejects the authentication  
   **Then** the user is shown an appropriate error message  
   **And** the failed attempt is logged in NorthStar  
   **And** after 5 failed attempts, the account is temporarily locked (Entra ID policy)  
   **And** the user receives notification about the locked account  
   **And** an administrator can unlock the account via Entra ID admin portal

2. **Given** a failed authentication attempt occurs  
   **When** the failure is logged  
   **Then** the AuditRecords table captures: user email, timestamp, IP address, failure reason  
   **And** the log entry does not include password information

3. **Given** a user account is locked due to failed attempts  
   **When** the lockout period expires (30 minutes default)  
   **Then** the user can attempt to log in again  
   **And** successful login resets the failed attempt counter

---

### Edge Cases

1. **Entra ID Outage During Initial Sign-In**  
   **Scenario**: Microsoft Entra ID service is unavailable when a user tries to log in.  
   **Handling**: Display a clear error message: "Authentication service temporarily unavailable. Please try again in a few minutes." Log the incident for monitoring. No fallback to local authentication (security policy).

2. **Entra ID Outage with Valid Cached Session**  
   **Scenario**: Entra ID is down but user has an active, unexpired session in NorthStar.  
   **Handling**: Allow continued access using the cached session until it expires. Background token refresh attempts fail gracefully with exponential backoff. User sees a banner: "Authentication provider connectivity issue - session may expire earlier than usual."

3. **User Role Changes During Active Session**  
   **Scenario**: An administrator revokes a user's "Teacher" role and assigns "ReadOnly" while the user is actively logged in.  
   **Handling**: `UserRoleRevoked` event triggers cache invalidation. User's next authorization check fetches updated roles from database. Ongoing requests complete with old permissions; new requests use updated permissions. User may need to refresh their browser to see UI changes reflecting the new role.

4. **Authorization Service Slow or Unavailable**  
   **Scenario**: Database or Redis latency exceeds SLO during authorization check.  
   **Handling**: Use circuit breaker pattern. First failure tries database. If database fails, deny access (fail closed) with error message: "Unable to verify permissions - please try again." After 3 consecutive failures, open circuit and deny all requests for 30 seconds. Alert monitoring system.

5. **User Belongs to 50+ Districts**  
   **Scenario**: A state-level administrator has access to every district in the state.  
   **Handling**: Tenant selector implements paginated dropdown with search/filter. Load first 20 tenants on page load. Fetch additional tenants on scroll or search. Redis caches tenant list per user. Switching to any tenant completes in <200ms using indexed database lookups.

6. **User Attempts to Switch to Revoked Tenant**  
   **Scenario**: User had access to District B yesterday, but it was revoked. They attempt to switch to District B from the tenant selector.  
   **Handling**: Authorization check fails. Display message: "Your access to [District B] has been revoked. Please contact your administrator if you believe this is an error." Remove District B from the tenant selector UI. Log the failed attempt.

7. **Token Refresh Fails During Active Session**  
   **Scenario**: Background token refresh request to Entra ID fails (network issue, token revoked, etc.).  
   **Handling**: Retry up to 3 times with exponential backoff (1s, 2s, 4s). If all retries fail, log the user out gracefully with message: "Your session has expired. Please log in again." Publish `UserLoggedOut` event with reason code.

8. **Concurrent Login Attempts from Different Devices**  
   **Scenario**: User logs in on their desktop, then immediately logs in on their tablet.  
   **Handling**: Allow concurrent sessions (standard behavior). Each device gets its own session ID. Sessions are tracked independently in Redis and database. If organization policy requires single-session enforcement, implement session displacement: new login invalidates previous session.

9. **Migration User Email Mismatch**  
   **Scenario**: Legacy IdentityServer database has user with email `john.doe@district.edu`, but Entra ID account is `j.doe@district.edu`.  
   **Handling**: Migration script flags the mismatch for manual review. Generate a report of unmatched users. Administrator can: (a) update Entra ID email to match, (b) update NorthStar email to match, or (c) manually create ExternalProviderLink with correct mapping.

10. **Session Hijacking Attempt**  
    **Scenario**: Attacker steals a session cookie and attempts to use it from a different IP address.  
    **Handling**: SessionAuthenticationHandler validates session exists in Redis/DB. Optionally implement IP address binding: if session IP doesn't match request IP, log security event and require re-authentication. Make this configurable per tenant (some districts use NAT, causing legitimate IP changes).

---

## Functional Requirements *(mandatory)*

### Must Have (P1)

1. **OAuth 2.0/OIDC Integration**
   - Implement Authorization Code Flow with PKCE for web applications
   - Register NorthStar Web and API applications in Entra ID tenant
   - Configure redirect URIs, scopes, and client secrets
   - Use Microsoft.Identity.Web library for token validation

2. **Token Exchange & Session Creation**
   - Validate Entra ID JWT tokens using RS256 signature verification
   - Extract claims: subject (sub), email, name, roles, tenant assignments
   - Create LMS session in PostgreSQL `identity.Sessions` table
   - Generate cryptographically random session ID (format: `lms_session_{guid}`)
   - Cache session in Redis with TTL matching database expiration

3. **Session Management**
   - Store sessions in HTTP-only, secure, SameSite=Strict cookies
   - Implement 8-hour sliding window for staff, 1-hour for admins
   - Background token refresh when access token within 5 minutes of expiration
   - Session validation from Redis cache (P95 < 20ms) with database fallback (P95 < 100ms)

4. **Authentication Events**
   - Publish `UserAuthenticated` event on successful login
   - Publish `UserLoggedOut` event on logout
   - Publish `SessionRefreshed` event on token refresh
   - Log all authentication events to `identity.AuditRecords` table

5. **Authorization Integration**
   - Validate JWT tokens using Microsoft.Identity.Web
   - Retrieve user role assignments from `identity.UserRoles` table
   - Cache permission decisions in Redis for session duration
   - Invalidate cached permissions on `UserRoleAssigned`/`UserRoleRevoked` events
   - Return authorization decisions within 50ms (P95 SLO)

6. **Logout Flow**
   - Mark session as terminated in database
   - Remove session from Redis cache
   - Clear session cookie
   - Redirect to Entra ID logout endpoint
   - Publish `UserLoggedOut` event

7. **Failed Authentication Handling**
   - Log failed attempts to `identity.AuditRecords` (email, timestamp, IP, reason)
   - Rely on Entra ID account lockout policies (typically 5 attempts → 30-minute lock)
   - Display generic error messages (don't reveal whether email or password was wrong)

### Should Have (P2)

8. **Multi-Tenant Context Switching**
   - Allow users with multiple district assignments to switch tenant context
   - Update session with new `tenant_id`
   - Re-evaluate permissions for new tenant
   - Refresh dashboard data for selected tenant
   - Complete switch within 200ms (P95)
   - Publish `TenantContextSwitched` event

9. **User Migration from IdentityServer**
   - Match legacy users by email to Entra ID accounts
   - Create `identity.ExternalProviderLinks` for each user
   - Preserve roles, permissions, and tenant associations
   - Mark legacy passwords as deprecated
   - Generate migration report for unmatched users

10. **Service Principal Authentication**
    - Register service principals in Entra ID for each microservice
    - Validate service principal tokens in API Gateway
    - Enforce tenant context in service-to-service calls
    - Log service-to-service authentication events

### Could Have (P3)

11. **Password Reset Delegation**
    - Redirect "Forgot Password" link to Entra ID password reset flow
    - Log password reset events in NorthStar
    - Invalidate existing sessions on password change

12. **Advanced Session Security**
    - IP address binding (configurable per tenant)
    - Device fingerprinting
    - Concurrent session limits
    - Anomaly detection (unusual login times, locations)

---

## Non-Functional Requirements

### Performance
- Token exchange (Entra → NorthStar session): P95 < 200ms, P99 < 500ms
- Session validation (Redis cached): P95 < 20ms, P99 < 50ms
- Session validation (database fallback): P95 < 100ms, P99 < 250ms
- Authorization decision: P95 < 50ms, P99 < 100ms
- Tenant context switch: P95 < 200ms, P99 < 400ms

### Security
- All JWT tokens validated using RS256 signature verification
- Session IDs generated using cryptographically secure random number generator
- HTTP-only, secure, SameSite=Strict cookies
- TLS 1.3 for all external communication
- Secrets stored in Azure Key Vault (client secrets, signing keys)
- Audit logging for all authentication/authorization events
- Row-level security enforced on all database queries (tenant_id filter)

### Scalability
- Support 10,000 concurrent authenticated users
- Handle 1,000 logins per minute during peak times (school start)
- Redis session cache horizontally scalable
- Database connection pooling with min 10, max 100 connections
- Multi-region: Azure Cache for Redis Enterprise active-active replication keeps session cache consistent; PostgreSQL logical replication/geo-redundant read replicas back session persistence with gateway enforcing region affinity.

### Reliability
- 99.9% uptime (dependent on Entra ID at 99.99%)
- Graceful degradation: continue with cached sessions if Entra ID unavailable
- Circuit breaker pattern for authorization checks
- Retry with exponential backoff for token refresh (3 attempts: 1s, 2s, 4s)

### Observability
- Distributed tracing for all authentication flows (OpenTelemetry)
- Metrics: login_success_count, login_failure_count, session_refresh_count, authorization_decision_latency
- Structured logging with correlation IDs
- Alerts for: auth failure rate >5%, session validation P95 >50ms, token refresh failure rate >1%

---

## Database Schema

### identity.Users
```sql
CREATE TABLE identity.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants.districts(id),
    email VARCHAR(255) NOT NULL,
    display_name VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMP NULL,
    UNIQUE(tenant_id, email)
);

CREATE INDEX idx_users_tenant_email ON identity.users(tenant_id, email) WHERE deleted_at IS NULL;
```

### identity.Roles
```sql
CREATE TABLE identity.roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants.districts(id),
    role_name VARCHAR(100) NOT NULL,
    permissions JSONB NOT NULL DEFAULT '[]',
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(tenant_id, role_name)
);
```

### identity.UserRoles
```sql
CREATE TABLE identity.user_roles (
    user_id UUID NOT NULL REFERENCES identity.users(id),
    role_id UUID NOT NULL REFERENCES identity.roles(id),
    tenant_id UUID NOT NULL REFERENCES tenants.districts(id),
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_by UUID REFERENCES identity.users(id),
    PRIMARY KEY(user_id, role_id, tenant_id)
);

CREATE INDEX idx_user_roles_user_tenant ON identity.user_roles(user_id, tenant_id);
```

### identity.Sessions
```sql
CREATE TABLE identity.sessions (
    id VARCHAR(255) PRIMARY KEY, -- Format: lms_session_{guid}
    user_id UUID NOT NULL REFERENCES identity.users(id),
    entra_subject_id VARCHAR(255) NOT NULL,
    tenant_id UUID NOT NULL REFERENCES tenants.districts(id),
    access_token_hash VARCHAR(64) NOT NULL, -- SHA256 hash for validation
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    refreshed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    ip_address INET,
    user_agent TEXT
);

CREATE INDEX idx_sessions_user_tenant ON identity.sessions(user_id, tenant_id) WHERE expires_at > NOW();
CREATE INDEX idx_sessions_expires_at ON identity.sessions(expires_at);
```

### identity.ExternalProviderLinks
```sql
CREATE TABLE identity.external_provider_links (
    user_id UUID NOT NULL REFERENCES identity.users(id),
    provider VARCHAR(50) NOT NULL, -- 'EntraID'
    external_user_id VARCHAR(255) NOT NULL, -- Entra subject ID
    email VARCHAR(255) NOT NULL,
    last_sync TIMESTAMP NOT NULL DEFAULT NOW(),
    tenant_id UUID NOT NULL REFERENCES tenants.districts(id),
    PRIMARY KEY(user_id, provider)
);

CREATE INDEX idx_external_links_provider_external_id ON identity.external_provider_links(provider, external_user_id);
```

### identity.AuditRecords
```sql
CREATE TABLE identity.audit_records (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID REFERENCES identity.users(id),
    event_type VARCHAR(100) NOT NULL, -- 'UserAuthenticated', 'UserLoggedOut', 'SessionRefreshed', etc.
    tenant_id UUID REFERENCES tenants.districts(id),
    ip_address INET,
    timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    details JSONB
);

CREATE INDEX idx_audit_records_user_timestamp ON identity.audit_records(user_id, timestamp);
CREATE INDEX idx_audit_records_tenant_timestamp ON identity.audit_records(tenant_id, timestamp);
CREATE INDEX idx_audit_records_event_type ON identity.audit_records(event_type, timestamp);
```

---

## Authentication Flow (Backend-for-Frontend Pattern)

### Login Flow
1. **User clicks "Sign in with Microsoft"** → Web app redirects to Entra ID authorize endpoint
2. **User authenticates with Entra ID** → Entra ID returns authorization code to callback URL
3. **Web app exchanges code for tokens** → Receives access_token, refresh_token, id_token
4. **Web app calls POST /api/auth/exchange-token** with Entra Bearer token in Authorization header
5. **API validates Entra token** using Microsoft.Identity.Web (signature, issuer, audience, expiration)
6. **API creates LMS session**:
   - Extract claims from Entra token (sub, email, name, roles)
   - Upsert user in `identity.users` table (match by email)
   - Create session in `identity.sessions` table with 8-hour expiration
   - Cache session in Redis with matching TTL
   - Require custom Entra claims: `district_id` (GUID), `school_ids` (array), `northstar_role` (string) for authorization downstream
   - Generate session ID: `lms_session_{guid}`
7. **API returns session ID** in response body (JSON: `{ sessionId: "lms_session_..." }`)
8. **Web stores session ID** in HTTP-only, secure, SameSite=Strict cookie
9. **Web redirects** to user's dashboard

### Authenticated Request Flow
1. **Web makes API request** → Includes session cookie (auto-sent by browser)
2. **API Gateway extracts session ID** from cookie
3. **SessionAuthenticationHandler validates session**:
   - Check Redis cache for session (P95 < 20ms)
   - If not in cache, query `identity.sessions` table (P95 < 100ms)
   - Verify session not expired (`expires_at > NOW()`)
   - If invalid/expired, return 401 Unauthorized
4. **Populate ClaimsPrincipal** with user_id, tenant_id, roles from session
5. **Controller action executes** with authenticated user context
6. **Authorization checks** use ClaimsPrincipal for permission validation

### Token Refresh Flow
1. **Background job detects token expiration** (runs every 4 minutes)
2. **For each session expiring within 5 minutes**:
   - Call Entra ID token refresh endpoint with refresh_token
   - Receive new access_token
   - Update `identity.sessions.refreshed_at` and extend `expires_at` by 8 hours
   - Update Redis cache with new expiration
   - Publish `SessionRefreshed` event

### Logout Flow
1. **User clicks Logout** → Web calls POST /api/auth/logout
2. **API terminates session**:
   - Update `identity.sessions` set `expires_at = NOW()`
   - Delete session from Redis cache
   - Publish `UserLoggedOut` event
3. **API returns 200 OK** with Entra ID logout URL
4. **Web clears session cookie** (set Max-Age=0)
5. **Web redirects to Entra ID logout** endpoint to terminate Entra session
6. **Entra ID redirects back** to NorthStar public login page

---

## Configuration

### Entra ID App Registration (Web App)
```json
{
  "displayName": "NorthStar LMS Web",
  "signInAudience": "AzureADMyOrg",
  "web": {
    "redirectUris": [
      "https://lms.northstaret.com/signin-oidc",
      "https://localhost:7002/signin-oidc"
    ],
    "logoutUrl": "https://lms.northstaret.com/signout-oidc"
  },
  "requiredResourceAccess": [
    {
      "resourceAppId": "00000003-0000-0000-c000-000000000000",
      "resourceAccess": [
        {
          "id": "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
          "type": "Scope"
        }
      ]
    }
  ]
}
```

### Entra ID App Registration (API)
```json
{
  "displayName": "NorthStar LMS API",
  "signInAudience": "AzureADMyOrg",
  "identifierUris": ["api://northstar-lms"],
  "api": {
    "oauth2PermissionScopes": [
      {
        "adminConsentDescription": "Allow the application to access NorthStar LMS API on behalf of the signed-in user.",
        "adminConsentDisplayName": "Access NorthStar LMS API",
        "id": "00000000-0000-0000-0000-000000000001",
        "isEnabled": true,
        "type": "User",
        "userConsentDescription": "Allow the application to access NorthStar LMS API on your behalf.",
        "userConsentDisplayName": "Access NorthStar LMS API",
        "value": "access_as_user"
      }
    ]
  }
}
```

### appsettings.json
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<web-app-client-id>",
    "ClientSecret": "<stored-in-key-vault>",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-oidc"
  },
  "AzureAdApi": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<api-client-id>",
    "Audience": "api://northstar-lms"
  },
  "SessionManagement": {
    "StaffSessionDurationHours": 8,
    "AdminSessionDurationHours": 1,
    "TokenRefreshThresholdMinutes": 5,
    "EnableIpAddressBinding": false
  },
  "Redis": {
    "ConnectionString": "<aspire-provided>",
    "SessionCachePrefix": "lms_session:",
    "PermissionCachePrefix": "lms_permissions:"
  }
}
```

---

## Clarifications

### Session 2025-11-21

- Q: Entra ID tenant topology - Should each district have its own tenant or should we centralize authentication in a single Entra tenant? → A: Use one Microsoft Entra tenant with district context encoded in custom claims to simplify management and enable cross-district admin accounts.
- Q: Role synchronization strategy - Should roles live entirely in Entra ID, entirely in NorthStar, or a hybrid? → A: Hybrid — coarse roles managed as Entra App Roles, fine-grained permissions persisted in NorthStar DB and merged into the session.
- Q: Session storage source of truth - Should Redis hold authoritative sessions or act as cache in front of PostgreSQL? → A: PostgreSQL `identity.sessions` is canonical, Redis provides low-latency cache with identical TTL.
- Q: Multi-region session replication - How do we keep sessions consistent across regions? → A: Azure Cache for Redis Enterprise active-active combined with PostgreSQL logical replication/read replicas, plus region-affinity routing at the gateway.
- Q: Required custom Entra claims - Which claims must Entra issue for downstream authorization? → A: `district_id`, `school_ids[]`, and `northstar_role` are mandatory custom claims attached during app role assignment.

---

## Open Questions for /plan

1. **Offline Access**: Should we support any offline scenarios or require constant connectivity? (Recommendation: Require connectivity for initial auth, allow cached session for temporary network blips)

2. **SMTP Provider**: Which email service for password reset notifications? (Options: SendGrid, Azure Communication Services, Office 365 SMTP)

3. **Audit Retention**: How long to retain audit records? (Recommendation: 90 days hot storage, 7 years cold storage for compliance)

---

## Success Metrics

- **Adoption**: 100% of users authenticate via Entra ID within 30 days of production deployment
- **Performance**: 95% of login flows complete in under 2 seconds
- **Security**: Zero credential-related security incidents (password breaches, unauthorized access)
- **Uptime**: 99.9% authentication service availability
- **User Satisfaction**: <2% of support tickets related to authentication issues
- **Migration Success**: 100% of legacy users successfully migrated with preserved roles/permissions

---

## Handoff to /plan

The specification is complete and ready for planning. The /plan agent should focus on:

1. **Technical Architecture**: Detailed class diagrams, service boundaries, API contracts
2. **Implementation Phases**: Break down into vertical slices (Login → Session Management → Multi-tenant → Service Auth)
3. **Testing Strategy**: Unit, integration, E2E test plans; Reqnroll feature files for each scenario
4. **Infrastructure Setup**: Entra ID app registration scripts, Azure Key Vault configuration
5. **Migration Strategy**: Step-by-step user migration plan with rollback procedures
6. **Monitoring & Alerting**: Specific metrics, dashboards, alert thresholds

**Dependencies**:
- Azure subscription with Entra ID tenant
- PostgreSQL database (provided by Aspire)
- Redis cache (provided by Aspire)
- Domain names and SSL certificates for callback URLs
- SMTP service for notifications (if not using Entra ID directly)

**Risks**:
- Entra ID service outages (mitigation: cached session fallback)
- User migration edge cases (mitigation: phased rollout with manual review process)
- Performance under peak load (mitigation: load testing, Redis cluster scaling)
- Scope creep into custom MFA implementation (mitigation: strict reliance on Entra ID policies)
