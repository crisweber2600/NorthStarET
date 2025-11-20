# Feature Specification: API Gateway - YARP Service Orchestration

**Specification Branch**: `CrossCuttingConcerns/001-api-gateway-yarp-spec` *(planning artifacts only)*  
**Implementation Branch**: `CrossCuttingConcerns/001-api-gateway-yarp` *(created after spec approval)*  
**Created**: 2025-11-20  
**Status**: Draft  
**Input**: Scenario file `Plan/CrossCuttingConcerns/scenarios/02-api-gateway.md`

---

## Layer Identification (MANDATORY)

**Target Layer**: CrossCuttingConcerns  
*This feature implements the API Gateway service that acts as a unified entry point for all Foundation and DigitalInk services, providing cross-cutting concerns like authentication, routing, rate limiting, and observability.*

**Layer Validation Checklist**:
- [x] Layer explicitly identified (CrossCuttingConcerns)
- [x] Layer exists in mono-repo structure (`Plan/CrossCuttingConcerns/`)
- [ ] If new layer: Architecture Review documented in `Plan/CrossCuttingConcerns/README.md`
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (ServiceDefaults, Domain for common types, Application for MediatR patterns)  
*The API Gateway depends on Foundation's ServiceDefaults for Aspire orchestration, observability, and health checks. It validates tokens issued by Foundation's Identity Service and routes requests to both Foundation and DigitalInk services.*

**Justification**: The API Gateway is a cross-cutting service that sits in front of all layers, providing unified authentication, routing, and operational concerns. It enables the Strangler Fig migration pattern by routing requests to either new microservices or the legacy monolith based on configuration.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Authentication and Request Routing (Priority: P1)

As a client application, I need all my API requests to be authenticated and routed to the correct service so that I have a single, secure entry point for all backend services.

**Why this priority**: Core gateway functionality - without this, no requests can be processed. This is the MVP that enables all other features.

**Independent Test**: Can be fully tested by sending authenticated requests to various endpoints and verifying they reach the correct services with proper authentication context.

**Acceptance Scenarios**:

1. **Scenario 1: Route Request to New Microservice**
   - **Given** the API Gateway is configured with routes for all services
   - **And** the Student Management Service is deployed and healthy
   - **When** a client sends `GET /api/v1/students/123`
   - **Then** the API Gateway receives the request
   - **And** validates the JWT token with Identity Service
   - **And** routes the request to Student Management Service
   - **And** forwards the tenant context from the token
   - **And** returns the response to the client
   - **And** the entire flow completes within 150ms

2. **Scenario 3: Authentication Validation at Gateway**
   - **Given** a request includes a JWT token in the Authorization header
   - **When** the request arrives at the API Gateway
   - **Then** the gateway validates the token signature
   - **And** checks token expiration
   - **And** validates the issuer and audience claims
   - **And** extracts tenant_id from claims
   - **And** if valid, forwards request with tenant context
   - **And** if invalid, returns HTTP 401 Unauthorized
   - **And** authentication check completes within 20ms

---

### User Story 2 - Strangler Fig Migration Support (Priority: P1)

As a system architect, I need the gateway to route requests to either new microservices or the legacy monolith based on migration status, so that I can gradually migrate functionality without disrupting users.

**Why this priority**: Critical for the migration strategy - enables phased migration from legacy to microservices architecture.

**Independent Test**: Can be tested by configuring routes to point to both new and legacy services, then sending requests and verifying correct routing based on configuration.

**Acceptance Scenarios**:

1. **Scenario 2: Route Request to Legacy Monolith During Migration**
   - **Given** the Assessment Service is not yet migrated
   - **And** legacy NS4.WebAPI handles assessment endpoints
   - **When** a client sends `GET /api/v1/assessments/456`
   - **Then** the API Gateway routes to legacy NS4.WebAPI
   - **And** translates any header/token formats if needed
   - **And** returns the legacy response
   - **And** the client is unaware it's calling legacy vs. new service

