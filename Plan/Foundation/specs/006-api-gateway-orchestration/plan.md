# Implementation Plan: API Gateway and Service Orchestration

**Specification Branch**: `Foundation/006-api-gateway-orchestration-spec` *(current branch - planning artifacts)*  
**Implementation Branch**: `Foundation/006-api-gateway-orchestration` *(created after approval)*  
**Date**: 2025-11-20 | **Spec**: Plan/Foundation/specs/006-api-gateway-orchestration/spec.md

**Note**: Gateway supports legacy + new services during migration; enforces tenant context and cross-cutting concerns.

## Summary

Implement YARP-based gateway providing unified ingress, JWT/session validation, tenant/user header injection, rate limiting, routing to migrated microservices and legacy monolith, and resilience (circuit breaking, retries). Deliver versioned routes, correlation propagation, and health aggregation with p95 overhead <50ms.

## Technical Context

**Language/Version**: C# / .NET 8 (Aspire)  
**Primary Dependencies**: YARP, Microsoft.Identity.Web, Polly, AspNetCore Rate Limiting, OpenTelemetry, MassTransit for event-forwarding if needed  
**Storage**: Minimal; configuration via appsettings and config store; Redis optional for rate limiting counters  
**Testing**: xUnit + Reqnroll BDD for routing/auth/rate-limit scenarios; integration harness for failure injection; k6 or NBomber perf tests for overhead; Playwright smoke via UI paths  
**Target Platform**: Linux containers via Aspire  
**Project Type**: Gateway service  
**Performance Goals**: Gateway overhead p95 <50ms; auth validation <20ms; health aggregation <500ms; per-tenant rate limiting enforced  
**Constraints**: Tenant claim required; TLS 1.3; header sanitization; branch naming with layer prefix  
**Scale/Scope**: Routes for legacy monolith + new microservices during strangler migration

### Identity & Authentication Guidance

- Identity Provider: Microsoft Entra ID; tokens validated via Microsoft.Identity.Web  
- Authentication Pattern: Session/BFF; gateway enforces auth and injects tenant/user IDs upstream  
- Session Storage: Managed by Identity/BFF; gateway is stateless aside from rate-limit counters

## Layer Identification (MANDATORY)

**Target Layer**: Foundation  
**Implementation Path**: `Src/Foundation/gateway`  
**Specification Path**: `Plan/Foundation/specs/006-api-gateway-orchestration/`

### Layer Consistency Validation

- [x] Target Layer matches specification (Foundation)  
- [x] Implementation path follows layer structure (`Src/Foundation/...`)  
- [x] Specification path follows layer structure (`Plan/Foundation/specs/...`)  
- [x] Branch naming includes layer prefix

### Shared Infrastructure Dependencies

- [x] ServiceDefaults - Hosting, telemetry, health checks  
- [x] Domain/Application - Not primary; gateway stays thin  
- [x] Infrastructure - Redis (rate limiting), OpenTelemetry exporters

### Cross-Layer Dependencies

**Depends on layers**: Foundation services only through routed HTTP calls  
**Specific Dependencies**: Identity for token validation metadata; ServiceDefaults for hosting; Redis for rate limiting (shared infra)  
**Justification**: Gateway is cross-cutting but remains in Foundation; no higher-layer coupling.  
**Constitutional Compliance**: Principle 6 upheld; gateway only routes, does not bind layers.

### Constitution Check

- Layer-prefixed branch pattern OK  
- Planning artifacts only in spec branch OK  
- Multi-tenancy enforced via tenant headers and validation OK  
- Security: TLS, header sanitation, no secrets in code OK  
- Testing: routing/auth/rate-limit/circuit tests planned OK  
- No UI; Figma requirement not applicable OK

## Project Structure

### Documentation (this feature)

```
specs/006-api-gateway-orchestration/
- plan.md
- research.md          # Routing, resilience patterns
- data-model.md        # Route/cluster configuration schema
- quickstart.md        # Running gateway with Aspire
- contracts/           # Route config samples, health aggregation contract
- tasks.md
```

### Source Code (repository root)

```
Src/Foundation/gateway/
- Configuration/       # YARP routes, clusters, transforms
- Middleware/          # Auth, tenant headers, correlation, rate limiting
- Health/              # Aggregated health endpoints
- Observability/       # OpenTelemetry exporters, logging
- Tests/               # Unit, integration, BDD, perf harness

tests/gateway/
- integration/
- performance/
- contract/            # Route validation snapshots
```

**Structure Decision**: Single gateway service with clear middleware + configuration folders; tests alongside.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | - | - |

