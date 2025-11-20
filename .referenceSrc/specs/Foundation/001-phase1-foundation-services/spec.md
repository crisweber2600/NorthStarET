# Feature Specification: Phase 1 Foundation Services Implementation

**Feature Branch**: `001-phase1-foundation-services`  
**Created**: 2025-11-19  
**Status**: Draft  
**Input**: User description: "Implement Phase 1 of the NorthStar Master Migration Plan, which establishes the foundational infrastructure for the microservices architecture including Identity & Authentication Service, API Gateway (YARP), and Configuration Service"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Staff Authentication with Microsoft Entra ID (Priority: P1)

As a district administrator or staff member, I need to authenticate using my organization's Microsoft Entra ID account so that I can securely access the NorthStar LMS without managing separate credentials.

**Why this priority**: Authentication is the absolute foundation - no other services can function until users can securely authenticate. This is the critical path blocker for all downstream services.

**Independent Test**: Can be fully tested by attempting login with organizational credentials through Microsoft Entra ID SSO flow and receiving a valid authentication token that grants access to the system. Delivers immediate value by enabling secure single sign-on for staff.

**Acceptance Scenarios**:

1. **Given** a staff member with valid Microsoft Entra ID credentials, **When** they access the login page and click "Sign in with Microsoft", **Then** they are redirected to Microsoft authentication, authenticate successfully, and return to the application with an active authenticated session
2. **Given** an authenticated staff member, **When** they access a protected resource with their session token, **Then** the system validates their token and grants access based on their assigned roles
3. **Given** a staff member with expired session, **When** they attempt to access a protected resource, **Then** the system prompts for re-authentication without data loss
4. **Given** a staff member using local authentication fallback, **When** Microsoft Entra ID is unavailable, **Then** they can authenticate using local username/password credentials stored in the Identity Service

---

### User Story 2 - API Gateway Routing to Services (Priority: P2)

As a system architect, I need all client requests to route through a centralized API Gateway so that authentication, rate limiting, and service routing are handled consistently across all microservices.

**Why this priority**: The API Gateway is essential infrastructure that enables the Strangler Fig pattern - allowing legacy and new services to coexist during migration. Without it, we cannot incrementally migrate functionality.

**Independent Test**: Can be tested by sending authenticated requests to gateway endpoints that route to both new microservices and legacy system endpoints. Delivers value by establishing the routing infrastructure for all future services.

**Acceptance Scenarios**:

1. **Given** an authenticated user request to `/api/identity/users`, **When** the API Gateway receives the request, **Then** it validates the authentication token, applies rate limiting, and routes to the Identity Service
2. **Given** a request to a legacy endpoint `/api/legacy/students`, **When** the API Gateway receives the request, **Then** it routes to the OldNorthStar system while maintaining authentication context
3. **Given** multiple concurrent requests from a single client, **When** rate limits are exceeded, **Then** the API Gateway returns appropriate throttling responses without forwarding to backend services
4. **Given** a backend service failure, **When** the circuit breaker threshold is reached, **Then** the API Gateway returns cached responses or graceful error messages without cascading failures

---

### User Story 3 - Configuration Retrieval for Multi-Tenant Context (Priority: P3)

As a service developer, I need to retrieve district-specific and school-specific configuration settings so that my service can operate correctly within the multi-tenant architecture with appropriate organizational context.

**Why this priority**: Configuration Service provides essential organizational context (districts, schools, calendars) that all domain services depend on. While critical, it can be implemented after authentication and routing are working.

**Independent Test**: Can be tested by making authenticated requests to configuration endpoints and receiving district settings, school information, and academic calendar data scoped to the authenticated user's tenant. Delivers value by enabling tenant-aware operations.

**Acceptance Scenarios**:

1. **Given** an authenticated district administrator, **When** they request district settings, **Then** the system returns configuration data scoped to their district only
2. **Given** a school administrator, **When** they request school information, **Then** the system returns only schools within their authorized district
3. **Given** a service requesting academic calendar data, **When** specifying a school year and district, **Then** the system returns calendar definitions with instructional days, holidays, and grading periods
4. **Given** an update to district settings, **When** configuration changes are saved, **Then** events are published to notify dependent services and caches are invalidated