2. **Scenario 10: API Versioning Support**
   - **Given** the API supports both v1 and v2 endpoints
   - **When** a client requests `/api/v1/students`
   - **Then** the gateway routes to v1 Student Service
   - **When** a client requests `/api/v2/students`
   - **Then** the gateway routes to v2 Student Service
   - **And** both versions can coexist during migration
   - **And** v1 is deprecated with sunset date in response headers
   - **And** clients are encouraged to migrate to v2

---

### User Story 3 - Request Transformation and Context Injection (Priority: P1)

As a downstream service, I need the gateway to inject tenant and user context into every request so that I don't have to extract this information from tokens myself.

**Why this priority**: Simplifies service implementation by centralizing token parsing and context extraction at the gateway level.

**Independent Test**: Can be tested by sending authenticated requests and verifying that downstream services receive the injected headers (X-Tenant-Id, X-User-Id, X-Correlation-Id).

**Acceptance Scenarios**:

1. **Scenario 9: Request Transformation and Header Injection**
   - **Given** downstream services need tenant context
   - **When** a request flows through the gateway
   - **Then** the gateway extracts tenant_id from JWT claims
   - **And** injects `X-Tenant-Id` header for downstream services
   - **And** injects `X-User-Id` header with user identifier
   - **And** injects `X-Correlation-Id` for request tracing
   - **And** removes sensitive headers before forwarding
   - **And** downstream services can rely on these headers

2. **Scenario 6: Request Logging and Correlation IDs**
   - **Given** a client makes a request
   - **When** the request enters the API Gateway
   - **Then** a correlation ID is generated (or extracted from X-Correlation-ID header)
   - **And** the correlation ID is added to all downstream service calls
   - **And** the request is logged with: timestamp, path, method, status, duration, correlation_id
   - **And** all services use the same correlation ID for logging
   - **And** the correlation ID is returned in response headers
   - **And** distributed tracing is enabled across all services

---

### User Story 4 - Rate Limiting and Protection (Priority: P2)

As a platform operator, I need the gateway to enforce rate limits per tenant so that no single tenant can overwhelm the system and affect other tenants.

**Why this priority**: Important for production stability and fairness, but not required for initial functionality.

**Independent Test**: Can be tested by sending requests at high volume from a single tenant and verifying rate limits are enforced correctly.

**Acceptance Scenarios**:

1. **Scenario 4: Rate Limiting by Tenant**
   - **Given** the API Gateway has rate limits configured per tenant
   - **And** District A is limited to 1000 requests/minute
   - **When** District A makes 1001 requests in one minute
   - **Then** the first 1000 requests are processed normally
   - **And** the 1001st request is rejected with HTTP 429 Too Many Requests
   - **And** the response includes Retry-After header
   - **And** the rate limit resets after one minute
   - **And** other districts are unaffected by District A's limit

2. **Scenario 12: Request Size Limits and Validation**
   - **Given** the gateway has request size limits configured
   - **When** a client uploads a file larger than 10MB
   - **Then** the gateway rejects the request with HTTP 413 Payload Too Large
   - **And** returns an error message about the size limit
   - **When** a client sends malformed JSON
   - **Then** the gateway rejects with HTTP 400 Bad Request
   - **And** includes validation error details
   - **And** the request never reaches downstream services

---

### User Story 5 - Resilience and Health Monitoring (Priority: P2)

As a platform operator, I need the gateway to detect failing services, open circuit breakers, and aggregate health status so that the system degrades gracefully and I can monitor service health.

**Why this priority**: Critical for production reliability, but can be added after basic routing works.

**Independent Test**: Can be tested by simulating service failures and verifying circuit breakers open/close correctly, and health endpoint aggregates status.

**Acceptance Scenarios**:

1. **Scenario 8: Circuit Breaker for Failing Service**
   - **Given** the Assessment Service is experiencing issues
   - **And** requests to Assessment Service are timing out
   - **When** 5 consecutive requests fail
   - **Then** the circuit breaker opens for Assessment Service
   - **And** subsequent requests immediately return HTTP 503 Service Unavailable
   - **And** the circuit breaker attempts recovery after 30 seconds
   - **And** if recovery succeeds, the circuit closes
   - **And** if recovery fails, the circuit remains open
   - **And** other services are unaffected by the circuit breaker

