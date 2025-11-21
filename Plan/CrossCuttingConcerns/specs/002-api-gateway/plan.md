# Implementation Plan: API Gateway - YARP Service Orchestration

**Specification Branch**: `CrossCuttingConcerns/002-api-gateway-spec` *(planning artifacts)*  
**Implementation Branch**: `CrossCuttingConcerns/002-api-gateway` *(created after approval)*  
**Date**: November 21, 2025 | **Spec**: [spec.md](./spec.md)

## Summary

The API Gateway provides a unified, secure entry point for all NorthStar LMS clients using YARP (Yet Another Reverse Proxy) as the routing engine. It centralizes cross-cutting concerns including JWT authentication, tenant-scoped rate limiting, circuit breaker patterns, Strangler Fig migration routing, request/response transformation, and health aggregation. The gateway dynamically loads route definitions and resiliency policies from Azure App Configuration (30-second refresh cycle), stores rate-limit and circuit-breaker state in Redis for cluster-wide consistency, and leverages .NET Aspire Service Discovery to automatically resolve backend service endpoints. This architecture decouples clients from the evolving microservice topology while enabling seamless coexistence of legacy NS4.WebAPI and new .NET 10 services during the migration.

## Technical Context

**Language/Version**: C# 12 / .NET 10.0  
**Primary Dependencies**: 
- `Yarp.ReverseProxy` 2.3+ (reverse proxy framework)
- `Aspire.Hosting.Yarp` 9.4+ (Aspire YARP integration with programmatic configuration)
- `Microsoft.Extensions.Configuration.AzureAppConfiguration` 8.0+ (dynamic config refresh)
- `StackExchange.Redis` 2.8+ (distributed rate limiting & circuit breaker state)
- `Microsoft.Identity.Web` 3.5+ (Entra ID JWT validation)
- `AspNetCore.HealthChecks` 9.0+ (health aggregation)
- `Microsoft.AspNetCore.RateLimiting` 10.0+ (ASP.NET Core rate limiting middleware)
- `System.Threading.RateLimiting` 10.0+ (rate limiter algorithms)

**Storage**: 
- **Azure App Configuration**: Route definitions, Strangler Fig feature flags, circuit breaker policies (read-only, 30s refresh)
- **Redis**: Per-tenant rate limit counters (keys: `ratelimit:{tenantId}:{route}`), circuit breaker health metrics (keys: `circuit:{service}:state`)
- **No Persistent Database**: Gateway is stateless; all state in Redis or Azure App Config

**Testing**: 
- **Unit**: xUnit, Moq (YARP middleware transforms, rate limit logic)
- **Integration**: Aspire test project validating gateway routing, auth validation, rate limiting, circuit breaker behavior
- **BDD**: Reqnroll feature files for 12 scenarios in spec.md (JWT validation, Strangler Fig routing, rate limiting, health aggregation)
- **Performance**: k6 load testing scripts (validate <20ms auth overhead, 150ms P95 end-to-end latency)

**Target Platform**: 
- **Local Development**: .NET Aspire AppHost (Docker container via `mcr.microsoft.com/dotnet/nightly/yarp` image)
- **Production**: Azure Container Apps (auto-scale, ingress routing)
- **Service Discovery**: Aspire ServiceDefaults registry (automatic endpoint resolution)

**Project Type**: ASP.NET Core Web API (minimal APIs + YARP middleware pipeline)

**Performance Goals**: 
- JWT validation: <20ms P95 overhead per request
- Total gateway latency: <150ms P95 (including backend)
- Rate limit check: <5ms P95 (Redis atomic operations)
- Health aggregation: <500ms total response time
- Circuit breaker decision: <1ms (in-memory state check)
- Configuration refresh: Every 30 seconds from Azure App Config (non-blocking)

**Constraints**: 
- **Multi-Replica Consistency**: Rate limiting and circuit breaker state MUST be synchronized across gateway replicas via Redis (no in-memory state)
- **Zero-Downtime Config Updates**: Route changes, Strangler Fig toggles, and policy updates propagate within 30 seconds without service restart
- **Legacy Compatibility**: Gateway MUST transparently route to NS4.WebAPI (ASP.NET Framework 4.8) endpoints during migration
- **Tenant Isolation**: Rate limits enforced per `tenant_id` extracted from JWT; circuit breakers scoped per backend service
- **Correlation ID Propagation**: All requests generate/forward `X-Correlation-Id` for distributed tracing