---

### User Story 4 - District Admin Invitation and Promotion (Priority: P3)

As a System Administrator, I need to invite and promote users to District Admin roles for specific districts so that I can delegate district management responsibilities while maintaining strict tenant isolation and secure verification workflows.

**Why this priority**: Delegation is essential for scalable multi-district operations. Without invitation-based role assignment, System Admins become bottlenecks for all district-level administrative tasks. This enables self-service district management.

**Independent Test**: Can be tested by System Admin creating a district, inviting a user via email to be District Admin, verifying invitation workflow (email sent, 7-day expiration, acceptance link), and confirming promoted user has district-scoped access but cannot access other districts.

**Acceptance Scenarios**:

1. **Given** a System Admin on District Management, **When** they invite a user email to District Admin role for a specific district, **Then** system creates Membership with status Invited, sends invitation email with secure token, sets 7-day expiration, and logs invitation event
2. **Given** an invited user receives the email, **When** they click the verification link within 7 days, **Then** system verifies token, updates Membership status to Active, creates Session with tenant context set to invited district, and logs verification event
3. **Given** a District Admin with verified membership, **When** they sign in and access the system, **Then** they land on District Home scoped to their assigned district with appropriate management capabilities but no access to other districts or platform-wide admin features
4. **Given** an invitation expires after 7 days, **When** the user attempts to verify, **Then** system rejects the token and provides option to request resend from System Admin
5. **Given** System Admin resends an invitation within 10-minute idempotency window, **When** multiple resend requests occur, **Then** system collapses duplicate requests into single invitation with latest timestamp winning

---

### User Story 5 - Multi-Tenant Context Switching (Priority: P3)

As a District Admin with access to multiple schools, I need to switch my active tenant context between district-wide view and specific school contexts so that I can efficiently manage different organizational scopes without re-authenticating.

**Why this priority**: Multi-tenant users (District Admins overseeing multiple schools, support staff with cross-district access) need efficient context switching to match their workflow patterns. Without this, they must logout/login repeatedly or work in multiple browser sessions.

**Independent Test**: Can be tested by logging in as District Admin with membership in multiple schools, using tenant selector to switch between district and school contexts, and verifying UI updates, authorization scope changes, and performance (< 200ms switch time).

**Acceptance Scenarios**:

1. **Given** a user authenticated with memberships in multiple tenants (1 district + 3 schools), **When** they view the tenant selector, **Then** all authorized tenants display with clear labels (district name, school names) and current active tenant is highlighted
2. **Given** a user selects a different tenant from selector, **When** the switch executes, **Then** Session.ActiveTenantId updates, authorization cache clears for previous tenant, UI updates to reflect new tenant scope, and operation completes in under 200 milliseconds
3. **Given** a user switches from district context to school context, **When** they attempt an action, **Then** authorization decisions reflect school-scoped permissions (more restricted than district-wide)
4. **Given** a user's membership is revoked from a tenant while they have active session, **When** they attempt to switch to that tenant, **Then** system denies switch, shows "Access revoked" message, refreshes available tenant list, and logs security event

---

### User Story 6 - Service Health Monitoring (Priority: P4)

As a DevOps engineer, I need to monitor the health status of all foundation services so that I can detect and respond to service degradation or failures before users are impacted.

**Why this priority**: While important for operations, health monitoring is not blocking for basic functionality. It provides operational visibility but can be added after core services are operational.

**Independent Test**: Can be tested by accessing health check endpoints for each service and receiving status information including dependencies. Delivers value by enabling proactive monitoring and alerting.

**Acceptance Scenarios**:

1. **Given** all foundation services are running, **When** health check endpoints are queried, **Then** each service reports healthy status with dependency checks
2. **Given** a database connection failure, **When** health checks are performed, **Then** the affected service reports degraded or unhealthy status with specific failure details
3. **Given** the Aspire Dashboard is running, **When** viewing service status, **Then** all three foundation services appear with real-time health status and telemetry data
4. **Given** a service is under high load, **When** health checks run, **Then** performance metrics are reported showing response times and resource utilization

