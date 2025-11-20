# Feature Specification: Unified SSO & Authorization via Entra (LMS Identity Module)

**Feature Branch**: `001-unified-sso-auth`  
**Created**: 2025-10-20  
**Status**: Draft  
**Input**: User description: "Unified SSO & Authorization via Entra - Users should sign in once through Microsoft Entra (ID/B2C) and gain seamless access across all LMS and admin portals without re-authentication."

## Clarifications

### Session 2025-10-21

- Q: How should we resolve references to the standalone Identity API that is not available? → A: Integrate the identity endpoints and authorization responsibilities into the existing LMS API.
- Q: Where should the LMS Identity data live now that it is part of the LMS API? → A: Store it in the LMS database using a dedicated identity schema for separation.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Single Sign-On Access Across All Portals (Priority: P1)

Any user (admin, teacher, parent, or student) navigates to any LMS or admin UI and is automatically redirected to Microsoft Entra for authentication. After successful sign-in, the user receives a session that grants access to all authorized portals and services without requiring additional login prompts. The user sees their name, role, and active tenant context displayed consistently across all interfaces.

**Why this priority**: This is the foundation of the entire feature. Without single sign-on working, users cannot access the system at all. This delivers immediate value by eliminating the friction of multiple logins and establishes the security foundation for all other features.

**Independent Test**: Can be fully tested by having a user sign in through Entra and then navigating between at least two different portals (e.g., LMS and Admin UI) without being prompted to log in again. Success is measured by zero additional authentication prompts and consistent user context display.

**Acceptance Scenarios**:

1. **Given** a user has not signed in, **When** they navigate to any LMS or admin portal URL, **Then** they are redirected to Microsoft Entra sign-in page
2. **Given** a user successfully authenticates with Entra, **When** the authentication completes, **Then** they are redirected back to their original destination with an active session
3. **Given** a user has an active session in one portal, **When** they navigate to a different portal within the LMS ecosystem, **Then** they gain immediate access without re-authentication
4. **Given** a user is authenticated, **When** they view any portal interface, **Then** their name, role, and active tenant (district/school) are consistently displayed

---

### User Story 2 - Fast Authorization Decisions for Protected Actions (Priority: P2)

When a user attempts to perform a protected action (such as viewing student grades, managing school settings, or editing curriculum), the system validates their permissions through the LMS API's Identity module in real-time. Authorization decisions complete within milliseconds, providing immediate feedback about whether the action is allowed or denied. Users with insufficient permissions receive clear, actionable feedback about why access was denied.

**Why this priority**: After basic authentication (P1), users need to actually perform their work. This story ensures that permission checks are fast enough to feel instant and that users understand their access boundaries. Without this, the system would be authenticated but unusable.

**Independent Test**: Can be fully tested by having users with different roles (e.g., teacher vs. district admin) attempt the same protected action (e.g., "Edit School Settings") and verifying that authorization decisions return in under 50ms with appropriate allow/deny responses and clear UI feedback.

**Acceptance Scenarios**:

1. **Given** a user attempts a protected action, **When** the authorization check occurs, **Then** the decision completes in under 50 milliseconds for 95% of requests
2. **Given** a user has the required permissions, **When** they attempt a protected action, **Then** the action proceeds immediately without visible delay
3. **Given** a user lacks the required permissions, **When** they attempt a protected action, **Then** they receive a clear message explaining what permission is missing and what role/membership would grant it
4. **Given** multiple users perform actions simultaneously, **When** authorization checks occur, **Then** decisions remain fast (<50ms P95) even under concurrent load
5. **Given** an authorization decision is made, **When** the outcome is logged, **Then** the audit record includes user, tenant, resource, action, and role context

---

### User Story 3 - Seamless Tenant Context Switching (Priority: P3)

A district administrator who oversees multiple schools can switch their active tenant context (from district-wide view to a specific school) instantly without re-authentication. The system updates the user interface to reflect the new tenant scope, adjusts available actions based on the new context, and caches tenant facts to ensure switching feels instantaneous.

**Why this priority**: This addresses the workflow efficiency for multi-tenant users (district admins, support staff) who need to move between organizational scopes throughout their day. While not required for basic system access (P1) or single-context operations (P2), this significantly improves productivity for administrative users.

**Independent Test**: Can be fully tested by logging in as a district administrator with access to multiple schools, switching between school contexts via the UI, and verifying that each switch completes in under 200ms with the UI updating to show the new tenant scope and adjusted permissions.

**Acceptance Scenarios**:

