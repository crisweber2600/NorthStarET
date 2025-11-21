# Research: API Gateway and Service Orchestration
Layer: Foundation
Version: 0.1.0

## Decisions
- **YARP for reverse proxy and transforms**  
  - Rationale: native .NET option with strong support for transforms, rate limiting, resiliency policies; aligns with Aspire hosting.  
  - Alternatives: NGINX/Envoy (heavier ops footprint), Ocelot (less active).

- **Microsoft.Identity.Web + session/BFF pattern**  
  - Rationale: reuse Identity/BFF session tokens; validation stays consistent with constitution’s Entra-only stance.  
  - Alternatives: custom JWT validation (duplicated logic), API keys (security gap).

- **Per-tenant rate limiting using AspNetCore Rate Limiting + Redis counters**  
  - Rationale: supports distributed throttling and tenancy alignment.  
  - Alternatives: in-memory (single instance only), external API management (new infra dependency).

- **Resilience via Polly policies with circuit breakers and retries on idempotent routes**  
  - Rationale: proven pattern; required by spec scenarios (circuit breaker, failure handling).  
  - Alternatives: YARP passive retries only (insufficient control).

## Open Questions
1. Which legacy routes require header transforms to match monolith expectations (e.g., auth cookies)? Document mapping list.
2. Do we need per-route JWT audiences for migrated services vs monolith? Clarify token audience matrix.
3. Health aggregation sources: include database/service health endpoints or only HTTP health? Define set for `_health/aggregate`.