---

### Edge Cases

- **Token Expiration During Long Sessions**: What happens when a user's JWT token expires while they have an active session with unsaved work? System must gracefully handle token refresh or prompt re-authentication with session state preservation.

- **Microsoft Entra ID Service Outage**: How does the system handle authentication when Microsoft Entra ID is temporarily unavailable? System must fall back to local authentication with clear messaging about reduced functionality.

- **Multi-Tenant Data Isolation Breach Attempt**: What happens when a malicious user attempts to access another district's configuration data by manipulating request parameters? System must enforce tenant boundaries at multiple layers (application, database RLS) and log security violations.

- **Circuit Breaker Cascading Failures**: How does the API Gateway prevent cascading failures when multiple backend services become unhealthy simultaneously? System must implement independent circuit breakers per service with graceful degradation.

- **Database Connection Pool Exhaustion**: What happens when tenant-scoped database connections exceed pool limits during peak load? System must queue requests appropriately and return meaningful timeout errors rather than failing silently.

- **Configuration Cache Inconsistency**: How does the system handle situations where cached configuration data becomes stale due to failed cache invalidation? System must implement TTL-based expiration as a fallback and provide manual cache refresh capabilities.

- **Entra Token Exchange Failure**: What happens when Entra token validation succeeds but LMS custom token generation fails during token exchange? System must return appropriate error without invalidating Entra session, log failure for monitoring, and allow retry with exponential backoff.

- **Invitation Token Reuse Attack**: What happens when a malicious actor intercepts an invitation token and attempts to use it multiple times? System must implement single-use token pattern, invalidate token immediately upon first verification, and log suspicious multiple verification attempts.

- **Tenant Context Switch with Pending Operations**: What happens when a user switches tenant context while having uncommitted changes in the previous tenant? System must prompt user to save/discard changes before switching, preserve operation state per tenant context, and prevent data loss or cross-tenant contamination.

- **Authorization Cache Inconsistency During Role Change**: What happens when a user's role is changed but cached authorization decisions reflect old permissions? System must immediately invalidate all cache entries for that user-tenant combination upon role change event, force re-evaluation of in-flight requests, and log cache invalidation activity.

## Requirements *(mandatory)*

### Functional Requirements

#### Identity & Authentication Service

- **FR-001**: System MUST integrate with Microsoft Entra ID supporting BOTH B2B (organizational accounts for staff and administrators) and B2C (external identities for future student/parent access) scenarios using OAuth 2.0/OpenID Connect protocols via Microsoft.Identity.Web library
- **FR-002**: System MUST support local username/password authentication as a fallback when Microsoft Entra ID is unavailable
- **FR-003**: System MUST implement token exchange pattern (BFF) that validates incoming Entra JWT tokens locally and exchanges them for short-lived LMS custom JWT tokens with tenant context, role-based claims, and cached permissions for authorization across all microservices
- **FR-004**: System MUST implement token refresh mechanisms to maintain long-running sessions without requiring frequent re-authentication
- **FR-005**: System MUST provide user registration, password reset, and profile management capabilities for local accounts
- **FR-006**: System MUST enforce tenant-scoped role-based access control (RBAC) with support for System Admin (platform-wide), District Admin (district-scoped), School Admin (school-scoped), and Staff roles, using Membership entity to track user-tenant-role associations
- **FR-007**: System MUST publish domain events for user authentication activities (login, logout, password changes) to enable audit logging
- **FR-008**: System MUST persist user accounts, roles, claims, refresh tokens, sessions (with active tenant context), and memberships (user-tenant-role associations) in a dedicated Identity database with multi-tenant isolation
- **FR-008a**: System MUST implement Session entity with active tenant context tracking to support multi-tenant users who can switch between district and school scopes
- **FR-008b**: System MUST implement Membership entity to associate users with specific tenants (districts/schools) and their roles within those tenants
- **FR-008c**: System MUST implement invitation-based role assignment workflow with assignment status tracking (Active, Invited, Revoked) and 7-day expiration for District Admin and School Admin promotions
- **FR-008d**: System MUST support tenant context switching for multi-tenant users (e.g., District Admin viewing multiple schools) with session persistence and cache invalidation completing in under 200 milliseconds
- **FR-008e**: System MUST implement authorization decision caching in Redis with tenant-scoped cache keys (pattern: authz:{userId}:{tenantId}:{resource}:{action}) achieving sub-50ms authorization checks at 95th percentile with 10-minute TTL and cache invalidation on role/membership changes
- **FR-008f**: System MUST implement idempotent invitation workflow within 10-minute window to prevent duplicate role assignments from rapid resubmissions