1. **Given** a user has membership in multiple tenants, **When** they view the tenant selector, **Then** all their authorized tenants are listed with clear names and types (district vs. school)
2. **Given** a user selects a different tenant, **When** the switch occurs, **Then** the context changes in under 200 milliseconds
3. **Given** a user switches tenant context, **When** the UI updates, **Then** all visible elements reflect the new tenant (name, scope, available actions)
4. **Given** a user switches to a new tenant, **When** they attempt an action, **Then** authorization decisions reflect the new tenant's permissions
5. **Given** tenant facts are cached, **When** a user switches context, **Then** the system uses cached data and doesn't require a round-trip to the LMS Identity module for basic tenant information

---

### User Story 4 - Graceful Session Expiration and Renewal (Priority: P2)

When a user's session token expires (due to time limits or inactivity), the system detects the expiration and presents a single, unified "Session expired" prompt. The user is redirected to Entra for re-authentication, and upon successful re-authentication, they are returned to their original location with their work context preserved. The system never shows cascading 401 errors from multiple services.

**Why this priority**: Session expiration is inevitable in any authentication system. How the system handles it directly impacts user trust and experience. This story ensures that expiration is handled gracefully rather than appearing as a system failure, which is critical for user satisfaction but doesn't block initial feature delivery.

**Independent Test**: Can be fully tested by forcing a token expiration (either waiting for natural timeout or manipulating token TTL in a test environment), triggering an action that requires authentication, and verifying that the user sees a single clear prompt and is successfully redirected back after re-authentication.

**Acceptance Scenarios**:

1. **Given** a user's session token expires, **When** they attempt any action, **Then** they see a single "Session expired" message (not multiple error dialogs)
2. **Given** a user sees the session expired prompt, **When** they choose to re-authenticate, **Then** they are redirected to Entra sign-in
3. **Given** a user re-authenticates after expiration, **When** authentication succeeds, **Then** they are returned to their original page/context
4. **Given** a token is nearing expiration, **When** the user is actively using the system, **Then** the token is refreshed transparently without interrupting their work
5. **Given** multiple backend services detect an expired token, **When** errors occur, **Then** the frontend consolidates them into a single user-facing session expiration state

---

### Edge Cases

- What happens when Entra is temporarily unavailable during initial sign-in? (User sees a clear error message explaining the authentication service is unavailable and to try again shortly)
- What happens when Entra is unavailable but a user has a valid cached token? (System continues to operate using the cached token until it expires)
- What happens when a user's role or tenant membership changes while they have an active session? (System receives an event from the LMS Identity module, invalidates relevant caches, and the user sees updated permissions on their next action or after a background refresh)
- What happens when the LMS Identity module is slow or unavailable during an authorization check? (Cached authorization decisions are used if available; otherwise, the system fails closed with a clear error message and retries with exponential backoff)
- What happens when a user belongs to many tenants (e.g., 50+ schools)? (Tenant selector is paginated or searchable; switching to any tenant still completes in under 200ms using indexed lookups)
- What happens when a user attempts to switch to a tenant they no longer have access to? (System denies the switch and shows a message that their access to that tenant has been revoked, refreshing their available tenant list)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST authenticate all user personas (admin, teacher, parent, student) exclusively through Microsoft Entra ID/B2C
- **FR-002**: System MUST redirect unauthenticated users to Entra sign-in and return them to their original destination after successful authentication
- **FR-003**: System MUST validate Entra-issued JWTs locally at the BFF/Gateway layer without requiring a round-trip to Entra for each request
- **FR-004**: System MUST exchange Entra JWTs for short-lived LMS access tokens that are recognized by all backend services
- **FR-005**: System MUST call the LMS API Identity module endpoints for all authorization decisions before allowing protected actions
- **FR-006**: System MUST complete 95% of authorization decisions in under 50 milliseconds
- **FR-007**: System MUST complete the full login flow (redirect to Entra, authenticate, redirect back, display dashboard) in under 800 milliseconds at the 95th percentile
- **FR-008**: System MUST allow users with active sessions to navigate between micro-frontends and services without re-authentication
- **FR-009**: System MUST display user context (name, role, active tenant) consistently across all portal interfaces
- **FR-010**: System MUST provide tenant switching functionality for users with membership in multiple tenants
- **FR-011**: System MUST complete tenant context switches in under 200 milliseconds
- **FR-012**: System MUST cache authorization decisions and tenant facts with a 10-minute cache window to support idempotent responses
- **FR-013**: System MUST detect expired session tokens and present a unified "Session expired" prompt to users
- **FR-014**: System MUST refresh tokens transparently when users are actively using the system and tokens are nearing expiration
- **FR-015**: System MUST log all authorization decisions (allow and deny) with user, tenant, resource, action, and role context for audit purposes
- **FR-016**: System MUST continue operating with cached tokens during transient Entra outages
- **FR-017**: System MUST consume domain events from the LMS Identity module regarding role and tenant membership changes to update caches
- **FR-018**: System MUST provide clear, actionable error messages when authorization is denied, explaining what permission is required

