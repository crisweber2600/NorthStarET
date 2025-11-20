# Implementation Plan: API Gateway - YARP Service Orchestration

**Specification Branch**: `CrossCuttingConcerns/001-api-gateway-yarp-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `CrossCuttingConcerns/001-api-gateway-yarp` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: [spec.md](./spec.md)

**Note**: This plan was generated from scenario file `Plan/CrossCuttingConcerns/scenarios/02-api-gateway.md`.

## Summary

The API Gateway provides a unified entry point for all NorthStarET services using YARP (Yet Another Reverse Proxy). It implements the Backend-for-Frontend (BFF) pattern and enables the Strangler Fig migration by routing requests to either new microservices or the legacy NS4.WebAPI monolith. Key capabilities include JWT token validation, tenant context injection, rate limiting, circuit breaking, health check aggregation, load balancing, and CORS support.

**Primary Technical Approach**: Build an ASP.NET Core 9.0 service using YARP for reverse proxy capabilities, integrate with Foundation Identity Service for token validation, implement rate limiting with Redis, configure circuit breakers with Polly, and orchestrate with .NET Aspire for service discovery and health monitoring.

## Technical Context

**Language/Version**: C# 12 / .NET 9.0  
**Primary Dependencies**: 
- Yarp.ReverseProxy 2.2.0 (reverse proxy and routing)
- Microsoft.Identity.Web 3.x (JWT token validation)
- Polly 8.x (circuit breakers and resilience)
- StackExchange.Redis 2.x (rate limiting storage)
- Aspire.Hosting 9.x (orchestration and service discovery)
- OpenTelemetry.Instrumentation.AspNetCore 1.x (distributed tracing)

**Storage**: Redis for rate limiting state and idempotency windows (no persistent storage required - gateway is stateless)  
**Testing**: 
- xUnit for unit tests
- Aspire.Testing for integration tests with full service stack
- Testcontainers for Redis in tests
- Moq for mocking downstream services

**Target Platform**: Linux containers (Docker), deployed to Azure Container Apps or AKS  
**Project Type**: Web API / Reverse Proxy (single ASP.NET Core application)  
**Performance Goals**: 
- Authentication validation: <20ms (P95)
- Routing overhead: <50ms (P95)
- Throughput: >5000 requests/second per instance
- Health checks: <500ms for full aggregation

**Constraints**: 
- Must be stateless (horizontally scalable)
- Zero downtime deployments required
- Must handle 10,000+ concurrent connections per instance
- Circuit breaker decisions: <1ms
- Memory footprint: <512MB per instance at steady state

**Scale/Scope**: 
- Support 100+ downstream service instances
- Handle 20+ tenant rate limit policies
- Aggregate health from 15+ services
- Support 50+ route configurations
- Process 1M+ requests/day initially, scaling to 100M+

### Identity & Authentication Guidance

**Authentication Role**: Token Validation ONLY (does NOT issue tokens)

- **Identity Provider**: Foundation Identity Service (Entra ID-backed) issues JWT tokens
- **Gateway Responsibility**: Validate JWT signatures, expiration, issuer, audience using Microsoft.Identity.Web
- **Token Validation**: JWT bearer token validation middleware (NOT session-based at gateway level)
- **Claims Extraction**: Extract tenant_id, user_id, roles from validated JWT for downstream injection
- **No Session Storage**: Gateway is stateless - validates tokens on every request, does not maintain sessions
- **Architecture Reference**: See `Plan/Foundation/docs/legacy-identityserver-migration.md` for Identity Service architecture
- **Key Dependencies**: Microsoft.Identity.Web 3.x for token validation only

## Layer Identification (MANDATORY)

**Target Layer**: CrossCuttingConcerns  
**Implementation Path**: `Src/Foundation/services/ApiGateway/` *(Note: Implemented in Foundation layer as shared infrastructure service)*  
**Specification Path**: `Plan/CrossCuttingConcerns/specs/001-api-gateway-yarp/` *(where this plan.md resides)*

**Implementation Note**: While the specification resides in CrossCuttingConcerns (as it applies to all layers), the actual service implementation will be in `Src/Foundation/services/ApiGateway/` because:
1. The gateway must be registered in the Aspire AppHost (located in Foundation)
2. It acts as Foundation-provided shared infrastructure
3. It integrates tightly with Foundation's Identity Service
4. All layers depend on it as an infrastructure service (like AppHost)

### Layer Consistency Validation

- [x] Target Layer (CrossCuttingConcerns) matches specification
- [x] Implementation path follows Foundation services structure (gateway is infrastructure)
- [x] Specification path follows CrossCuttingConcerns structure
- [x] Architecture rationale documented (gateway is Foundation-provided infrastructure)

### Shared Infrastructure Dependencies

- [x] **ServiceDefaults** - Aspire orchestration, OpenTelemetry, health checks, default middleware
- [ ] **Domain** - Not needed (gateway is stateless infrastructure, no domain logic)
- [ ] **Application** - Not needed (no CQRS, no business logic)
- [ ] **Infrastructure** - Partial use for Redis client configuration only

**Rationale**: API Gateway is pure infrastructure - it has no business domain, no entities, no use cases. It purely routes, validates, and transforms requests.

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure (ServiceDefaults, partial Infrastructure for Redis)

**Specific Dependencies**:
- **ServiceDefaults**: Required for Aspire registration, OpenTelemetry, health checks
- **Infrastructure.Caching**: Redis client configuration for rate limiting state
- **Foundation Identity Service** (runtime dependency): Validates JWT tokens, configured via OIDC discovery

**Justification**: 
- ServiceDefaults: Required for all Aspire-orchestrated services (constitutional requirement)
- Redis: Rate limiting state must be shared across gateway instances for correctness
- Identity Service: Gateway validates tokens issued by Identity Service (runtime dependency, not compile-time)

**Constitutional Compliance**: 
- ✅ Gateway depends ONLY on approved shared infrastructure (ServiceDefaults, Redis client)
- ✅ Runtime dependency on Identity Service is via standard protocols (OIDC/JWT), not direct code reference
- ✅ Gateway is independently deployable and testable (can mock Identity Service for tests)
- ✅ No service-to-service code dependencies - all integration via HTTP/OIDC

### Constitution Check

**Principle 1 (Incremental TDD)**: ✅ PASS  
- Plan includes test strategy with unit, integration, and load tests
- Red-Green-Refactor workflow documented in test approach

**Principle 2 (Evidence-Based Quality)**: ✅ PASS  
- Performance targets defined with measurable thresholds
- Test coverage requirements specified (≥80%)
- Red→Green evidence gates planned for each phase

**Principle 3 (UI Preservation During Migration)**: ⚠️ N/A  
- Gateway is backend infrastructure, no UI components

**Principle 4 (Spec-First with User Scenarios)**: ✅ PASS  
- Specification created from scenario file with 12 scenarios
- Each scenario has Given/When/Then acceptance criteria
- Prioritized user stories (P1/P2/P3)

**Principle 5 (Ship Minimum First)**: ✅ PASS  
- P1 stories identified as MVP (core routing, auth, context injection)
- P2 and P3 features can be added incrementally

**Principle 6 (Layer Isolation)**: ✅ PASS  
- Implementation in Foundation as shared infrastructure
- Depends only on approved shared components
- No direct service-to-service code dependencies

**Principle 7 (No Premature Frameworks)**: ✅ PASS  
- YARP is justified: industry-standard reverse proxy, not custom abstraction
- Polly is justified: proven resilience library, not custom retry logic
- All dependencies have clear rationale

**Overall**: ✅ PASS - Ready for Phase 0 research

## Project Structure

### Documentation (this feature)

```text
Plan/CrossCuttingConcerns/specs/001-api-gateway-yarp/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file - implementation plan
├── research.md          # Technology research and decisions (to be created)
├── data-model.md        # Configuration schema (to be created)
├── contracts/           # API contracts and YARP configuration schemas
│   ├── route-config.json        # YARP route configuration schema
│   ├── cluster-config.json      # YARP cluster configuration schema
│   ├── rate-limit-policy.json   # Rate limiting policy schema
│   └── health-check-config.json # Health check configuration schema
└── tasks.md             # Phase breakdown (/speckit.tasks command output)
```

### Source Code (repository root)

```text
Src/Foundation/services/ApiGateway/
├── ApiGateway.csproj                    # Service project file
├── Program.cs                            # Startup, YARP config, middleware pipeline
├── appsettings.json                      # Default configuration
├── appsettings.Development.json          # Local dev overrides
├── Configuration/
│   ├── YarpConfiguration.cs              # YARP route/cluster config builder
│   ├── RateLimitConfiguration.cs         # Rate limit policy configuration
│   ├── CorsConfiguration.cs              # CORS policy configuration
│   └── CircuitBreakerConfiguration.cs    # Polly circuit breaker policies
├── Middleware/
│   ├── JwtValidationMiddleware.cs        # Token validation using Microsoft.Identity.Web
│   ├── CorrelationIdMiddleware.cs        # Generate/propagate correlation IDs
│   ├── RequestLoggingMiddleware.cs       # Structured logging with correlation ID
│   ├── HeaderTransformMiddleware.cs      # Inject X-Tenant-Id, X-User-Id headers
│   └── ErrorHandlingMiddleware.cs        # Global exception handling
├── Transforms/
│   ├── TenantContextTransform.cs         # YARP transform: extract tenant from JWT
│   ├── UserContextTransform.cs           # YARP transform: extract user from JWT
│   ├── CorrelationIdTransform.cs         # YARP transform: add correlation ID
│   └── SensitiveHeaderRemovalTransform.cs # YARP transform: remove internal headers
├── RateLimiting/
│   ├── TenantRateLimiter.cs              # Per-tenant rate limiting logic
│   ├── RateLimitStore.cs                 # Redis-backed rate limit counter
│   └── RateLimitPolicy.cs                # Rate limit policy definition
├── Health/
│   ├── AggregatedHealthCheck.cs          # Aggregate downstream service health
│   ├── ServiceHealthChecker.cs           # Check individual service health
│   └── CircuitBreakerHealthCheck.cs      # Include circuit breaker state in health
├── Resilience/
│   ├── CircuitBreakerRegistry.cs         # Polly circuit breaker registry per service
│   └── CircuitBreakerMetrics.cs          # Export circuit breaker state as metrics
└── Extensions/
    ├── YarpExtensions.cs                 # YARP service registration extensions
    ├── RateLimitExtensions.cs            # Rate limiting service registration
    └── ObservabilityExtensions.cs        # OpenTelemetry configuration