#### API Gateway (YARP)

- **FR-009**: System MUST route all incoming client requests through a centralized API Gateway for consistent request handling
- **FR-010**: System MUST validate JWT authentication tokens on all protected routes before forwarding to backend services
- **FR-011**: System MUST implement rate limiting per client to prevent abuse and ensure fair resource allocation
- **FR-012**: System MUST implement circuit breaker patterns to prevent cascading failures when backend services are unhealthy
- **FR-013**: System MUST support routing to both new microservices and legacy OldNorthStar endpoints during the migration period (Strangler Fig pattern)
- **FR-014**: System MUST provide request/response logging and distributed tracing for debugging and monitoring
- **FR-015**: System MUST implement request timeout policies to prevent resource exhaustion from long-running requests
- **FR-016**: System MUST support dynamic routing configuration without requiring service restarts

#### Configuration Service

- **FR-017**: System MUST manage district-level settings including name, code, state, timezone, and database connection strings
- **FR-018**: System MUST manage school-level settings including name, code, address, principal assignment, and phone contact within districts
- **FR-019**: System MUST define and manage academic calendars with school years, instructional days, holidays, and early dismissal dates
- **FR-020**: System MUST define grade levels and sequences customizable per district
- **FR-021**: System MUST enforce strict multi-tenant isolation ensuring districts can only access their own configuration data
- **FR-022**: System MUST implement caching for frequently accessed configuration data with cache invalidation on updates
- **FR-023**: System MUST publish domain events when configuration changes occur (district created, school created, calendar updated, settings changed)
- **FR-024**: System MUST support custom attribute definitions for extensibility per district and entity type
- **FR-025**: System MUST persist all configuration data with audit trails tracking who made changes and when

#### Cross-Cutting Requirements

- **FR-026**: All services MUST follow Clean Architecture patterns with clear separation between Domain, Application, Infrastructure, and API layers
- **FR-027**: All services MUST be orchestrated through .NET Aspire 13.0.0 for consistent service discovery and configuration management
- **FR-028**: All services MUST implement comprehensive health checks reporting status of service and dependencies
- **FR-029**: All services MUST use PostgreSQL databases with multi-tenant isolation using tenant_id and Row-Level Security policies
- **FR-030**: All services MUST publish domain events to Azure Service Bus for event-driven integration with eventual consistency
- **FR-031**: All services MUST store secrets exclusively in Azure Key Vault with no hardcoded credentials in source code or configuration files
- **FR-032**: All services MUST achieve ≥80% test coverage with unit tests, integration tests, and BDD acceptance tests

### Key Entities

#### Identity Service

