# Data Model: API Gateway Configuration

**Feature ID**: 006-api-gateway-orchestration  
**Storage**: Configuration files + config store (optional Redis-backed cache)  
**Last Updated**: 2025-11-20

## Configuration Entities
- **RouteConfig**
  - Fields: `route_id`, `path`, `methods`, `cluster_id`, `transformations` (header inject/strip, query transforms), `version` (v1|v2), `auth_policy` (SessionRequired|AllowAnonymous), `rate_limit_policy_id`, `timeout_ms`.
  - Notes: emitted as YARP route entries; source of truth in config store with validation pipeline.

- **ClusterConfig**
  - Fields: `cluster_id`, `destinations` (list of `uri`, `weight`, `health`), `circuit_breaker` (failure_threshold, sampling_duration), `load_balancing` (RoundRobin|LeastRequests), `metadata` (service name, layer).
  - Notes: supports per-destination overrides for timeouts and TLS settings.

- **RateLimitPolicy**
  - Fields: `policy_id`, `window` (timespan), `permit_limit`, `queue_limit`, `segmentation_key` (TenantId|UserId), `exempt_roles`.
  - Notes: backed by Redis counters to enforce per-tenant throttles.

- **AuthPolicy**
  - Fields: `policy_id`, `audiences`, `scopes`, `allow_session`, `forward_headers` (X-Tenant-Id, X-User-Id).
  - Notes: references Microsoft.Identity.Web configuration; tied to route.

- **HealthCheckSource**
  - Fields: `service_name`, `endpoint`, `expected_status`, `timeout_ms`, `tags`.
  - Notes: aggregated by gateway `/health/aggregate`.

## Observability
- Logs/metrics annotated with `route_id`, `cluster_id`, `tenant_id` (when injected), `correlation_id`.
- OpenTelemetry spans emitted for each proxied call with route attributes; sampling tuned via ServiceDefaults.

## Validation Rules
- Route path must not overlap without explicit precedence rules.
- Auth policy required unless explicitly marked `AllowAnonymous` (rare).
- Rate limit policy required for external-facing routes; internal-only routes may opt out with justification.
- Versioned routes (v1/v2) must include deprecation metadata for v1.
