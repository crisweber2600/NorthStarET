---
description: "Task breakdown for API Gateway YARP Service Orchestration"
---

# Tasks: API Gateway - YARP Service Orchestration

**Specification Branch**: `CrossCuttingConcerns/002-api-gateway-spec` *(planning artifacts)*  
**Implementation Branch**: `CrossCuttingConcerns/002-api-gateway` *(created when starting implementation)*

**Input**: Design documents from `Plan/CrossCuttingConcerns/specs/002-api-gateway/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: CrossCuttingConcerns  
**Implementation Path**: `Src/Foundation/services/ApiGateway/`  
**Specification Path**: `Plan/CrossCuttingConcerns/specs/002-api-gateway/`

### Layer Consistency Checklist

- [x] Target Layer matches spec.md Layer Identification (CrossCuttingConcerns)
- [x] Target Layer matches plan.md Layer Identification (CrossCuttingConcerns)
- [x] Implementation path follows layer structure (`Src/Foundation/services/ApiGateway/`)
- [x] Specification path follows layer structure (`Plan/CrossCuttingConcerns/specs/002-api-gateway/`)
- [x] Shared infrastructure dependencies match between spec and plan
- [x] Cross-layer dependencies justified: Gateway routes to all Foundation services via Aspire Service Discovery

---

## Layer Compliance Validation

*MANDATORY: Include these validation tasks to ensure mono-repo layer isolation (Constitution Principle 6)*

- [ ] T001 Verify project references ONLY shared infrastructure (ServiceDefaults, Infrastructure.Caching, Infrastructure.Resilience)
- [ ] T002 Verify NO direct service-to-service references (gateway uses Aspire Service Discovery for dynamic routing)
- [ ] T003 Verify AppHost orchestration includes ApiGateway with correct service references
- [ ] T004 Verify README.md documents layer position (CrossCuttingConcerns) and shared infrastructure dependencies
- [ ] T005 Verify no circular dependencies between layers

---

## Identity & Authentication Compliance

*MANDATORY: API Gateway validates authentication at the edge*

- [ ] T006 Verify Microsoft.Identity.Web used for JWT token validation (Entra ID issuer/audience)
- [ ] T007 Verify NO token issuance (gateway validates only, Identity Service issues tokens)
- [ ] T008 Verify TenantContextMiddleware extracts tenant_id from JWT claims
- [ ] T009 Verify correlation ID generation/propagation for distributed tracing
- [ ] T010 Verify authentication flow aligns with spec.md Scenario 3 (JWT validation <20ms P95)

---

## Phase 1: Setup (Project Foundation)

**Purpose**: Initialize API Gateway project structure and Aspire orchestration

- [ ] T011 Create ApiGateway.csproj with YARP 2.3.0, Aspire.Hosting.Yarp 9.4.0, Microsoft.Identity.Web 3.5.0 dependencies
- [ ] T012 [P] Initialize Program.cs with minimal API host and YARP middleware pipeline setup
- [ ] T013 [P] Create appsettings.json with base configuration (Redis connection strings, JWT issuer/audience placeholders)
- [ ] T014 [P] Create appsettings.Development.json for local dev overrides (User Secrets references)
- [ ] T015 [P] Create appsettings.Production.json for Azure Container Apps overrides
- [ ] T016 Register API Gateway in Src/Foundation/AppHost/Program.cs with Aspire service discovery
- [ ] T017 [P] Setup User Secrets for Redis connection string and Azure App Configuration endpoint
- [ ] T018 [P] Create .gitignore entries for User Secrets and local dev credentials
- [ ] T019 Create README.md documenting gateway architecture, local dev setup, and testing approach

**Checkpoint**: Project structure ready; AppHost can orchestrate gateway (no routes yet)

---

## Phase 2: Foundational (Core Infrastructure - BLOCKS all scenarios)

**Purpose**: Core middleware, authentication, and observability infrastructure that ALL scenarios depend on

**⚠️ CRITICAL**: No scenario implementation can begin until this phase is complete

- [ ] T020 Implement CorrelationIdMiddleware in Middleware/CorrelationIdMiddleware.cs (generate X-Correlation-Id if missing)
- [ ] T021 Implement TenantContextMiddleware in Middleware/TenantContextMiddleware.cs (extract tenant_id from JWT claims)
- [ ] T022 Implement ErrorHandlingMiddleware in Middleware/ErrorHandlingMiddleware.cs (global exception handler with structured logging)
- [ ] T023 Configure Microsoft.Identity.Web JWT validation in Program.cs (Entra ID issuer, audience api://northstar-lms)
- [ ] T024 [P] Create middleware registration extension methods in Extensions/MiddlewareExtensions.cs
- [ ] T025 [P] Configure structured logging with OpenTelemetry integration (ServiceDefaults)
- [ ] T026 [P] Configure health check aggregation endpoint in Health/GatewayHealthCheck.cs
- [ ] T027 Integrate ServiceDefaults for OpenTelemetry distributed tracing and metrics
- [ ] T028 Create unit tests for CorrelationIdMiddleware in tests/unit/ApiGateway.UnitTests/Middleware/
- [ ] T029 [P] Create unit tests for TenantContextMiddleware in tests/unit/ApiGateway.UnitTests/Middleware/
- [ ] T030 [P] Create unit tests for ErrorHandlingMiddleware in tests/unit/ApiGateway.UnitTests/Middleware/

**Checkpoint**: Core infrastructure ready - all requests have correlation IDs, tenant context, error handling, and observability

---

## Phase 3: YARP Configuration & Service Routing (Scenarios 1, 2, 10)

**Goal**: Implement dynamic routing to new microservices and legacy monolith with Strangler Fig pattern

**Scenarios Covered**:
- Scenario 1: Route Request to New Microservice
- Scenario 2: Route Request to Legacy Monolith During Migration
- Scenario 10: API Versioning Support

**Independent Test**: Gateway routes /api/v1/students to Student Service and /api/v1/assessments to legacy NS4.WebAPI

### YARP Configuration

- [ ] T031 Implement YarpConfigurationProvider in Configuration/YarpConfigurationProvider.cs (load routes from Azure App Config)
- [ ] T032 [P] Create RouteConfig model in Configuration/RouteConfig.cs (route definition schema)
- [ ] T033 [P] Create ClusterConfig model in Configuration/ClusterConfig.cs (backend cluster definitions)
- [ ] T034 Configure YARP routes in Src/Foundation/AppHost/Program.cs using fluent API (.AddRoute, .WithConfiguration)
- [ ] T035 [P] Implement route for Student API (/api/v1/students → http://student-api) in Src/Foundation/AppHost/Program.cs
- [ ] T036 [P] Implement route for Identity API (/api/v1/auth → http://identity-api) in Src/Foundation/AppHost/Program.cs
- [ ] T037 [P] Implement route for legacy NS4.WebAPI (/api/v1/assessments → legacy-ns4) in Src/Foundation/AppHost/Program.cs
- [ ] T038 Configure API versioning routes (/api/v1/*, /api/v2/*) with version-specific clusters in Src/Foundation/AppHost/Program.cs

### Request/Response Transforms

- [ ] T039 Implement HeaderInjectionTransform in Transforms/HeaderInjectionTransform.cs (inject X-Tenant-Id, X-User-Id, X-Correlation-Id)
- [ ] T040 [P] Implement PathRewriteTransform in Transforms/PathRewriteTransform.cs (Strangler Fig legacy path rewrites)
- [ ] T041 [P] Implement ResponseHeaderTransform in Transforms/ResponseHeaderTransform.cs (add Sunset, X-Api-Version headers)
- [ ] T042 Register transforms in YARP route configuration (WithTransformRequestHeader, WithTransformPathPrefix)

### Tests

- [ ] T043 Create unit tests for HeaderInjectionTransform in tests/unit/ApiGateway.UnitTests/Transforms/
- [ ] T044 [P] Create unit tests for PathRewriteTransform in tests/unit/ApiGateway.UnitTests/Transforms/
- [ ] T045 [P] Create unit tests for ResponseHeaderTransform in tests/unit/ApiGateway.UnitTests/Transforms/
- [ ] T046 Create BDD feature file for StranglerFigRouting in tests/bdd/ApiGateway.Specs/Features/StranglerFigRouting.feature
- [ ] T047 Implement step definitions for StranglerFigRouting.feature in tests/bdd/ApiGateway.Specs/StepDefinitions/
- [ ] T048 Create BDD feature file for ApiVersioning in tests/bdd/ApiGateway.Specs/Features/ApiVersioning.feature
- [ ] T049 Implement step definitions for ApiVersioning.feature in tests/bdd/ApiGateway.Specs/StepDefinitions/
- [ ] T050 Create integration tests for route resolution in tests/integration/ApiGateway.IntegrationTests/Scenarios/RoutingTests.cs
- [ ] T051 Verify BDD tests FAIL before implementation (Red phase - capture output)
- [ ] T052 Verify BDD tests PASS after implementation (Green phase - capture output)

**Checkpoint**: Gateway successfully routes to new microservices and legacy monolith; API versioning functional; transforms inject context headers

---

## Phase 4: JWT Authentication Validation (Scenario 3)

**Goal**: Validate JWT tokens at gateway edge with <20ms P95 overhead

**Scenario Covered**:
- Scenario 3: Authentication Validation at Gateway

**Independent Test**: Requests with valid JWT pass through; invalid tokens return 401; validation completes <20ms P95

### Authentication Implementation

- [ ] T053 Configure JwtBearerOptions in Program.cs (Entra ID issuer https://login.microsoftonline.com/{tenantId}/v2.0)
- [ ] T054 Configure JWT audience validation (api://northstar-lms)
- [ ] T055 Implement JWT validation caching in Redis (5-minute sliding expiration) for metadata
- [ ] T056 Configure authorization policies (require authenticated user for all routes except /health, /metrics)
- [ ] T057 Implement 401 response handler with structured error messages

### Tests

- [ ] T058 Create BDD feature file for AuthenticationValidation in tests/bdd/ApiGateway.Specs/Features/AuthenticationValidation.feature
- [ ] T059 Implement step definitions for AuthenticationValidation.feature (valid token, expired token, invalid signature)
- [ ] T060 Create integration tests for JWT validation edge cases in tests/integration/ApiGateway.IntegrationTests/Scenarios/AuthenticationValidationTests.cs (token refresh, concurrent requests, cache invalidation)
- [ ] T061 Create performance test for auth overhead in tests/performance/scenarios/authentication-overhead.js (k6 script)
- [ ] T062 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T063 Verify BDD tests PASS after implementation (Green phase)
- [ ] T064 Run k6 performance test to validate <20ms P95 auth overhead

**Checkpoint**: JWT authentication functional; <20ms P95 overhead validated; 401 responses for invalid tokens

---

## Phase 5: Per-Tenant Rate Limiting (Scenario 4)

**Goal**: Implement Redis-backed rate limiting with per-tenant isolation

**Scenario Covered**:
- Scenario 4: Rate Limiting by Tenant

**Independent Test**: Tenant A limited to 1000 req/min; 1001st request returns 429; Tenant B unaffected

### Rate Limiting Implementation

- [ ] T065 Create RateLimitOptions model in RateLimiting/RateLimitOptions.cs (token bucket, sliding window configs)
- [ ] T066 Implement RedisTenantRateLimiter in RateLimiting/RedisTenantRateLimiter.cs (PartitionedRateLimiter by tenant_id)
- [ ] T067 Configure Redis connection for rate limit counters (keys: ratelimit:{tenantId}:{route})
- [ ] T068 Configure ASP.NET Core rate limiting middleware in Program.cs (OnRejected callback for 429 responses)
- [ ] T069 Implement Retry-After header calculation in rate limit rejection handler
- [ ] T070 Configure per-tenant rate limit profiles (default 1000 req/min, configurable per district)

### Tests

- [ ] T071 Create unit tests for RedisTenantRateLimiter in tests/unit/ApiGateway.UnitTests/RateLimiting/
- [ ] T072 Create BDD feature file for RateLimiting in tests/bdd/ApiGateway.Specs/Features/RateLimiting.feature
- [ ] T073 Implement step definitions for RateLimiting.feature (tenant isolation, 429 responses, Retry-After header)
- [ ] T074 Create integration tests for rate limiting in tests/integration/ApiGateway.IntegrationTests/Scenarios/RateLimitingTests.cs
- [ ] T075 Create performance test for rate limit accuracy in tests/performance/scenarios/rate-limiting-accuracy.js
- [ ] T076 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T077 Verify BDD tests PASS after implementation (Green phase)
- [ ] T078 Run k6 performance test to validate rate limit enforcement accuracy

**Checkpoint**: Per-tenant rate limiting functional; 429 responses with Retry-After; Redis-backed counters prevent cross-replica inconsistencies

---

## Phase 6: Circuit Breaker Resilience (Scenario 8)

**Goal**: Implement circuit breakers for failing backend services with distributed state

**Scenario Covered**:
- Scenario 8: Circuit Breaker for Failing Service

**Independent Test**: 5 consecutive failures open circuit; subsequent requests fail-fast with 503; recovery after 30s

### Circuit Breaker Implementation

- [ ] T079 Create CircuitBreakerOptions model in Resilience/CircuitBreakerOptions.cs (failure threshold, break duration)
- [ ] T080 Implement RedisCircuitBreakerStore in Resilience/RedisCircuitBreakerStore.cs (distributed state across replicas)
- [ ] T081 Configure Polly circuit breaker policies for YARP clusters (5 failures in 10 seconds → open for 30 seconds)
- [ ] T082 Configure circuit breaker state storage in Redis (keys: circuit:{serviceId}:state)
- [ ] T083 Implement 503 fallback response handler with Retry-After header
- [ ] T084 Configure active health checks for backend services (30-second interval, 5-second timeout)
- [ ] T085 Implement circuit breaker recovery logic (half-open state testing)

### Tests

- [ ] T086 Create unit tests for RedisCircuitBreakerStore in tests/unit/ApiGateway.UnitTests/Resilience/
- [ ] T087 Create BDD feature file for CircuitBreakers in tests/bdd/ApiGateway.Specs/Features/CircuitBreakers.feature
- [ ] T088 Implement step definitions for CircuitBreakers.feature (failure threshold, recovery, 503 responses)
- [ ] T089 Create integration tests for circuit breakers in tests/integration/ApiGateway.IntegrationTests/Scenarios/CircuitBreakerTests.cs
- [ ] T090 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T091 Verify BDD tests PASS after implementation (Green phase)

**Checkpoint**: Circuit breakers isolate failing services; fail-fast 503 responses; automatic recovery testing; Redis-backed state prevents per-replica circuit flapping

---

## Phase 7: CORS Configuration (Scenario 5)

**Goal**: Configure CORS policies for browser-based clients

**Scenario Covered**:
- Scenario 5: Cross-Origin Resource Sharing (CORS)

**Independent Test**: Preflight OPTIONS requests return correct CORS headers; credentialed requests pass through

### CORS Implementation

- [ ] T092 Configure CORS policies in Program.cs (allowed origins: *.northstar.edu, localhost:*)
- [ ] T093 Configure CORS allowed methods (GET, POST, PUT, DELETE, OPTIONS)
- [ ] T094 Configure CORS allowed headers (Authorization, Content-Type, X-Correlation-Id)
- [ ] T095 Configure CORS credentials support (allow cookies/auth headers)
- [ ] T096 Configure CORS preflight caching (Access-Control-Max-Age: 3600)

### Tests

- [ ] T097 Create BDD feature file for CORS in tests/bdd/ApiGateway.Specs/Features/CORS.feature
- [ ] T098 Implement step definitions for CORS.feature (preflight requests, credentialed requests, origin validation)
- [ ] T099 Create integration tests for CORS in tests/integration/ApiGateway.IntegrationTests/Scenarios/CorsTests.cs
- [ ] T100 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T101 Verify BDD tests PASS after implementation (Green phase)

**Checkpoint**: CORS functional; browser preflight requests succeed; credentialed cross-origin requests allowed

---

## Phase 8: Health Check Aggregation (Scenario 7)

**Goal**: Aggregate health checks from all backend services with <500ms response time

**Scenario Covered**:
- Scenario 7: Health Check Aggregation

**Independent Test**: GET /health returns 200 if all services healthy, 503 if any unhealthy; completes <500ms

### Health Check Implementation

- [ ] T102 Implement BackendHealthAggregator in Health/BackendHealthAggregator.cs (parallel health checks with 5s timeout)
- [ ] T103 Implement HealthResponseWriter in Health/HealthResponseWriter.cs (JSON response formatter)
- [ ] T104 Configure health check endpoint in Program.cs (MapHealthChecks("/health"))
- [ ] T105 Implement IHealthCheck for each backend service (Identity API, Student API, legacy NS4)
- [ ] T106 Configure health check response format (status, services[], totalDuration)
- [ ] T107 Implement 503 response if ANY service unhealthy (fail-fast for load balancers)

### Tests

- [ ] T108 Create unit tests for BackendHealthAggregator in tests/unit/ApiGateway.UnitTests/Health/
- [ ] T109 Create BDD feature file for HealthChecks in tests/bdd/ApiGateway.Specs/Features/HealthChecks.feature
- [ ] T110 Implement step definitions for HealthChecks.feature (healthy services, unhealthy service, timeout handling)
- [ ] T111 Create integration tests for health aggregation in tests/integration/ApiGateway.IntegrationTests/Scenarios/HealthCheckTests.cs
- [ ] T112 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T113 Verify BDD tests PASS after implementation (Green phase)
- [ ] T114 Verify health endpoint responds <500ms under load

**Checkpoint**: Health aggregation functional; granular service status reporting; load balancer-compatible 503 responses

---

## Phase 9: Request Transformation & Header Injection (Scenario 9)

**Goal**: Standardize downstream context via header injection

**Scenario Covered**:
- Scenario 9: Request Transformation and Header Injection

**Independent Test**: Downstream services receive X-Tenant-Id, X-User-Id, X-Correlation-Id headers; sensitive headers stripped

### Header Injection Implementation

- [ ] T115 Enhance HeaderInjectionTransform to inject X-Tenant-Id from JWT claims
- [ ] T116 Enhance HeaderInjectionTransform to inject X-User-Id from JWT claims
- [ ] T117 Enhance HeaderInjectionTransform to inject X-Correlation-Id from middleware
- [ ] T118 Implement sensitive header removal (Authorization, Cookie) before backend forwarding
- [ ] T119 Configure header injection transforms in YARP route configuration

### Tests

- [ ] T120 Create BDD feature file for RequestTransformation in tests/bdd/ApiGateway.Specs/Features/RequestTransformation.feature
- [ ] T121 Implement step definitions for RequestTransformation.feature (header injection, sensitive header removal)
- [ ] T122 Create integration tests for header injection in tests/integration/ApiGateway.IntegrationTests/Scenarios/HeaderInjectionTests.cs
- [ ] T123 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T124 Verify BDD tests PASS after implementation (Green phase)

**Checkpoint**: Standardized context headers injected; sensitive headers removed; downstream services rely on gateway-injected context

---

## Phase 10: Load Balancing (Scenario 11)

**Goal**: Configure round-robin load balancing across multiple service instances

**Scenario Covered**:
- Scenario 11: Load Balancing Across Service Instances

**Independent Test**: Requests distributed evenly across 3 Student Service instances; unhealthy instances excluded

### Load Balancing Implementation

- [ ] T125 Configure YARP cluster load balancing policy (RoundRobin) in AppHost/Program.cs
- [ ] T126 Configure active health checks for cluster destinations (30-second interval)
- [ ] T127 Implement destination health state management (remove unhealthy destinations)
- [ ] T128 Configure passive health check policy (circuit breaker integration)

### Tests

- [ ] T129 Create BDD feature file for LoadBalancing in tests/bdd/ApiGateway.Specs/Features/LoadBalancing.feature
- [ ] T130 Implement step definitions for LoadBalancing.feature (round-robin distribution, unhealthy instance exclusion)
- [ ] T131 Create integration tests for load balancing in tests/integration/ApiGateway.IntegrationTests/Scenarios/LoadBalancingTests.cs
- [ ] T132 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T133 Verify BDD tests PASS after implementation (Green phase)

**Checkpoint**: Load balancing functional; requests distributed evenly; unhealthy instances automatically excluded

---

## Phase 11: Request Size Limits & Validation (Scenario 12)

**Goal**: Enforce request size limits and early validation

**Scenario Covered**:
- Scenario 12: Request Size Limits and Validation

**Independent Test**: >10MB requests return 413; malformed JSON returns 400 with validation details

### Request Validation Implementation

- [ ] T134 Configure request body size limit (10MB) in Program.cs (Kestrel options)
- [ ] T135 Implement 413 response handler with clear error message
- [ ] T136 Configure request body validation middleware (malformed JSON detection)
- [ ] T137 Implement 400 response handler with validation details
- [ ] T138 Configure early request validation (before backend forwarding)

### Tests

- [ ] T139 Create BDD feature file for RequestValidation in tests/bdd/ApiGateway.Specs/Features/RequestValidation.feature
- [ ] T140 Implement step definitions for RequestValidation.feature (size limits, JSON validation, error responses)
- [ ] T141 Create integration tests for request validation in tests/integration/ApiGateway.IntegrationTests/Scenarios/RequestValidationTests.cs
- [ ] T142 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T143 Verify BDD tests PASS after implementation (Green phase)

**Checkpoint**: Request size limits enforced; malformed requests rejected early; validation details in error responses

---

## Phase 12: Correlation ID Propagation (Scenario 6)

**Goal**: Ensure distributed tracing correlation across all requests

**Scenario Covered**:
- Scenario 6: Request Logging and Correlation IDs

**Independent Test**: All requests have correlation IDs; IDs propagated to downstream services; logs include correlation IDs

### Correlation ID Implementation

- [ ] T144 Enhance CorrelationIdMiddleware to read existing X-Correlation-Id from client requests
- [ ] T145 Configure correlation ID propagation to downstream services (via HeaderInjectionTransform)
- [ ] T146 Configure correlation ID in response headers (X-Correlation-Id)
- [ ] T147 Integrate correlation IDs with OpenTelemetry distributed tracing
- [ ] T148 Configure structured logging to include correlation IDs in all log entries

### Tests

- [ ] T149 Create BDD feature file for CorrelationIds in tests/bdd/ApiGateway.Specs/Features/CorrelationIds.feature
- [ ] T150 Implement step definitions for CorrelationIds.feature (ID generation, propagation, logging)
- [ ] T151 Create integration tests for correlation ID propagation in tests/integration/ApiGateway.IntegrationTests/Scenarios/CorrelationIdTests.cs
- [ ] T152 Verify BDD tests FAIL before implementation (Red phase)
- [ ] T153 Verify BDD tests PASS after implementation (Green phase)

**Checkpoint**: Correlation IDs generated/propagated consistently; distributed tracing spans correlated; structured logging includes IDs

---

## Phase 13: Azure App Configuration Dynamic Refresh

**Goal**: Enable zero-downtime configuration updates via Azure App Configuration

**Independent Test**: Route configuration changes propagate within 30 seconds without service restart

### Dynamic Configuration Implementation

- [ ] T154 Configure Azure App Configuration connection in Program.cs
- [ ] T155 Implement sentinel key-based refresh (Gateway:ConfigVersion triggers full reload)
- [ ] T156 Configure 30-second refresh interval for configuration polling
- [ ] T157 Implement IOptionsSnapshot<YarpConfig> for route reconfiguration
- [ ] T158 Implement zero-downtime updates (new requests use updated routes, in-flight requests complete on old routes)
- [ ] T159 Configure fallback to appsettings.json if Azure App Config unavailable

### Tests

- [ ] T160 Create integration tests for dynamic configuration refresh in tests/integration/ApiGateway.IntegrationTests/Scenarios/ConfigurationRefreshTests.cs
- [ ] T161 Verify configuration changes propagate within 30 seconds
- [ ] T162 Verify in-flight requests complete with old configuration during refresh

**Checkpoint**: Dynamic configuration functional; zero-downtime updates; 30-second refresh cycle validated

---

## Phase 14: Performance Testing & Optimization

**Goal**: Validate all SLO compliance and optimize critical paths

**Performance SLOs**:
- JWT validation: <20ms P95 overhead
- Total gateway latency: <150ms P95
- Rate limit check: <5ms P95
- Health aggregation: <500ms total
- Circuit breaker decision: <1ms

### Performance Testing

- [ ] T163 Create k6 performance test for end-to-end latency in tests/performance/scenarios/end-to-end-latency.js
- [ ] T164 Verify authentication overhead test results meet <20ms P95 SLO
- [ ] T165 Verify rate limiting accuracy test results meet <5ms P95 SLO
- [ ] T166 Run load tests at 10,000 req/s to validate scalability
- [ ] T167 Profile Redis operations to ensure <5ms P95 for rate limit checks
- [ ] T168 Profile JWT validation to ensure <20ms P95 overhead
- [ ] T169 Optimize hot paths based on performance test results
- [ ] T170 Document performance test results in quickstart.md

**Checkpoint**: All performance SLOs validated; optimization opportunities identified and addressed

---

## Phase 15: Integration Testing & Coverage

**Goal**: Validate full integration stack and achieve ≥80% code coverage

### Integration Testing

- [ ] T171 Create ApiGatewayTestHost.cs in tests/integration/ApiGateway.IntegrationTests/ (Aspire test host setup)
- [ ] T172 Verify all 12 BDD scenarios pass across 11 feature files (AuthenticationValidation, StranglerFigRouting [covers Scenarios 1&2], RateLimiting, CORS, CorrelationIds, HealthChecks, CircuitBreakers, RequestTransformation, ApiVersioning, LoadBalancing, RequestValidation)
- [ ] T173 Run dotnet test with code coverage collection (--collect:"XPlat Code Coverage")
- [ ] T174 Verify ≥80% line coverage threshold met
- [ ] T175 Address any coverage gaps in critical paths (middleware, transforms, rate limiting)

**Checkpoint**: All scenarios validated via BDD tests; ≥80% code coverage; integration test suite passing

---

## Phase 16: Documentation & Deployment Preparation

**Goal**: Complete documentation and prepare for production deployment

### Documentation

- [ ] T176 Create quickstart.md with local development setup guide
- [ ] T177 Document prerequisites (Docker, .NET 10 SDK, Aspire workload, Azure CLI)
- [ ] T178 Document User Secrets setup for Redis and Azure App Configuration
- [ ] T179 Document testing workflows (unit, BDD, integration, performance)
- [ ] T180 Document troubleshooting guide (JWT validation failures, Redis connection errors, service discovery delays)
- [ ] T181 Create data-model.md documenting route config schema, rate limit profiles, circuit breaker states
- [ ] T182 Create contracts/health-api.yaml OpenAPI spec for /health endpoint
- [ ] T183 Create contracts/metrics-api.yaml OpenAPI spec for /metrics endpoint (Prometheus format)

### Deployment Configuration

- [ ] T184 Configure Azure Container Apps deployment manifest
- [ ] T185 Configure Azure App Configuration connection for production
- [ ] T186 Configure Azure Key Vault for secrets (Redis connection strings, Azure App Config endpoint)
- [ ] T187 Document deployment process in README.md
- [ ] T188 Create deployment validation checklist

**Checkpoint**: Complete documentation; deployment configuration ready; quickstart guide validated

---

## Phase 17: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements affecting multiple scenarios

- [ ] T189 [P] Code cleanup and refactoring (remove TODO comments, simplify complex methods)
- [ ] T190 [P] Performance optimization across all scenarios (based on profiling results)
- [ ] T191 [P] Security hardening (secrets redaction in logs, CORS policy validation)
- [ ] T192 [P] Add XML documentation comments for public APIs
- [ ] T193 Run quickstart.md validation (follow local dev setup guide end-to-end)
- [ ] T194 Final Red→Green evidence collection (attach all BDD test transcripts to phase review)
- [ ] T195 Update .github/copilot-instructions.md with API Gateway patterns
- [ ] T196 Final constitution compliance check (Principles 1-7)

**Checkpoint**: All scenarios complete; documentation validated; ready for architecture review

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all scenarios
- **Scenarios (Phases 3-12)**: All depend on Foundational phase completion
  - Phases 3-12 can proceed in parallel (if staffed) after Foundational completes
  - Or sequentially by priority (Routing → Auth → Rate Limiting → Circuit Breakers → ...)
- **Dynamic Configuration (Phase 13)**: Depends on Phase 3 (YARP Configuration) completion
- **Performance Testing (Phase 14)**: Depends on all scenario phases (3-12) completion
- **Integration Testing (Phase 15)**: Depends on all scenario phases (3-12) completion
- **Documentation (Phase 16)**: Can proceed in parallel with scenarios; finalized after Phase 15
- **Polish (Phase 17)**: Depends on all previous phases completion

### Scenario Dependencies

- **Phase 3 (YARP Configuration)**: Can start after Foundational (Phase 2) - No dependencies on other scenarios
- **Phase 4 (JWT Authentication)**: Can start after Foundational (Phase 2) - No dependencies on other scenarios
- **Phase 5 (Rate Limiting)**: Depends on Phase 4 (JWT Authentication) for tenant_id extraction
- **Phase 6 (Circuit Breaker)**: Can start after Phase 3 (YARP Configuration) - No other dependencies
- **Phase 7 (CORS)**: Can start after Foundational (Phase 2) - No dependencies on other scenarios
- **Phase 8 (Health Checks)**: Can start after Phase 3 (YARP Configuration) - No other dependencies
- **Phase 9 (Header Injection)**: Depends on Phase 4 (JWT Authentication) for tenant/user extraction
- **Phase 10 (Load Balancing)**: Core load balancing (T125-T127) depends on Phase 3 (YARP Configuration); passive health check policy (T128, circuit breaker integration) depends on Phase 6 (Circuit Breaker)
- **Phase 11 (Request Validation)**: Can start after Foundational (Phase 2) - No dependencies on other scenarios
- **Phase 12 (Correlation IDs)**: Basic correlation ID generation and propagation (CorrelationIdMiddleware) is implemented in Phase 2 (Foundational); Phase 12 adds enhancements including client correlation ID reading, response headers, and OpenTelemetry distributed tracing integration
- **Phase 13 (Dynamic Config)**: Depends on Phase 3 (YARP Configuration) completion

### Within Each Scenario Phase

- BDD feature files MUST be written and tests FAIL before implementation
- Unit tests for components before integration tests
- Implementation before BDD test verification (Red → Green workflow)
- Performance tests after functional tests pass

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T012-T018)
- All Foundational tasks marked [P] can run in parallel within Phase 2 (T024-T030)
- Once Foundational completes, these scenarios can start in parallel:
  - Phase 3 (YARP Configuration) - requires 1-2 developers
  - Phase 4 (JWT Authentication) - requires 1 developer
  - Phase 6 (Circuit Breaker) - requires 1 developer
  - Phase 7 (CORS) - requires 1 developer
  - Phase 8 (Health Checks) - requires 1 developer
  - Phase 11 (Request Validation) - requires 1 developer
- After Phase 4 completes, these can start:
  - Phase 5 (Rate Limiting) - requires 1 developer
  - Phase 9 (Header Injection) - requires 1 developer
- Within each phase, all tasks marked [P] can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# After Phase 1 (Setup) completes, launch these Foundational tasks in parallel:

# Developer A:
T024 Configure Microsoft.Identity.Web JWT validation in Program.cs
T025 Configure structured logging with OpenTelemetry integration

# Developer B:
T020 Implement CorrelationIdMiddleware in Middleware/CorrelationIdMiddleware.cs
T028 Create unit tests for CorrelationIdMiddleware

# Developer C:
T021 Implement TenantContextMiddleware in Middleware/TenantContextMiddleware.cs
T029 Create unit tests for TenantContextMiddleware

# Developer D:
T022 Implement ErrorHandlingMiddleware in Middleware/ErrorHandlingMiddleware.cs
T030 Create unit tests for ErrorHandlingMiddleware

# Developer E:
T026 Configure health check aggregation endpoint in Health/GatewayHealthCheck.cs
T027 Integrate ServiceDefaults for OpenTelemetry distributed tracing and metrics
```