- **User**: Represents an authenticated user account including email, password hash (for local accounts), Entra subject ID (for B2B/B2C), email confirmation status, and account lockout status
- **Role**: Represents an authorization role (System Admin, District Admin, School Admin, Staff, Student) with associated permissions
- **Claim**: Represents a specific permission or attribute assigned to a user for fine-grained authorization
- **RefreshToken**: Represents a long-lived token for obtaining new access tokens without re-authentication
- **ExternalProviderLink**: Represents the linkage between a local user account and an external identity provider (Microsoft Entra ID B2B/B2C) with external user identifier and last synchronization timestamp
- **Session**: Represents an active user session including session ID, user ID, active tenant ID (for multi-tenant users), Entra token hash, LMS token metadata, expiration timestamp, and last activity timestamp for tracking tenant context and enabling secure token exchange
- **Membership**: Represents user-tenant-role association including user ID, tenant ID (district or school), role ID, assignment status (Active, Invited, Revoked), invite sent timestamp, verification timestamp, and expiration timestamp for managing tenant-scoped role assignments and delegated administration
- **Invitation**: Represents an invitation for role assignment including invitation token, target email, target tenant, target role, status (Pending, Accepted, Expired, Revoked), sent timestamp, accepted timestamp, expiration timestamp (7 days), and idempotency window (10 minutes) for secure delegation workflow

#### Configuration Service

- **District**: Represents a school district including name, unique code, state, timezone, active status, and creation date
- **School**: Represents a school within a district including name, unique code, principal assignment, physical address, contact phone, and active status
- **Calendar**: Represents an academic calendar for a school year including start date, end date, total instructional days, and association to district or specific school
- **CalendarDay**: Represents a specific day within a calendar including date, day type (instructional, holiday, early dismissal, professional development), and optional description
- **GradeLevel**: Represents a grade level definition (PK, K, 1-12) within a district including name, sequence order, and active status
- **CustomAttribute**: Represents extensible custom attributes for districts to define additional fields on entities
- **SystemSetting**: Represents configurable settings at district, school, or user level including setting key, value, category, and last update metadata

#### API Gateway

- **RouteConfiguration**: Represents routing rules including source path patterns, destination service addresses, authentication requirements, and rate limiting policies
- **CircuitBreakerState**: Represents the state of circuit breakers for backend services including failure threshold, timeout duration, and current status

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Staff and administrators can successfully authenticate using Microsoft Entra ID SSO with authentication completing in under 3 seconds from clicking "Sign in with Microsoft" to landing on the authenticated dashboard

- **SC-002**: Local authentication fallback works when Microsoft Entra ID is unavailable, with users able to login using username and password within 2 seconds

- **SC-003**: API Gateway successfully routes 100% of requests to appropriate backend services (new or legacy) with routing decisions completing in under 10 milliseconds

- **SC-004**: Authentication token validation at the API Gateway completes in under 50 milliseconds at the 95th percentile

- **SC-005**: Configuration data retrieval for district settings, school lists, and calendar information completes in under 100 milliseconds at the 95th percentile when cached

- **SC-006**: All three foundation services achieve ≥80% test coverage with passing unit tests, integration tests, and BDD acceptance tests

- **SC-007**: All three services can be started via .NET Aspire AppHost and appear healthy in the Aspire Dashboard within 30 seconds of startup

- **SC-008**: Health check endpoints for all services respond within 1 second with accurate status information

- **SC-009**: Multi-tenant data isolation prevents cross-district data access with 100% of unauthorized access attempts blocked and logged

- **SC-010**: Configuration changes propagate to dependent services through event publication within 2 seconds of being saved

- **SC-011**: The system supports at least 1,000 concurrent authenticated users without performance degradation below defined SLO thresholds

- **SC-012**: Circuit breakers activate within 5 seconds of detecting backend service failures, preventing cascading failures to other services

- **SC-013**: All secrets are retrieved from Azure Key Vault with zero hardcoded credentials found in source code or configuration files

- **SC-014**: End-to-end authentication flow from user login through token generation, API Gateway validation, and service request completes successfully in under 5 seconds

- **SC-015**: System logs all security events (authentication attempts, authorization failures, tenant boundary violations) with complete audit trail

- **SC-016**: Authorization decisions cached in Redis achieve sub-50ms response time at 95th percentile with cache hit rate exceeding 90%

- **SC-017**: District Admin invitation workflow achieves 80% acceptance rate within 48 hours of sending invitation email

- **SC-018**: Tenant context switching for multi-tenant users completes in under 200 milliseconds including session update and cache invalidation

## Assumptions