**Scale/Scope**: 
- **Concurrent Requests**: 10,000 req/s peak (district-wide assessment windows)
- **Backend Services**: 11 new microservices + 1 legacy monolith (NS4.WebAPI)
- **Routes**: ~50 route definitions (versioned APIs: `/api/v1/*`, `/api/v2/*`)
- **Tenants**: 100+ districts with individual rate limit profiles
- **Circuit Breakers**: 12 backend services monitored independently
- **CORS Origins**: 5 frontend domains (`*.northstar.edu`, localhost)

### Identity & Authentication Guidance

**Identity Provider**: Microsoft Entra ID (Azure AD) - Tokens issued by Identity Service (see `001-identity-service-entra-id`)  
**Authentication Pattern**: JWT Bearer token validation at gateway edge; session context NOT maintained by gateway (stateless validation only)  
**Token Validation**: 
- `Microsoft.Identity.Web` validates JWT signature, expiration, issuer (`https://login.microsoftonline.com/{tenantId}/v2.0`), audience (`api://northstar-lms`)
- Extract `tenant_id`, `user_id`, `roles` claims from JWT for downstream injection
- Cache JWT validation results in memory (5-minute sliding expiration) to reduce Entra ID metadata endpoint calls

**Session Storage**: N/A (gateway does not maintain sessions; delegates to Identity Service)  
**Architecture Reference**: See `Plan/CrossCuttingConcerns/specs/001-identity-service-entra-id/` for token issuance flow  
**Key Dependencies**: 
- `Microsoft.Identity.Web` 3.5+
- `StackExchange.Redis` 2.8+ (for optional JWT validation cache - if performance testing shows >10ms P95 overhead)

## Layer Identification (MANDATORY)

**Target Layer**: CrossCuttingConcerns  
**Implementation Path**: `Src/Foundation/services/ApiGateway/`  
**Specification Path**: `Plan/CrossCuttingConcerns/specs/002-api-gateway/`

### Layer Consistency Validation

- [x] Target Layer matches specification (CrossCuttingConcerns per spec.md)
- [x] Implementation path follows layer structure (`Src/Foundation/services/ApiGateway/`)
- [x] Specification path follows layer structure (`Plan/CrossCuttingConcerns/specs/002-api-gateway/`)
- [x] Not a new layer: Aligns with existing Foundation infrastructure architecture

### Shared Infrastructure Dependencies

- [x] **ServiceDefaults** - Aspire orchestration, OpenTelemetry distributed tracing, structured logging, health checks
- [x] **Domain** - NOT APPLICABLE (gateway has no domain logic; purely infrastructure routing)
- [x] **Application** - NOT APPLICABLE (gateway delegates all business logic to backend services)
- [x] **Infrastructure** - Multi-tenancy primitives (tenant extraction utilities), caching abstractions (Redis wrappers), resilience policies (Polly extensions)

### Cross-Layer Dependencies

**Depends on layers**: Foundation shared infrastructure only  
**Specific Dependencies**: 
- `ServiceDefaults` - Aspire service discovery (resolves backend endpoints: `http://identity-api`, `http://student-api`, etc.)
- `Infrastructure.Caching` - Redis client wrappers for rate limiting counters and circuit breaker state
- `Infrastructure.Resilience` - Circuit breaker policy definitions (reused by gateway transforms)

**Justification**: Gateway is a cross-cutting infrastructure service that routes to all Foundation layer microservices. It MUST NOT contain business logic or domain concepts - only routing, authentication, rate limiting, and resilience orchestration. Shared Infrastructure dependencies enable consistent observability, caching patterns, and configuration management across the platform.

**Constitutional Compliance**: See Constitution Principle 6 (Mono-Repo Layer Isolation) - Gateway consumes only approved shared infrastructure; no direct service-to-service dependencies. Backend services remain independently deployable; gateway dynamically discovers endpoints via Aspire Service Discovery.

### Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

#### Principle 1: Clean Architecture & Aspire Orchestration

- [x] **Gateway registered in AppHost**: `builder.AddYarp("api-gateway").WithConfiguration(...)` orchestrates gateway as Aspire resource
- [x] **ServiceDefaults applied**: Gateway references `ServiceDefaults` for OpenTelemetry, health checks, structured logging
- [x] **No layer violations**: Gateway does not reference Domain or Application logic; purely infrastructure routing
- [x] **Integration tests via Aspire**: Test project `tests/integration/ApiGateway.IntegrationTests` validates routing, auth, rate limiting using Aspire hosting

