---
description: "Task list for API Gateway and Service Orchestration (YARP, auth, rate limiting)"
---

# Tasks: API Gateway and Service Orchestration

**Specification Branch**: `Foundation/006-api-gateway-orchestration-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/006-api-gateway-orchestration` *(created when starting implementation)*

**Input**: Design documents from `Plan/Foundation/specs/006-api-gateway-orchestration/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

---

## Layer Context (MANDATORY)

*Verify consistency across specification, plan, and task artifacts*

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/gateway/`  
**Specification Path**: `Plan/Foundation/specs/006-api-gateway-orchestration/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/Foundation/gateway/`)
- [ ] Specification path follows layer structure (`Plan/Foundation/specs/006-api-gateway-orchestration/`)
- [ ] Shared dependencies (YARP, Microsoft.Identity.Web, Polly, RateLimiting, OpenTelemetry) align between plan and spec
- [ ] Legacy/microservice routes documented consistently

---

## Layer Compliance Validation

- [ ] T001 Verify gateway project references stay within Foundation shared libraries (`Src/Foundation/gateway/Gateway.csproj`)
- [ ] T002 Verify downstream services consumed only via HTTP reverse proxy config (no cross-service project references)
- [ ] T003 Ensure AppHost registers gateway with proper isolation + TLS termination (`Src/Foundation/AppHost/Program.cs`)
- [ ] T004 Update README with layer placement, config files, and dependency map (`Src/Foundation/gateway/README.md`)

---

## Identity & Authentication Compliance

- [ ] T005 Configure Microsoft.Identity.Web JWT validation (issuer/audience/tenant claim required) in `Src/Foundation/gateway/Program.cs`
- [ ] T006 Add middleware to reject missing `tenant_id` and inject `X-Tenant-Id`/`X-User-Id` headers (`Src/Foundation/gateway/Middleware/TenantHeaderMiddleware.cs`)
- [ ] T007 Ensure no custom token issuance or Duende references remain (`Src/Foundation/gateway/`)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Bootstrap gateway project, configuration, and pipelines