- Microsoft Entra ID tenant is already configured with BOTH B2B (organizational accounts) and B2C (external identity) capabilities for future student/parent access scenarios
- Azure infrastructure (Key Vault, Service Bus, PostgreSQL) is provisioned and accessible
- .NET Aspire 13.0.0 runtime and tooling are installed in development and deployment environments
- WIPNorthStar Feature 001 (Unified SSO) and Feature 002 (Bootstrap Tenant Access) implementation patterns are available as reference for Entra ID B2B/B2C integration, token exchange, and invitation workflows
- Legacy OldNorthStar system remains operational during Phase 1 implementation for Strangler Fig pattern
- Database schema for legacy LoginContext and DistrictContext is documented and accessible for data migration planning
- Development team has access to Microsoft documentation and samples for Entra ID, YARP, and Aspire technologies
- Testing environments can support multi-tenant scenarios with at least 2-3 test districts
- CI/CD pipeline infrastructure exists for automated builds, tests, and deployments

## Dependencies

- Microsoft Entra ID service availability for SSO authentication
- Azure Key Vault for secrets management
- Azure Service Bus for event-driven messaging
- PostgreSQL databases for service data persistence
- Redis for distributed caching
- .NET Aspire 13.0.0 runtime and hosting infrastructure
- OldNorthStar legacy system availability during migration (Strangler Fig pattern)
- WIPNorthStar codebase as reference implementation for patterns

## Scope Boundaries

### In Scope

- Implementation of three foundation services: Identity & Authentication, API Gateway (YARP), Configuration
- Microsoft Entra ID integration for staff and administrator SSO
- Local authentication fallback for all user types
- JWT token generation, validation, and refresh mechanisms
- API Gateway routing to both new microservices and legacy endpoints
- Rate limiting, circuit breakers, and request timeout policies at gateway
- District, school, and academic calendar management
- Multi-tenant data isolation with PostgreSQL Row-Level Security
- Event-driven integration via Azure Service Bus
- Comprehensive health checks and observability
- TDD workflow with ≥80% test coverage
- .NET Aspire orchestration for all services

### Out of Scope (Deferred to Future Phases)

- Student Management Service (Phase 2)
- Staff Management Service (Phase 2)
- Assessment Service (Phase 2)
- UI/frontend implementation (Phase 3-4 parallel)
- Data migration from legacy databases (planned, not executed in Phase 1)
- Multi-factor authentication (future enhancement)
- Social login providers beyond Microsoft Entra ID (future enhancement)
- Advanced reporting and analytics (Phase 4)
- Mobile application support (future)
- Automated user provisioning from external HR systems (future)
- Advanced authorization policies beyond RBAC (future enhancement)

## Risks & Mitigation

- **Risk**: Microsoft Entra ID integration complexity may exceed estimates  
  **Mitigation**: Leverage WIPNorthStar Feature 001 proven patterns; allocate buffer time for authentication testing; implement local authentication fallback early

- **Risk**: Multi-tenant data isolation may have implementation gaps  
  **Mitigation**: Implement PostgreSQL Row-Level Security from day one; conduct security testing with penetration testing scenarios; maintain defense-in-depth with application-layer filtering

- **Risk**: API Gateway performance may become a bottleneck  
  **Mitigation**: Implement aggressive caching for token validation; conduct load testing early; use Redis for distributed caching; monitor gateway performance metrics continuously

- **Risk**: Event-driven architecture adds complexity to debugging  
  **Mitigation**: Implement comprehensive distributed tracing with correlation IDs; use Aspire Dashboard for observability; maintain event schema versioning; implement dead letter queues

- **Risk**: Test coverage requirements may slow development velocity  
  **Mitigation**: Follow TDD from start to prevent rework; use BDD for acceptance criteria alignment; leverage Aspire integration test patterns; automate test execution in CI/CD

- **Risk**: Configuration changes may cause cache inconsistencies  
  **Mitigation**: Implement reliable event publication for cache invalidation; use TTL-based cache expiration as fallback; provide manual cache refresh capabilities; monitor cache hit/miss ratios