#### Principle 2: Test-Driven Quality Gates

- [x] **BDD via Reqnroll**: 12 `.feature` files map to spec scenarios (JWT validation, Strangler Fig routing, rate limiting, health aggregation)
- [x] **Red → Green workflow**: For each scenario: (1) write failing Reqnroll test, (2) implement gateway configuration, (3) verify test passes
- [x] **Unit tests for transforms**: YARP request/response transform logic (header injection, path rewriting) covered by xUnit tests
- [x] **Performance tests**: k6 load scripts validate <20ms auth overhead, 150ms P95 latency, rate limit accuracy under load
- [x] **Coverage target**: ≥80% line coverage (BDD scenarios + unit tests for transform/middleware logic)

#### Principle 3: UX Traceability & Figma Accountability

- [x] **Not applicable**: Gateway has no UI; purely backend infrastructure service
- [x] **Admin UI (future)**: If gateway admin UI added later, Figma requirement applies per Constitution Principle 3

#### Principle 4: Event-Driven Data Discipline

- [x] **Stateless design**: Gateway does not emit domain events; synchronous request/response only
- [x] **Multi-tenancy enforced**: Extracts `tenant_id` from JWT, injects into `X-Tenant-Id` header for downstream services
- [x] **Rate limiting per tenant**: Redis keys scoped by `tenant_id` to isolate throttling between districts
- [x] **Idempotency NOT applicable**: Gateway delegates idempotency to backend services (Infrastructure shared layer)

#### Principle 5: Security & Compliance Safeguards

- [x] **JWT validation at edge**: Microsoft.Identity.Web validates Entra ID tokens before routing to backends
- [x] **Secrets in platform store**: Azure App Configuration connection string stored in Azure Key Vault (local: User Secrets)
- [x] **No secrets in logs**: Correlation IDs and tenant IDs logged; JWT tokens and Redis keys redacted
- [x] **Rate limiting prevents DoS**: Per-tenant throttling (1000 req/min default) protects backend services
- [x] **CORS policies enforced**: `AllowedOrigins` configured per environment (`*.northstar.edu` prod, `localhost:*` dev)
- [x] **Request size limits**: 10MB max payload (413 response for oversized requests)

#### Principle 6: Mono-Repo Layer Isolation

- [x] **Layer correctly identified**: CrossCuttingConcerns per spec.md and plan.md
- [x] **Shared infrastructure only**: Depends on `ServiceDefaults`, `Infrastructure.Caching`, `Infrastructure.Resilience`
- [x] **No cross-layer violations**: Does not reference DigitalInk or future layers
- [x] **Specification branch pattern**: `CrossCuttingConcerns/002-api-gateway-spec` for planning artifacts
- [x] **Implementation branch pattern**: `CrossCuttingConcerns/002-api-gateway` for code (created after approval)

#### Principle 7: Tool-Assisted Development Workflow

- [x] **Microsoft Docs queried**: YARP configuration patterns, ASP.NET Core rate limiting, JWT validation researched via `#microsoft.docs.mcp`
- [x] **Sequential thinking applied**: Plan generation uses `#think` to structure research → design → implementation phases
- [x] **Code samples retrieved**: `#mcp_microsoftdocs_microsoft_code_sample_search` for YARP transforms, rate limiting middleware examples
- [x] **UI tools NOT applicable**: Gateway has no UI; Figma/Playwright/ChromeDevTools not required

**GATE STATUS**: ✅ **PASSED** - No constitutional violations. Ready for Phase 0 research.

---

## Project Structure

### Documentation (this feature)

```text
Plan/CrossCuttingConcerns/specs/002-api-gateway/
├── spec.md              # Feature specification with 12 scenarios
├── clarify.md           # Clarification decisions (Azure App Config, Entra ID, Redis, Aspire)
├── plan.md              # This file - implementation plan
├── research.md          # Phase 0 output - YARP patterns, rate limiting algorithms, circuit breaker strategies
├── data-model.md        # Phase 1 output - route config schema, rate limit profiles, circuit breaker states
├── quickstart.md        # Phase 1 output - local dev setup, testing guide
└── contracts/           # Phase 1 output - OpenAPI specs for health/metrics endpoints
    ├── health-api.yaml
    └── metrics-api.yaml
```