Src/Foundation/AppHost/
├── Program.cs                            # Add ApiGateway service registration
└── appsettings.json                      # Configure gateway port (7000) and dependencies

tests/ApiGateway.Tests/
├── Unit/
│   ├── Middleware/
│   │   ├── JwtValidationMiddlewareTests.cs
│   │   ├── CorrelationIdMiddlewareTests.cs
│   │   └── HeaderTransformMiddlewareTests.cs
│   ├── RateLimiting/
│   │   ├── TenantRateLimiterTests.cs
│   │   └── RateLimitStoreTests.cs
│   ├── Resilience/
│   │   └── CircuitBreakerRegistryTests.cs
│   └── Health/
│       └── AggregatedHealthCheckTests.cs
├── Integration/
│   ├── RoutingTests.cs                   # End-to-end routing scenarios
│   ├── AuthenticationTests.cs            # JWT validation scenarios
│   ├── RateLimitingTests.cs              # Rate limiting enforcement
│   ├── CircuitBreakerTests.cs            # Circuit breaker behavior
│   ├── HealthCheckTests.cs               # Health aggregation
│   └── LoadBalancingTests.cs             # Load balancing behavior
└── Load/
    ├── GatewayLoadTests.cs               # Performance and throughput tests
    └── load-test-config.yaml             # k6 or NBomber configuration

