# Plan: API Gateway and Service Orchestration
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 006-api-gateway-orchestration/spec.md

## Objectives
- Provide secure, resilient, low-latency routing for microservices + legacy during migration.
- Enforce tenant context + rate limits globally.
- Offer unified observability (correlation, tracing) + health aggregation.

## Architecture Components
1. YARP Reverse Proxy (ReverseProxy configuration section).
2. Middleware pipeline: Auth → Correlation → RateLimiter → ReverseProxy.
3. Policy Layer: Rate limiting per tenant; circuit breaker per cluster.
4. Health Aggregator Endpoint `/health` calling downstream health paths.
5. Config Validation: Startup routine verifying cluster/destination reachability.

## Configuration Strategy
- `appsettings.Gateway.json` segregated; loaded at startup.
- Hot-reload via file watcher (optional phase 2).
- Templates for route transforms (inject headers).

## Middleware Order
1. Correlation ID creation.
2. Authentication & claim extraction.
3. Rate limiting partitioning by tenant.
4. Request size validation.
5. Reverse proxy forwarding.
6. Response header enrichment (correlation, version deprecation).

## Tenant Context
- Extract `tenant_id` claim; inject `X-Tenant-Id` header.
- Fallback: reject if absent.

## Versioning
- Path-based: `/api/v{version}/...`.
- Deprecation header for sunset schedule: `X-API-Sunset: 2026-06-01`.

## Circuit Breaker Policy
- 5 consecutive failures opens breaker for 30s.
- Half-open test with single probe request.
- Metrics exported: `circuit_open_total`.

## Rate Limiting
- Fixed-window per tenant: 1000 req/min default.
- Configurable overrides via settings.
- Return 429 with `Retry-After` header.

## Health Aggregation
- Parallel fetch to `/healthz` endpoints.
- Aggregate JSON structure:
```json
{
  "status": "Healthy|Degraded|Unhealthy",
  "services": {
    "students": {"status": "Healthy", "latencyMs": 12},
    "staff": {"status": "Healthy", "latencyMs": 14},
    "legacy-assessment": {"status": "Degraded", "latencyMs": 220}
  }
}
```

## Observability
- OpenTelemetry spans: `gateway.request` with attributes: tenant.id, route.id, cluster.id.
- Logging scope includes correlation id + tenant id.

## Security
- Reject missing/invalid tokens early.
- Strip potentially sensitive headers (X-Forwarded-* normalized, remove internal debugging headers).
- Enforce max content length (10MB) pre-forward.

## Testing Strategy
- Unit: auth middleware claim extraction, rate limiter partition logic.
- Integration: route forwarding, fallback to legacy cluster, circuit breaker transitions.
- Performance: load test 1000 RPS mixed services measuring overhead.
- Chaos tests: inject downstream failures verifying breaker behavior.

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Legacy endpoint response variance | User confusion | Uniform response wrapping layer (optional) |
| JWKS fetching latency | Slower auth | Cache keys + background refresh |
| Rate limit misconfig | False throttling | Config linter + staging verification |

## Completion Criteria
- All spec scenarios green.
- p95 gateway overhead <50ms under load.
- Health endpoint returns correct aggregate.
- Circuit breaker metrics published.

---
Draft plan.