### Source Code (Foundation Layer)

```text
Src/Foundation/services/ApiGateway/
├── ApiGateway.csproj
├── Program.cs                          # Minimal API host + YARP pipeline setup
├── appsettings.json                    # Base config (local dev defaults)
├── appsettings.Development.json        # Local overrides (User Secrets for Redis/Azure)
├── appsettings.Production.json         # Prod overrides (Azure App Config connection)
├── Middleware/
│   ├── CorrelationIdMiddleware.cs      # Generate/propagate X-Correlation-Id
│   ├── TenantContextMiddleware.cs      # Extract tenant_id from JWT, inject X-Tenant-Id
│   └── ErrorHandlingMiddleware.cs      # Global exception handler (500 responses)
├── Configuration/
│   ├── YarpConfigurationProvider.cs    # Loads routes from Azure App Config
│   ├── RouteConfig.cs                  # Route definition models
│   ├── ClusterConfig.cs                # Backend cluster models
│   └── RateLimitPolicies.cs            # Per-tenant rate limit profiles
├── Transforms/
│   ├── HeaderInjectionTransform.cs     # Inject X-Tenant-Id, X-User-Id, X-Correlation-Id
│   ├── PathRewriteTransform.cs         # Strangler Fig path rewrites (legacy routes)
│   └── ResponseHeaderTransform.cs      # Add deprecation headers (Sunset, X-Api-Version)
├── RateLimiting/
│   ├── RedisTenantRateLimiter.cs       # Redis-backed per-tenant rate limiting
│   └── RateLimitOptions.cs             # Rate limit configuration models
├── Resilience/
│   ├── RedisCircuitBreakerStore.cs     # Circuit breaker state in Redis
│   └── CircuitBreakerOptions.cs        # Circuit breaker policy configuration
└── Health/
    ├── GatewayHealthCheck.cs           # Gateway self-health check
    ├── BackendHealthAggregator.cs      # Aggregates /health from all backends
    └── HealthResponseWriter.cs         # JSON health response formatter

Src/Foundation/AppHost/
└── Program.cs                          # Register API Gateway in Aspire orchestration

tests/unit/ApiGateway.UnitTests/
├── Middleware/
│   ├── CorrelationIdMiddlewareTests.cs
│   └── TenantContextMiddlewareTests.cs
├── Transforms/
│   ├── HeaderInjectionTransformTests.cs
│   └── PathRewriteTransformTests.cs
└── RateLimiting/
    └── RedisTenantRateLimiterTests.cs

tests/integration/ApiGateway.IntegrationTests/
├── Scenarios/
│   ├── AuthenticationValidationTests.cs   # Scenario 3 - JWT validation
│   ├── StranglerFigRoutingTests.cs        # Scenarios 1, 2 - New/legacy routing
│   ├── RateLimitingTests.cs               # Scenario 4 - Per-tenant throttling
│   ├── CircuitBreakerTests.cs             # Scenario 8 - Failing service isolation
│   └── HealthCheckTests.cs                # Scenario 7 - Health aggregation
└── ApiGatewayTestHost.cs                  # Aspire test host setup

tests/bdd/ApiGateway.Specs/
├── Features/
│   ├── AuthenticationValidation.feature    # Scenario 3
│   ├── StranglerFigRouting.feature         # Scenarios 1, 2
│   ├── RateLimiting.feature                # Scenario 4
│   ├── CORS.feature                        # Scenario 5
│   ├── CorrelationIds.feature              # Scenario 6
│   ├── HealthChecks.feature                # Scenario 7
│   ├── CircuitBreakers.feature             # Scenario 8
│   ├── RequestTransformation.feature       # Scenario 9
│   ├── ApiVersioning.feature               # Scenario 10
│   ├── LoadBalancing.feature               # Scenario 11
│   └── RequestValidation.feature           # Scenario 12
└── StepDefinitions/
    └── [Corresponding step definition classes]

tests/performance/
├── scenarios/
│   ├── authentication-overhead.js          # k6 script - validate <20ms auth P95
│   ├── end-to-end-latency.js               # k6 script - validate <150ms P95 total
│   └── rate-limiting-accuracy.js           # k6 script - validate 429 at limit
└── README.md                               # k6 setup and execution guide
```