---

## Parallel Example: Scenario Phases (After Foundational Complete)

```bash
# Once Phase 2 (Foundational) completes, these scenarios can start in parallel:

# Team A (2 developers):
Phase 3 - YARP Configuration & Service Routing
  - Developer A1: T031-T038 (configuration and models)
  - Developer A2: T039-T042 (transforms)

# Team B (1 developer):
Phase 4 - JWT Authentication Validation
  - Developer B: T053-T057 (authentication implementation)

# Team C (1 developer):
Phase 6 - Circuit Breaker Resilience
  - Developer C: T079-T085 (circuit breaker implementation)

# Team D (1 developer):
Phase 7 - CORS Configuration
  - Developer D: T092-T096 (CORS implementation)

# Team E (1 developer):
Phase 8 - Health Check Aggregation
  - Developer E: T102-T107 (health check implementation)
```

---

## Implementation Strategy

### Critical Path (Minimum Viable Gateway)

**Goal**: Get basic routing functional as quickly as possible

1. **Complete Phase 1**: Setup (T011-T019) → Project initialized
2. **Complete Phase 2**: Foundational (T020-T030) → Core infrastructure ready
3. **Complete Phase 3**: YARP Configuration (T031-T052) → Routing functional
4. **Complete Phase 4**: JWT Authentication (T053-T064) → Security enforced
5. **STOP and VALIDATE**: Test basic authenticated routing end-to-end
6. **Deploy/Demo**: Minimum viable gateway operational