2. **Scenario 7: Health Check Aggregation**
   - **Given** the API Gateway monitors health of all downstream services
   - **When** a monitoring system queries `GET /health`
   - **Then** the gateway checks health of: Identity, Student, Staff, Assessment, etc.
   - **And** aggregates the health status
   - **And** returns HTTP 200 if all services are healthy
   - **And** returns HTTP 503 if any critical service is unhealthy
   - **And** includes details about each service's health status
   - **And** the health check completes within 500ms

3. **Scenario 11: Load Balancing Across Service Instances**
   - **Given** the Student Service has 3 instances running at: student-1, student-2, student-3
   - **When** multiple requests arrive for student data
   - **Then** the gateway uses round-robin load balancing
   - **And** distributes requests evenly across all instances
   - **And** monitors instance health continuously
   - **And** removes unhealthy instances from the pool
   - **And** reintroduces instances when they recover

---

### User Story 6 - CORS Support for Web Clients (Priority: P3)

As a web application developer, I need the gateway to handle CORS preflight requests so that my browser-based application can make cross-origin requests to the API.

**Why this priority**: Important for web clients, but not required for server-to-server or mobile applications.

**Independent Test**: Can be tested by making preflight OPTIONS requests from a browser and verifying correct CORS headers are returned.

**Acceptance Scenarios**:

1. **Scenario 5: Cross-Origin Resource Sharing (CORS)**
   - **Given** the frontend is hosted at `https://app.northstar.edu`
   - **And** the API Gateway is at `https://api.northstar.edu`
   - **When** the browser makes a preflight OPTIONS request
   - **Then** the gateway responds with CORS headers:
     - Access-Control-Allow-Origin: https://app.northstar.edu
     - Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH
     - Access-Control-Allow-Headers: Authorization, Content-Type
     - Access-Control-Max-Age: 86400
   - **And** the browser caches the CORS policy
   - **And** subsequent requests include credentials

---

### Edge Cases

- What happens when a service is completely unavailable (not just slow)?
- How does the gateway handle partial token claims (missing tenant_id or user_id)?
- What happens if the Identity Service is down and tokens cannot be validated?
- How does the gateway handle WebSocket upgrade requests?
- What happens when correlation IDs are malformed or excessively long?
- How does the gateway handle requests with multiple conflicting versioning headers?
- What happens when rate limit storage (Redis) is unavailable?
- How does the gateway handle circular redirects or routing loops?
- What happens when a downstream service returns a streaming response?
- How does the gateway handle requests that timeout before reaching downstream services?

## Requirements *(mandatory)*

### Functional Requirements

**Core Routing & Authentication**
- **FR-001**: Gateway MUST validate JWT tokens issued by Foundation Identity Service on every authenticated request
- **FR-002**: Gateway MUST extract tenant_id and user_id from validated JWT claims
- **FR-003**: Gateway MUST route requests to correct destination services based on URL path patterns
- **FR-004**: Gateway MUST support routing to both new microservices and legacy NS4.WebAPI monolith
- **FR-005**: Gateway MUST inject X-Tenant-Id, X-User-Id, and X-Correlation-Id headers into all downstream requests

**Request Transformation & Context**
- **FR-006**: Gateway MUST generate a unique correlation ID for each request (or preserve existing X-Correlation-ID)
- **FR-007**: Gateway MUST remove sensitive headers (internal tokens, secrets) before forwarding to downstream services
- **FR-008**: Gateway MUST log every request with: timestamp, path, method, status code, duration, correlation_id, tenant_id
- **FR-009**: Gateway MUST support distributed tracing using OpenTelemetry with correlation ID propagation

**Rate Limiting & Protection**
- **FR-010**: Gateway MUST enforce rate limits per tenant (configurable per tenant, default 1000 requests/minute)
- **FR-011**: Gateway MUST return HTTP 429 Too Many Requests with Retry-After header when rate limit is exceeded
- **FR-012**: Gateway MUST reject requests larger than configured size limit (default 10MB) with HTTP 413
- **FR-013**: Gateway MUST validate request content-type and reject malformed payloads with HTTP 400

