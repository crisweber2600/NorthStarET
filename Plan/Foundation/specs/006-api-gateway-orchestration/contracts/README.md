# Contracts: API Gateway and Service Orchestration
Layer: Foundation

## Ingress Surface
- Proxies external traffic to downstream services with versioned routes, e.g.:
  - `/api/students/**` -> Student service cluster (`v1`, `v2`)
  - `/api/staff/**` -> Staff service cluster
  - `/legacy/**` -> Legacy monolith passthrough (Strangler Fig)
- Management endpoints:
  - `/health/aggregate` — aggregated downstream health.
  - `/_metrics` — optional Prometheus/OpenTelemetry exporter.

## Headers & Auth
- Requires `Authorization` bearer token (Entra ID) or session cookie from BFF.
- Injected headers upstream: `X-Tenant-Id`, `X-User-Id`, `X-Correlation-Id`.
- Rate limit headers returned: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `Retry-After` when throttled.

## Config Artifacts
- Route/cluster definitions stored as JSON (validated by Gateway.ConfigValidator).
- Rate limit policies per tenant/user persisted in config store; Redis used for counters.

## Events (optional)
- `GatewayCircuitOpened` / `GatewayCircuitClosed` telemetry events for alerting.
- `GatewayRateLimitExceeded` event for audit/ops visibility.

## Consumers
- All Foundation services rely on standardized headers and correlation IDs enforced here; no direct contracts beyond proxying but downstream services assume header presence and validated tokens.