### Incremental Delivery

1. **MVP (Phases 1-4)**: Basic authenticated routing → Deploy
2. **Add Rate Limiting (Phase 5)**: Tenant throttling → Deploy
3. **Add Resilience (Phase 6)**: Circuit breakers → Deploy
4. **Add Observability (Phases 7-8, 12)**: CORS, health checks, correlation → Deploy
5. **Add Advanced Features (Phases 9-11)**: Header injection, load balancing, validation → Deploy
6. **Add Dynamic Config (Phase 13)**: Zero-downtime updates → Deploy
7. **Optimize & Document (Phases 14-16)**: Performance, testing, docs → Final Release

### Parallel Team Strategy

With 5 developers after Foundational phase completes:

1. **Team completes Setup + Foundational together** (Phases 1-2)
2. **Once Foundational done, split scenarios**:
   - Developer A + B: Phase 3 (YARP Configuration) - 2-3 days
   - Developer C: Phase 4 (JWT Authentication) - 2-3 days
   - Developer D: Phase 6 (Circuit Breaker) - 2 days
   - Developer E: Phase 7 (CORS) + Phase 8 (Health Checks) - 2 days
3. **After Phase 3-4 complete**:
   - Developer A: Phase 5 (Rate Limiting) - 2 days
   - Developer B: Phase 9 (Header Injection) - 1 day
   - Developer C: Phase 10 (Load Balancing) - 1 day
   - Developer D: Phase 11 (Request Validation) - 1 day
   - Developer E: Phase 12 (Correlation IDs) - 1 day
4. **Converge for testing/docs** (Phases 14-16) - All developers

**Estimated Timeline**: 2-3 weeks with 5 developers working in parallel

---

## Notes

- [P] tasks = different files, no dependencies within the same phase
- All BDD feature files must be written BEFORE implementation (TDD)
- Verify tests FAIL before implementing (Red phase - capture output)
- Verify tests PASS after implementing (Green phase - capture output)
- Commit after each logical task group (e.g., after completing a scenario phase)
- Stop at any checkpoint to validate scenario independently
- Gateway is infrastructure - no UI, no domain logic, purely routing/orchestration
- Performance SLOs are CRITICAL - validate with k6 load tests throughout
- Redis is REQUIRED for multi-replica consistency (rate limiting, circuit breakers)
- Azure App Configuration enables zero-downtime updates (30-second refresh cycle)