tests/ApiGateway.Aspire.Tests/
├── ApiGatewayAspireTests.cs              # Full Aspire orchestration tests
└── appsettings.Test.json                 # Test environment configuration
```

**Structure Decision**: Single ASP.NET Core Web API project with YARP middleware. The gateway is stateless infrastructure with no business logic, so the structure focuses on middleware, transforms, and configuration. All routing logic is declarative YARP configuration loaded from appsettings.json or configuration service.

**Key Design Decisions**:
1. **No Controllers**: Gateway is pure middleware/proxy - no API controllers needed
2. **YARP Transforms**: Request transformation via YARP's built-in transform pipeline
3. **Middleware Chain**: Authentication → Correlation ID → Logging → YARP proxy
4. **Configuration-Driven**: All routes, clusters, and policies defined in configuration (code handles logic only)
5. **Stateless**: No database, no persistent state except Redis for rate limiting (shared across instances)

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No Violations**: All constitutional checks passed. No complexity justification required.

## Implementation Phases

### Phase 0: Research & Technology Selection (1-2 days)

**Objective**: Validate YARP capabilities, token validation approach, and performance characteristics

**Research Areas**:
1. YARP Configuration Options
   - Route matching patterns (path, query, headers)
   - Cluster configuration (destinations, load balancing)
   - Transform pipeline capabilities
   - Performance characteristics (latency overhead)

2. JWT Validation with Microsoft.Identity.Web
   - Integration with YARP middleware
   - Token caching strategies
   - Claims extraction patterns
   - Error handling for invalid/expired tokens

3. Rate Limiting Strategies
   - Redis-backed distributed rate limiting
   - Fixed window vs sliding window algorithms
   - Per-tenant partition key strategies
   - Performance impact of Redis round trips

4. Circuit Breaker Patterns with Polly
   - Integration with YARP HttpClient factory
   - Circuit state management and metrics
   - Recovery strategies and timeouts
   - Testing circuit breaker behavior

5. Health Check Aggregation
   - ASP.NET Core health checks with dependencies
   - Async health check patterns
   - Timeout and cancellation strategies
   - Health check UI and monitoring integration

**Deliverables**:
- `research.md` with technology decisions and justifications
- Proof-of-concept spike for YARP + JWT validation
- Performance baseline measurements
- Risk assessment for distributed rate limiting

**Red→Green Evidence**: N/A (research phase, no tests)

---

### Phase 1: Core Gateway Routing & Authentication (3-4 days)

**Objective**: Implement MVP gateway with basic routing and JWT validation (P1 User Stories 1)

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Unit tests for JWT validation middleware (invalid token, expired token, missing claims)
   - Integration tests for route matching (Student Service, legacy monolith routes)
   - Integration tests for authentication flow (valid token → downstream service)

2. **GREEN Phase** - Implement to pass tests:
   - YARP configuration for Student Service and legacy routes
   - JwtValidationMiddleware using Microsoft.Identity.Web
   - Basic Program.cs with middleware pipeline
   - Aspire AppHost registration

3. **REFACTOR Phase**:
   - Extract configuration builders
   - Add structured logging
   - Optimize token validation caching

**Key Components**:
- `Program.cs` with YARP and authentication middleware
- `Configuration/YarpConfiguration.cs` for route/cluster definitions
- `Middleware/JwtValidationMiddleware.cs` for token validation
- Integration tests with Testcontainers and mocked Identity Service

**Success Criteria** (from spec.md):
- SC-010: 100% of authenticated requests validated (invalid tokens → 401)
- SC-001: Authentication validation < 20ms P95
- SC-003: End-to-end request < 150ms

**Red→Green Evidence**:
- `phase1-red-unit-tests.txt`: Unit test failures before implementation
- `phase1-green-unit-tests.txt`: Unit test passes after implementation
- `phase1-red-integration-tests.txt`: Integration test failures before implementation
- `phase1-green-integration-tests.txt`: Integration test passes after implementation

---

### Phase 2: Request Transformation & Context Injection (2-3 days)

**Objective**: Add tenant/user context injection and correlation IDs (P1 User Stories 3)

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Unit tests for header transform middleware (tenant_id, user_id extraction)
   - Unit tests for correlation ID generation/propagation
   - Integration tests verifying headers reach downstream services
   - Integration tests for distributed tracing spans

2. **GREEN Phase** - Implement to pass tests:
   - `Middleware/CorrelationIdMiddleware.cs`
   - `Middleware/HeaderTransformMiddleware.cs`
   - `Transforms/TenantContextTransform.cs` (YARP transform)
   - `Transforms/UserContextTransform.cs` (YARP transform)
   - OpenTelemetry integration for distributed tracing

3. **REFACTOR Phase**:
   - Consolidate header injection logic
   - Add request/response logging enrichment
   - Performance optimization for header parsing

**Key Components**:
- Correlation ID middleware with logging scope
- YARP transforms for tenant/user context
- Header sanitization (remove sensitive headers)
- Structured logging with correlation ID, tenant, user

**Success Criteria** (from spec.md):
- SC-011: 100% of downstream requests include tenant context
- SC-014: 100% of requests logged with correlation IDs
- SC-015: Distributed traces link gateway → services

**Red→Green Evidence**: 4 transcript files (RED/GREEN for unit and integration tests)

---

### Phase 3: Strangler Fig Migration Support (2 days)

**Objective**: Support routing to both new microservices and legacy monolith (P1 User Stories 2)

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Integration tests for legacy endpoint routing (/api/v1/assessments → NS4.WebAPI)
   - Integration tests for versioning (v1 vs v2 routes)
   - Integration tests for header translation (if needed for legacy)
   - Tests verifying clients can't distinguish legacy from new services

2. **GREEN Phase** - Implement to pass tests:
   - YARP configuration for legacy cluster
   - Version-based routing configuration
   - Header translation transforms (if needed)
   - Deprecation header injection for v1

3. **REFACTOR Phase**:
   - Simplify routing configuration
   - Add configuration validation at startup
   - Document migration patterns

**Key Components**:
- Legacy cluster configuration in appsettings.json
- Version-based route matchers
- Optional header translation middleware
- Sunset-Date header injection for deprecated endpoints

**Success Criteria** (from spec.md):
- SC-017: Legacy requests route correctly without client changes
- SC-018: New microservice requests route correctly without client changes
- SC-019: v1 and v2 coexist side-by-side

**Red→Green Evidence**: 4 transcript files

---

### Phase 4: Rate Limiting & Protection (2-3 days)

**Objective**: Enforce per-tenant rate limits and request validation (P2 User Stories 4)

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Unit tests for rate limiter (below limit → allow, over limit → reject)
   - Unit tests for Redis rate limit store (increment, reset, multi-tenant isolation)
   - Integration tests for rate limit enforcement with Redis
   - Integration tests for request size limits
   - Integration tests for malformed payload rejection

2. **GREEN Phase** - Implement to pass tests:
   - `RateLimiting/TenantRateLimiter.cs` with Redis backend
   - `RateLimiting/RateLimitStore.cs` for distributed state
   - ASP.NET Core rate limiting middleware integration
   - Request size limit middleware
   - Payload validation middleware

3. **REFACTOR Phase**:
   - Optimize Redis operations (pipelining)
   - Add rate limit metrics
   - Tune rate limit window size

**Key Components**:
- Redis-backed rate limit store with tenant partition keys
- ASP.NET Core rate limiting middleware
- Request size validation middleware
- Retry-After header generation

**Success Criteria** (from spec.md):
- SC-008: Rate limiting prevents tenant degradation (verified in load tests)
- Rate limit enforcement: 1001st request → HTTP 429

**Red→Green Evidence**: 4 transcript files + load test results

---

### Phase 5: Resilience & Health Monitoring (2-3 days)

**Objective**: Circuit breakers, health aggregation, load balancing (P2 User Stories 5)

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Unit tests for circuit breaker (5 failures → open circuit)
   - Integration tests for circuit breaker with failing service
   - Integration tests for health check aggregation
   - Integration tests for load balancing across instances

2. **GREEN Phase** - Implement to pass tests:
   - `Resilience/CircuitBreakerRegistry.cs` with Polly policies
   - `Health/AggregatedHealthCheck.cs` for service health
   - YARP load balancing configuration
   - Circuit breaker metrics export

3. **REFACTOR Phase**:
   - Tune circuit breaker thresholds
   - Add circuit breaker dashboard
   - Optimize health check concurrency

**Key Components**:
- Polly circuit breakers per downstream service
- Health check aggregation endpoint
- Load balancing with health-aware instance removal
- Circuit breaker state metrics

**Success Criteria** (from spec.md):
- SC-007: Circuit breakers prevent cascading failures
- SC-009: Load balancing within 10% variance
- SC-004: Health checks < 500ms
- SC-016: Health endpoint accurately reflects service status

**Red→Green Evidence**: 4 transcript files

---

### Phase 6: CORS & Web Client Support (1 day)

**Objective**: Enable cross-origin requests from web frontends (P3 User Stories 6)

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Integration tests for preflight OPTIONS requests
   - Integration tests for CORS header validation
   - Integration tests for credential handling

2. **GREEN Phase** - Implement to pass tests:
   - CORS policy configuration
   - CORS middleware registration
   - Allowed origins/methods/headers configuration

3. **REFACTOR Phase**:
   - Extract CORS configuration to appsettings
   - Add per-environment CORS policies

**Key Components**:
- `Configuration/CorsConfiguration.cs`
- CORS middleware with policy builder
- Per-environment origin allowlists

**Success Criteria** (from spec.md):
- SC-013: CORS policies prevent unauthorized origins (verified via security testing)
- Preflight requests return correct CORS headers

**Red→Green Evidence**: 4 transcript files

---

### Phase 7: Observability & Production Readiness (1-2 days)

**Objective**: Complete monitoring, logging, tracing for production deployment

**Test-Driven Development**:
1. **RED Phase** - Write failing tests:
   - Integration tests for OpenTelemetry trace export
   - Integration tests for structured logging
   - Load tests for performance targets

2. **GREEN Phase** - Implement to pass tests:
   - OpenTelemetry exporter configuration
   - Structured logging enrichment
   - Prometheus metrics export
   - Performance tuning

3. **REFACTOR Phase**:
   - Optimize hot paths
   - Reduce allocations
   - Add performance benchmarks

**Key Components**:
- OpenTelemetry traces, metrics, logs
- Prometheus metrics endpoint
- Grafana dashboard configuration
- Performance benchmarks

**Success Criteria** (from spec.md):
- SC-001: Auth validation < 20ms P95
- SC-002: Routing overhead < 50ms P95
- SC-005: Circuit breaker decision < 1ms
- SC-006: 99.9% uptime

**Red→Green Evidence**: 4 transcript files + performance benchmark results

---

## Test Strategy

### Unit Tests (≥80% coverage)

**Scope**: Individual middleware, transforms, rate limiters, circuit breakers

**Tools**: xUnit, Moq, FluentAssertions

**Examples**:
- `JwtValidationMiddlewareTests`: Token validation logic, claims extraction, error handling
- `TenantRateLimiterTests`: Rate limit algorithm, Redis interactions (mocked)
- `CircuitBreakerRegistryTests`: Circuit state transitions, recovery logic

**Red→Green**: Write failing test → implement → verify pass

---

### Integration Tests (full middleware pipeline)

**Scope**: End-to-end request flows with real dependencies (Redis, mock downstream services)

**Tools**: xUnit, Testcontainers (Redis), WebApplicationFactory, WireMock

**Examples**:
- `RoutingTests`: Request → gateway → downstream service (mocked)
- `AuthenticationTests`: Invalid token → 401, valid token → extracted claims
- `RateLimitingTests`: 1000 requests pass, 1001st rejected with 429

**Red→Green**: Write failing test → implement → verify pass

---

### Aspire Integration Tests (full orchestration)

**Scope**: Full Aspire stack with gateway, Identity Service, downstream services

**Tools**: Aspire.Testing, xUnit

**Examples**:
- `ApiGatewayAspireTests`: Full stack startup, service discovery, health checks

**Red→Green**: Write failing test → configure Aspire → verify pass

---

### Load Tests (performance validation)

**Scope**: Throughput, latency percentiles, concurrency limits

**Tools**: NBomber or k6

**Examples**:
- Baseline: 5000 req/s with <50ms P95 routing overhead
- Auth validation: <20ms P95
- Rate limiting: Verify 429 responses at limit

**Red→Green**: Define target metrics → implement → measure actual metrics

---

## Performance Benchmarks

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Auth validation latency | <20ms P95 | NBomber load test with JWT validation |
| Routing overhead | <50ms P95 | NBomber load test comparing gateway vs direct service |
| End-to-end request | <150ms | Integration test with mocked downstream (1ms response) |
| Health check aggregation | <500ms | Integration test with 10 downstream services |
| Circuit breaker decision | <1ms | Unit test timing circuit breaker check |
| Throughput per instance | >5000 req/s | NBomber load test with sustained load |
| Memory footprint | <512MB | Docker stats during load test |
| Startup time | <10s | Aspire dashboard startup logs |

---

## Deployment Strategy

**Container Image**: `northstaret/api-gateway:latest`

**Aspire Registration** (in `Src/Foundation/AppHost/Program.cs`):
```csharp
var redis = builder.AddRedis("rate-limit-redis");
var identity = builder.AddProject<Projects.IdentityService>("identity");

