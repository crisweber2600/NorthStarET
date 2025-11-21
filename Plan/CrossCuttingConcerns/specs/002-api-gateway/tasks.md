# Tasks: API Gateway (YARP Service Orchestration)

**Specification Branch**: `CrossCuttingConcerns/002-api-gateway-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `CrossCuttingConcerns/002-api-gateway` *(created by `/speckit.implement`)*  
**Feature**: 002-api-gateway  
**Input**: plan.md, spec.md, data-model.md, research.md  

---

## Layer Context (MANDATORY)

**Target Layer**: CrossCuttingConcerns  
**Implementation Path**: `Src/Foundation/services/ApiGateway/`  
**Specification Path**: `Plan/CrossCuttingConcerns/specs/002-api-gateway/`

### Layer Consistency Checklist

- [ ] Target Layer matches spec.md Layer Identification
- [ ] Target Layer matches plan.md Layer Identification
- [ ] Implementation path follows layer structure (`Src/{TargetLayer}/...`)
- [ ] Specification path follows layer structure (`Plan/{TargetLayer}/specs/...`)
- [ ] Shared infrastructure dependencies match between spec and plan
- [ ] Cross-layer dependencies (if any) justified in both spec and plan

---

## Layer Compliance Validation

- [ ] T001 Verify API Gateway references only shared infrastructure packages (no service-to-service coupling) in `Src/Foundation/services/ApiGateway/*.csproj`
- [ ] T002 Confirm AppHost registers gateway as entrypoint without referencing downstream implementations directly in `Src/Foundation/AppHost/Program.cs`
- [ ] T003 Ensure README documents gateway's CrossCuttingConcerns role and dependencies in `Src/Foundation/services/ApiGateway/README.md`
- [ ] T004 Check for circular dependencies (gateway should not depend on downstream service projects) via solution graph review

---

## Identity & Authentication Compliance

- [ ] T005 Configure JWT validation with Microsoft.Identity.Web against Identity service audiences in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T006 Remove/avoid any Duende IdentityServer packages or custom token issuance in `Src/Foundation/services/ApiGateway/*.csproj`
- [ ] T007 Enforce session/tenant propagation headers only (no session state) in `Src/Foundation/services/ApiGateway/Middleware/TenantPropagationMiddleware.cs`
- [ ] T008 Validate gateway auth policies align with `Plan/Foundation/docs/legacy-identityserver-migration.md` (no custom password/MFA logic) and document in `Src/Foundation/services/ApiGateway/README.md`

---

## Format

Tasks use `- [ ] T### [P?] [Story] Description with file path`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Establish gateway project scaffolding and baseline configuration.

- [ ] T009 Run `.specify/scripts/bash/check-prerequisites.sh --json` and capture FEATURE_DIR for this spec
- [ ] T010 Create ApiGateway project structure (Program.cs, appsettings, ReverseProxy config) in `Src/Foundation/services/ApiGateway/`
- [ ] T011 [P] Add gateway to Aspire AppHost with HTTPS endpoint and environment variables for downstream base URLs in `Src/Foundation/AppHost/Program.cs`
- [ ] T012 [P] Add developer launchSettings and CI configuration for gateway in `Src/Foundation/services/ApiGateway/Properties/launchSettings.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core routing and middleware foundation required for all scenarios.

- [ ] T013 [P] Configure YARP `ReverseProxy` routes/clusters for Identity, Configuration, Student, Staff, Assessment in `Src/Foundation/services/ApiGateway/appsettings.json`
- [ ] T014 [P] Implement correlation ID middleware and logging pipeline in `Src/Foundation/services/ApiGateway/Middleware/CorrelationIdMiddleware.cs`
- [ ] T015 [P] Add global exception handling and ProblemDetails responses in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T016 Implement tenant context extraction and header injection middleware in `Src/Foundation/services/ApiGateway/Middleware/TenantPropagationMiddleware.cs`
- [ ] T017 Configure health checks (self + downstream) endpoints `/health` and `/ready` in `Src/Foundation/services/ApiGateway/Program.cs`

---

## Phase 3: User Story 1 - Route Requests with JWT Validation (Priority: P1) ✔ MVP

**Goal**: Route authenticated requests to new microservices with JWT validation and tenant propagation.  
**Independent Test**: Authenticated request to `/api/v1/students/123` validates token, forwards to Student service, returns within 150ms.

### Tests (write-first)
- [ ] T018 [P] [US1] Integration test for JWT validation and routing to Student service in `Src/Foundation/services/ApiGateway/tests/ApiGateway.IntegrationTests/Routing/JwtRoutingTests.cs`
- [ ] T019 [P] [US1] Contract test ensuring tenant/user headers set for downstream services in `tests/ApiGateway.ContractTests/Headers/TenantHeaderTests.cs`

### Implementation
- [ ] T020 [US1] Configure authentication/authorization middleware order and default policies in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T021 [US1] Finalize YARP transforms for user/tenant/correlation headers in `Src/Foundation/services/ApiGateway/appsettings.json`
- [ ] T022 [US1] Add request timeout policies aligned with 150ms SLO in `Src/Foundation/services/ApiGateway/Program.cs`

---

## Phase 4: User Story 2 - Strangler Fig Legacy Routing (Priority: P1)

**Goal**: Route legacy endpoints to OldNorthStar while new services handle migrated routes.  
**Independent Test**: `/api/v1/assessments/456` routes to legacy backend transparently; headers translated if needed.

### Tests (write-first)
- [ ] T023 [P] [US2] Integration test verifying legacy route cluster and header transforms in `tests/ApiGateway.IntegrationTests/Routing/LegacyRoutingTests.cs`
- [ ] T024 [P] [US2] Smoke test for dual routing toggle (legacy vs new) via config flag in `tests/ApiGateway.IntegrationTests/Routing/StranglerToggleTests.cs`

### Implementation
- [ ] T025 [US2] Define legacy clusters/routes with translation transforms in `Src/Foundation/services/ApiGateway/appsettings.json`
- [ ] T026 [US2] Implement configuration flag for routing cutover and document operations steps in `Src/Foundation/services/ApiGateway/README.md`
- [ ] T027 [US2] Ensure backward-compatible headers/cookies for legacy services in `Src/Foundation/services/ApiGateway/Middleware/LegacyHeaderTransformMiddleware.cs`

---

## Phase 5: User Story 3 - Resiliency (Rate Limiting, Circuit Breakers, Load Balancing) (Priority: P1)

**Goal**: Enforce per-tenant rate limits, circuit breakers, and load balancing across instances.  
**Independent Test**: Exceeding tenant quota returns 429; failing service opens circuit; RoundRobin distributes requests.

### Tests (write-first)
- [ ] T028 [P] [US3] Rate limiting integration test per tenant returning 429 with Retry-After in `tests/ApiGateway.IntegrationTests/Resiliency/RateLimitingTests.cs`
- [ ] T029 [P] [US3] Circuit breaker test simulating downstream failures and recovery in `tests/ApiGateway.IntegrationTests/Resiliency/CircuitBreakerTests.cs`
- [ ] T030 [P] [US3] Load balancing test verifying RoundRobin distribution across destinations in `tests/ApiGateway.IntegrationTests/Resiliency/LoadBalancingTests.cs`

### Implementation
- [ ] T031 [US3] Configure ASP.NET rate limiting policies (per tenant/endpoint) in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T032 [US3] Add Polly circuit breaker/retry policies to YARP HttpClient pipelines in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T033 [US3] Set load balancing policies per cluster (RoundRobin, passive health) in `Src/Foundation/services/ApiGateway/appsettings.json`

---

## Phase 6: User Story 4 - Request Transformation, Headers, CORS, Size Limits (Priority: P1)

**Goal**: Standardize request transformations and enforce CORS and payload size limits.  
**Independent Test**: Preflight OPTIONS returns configured headers; >10MB payload returns 413; headers injected for downstream.

### Tests (write-first)
- [ ] T034 [P] [US4] CORS preflight integration test for allowed origins in `tests/ApiGateway.IntegrationTests/Cors/CorsPreflightTests.cs`
- [ ] T035 [P] [US4] Payload size limit test returning 413 for oversized requests in `tests/ApiGateway.IntegrationTests/Validation/RequestSizeLimitTests.cs`

### Implementation
- [ ] T036 [US4] Configure CORS policies per environment in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T037 [US4] Implement request/response transform middleware injecting `X-Tenant-Id`, `X-User-Id`, `X-Correlation-Id` in `Src/Foundation/services/ApiGateway/Middleware/HeaderInjectionMiddleware.cs`
- [ ] T038 [US4] Enforce request size limits and malformed JSON handling in `Src/Foundation/services/ApiGateway/Program.cs`

---

## Phase 7: User Story 5 - Observability & Health Aggregation (Priority: P1)

**Goal**: Provide correlated logging, tracing, and aggregated health endpoints.  
**Independent Test**: `/health` aggregates downstream statuses within 500ms; logs include correlation_id and duration.

### Tests (write-first)
- [ ] T039 [P] [US5] Integration test for aggregated `/health` response timing and content in `tests/ApiGateway.IntegrationTests/Health/HealthAggregationTests.cs`
- [ ] T040 [P] [US5] Logging/tracing test ensuring correlation IDs propagate to downstream spans in `tests/ApiGateway.IntegrationTests/Observability/CorrelationTracingTests.cs`

### Implementation
- [ ] T041 [US5] Configure OpenTelemetry tracing + logging enrichment for gateway requests in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T042 [US5] Implement health aggregation of downstream clusters with cached results in `Src/Foundation/services/ApiGateway/Health/HealthAggregationService.cs`
- [ ] T043 [US5] Add structured request/response logging with duration and status codes in `Src/Foundation/services/ApiGateway/Middleware/RequestLoggingMiddleware.cs`

---

## Phase 8: User Story 6 - API Versioning Support (Priority: P2)

**Goal**: Route versioned APIs with sunset/deprecation headers.  
**Independent Test**: `/api/v1/*` and `/api/v2/*` route to correct clusters; v1 responses include sunset metadata.

### Tests (write-first)
- [ ] T044 [P] [US6] Versioned routing integration test for v1/v2 clusters in `tests/ApiGateway.IntegrationTests/Routing/VersionRoutingTests.cs`
- [ ] T045 [P] [US6] Contract test verifying deprecation/sunset headers on v1 responses in `tests/ApiGateway.ContractTests/Headers/VersionDeprecationTests.cs`

### Implementation
- [ ] T046 [US6] Add versioned route/cluster definitions and transforms for v1/v2 in `Src/Foundation/services/ApiGateway/appsettings.json`
- [ ] T047 [US6] Implement deprecation header injection middleware for v1 responses in `Src/Foundation/services/ApiGateway/Middleware/ApiVersioningMiddleware.cs`
- [ ] T048 [US6] Document versioning strategy and rollout steps in `Src/Foundation/services/ApiGateway/README.md`

---

## Phase 9: Polish & Cross-Cutting Concerns

- [ ] T049 [P] Harden security headers and TLS settings for gateway in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T050 [P] Tune performance (connection limits, response buffering) to meet latency targets in `Src/Foundation/services/ApiGateway/Program.cs`
- [ ] T051 Update operational runbook for toggling legacy routes and rate limits in `Src/Foundation/services/ApiGateway/README.md`
- [ ] T052 [P] Final regression of integration/contract tests in `Src/Foundation/services/ApiGateway/tests/`

---

## Dependencies & Execution Order

- Setup → Foundational → User Stories (US1-5 are P1, US6 is P2) → Polish
- After foundational, US1 and US2 can start in parallel; US3 depends on base routing; US4/US5 depend on middleware baseline; US6 after routing stable.

## Parallel Execution Examples

- Marked [P] tasks (routing tests, resiliency tests, middleware work) can proceed concurrently as they target separate files.
- US1/US2 routing definitions and US3 resiliency policies can be implemented in parallel once appsettings schema is fixed.

## Implementation Strategy

- MVP: Phases 1-5 deliver authenticated routing with resiliency and observability.
- Next: Add versioning (Phase 8) and polish operational hardening.

---
