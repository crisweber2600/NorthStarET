# Spec: API Gateway and Service Orchestration

Short Name: api-gateway-orchestration
Layer: Foundation
Status: Draft (Specification)
Version: 0.1.0
Created: 2025-11-20

## Feature
Implement YARP-based API Gateway providing unified ingress, authentication, rate limiting, routing to migrated microservices and legacy monolith endpoints during transition, plus cross-cutting concerns (CORS, correlation, circuit breaking, header transforms, versioning).

## Business Value
Delivers a consistent entry point, reduces client complexity, enforces security and tenant context uniformly, enables incremental migration with Strangler Fig routing, and provides observability & resilience.

## Target Layer
Foundation

## Actors
- External Client (Web/UI)
- District Users (via UI)
- Monitoring System
- Legacy Monolith
- New Microservices (Student, Staff, Configuration, etc.)

## Assumptions
- Identity service issues JWT with tenant_id, user claims.
- Aspire orchestration provides service discovery endpoints (internal DNS / container names).
- OpenTelemetry instrumentation available.

## Constraints
- Gateway overhead p95 <50ms.
- Auth validation <20ms.
- Health aggregation <500ms.
- Rate limiting per tenant independent.

## Scenarios (Condensed)
1. Route to new microservice with auth + tenant header injection.
2. Route to legacy monolith seamlessly during migration.
3. Validate JWT (signature, exp, issuer, audience, tenant claim).
4. Per-tenant rate limiting & 429 on exceeding quota.
5. CORS preflight responses with configured headers.
6. Correlation ID generation/propagation across downstream calls.
7. Aggregate downstream health checks with status mapping.
8. Circuit breaker opens after consecutive failures, auto-recover.
9. Request transformation injecting X-Tenant-Id, X-User-Id, X-Correlation-Id.
10. Versioned routing (v1 vs v2) with deprecation headers.
11. Load balancing across multiple service instances (round-robin).
12. Enforce request size limits & malformed JSON rejection.

## Non-Functional Requirements
- Security: TLS 1.3, header sanitization, auth mandatory.
- Resilience: Circuit breakers, retries (idempotent GET).
- Observability: Structured logs + distributed tracing with correlation.

## Acceptance Criteria Summary
All scenarios validated via integration tests & simulated failure harness; performance metrics meet SLO; versioning + rate limits documented; tenant isolation enforced at header level.

## Out of Scope
- GraphQL aggregation gateway.
- API monetization / billing.

## Risks & Mitigations
| Risk | Mitigation |
|------|------------|
| Misconfigured route causes outage | Automated config validation + staging smoke tests |
| Token validation performance | Cache JWKS + reuse handlers |
| Circuit breaker thrash | Backoff strategy + metrics alerts |

## Initial Roadmap
1. Bootstrap YARP configuration structure.
2. Implement auth middleware + tenant/user header injection.
3. Add correlation + logging middleware.
4. Configure rate limiting policy per tenant.
5. Add circuit breaker & health aggregation endpoints.
6. Implement versioned route mapping & deprecation headers.

## Audit & Compliance
Log all auth failures (tenant_id if extractable) + rate limit violations + circuit breaker transitions with timestamps.

---
Generated manually (spec draft).