- [ ] T008 Scaffold gateway project with YARP + hosting config (`Src/Foundation/gateway/Gateway.csproj`)
- [ ] T009 Add configuration file `appsettings.Gateway.json` with route/cluster scaffolding (`Src/Foundation/gateway/appsettings.Gateway.json`)
- [ ] T010 [P] Add OpenTelemetry exporter + logging configuration for gateway spans (`Src/Foundation/gateway/Telemetry/TelemetryConfig.cs`)
- [ ] T011 [P] Setup test projects (unit for middleware, integration for proxy) (`tests/Foundation/Gateway/`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core middleware chain, correlation, and base routing

- [ ] T012 Implement correlation ID middleware + propagation to downstream headers (`Src/Foundation/gateway/Middleware/CorrelationMiddleware.cs`)
- [ ] T013 Add YARP reverse proxy setup with placeholder clusters/routes for microservices and legacy (`Src/Foundation/gateway/Program.cs`)
- [ ] T014 Add request size validator (10MB) + malformed JSON rejection (`Src/Foundation/gateway/Middleware/RequestValidationMiddleware.cs`)
- [ ] T015 [P] Add standardized error response transformer for 401/403/429/5xx (`Src/Foundation/gateway/Middleware/ErrorResponseMiddleware.cs`)

**Checkpoint**: Middleware pipeline ready for story work

---

## Phase 3: User Story 1 - Authenticated Routing to New Microservices (Priority: P1) **MVP**

**Goal**: Route requests to new services with auth validation and tenant/user header injection (scenarios 1, 3, 9)

**Independent Test**: Authenticated call to new microservice includes X-Tenant-Id/X-User-Id, JWT validated (<20ms), correlation propagated

### Implementation for User Story 1

- [ ] T016 [P] [US1] Configure routes/clusters for Student/Staff/Configuration services with path-based versioning (`Src/Foundation/gateway/config/routes.new.json`)
- [ ] T017 [US1] Add authentication middleware order (Correlation -> Auth -> Tenant header -> Proxy) (`Src/Foundation/gateway/Program.cs`)
- [ ] T018 [P] [US1] Implement header transform injecting tenant/user/correlation/version headers (`Src/Foundation/gateway/Transforms/HeaderTransformProvider.cs`)
- [ ] T019 [US1] Add integration tests for authenticated routing + header injection (`tests/Foundation/Gateway/Integration/AuthRoutingTests.cs`)

**Checkpoint**: Authenticated, tenant-aware routing to new services works

---

## Phase 4: User Story 2 - Legacy Strangler Routing & Versioning (Priority: P1)

**Goal**: Route selected paths to legacy monolith with versioning + deprecation headers (scenarios 2, 10)

**Independent Test**: Legacy-bound routes forward correctly; deprecation header returned for v1 routes; versioned routes resolve properly

### Implementation for User Story 2

- [ ] T020 [P] [US2] Configure legacy cluster/routes with path filters (`Src/Foundation/gateway/config/routes.legacy.json`)
- [ ] T021 [US2] Implement versioned routing rules and deprecation header injection (`Src/Foundation/gateway/Transforms/VersioningTransform.cs`)
- [ ] T022 [US2] Add integration tests covering legacy routing and sunset headers (`tests/Foundation/Gateway/Integration/LegacyRoutingTests.cs`)

**Checkpoint**: Legacy + versioning behaviors validated

---

## Phase 5: User Story 3 - Rate Limiting, Load Balancing, and Circuit Breaking (Priority: P2)

**Goal**: Enforce per-tenant rate limits, load balancing, and circuit breakers (scenarios 4, 8, 11)

**Independent Test**: Exceeding tenant quota returns 429 with Retry-After; breaker opens after 5 failures and recovers; round-robin distributes requests

### Implementation for User Story 3

- [ ] T023 [P] [US3] Configure per-tenant fixed-window rate limiter (1000 req/min default, overrides supported) in `Src/Foundation/gateway/Program.cs`
- [ ] T024 [US3] Implement circuit breaker policy per cluster via Polly (5 failures, 30s break) (`Src/Foundation/gateway/Resilience/CircuitBreakerPolicyBuilder.cs`)
- [ ] T025 [P] [US3] Configure load balancing policy (RoundRobin/PowerOfTwo) in YARP clusters (`Src/Foundation/gateway/config/clusters.json`)
- [ ] T026 [US3] Integration + chaos tests for rate limit, breaker transitions, and load balancing (`tests/Foundation/Gateway/Integration/ResilienceTests.cs`)

**Checkpoint**: Resilience + traffic governance enforced

---

## Phase 6: User Story 4 - CORS, Health Aggregation, Observability, Request Limits (Priority: P2)

**Goal**: Provide CORS handling, health aggregation, and observability with size limits (scenarios 5, 6, 7, 12)

**Independent Test**: CORS preflight responses correct; /health returns aggregate within <500ms; traces/logs include route/cluster/tenant; oversize payload rejected

### Implementation for User Story 4

- [ ] T027 [P] [US4] Configure CORS policy per environment (`Src/Foundation/gateway/Program.cs`)
- [ ] T028 [US4] Implement health aggregation endpoint calling downstream `/healthz` in parallel (`Src/Foundation/gateway/Health/HealthAggregatorController.cs`)
- [ ] T029 [P] [US4] Add tracing/logging enrichment for route.id, cluster.id, tenant.id (`Src/Foundation/gateway/Telemetry/GatewayEnricher.cs`)
- [ ] T030 [US4] Add integration tests for CORS, health aggregation latency, and max request size errors (`tests/Foundation/Gateway/Integration/HealthAndCorsTests.cs`)

**Checkpoint**: Cross-cutting concerns validated with observability

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T031 [P] Add configuration validation linter run at startup and CI (`Src/Foundation/gateway/Config/ConfigValidator.cs`)
- [ ] T032 Add JWKS metadata caching + background refresh to reduce auth latency (`Src/Foundation/gateway/Auth/JwksCache.cs`)
- [ ] T033 [P] Add dashboards/alerts for rate limit rejections and circuit breaker opens (`observability/dashboards/gateway.json`)
- [ ] T034 Final audit against spec scenarios and acceptance criteria (`Plan/Foundation/specs/006-api-gateway-orchestration/tasks.md`)

---

## Dependencies & Execution Order

- Setup (Phase 1)  Foundational (Phase 2)  US1/US2 (P1)  US3/US4 (P2)  Polish
- US1 depends on middleware chain + auth setup
- US2 depends on base routing + transforms
- US3 depends on routes/clusters existing
- US4 depends on core routing + middleware to expose CORS/health/telemetry

## Parallel Execution Examples

- T008-T011 can run in parallel (project, config, telemetry, tests)
- US1 tasks T016-T018 parallelizable before integration tests
- US3 tasks T023-T025 can run in parallel; T026 follows
- US4 tasks T027-T029 can run parallel once routing in place

## Implementation Strategy

- MVP = Phases 1-4 to deliver authenticated routing to new + legacy services with versioning
- Next: add rate limiting, load balancing, circuit breakers (US3), then observability/health (US4)
- Keep performance budget (<50ms overhead) by profiling after each stage; enforce config validation in CI