**Structure Decision**: API Gateway follows standard ASP.NET Core Web API structure with YARP-specific subdirectories (`Transforms/`, `Configuration/`, `RateLimiting/`, `Resilience/`). Middleware pipeline handles cross-cutting concerns (correlation IDs, tenant context, error handling). Integration tests use Aspire hosting to validate full routing stack. BDD features map 1:1 to spec scenarios for traceability. Performance tests use k6 (industry-standard load testing tool) to validate SLO compliance.

---

## Phase 0: Research & Clarification Resolution

**Goal**: Resolve all NEEDS CLARIFICATION items from Technical Context by researching best practices and patterns.

### Research Tasks

#### R1: YARP Configuration & Transform Patterns
- **Question**: How to programmatically configure YARP routes and transforms in Aspire 9.4+ (WithConfigFile removed)?
- **Sources**: 
  - Microsoft Docs: [Aspire YARP Integration](https://learn.microsoft.com/en-us/dotnet/aspire/proxies/yarp-integration)
  - Microsoft Docs: [YARP Transforms](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/extensibility-transforms)
- **Deliverable**: Document in `research.md`:
  - `.WithConfiguration(yarp => yarp.AddRoute(...))` fluent API patterns
  - Header injection transforms (`WithTransformRequestHeader`)
  - Path rewriting transforms (`WithTransformPathRemovePrefix`, `WithTransformPathPrefix`)
  - Response transforms for deprecation headers

#### R2: ASP.NET Core Rate Limiting Middleware
- **Question**: How to implement Redis-backed per-tenant rate limiting with ASP.NET Core's built-in `RateLimiter` APIs?
- **Sources**: 
  - Microsoft Docs: [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
  - Microsoft Docs: [Rate Limiting with YARP](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/rate-limiting)
- **Deliverable**: Document in `research.md`:
  - `PartitionedRateLimiter.Create<HttpContext, string>` pattern for tenant partitioning
  - Redis integration via custom `RateLimiter` implementation
  - `OnRejected` callback for 429 responses with `Retry-After` header
  - Token bucket vs. sliding window algorithm comparison (choose token bucket for burst tolerance)

#### R3: Circuit Breaker Integration with YARP
- **Question**: How to implement circuit breakers for YARP backend clusters using Polly resiliency policies?
- **Sources**: 
  - Microsoft Docs: [YARP HTTP Client Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/http-client-config)
  - Microsoft Docs: [Polly Circuit Breaker](https://www.pollydocs.org/strategies/circuit-breaker)
- **Deliverable**: Document in `research.md`:
  - Polly circuit breaker policy configuration (5 failures in 10 seconds → open for 30 seconds)
  - Redis-backed circuit breaker state store (distributed across gateway replicas)
  - YARP cluster health checks integration
  - Fallback responses (503 Service Unavailable with `Retry-After`)

#### R4: Azure App Configuration Dynamic Refresh
- **Question**: How to reload YARP routes and policies from Azure App Configuration every 30 seconds without service restart?
- **Sources**: 
  - Microsoft Docs: [Azure App Configuration with .NET](https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-aspnet-core-app)
  - Microsoft Docs: [Dynamic Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core)
- **Deliverable**: Document in `research.md`:
  - `AddAzureAppConfiguration(options => options.ConfigureRefresh(...))` pattern
  - Sentinel key-based refresh (`Gateway:ConfigVersion` sentinel triggers full reload)
  - YARP route reconfiguration on `IOptionsSnapshot<YarpConfig>` change
  - Zero-downtime updates (new requests use updated routes, in-flight requests complete on old routes)

#### R5: Health Check Aggregation Patterns
- **Question**: How to aggregate health checks from 11+ backend services and return structured JSON responses?
- **Sources**: 
  - Microsoft Docs: [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
  - Microsoft Docs: [Health Checks with Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks)
- **Deliverable**: Document in `research.md`:
  - `MapHealthChecks("/health", new HealthCheckOptions {...})` configuration
  - `IHealthCheck` implementation for each backend service (HTTP GET /health with 5s timeout)
  - JSON response format: `{ "status": "Healthy", "services": { "identity-api": "Healthy", ... } }`
  - 503 response if ANY service unhealthy (fail-fast for load balancers)

#### R6: JWT Validation Performance Optimization
- **Question**: How to minimize JWT validation overhead to <20ms P95?
- **Sources**: 
  - Microsoft Docs: [Microsoft.Identity.Web JWT Validation](https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-api-call-api-app-configuration)
  - Microsoft Docs: [ASP.NET Core Authentication Middleware](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- **Deliverable**: Document in `research.md`:
  - `JwtBearerOptions.TokenValidationParameters` tuning (disable issuer metadata refresh if < 1 hour)
  - Redis caching of validated JWT claims (5-minute sliding expiration)
  - `ValidateIssuer = true`, `ValidateAudience = true`, `ValidateLifetime = true` (no shortcuts)
  - Background refresh of OIDC metadata (OpenID Connect discovery endpoint cached 1 hour)

**Research Deliverable**: `research.md` document with decisions, rationale, and code examples for each research task.

---

## Phase 1: Design & Contracts

**Prerequisites**: `research.md` complete

### Data Model Design

**Goal**: Define schemas for route configuration, rate limit profiles, circuit breaker states, and health check responses.

#### D1: Route Configuration Schema (Azure App Configuration)
```json
{
  "Gateway": {
    "Routes": [
      {
        "RouteId": "student-api-v1",
        "Match": {
          "Path": "/api/v1/students/{**catch-all}",
          "Methods": ["GET", "POST", "PUT", "DELETE"]
        },
        "ClusterId": "student-api-cluster",
        "Order": 1,
        "Transforms": [
          { "RequestHeaderRemove": "X-Original-Host" },
          { "RequestHeader": { "Name": "X-Forwarded-Host", "Value": "gateway.northstar.edu" } }
        ],
        "RateLimitPolicy": "default-tenant-limit",
        "AuthorizationPolicy": "require-jwt"
      },
      {
        "RouteId": "legacy-assessment-api",
        "Match": {
          "Path": "/api/v1/assessments/{**catch-all}"
        },
        "ClusterId": "legacy-ns4-cluster",
        "Order": 2,
        "Transforms": [
          { "PathPrefix": "/NS4.WebAPI" },
          { "RequestHeader": { "Name": "X-Legacy-Request", "Value": "true" } }
        ],
        "RateLimitPolicy": "legacy-rate-limit",
        "FeatureFlag": "StranglerFig_AssessmentService_UseLegacy"
      }
    ],
    "Clusters": {
      "student-api-cluster": {
        "Destinations": {
          "student-api": {
            "Address": "http://student-api"
          }
        },
        "LoadBalancingPolicy": "RoundRobin",
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:30",
            "Timeout": "00:00:05",
            "Path": "/health"
          }
        },
        "CircuitBreaker": {
          "FailureThreshold": 5,
          "SamplingDuration": "00:00:10",
          "MinimumThroughput": 3,
          "BreakDuration": "00:00:30"
        }
      },
      "legacy-ns4-cluster": {
        "Destinations": {
          "legacy-ns4": {
            "Address": "http://oldnorthstar.internal"
          }
        }
      }
    }
  }
}
```

#### D2: Rate Limit Profiles
```json
{
  "RateLimiting": {
    "Policies": {
      "default-tenant-limit": {
        "Algorithm": "TokenBucket",
        "TokenLimit": 1000,
        "TokensPerPeriod": 100,
        "ReplenishmentPeriod": "00:00:06",
        "QueueLimit": 10
      },
      "legacy-rate-limit": {
        "Algorithm": "SlidingWindow",
        "PermitLimit": 500,
        "Window": "00:01:00",
        "SegmentsPerWindow": 6
      }
    }
  }
}
```

#### D3: Circuit Breaker State Model (Redis)
```csharp
public class CircuitBreakerState
{
    public string ServiceId { get; set; }
    public CircuitState State { get; set; } // Closed, Open, HalfOpen
    public int FailureCount { get; set; }
    public DateTime LastFailureTime { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? LastSuccessTime { get; set; }
}

public enum CircuitState
{
    Closed,   // Service healthy, requests pass through
    Open,     // Service failing, requests fail-fast with 503
    HalfOpen  // Testing recovery, limited requests allowed
}
```

**Data Model Deliverable**: `data-model.md` with complete schemas, validation rules, and Redis key patterns (`ratelimit:{tenantId}:{routeId}`, `circuit:{serviceId}:state`).

### API Contracts

**Goal**: Define OpenAPI specs for gateway management endpoints (health, metrics, configuration status).

#### C1: Health Check Endpoint
```yaml
# contracts/health-api.yaml
openapi: 3.0.3
info:
  title: API Gateway Health
  version: 1.0.0
paths:
  /health:
    get:
      summary: Aggregate health check for gateway and all backend services
      responses:
        '200':
          description: All services healthy
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/HealthResponse'
        '503':
          description: One or more services unhealthy
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/HealthResponse'
components:
  schemas:
    HealthResponse:
      type: object
      properties:
        status:
          type: string
          enum: [Healthy, Degraded, Unhealthy]
        totalDuration:
          type: string
          format: duration
        services:
          type: object
          additionalProperties:
            $ref: '#/components/schemas/ServiceHealth'
    ServiceHealth:
      type: object
      properties:
        status:
          type: string
          enum: [Healthy, Degraded, Unhealthy]
        description:
          type: string
        duration:
          type: string
          format: duration
        exception:
          type: string
          nullable: true
```

#### C2: Metrics Endpoint (Prometheus Format)
```yaml
# contracts/metrics-api.yaml
openapi: 3.0.3
info:
  title: API Gateway Metrics
  version: 1.0.0
paths:
  /metrics:
    get:
      summary: Prometheus-format metrics for gateway observability
      responses:
        '200':
          description: Metrics in Prometheus exposition format
          content:
            text/plain:
              schema:
                type: string
              example: |
                # HELP gateway_requests_total Total requests processed
                # TYPE gateway_requests_total counter
                gateway_requests_total{route="student-api-v1",status="200"} 15234
                
                # HELP gateway_request_duration_seconds Request duration histogram
                # TYPE gateway_request_duration_seconds histogram
                gateway_request_duration_seconds_bucket{route="student-api-v1",le="0.02"} 14532
                gateway_request_duration_seconds_bucket{route="student-api-v1",le="0.05"} 15120
                gateway_request_duration_seconds_bucket{route="student-api-v1",le="0.15"} 15234
                
                # HELP gateway_rate_limit_exceeded_total Rate limit exceeded count
                # TYPE gateway_rate_limit_exceeded_total counter
                gateway_rate_limit_exceeded_total{tenant_id="district-001"} 42
                
                # HELP gateway_circuit_breaker_state Circuit breaker state (0=Closed, 1=Open, 2=HalfOpen)
                # TYPE gateway_circuit_breaker_state gauge
                gateway_circuit_breaker_state{service="student-api"} 0
```

**Contracts Deliverable**: `contracts/` directory with OpenAPI YAML files for `/health` and `/metrics` endpoints.

### Agent Context Update

**Goal**: Update `.github/copilot-instructions.md` with API Gateway-specific patterns and technologies.

```bash
# Run agent context update script (detects Copilot agent automatically)
./specify/scripts/bash/update-agent-context.sh copilot
```

**Manual Additions** (preserved between script runs):
```markdown
## API Gateway Technologies (Feature 002)
- YARP 2.3+ for reverse proxy routing with programmatic configuration
- ASP.NET Core Rate Limiting middleware with Redis-backed per-tenant partitioning
- Polly circuit breakers with distributed state in Redis
- Azure App Configuration for dynamic route/policy updates (30s refresh)
- Microsoft.Identity.Web for Entra ID JWT validation at gateway edge
- Aspire Service Discovery for automatic backend endpoint resolution
- Health check aggregation across 11+ microservices (<500ms SLO)
```

**Deliverable**: Updated agent context file with Gateway-specific guidance (script handles merge automatically).

### Quickstart Guide

**Goal**: Document local development setup, testing workflows, and troubleshooting steps.

**Deliverable**: `quickstart.md` with:
1. Prerequisites (Docker, .NET 10 SDK, Aspire workload, Azure CLI)
2. Local setup steps (User Secrets for Redis/Azure App Config)
3. Running the gateway (`dotnet run --project Src/Foundation/AppHost`)
4. Testing routes (`curl http://localhost:7000/api/v1/students` → routes to Student API)
5. Verifying rate limiting (`k6 run tests/performance/rate-limiting-accuracy.js`)
6. Debugging with Aspire Dashboard (`http://localhost:15000` - view traces, logs, metrics)
7. Common issues (JWT validation failures, Redis connection errors, Aspire service discovery delays)

---

## Phase 2: Implementation Tasks (Generated by `/speckit.tasks`)

**NOTE**: Detailed phased tasks will be generated by the `/speckit.tasks` command. This plan defines the overall structure and design to inform task generation.

**Expected Task Phases** (to be detailed in `tasks.md`):

### Phase 2.1: Foundation Setup
- Initialize `ApiGateway.csproj` with YARP, Aspire, Microsoft.Identity.Web packages
- Configure `Program.cs` with minimal API host + YARP middleware pipeline
- Register gateway in `AppHost/Program.cs` with service discovery references
- Setup User Secrets for local Redis/Azure App Config credentials

### Phase 2.2: JWT Authentication & Tenant Context
- Implement `TenantContextMiddleware` (extract `tenant_id` from JWT)
- Configure `Microsoft.Identity.Web` JWT validation (Entra ID issuer/audience)
- Implement `CorrelationIdMiddleware` (generate/propagate `X-Correlation-Id`)
- Write BDD feature: `AuthenticationValidation.feature` (Scenario 3)

### Phase 2.3: YARP Configuration & Transforms
- Implement `YarpConfigurationProvider` (load routes from Azure App Config)
- Implement `HeaderInjectionTransform` (`X-Tenant-Id`, `X-User-Id`, `X-Correlation-Id`)
- Implement `PathRewriteTransform` (Strangler Fig legacy route rewrites)
- Write BDD features: `StranglerFigRouting.feature` (Scenarios 1, 2)

### Phase 2.4: Rate Limiting
- Implement `RedisTenantRateLimiter` (per-tenant token bucket algorithm)
- Configure ASP.NET Core rate limiting middleware with Redis backend
- Implement 429 response with `Retry-After` header
- Write BDD feature: `RateLimiting.feature` (Scenario 4)

### Phase 2.5: Circuit Breakers
- Implement `RedisCircuitBreakerStore` (distributed state across replicas)
- Configure Polly circuit breaker policies for YARP clusters
- Implement 503 fallback responses with `Retry-After`
- Write BDD feature: `CircuitBreakers.feature` (Scenario 8)

### Phase 2.6: Health Checks & Observability
- Implement `BackendHealthAggregator` (parallel health checks with 5s timeout)
- Configure `/health` endpoint with JSON response writer
- Implement Prometheus `/metrics` endpoint (request counts, latency histograms)
- Write BDD feature: `HealthChecks.feature` (Scenario 7)

### Phase 2.7: Integration & Performance Testing
- Build Aspire integration test host (`ApiGatewayTestHost.cs`)
- Run all BDD scenarios via Reqnroll (12 features)
- Execute k6 performance tests (auth overhead, latency, rate limiting accuracy)
- Validate coverage ≥80% (dotnet test with coverage collection)

---

## Complexity Tracking

**No constitutional violations** - Gateway follows standard ASP.NET Core + YARP patterns with shared infrastructure dependencies.

| Concern | Decision | Rationale |
|---------|----------|-----------|
| Multiple state stores (Redis + Azure App Config) | Justified | Redis for high-frequency mutable state (rate limits, circuit breakers); Azure App Config for low-frequency immutable config (routes, policies) - separation prevents config refresh from impacting rate limit performance |
| Custom rate limiter implementation | Justified | ASP.NET Core built-in limiters lack Redis backend for multi-replica consistency; custom `RedisTenantRateLimiter` enables tenant-scoped throttling across gateway instances |
| Strangler Fig routing complexity | Justified | Required for zero-downtime migration from NS4.WebAPI to new microservices; feature flags in Azure App Config enable gradual traffic shift per service |

---

## Next Steps

1. **Review & Approve Plan**: Architecture Review validates design against constitution and Foundation standards
2. **Execute Phase 0 Research**: Generate `research.md` with YARP patterns, rate limiting algorithms, circuit breaker strategies
3. **Execute Phase 1 Design**: Generate `data-model.md`, `contracts/`, `quickstart.md`, update agent context
4. **Generate Tasks**: Run `/speckit.tasks` to create phased implementation task breakdown in `tasks.md`
5. **Create Implementation Branch**: `CrossCuttingConcerns/002-api-gateway` branch from approved spec branch
6. **Begin TDD Workflow**: Write failing Reqnroll features → Implement gateway → Verify tests pass → Commit with Red/Green evidence

---

**Plan Status**: ✅ **READY FOR REVIEW** - Constitution checks passed, research scope defined, design phase structured, implementation path documented.