var gateway = builder.AddProject<Projects.ApiGateway>("gateway")
    .WithReference(redis)
    .WaitFor(redis)
    .WaitFor(identity)
    .WithExternalHttpEndpoints();
```

**Environment Variables**:
- `ASPNETCORE_ENVIRONMENT`: Development | Staging | Production
- `Redis__ConnectionString`: Aspire-injected Redis connection
- `Authentication__Authority`: Identity Service OIDC discovery URL
- `Authentication__Audience`: Gateway API audience claim
- `RateLimiting__DefaultLimit`: Default requests per minute per tenant

**Scaling**: Horizontal scaling with multiple instances (stateless, Redis-backed rate limiting)

**Health Endpoint**: `GET /health` (Kubernetes liveness/readiness probes)

**Monitoring**: OpenTelemetry → Azure Monitor or Prometheus/Grafana

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| YARP performance overhead exceeds target | Medium | High | Benchmark early in Phase 1, optimize transforms, consider native HttpClient if needed |
| Redis latency impacts rate limiting | Medium | Medium | Use Redis pipelining, implement client-side caching, fallback to local rate limit |
| Circuit breaker false positives | Low | Medium | Tune thresholds based on production traffic, add manual override |
| Token validation latency | Low | Medium | Cache validated tokens (short TTL), optimize JWT parsing |
| Distributed rate limiting skew | Medium | Low | Accept eventual consistency, use fixed time windows, monitor Redis replication lag |
| Legacy monolith incompatibility | Low | High | Test header translation early, implement adapter layer if needed |

---

## References

- **YARP Documentation**: https://microsoft.github.io/reverse-proxy/
- **Microsoft.Identity.Web**: https://github.com/AzureAD/microsoft-identity-web
- **Polly Resilience**: https://github.com/App-vNext/Polly
- **ASP.NET Core Rate Limiting**: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit
- **OpenTelemetry .NET**: https://opentelemetry.io/docs/languages/net/
- **Aspire Orchestration**: https://learn.microsoft.com/en-us/dotnet/aspire/

---

## Next Steps

1. **Review & Approve**: Stakeholder review of this implementation plan
2. **Research Phase**: Execute Phase 0 research, document findings in `research.md`
3. **Generate Tasks**: Run `/speckit.tasks` to generate phased task breakdown
4. **Begin Implementation**: Create implementation branch and start Phase 1 (TDD Red→Green)