**Resilience & Health**
- **FR-014**: Gateway MUST implement circuit breakers for each downstream service (5 failures opens circuit, 30s recovery attempt)
- **FR-015**: Gateway MUST return HTTP 503 Service Unavailable immediately when circuit breaker is open
- **FR-016**: Gateway MUST aggregate health status from all downstream services at /health endpoint
- **FR-017**: Gateway MUST return HTTP 200 when all services healthy, HTTP 503 when any critical service unhealthy
- **FR-018**: Gateway MUST support load balancing across multiple instances of the same service (round-robin)
- **FR-019**: Gateway MUST monitor instance health and remove unhealthy instances from load balancer pool

**Versioning & Migration**
- **FR-020**: Gateway MUST support API versioning through URL path (e.g., /api/v1/..., /api/v2/...)
- **FR-021**: Gateway MUST route v1 and v2 requests to different service versions when both exist
- **FR-022**: Gateway MUST include deprecation information in response headers for deprecated API versions

**CORS & Web Support**
- **FR-023**: Gateway MUST handle CORS preflight OPTIONS requests from configured origins
- **FR-024**: Gateway MUST return appropriate CORS headers: Allow-Origin, Allow-Methods, Allow-Headers, Max-Age
- **FR-025**: Gateway MUST support credentials in CORS requests (cookies, authorization headers)

**Identity & Authentication Requirements - Token Validation Only:**
- Gateway does **NOT issue tokens** - it only **validates** tokens issued by Foundation Identity Service (Entra ID-based)
- Token validation: Use Microsoft.Identity.Web to validate Entra ID JWT signatures, expiration, issuer, and audience
- No session management required at gateway level - gateway is stateless, validates tokens on every request
- See `Plan/Foundation/docs/legacy-identityserver-migration.md` for Identity Service architecture

### Key Entities *(include if feature involves data)*

The API Gateway is primarily stateless and routes configuration-driven. The following configuration entities are required:

- **Route Configuration**: Defines URL path patterns, destination service clusters, authentication requirements, and request transformations
- **Cluster Configuration**: Defines destination service addresses, load balancing policy, health check settings, and circuit breaker thresholds  
- **Rate Limit Policy**: Defines per-tenant request limits, time windows, and penalty behaviors
- **CORS Policy**: Defines allowed origins, methods, headers, and credential settings
- **Health Check Definition**: Defines service health endpoints, check intervals, and timeout settings

## Success Criteria *(mandatory)*

### Measurable Outcomes

**Performance**
- **SC-001**: Authentication validation completes within 20ms at P95 percentile
- **SC-002**: Routing decision and request forwarding adds no more than 50ms overhead at P95 percentile
- **SC-003**: End-to-end request flow (including downstream service) completes within 150ms for simple GET requests
- **SC-004**: Health check aggregation completes within 500ms even with 10+ downstream services
- **SC-005**: Circuit breaker decision (open circuit) completes within 1ms

**Reliability**
- **SC-006**: Gateway maintains 99.9% uptime (excluding scheduled maintenance)
- **SC-007**: Circuit breakers successfully prevent cascading failures when downstream services fail
- **SC-008**: Rate limiting successfully prevents individual tenants from degrading performance for other tenants
- **SC-009**: Load balancing distributes requests evenly within 10% variance across healthy instances

**Security**
- **SC-010**: 100% of authenticated requests have valid JWT tokens (all invalid tokens rejected with 401)
- **SC-011**: 100% of downstream requests include tenant context headers (no tenant data leakage)
- **SC-012**: All sensitive headers are removed before forwarding (verified through security audit)
- **SC-013**: CORS policies prevent unauthorized cross-origin requests (verified through penetration testing)

**Observability**
- **SC-014**: 100% of requests are logged with correlation IDs
- **SC-015**: Distributed traces successfully link gateway requests to downstream service calls
- **SC-016**: Health check endpoint accurately reflects downstream service status (no false positives/negatives)

**Migration Support**
- **SC-017**: Requests route to legacy monolith for non-migrated endpoints with zero client changes required
- **SC-018**: Requests route to new microservices for migrated endpoints with zero client changes required
- **SC-019**: API versioning supports side-by-side deployment of v1 and v2 with different routing