### Key Entities

- **User**: Represents a person who authenticates with the system; identified by Entra subject ID; has memberships in one or more tenants; associated with roles that grant permissions
- **Tenant**: Represents an organizational scope (district or school); owned by the LMS Identity module; contains users through memberships; defines the boundary for authorization decisions
- **Membership**: Represents a user's association with a tenant; includes role assignments; determines what actions a user can perform within a tenant context
- **Role**: Represents a named set of permissions; owned by the LMS Identity module; assigned to users through memberships; evaluated during authorization decisions
- **Session**: Represents an authenticated user's active access period; contains Entra JWT and LMS access token; has expiration times; subject to refresh and expiration logic
- **Authorization Decision**: Represents the outcome of a permission check; includes user, tenant, resource, action, and allow/deny result; cached for performance; logged for audit

All identity-related tables are hosted in the LMS database within a dedicated `identity` schema to maintain clear separation from other LMS data domains.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can sign in once and access all authorized portals without additional authentication prompts
- **SC-002**: Authorization decisions complete in under 50 milliseconds for 95% of requests
- **SC-003**: The complete login flow (from initial redirect to Entra through displaying the user's dashboard) completes in under 800 milliseconds at the 95th percentile
- **SC-004**: Tenant context switching completes in under 200 milliseconds for users with multiple tenant memberships
- **SC-005**: Users see a single unified "Session expired" prompt when their session expires, not multiple cascading error messages
- **SC-006**: The system continues to operate using cached tokens during transient Entra outages lasting up to the cache window duration (10 minutes)
- **SC-007**: User context (name, role, active tenant) displays consistently across all portal interfaces within 100 milliseconds of navigation
- **SC-008**: 100% of authorization decisions are logged with complete audit context (user, tenant, resource, action, role, outcome)
- **SC-009**: Users who are denied access to a protected action receive a clear explanation of what permission is required
- **SC-010**: Token refresh operations complete transparently without interrupting active user sessions

## Assumptions *(optional)*

- Microsoft Entra ID/B2C is already configured and operational with user accounts provisioned
- The LMS API exposes an Identity module that is deployed and accessible from the BFF/Gateway and backend services
- SCIM synchronization between Entra and the LMS Identity module is handled upstream and not part of this feature scope
- Network latency between services is within normal data center ranges (< 5ms)
- Users have modern browsers that support standard OAuth2/OIDC flows
- The BFF/Gateway has network access to both Entra and the LMS Identity module endpoints hosted within the LMS API
- Domain events for role/membership changes are emitted by the LMS Identity module in a reliable, at-least-once delivery pattern
- All backend services accept the LMS access token format issued by the BFF/Gateway

## Out of Scope *(optional)*

- Defining or modifying Entra ID/B2C policies, user provisioning, or SCIM sync logic
- Implementing per-service caching strategies (each service determines its own cache implementation)
- Changing business logic in downstream LMS modules beyond adding authorization checks
- Providing fallback identity providers beyond Entra (Entra is the sole authentication authority)
- Implementing a separate Identity API service (the LMS API will host the required identity endpoints)
- Defining the detailed event schema for role/membership changes (consumed as opaque events)
- User self-service password reset or account recovery (handled by Entra)
- Multi-factor authentication configuration (handled by Entra policies)
- Fine-grained permission modeling within the LMS Identity module (uses existing roles and policies)

## Dependencies *(optional)*

- **Microsoft Entra ID/B2C**: Must be available and configured with tenant, user, and application registrations
- **LMS Identity module**: Must provide `/authorize` endpoints for real-time permission checks and emit domain events for role/membership changes
- **BFF/Gateway**: Must be capable of validating Entra JWTs, issuing LMS access tokens, and routing requests to backend services
- **Caching Infrastructure**: Services need access to a caching layer (in-memory or distributed) to support 10-minute cache windows for authorization decisions and tenant facts
- **Audit/Logging Infrastructure**: Must accept structured logs with user, tenant, resource, action, and role context for authorization audit trail

