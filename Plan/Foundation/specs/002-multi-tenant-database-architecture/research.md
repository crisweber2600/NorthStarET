# Research: Multi-Tenant Database Architecture
Layer: Foundation
Version: 0.1.0

## PostgreSQL Row-Level Security (RLS)
- Provides per-row filtering based on session context.
- `current_setting('app.current_tenant')` used for performant tenant filtering.
- Policies: separate for SELECT/INSERT/UPDATE/DELETE to tighten constraints.

## EF Core 9 Features Utilized
- Interceptors for connection/session initialization.
- Bulk operations via Npgsql COPY (manual ADO.NET integration) for migration performance.
- Global query filters for multi-tenant entities.

## Alternatives Considered
| Approach | Pros | Cons |
|----------|------|------|
| Separate DB per tenant | Strong isolation | Operational explosion (hundreds of DBs) |
| Schema per tenant | Moderate isolation | Schema proliferation, migration complexity |
| Shared tables + RLS (Chosen) | Operational simplicity, strong logical isolation | Requires careful policy + perf tuning |
| Shared tables + app filtering only | Simplicity | No defense-in-depth; risk of leaks |

## Performance Considerations
- Index design critical for high cardinality `tenant_id` distribution.
- Suggested composite indexes: `(tenant_id, created_at)`, `(tenant_id, last_name)`.
- Avoid sequential scans; ensure parameterized queries.

## Observability Patterns
- OpenTelemetry span attributes: `tenant.id`, `db.system=postgresql`.
- Metric cardinality controlled by top active tenants + aggregated bucket.

## Migration Strategy References
- COPY-based bulk ingest for large entity volumes (>1M rows).
- Legacy mapping tables maintain referential continuity during phased cutover.

## Security Reinforcement
- Gateway token validation ensures claim authenticity.
- Application sets session variable; no direct user control.
- Audit & periodic RLS policy diff check.

## Open Questions
1. Do any tables intentionally ignore tenant isolation? (Reference data) -> Document exceptions.
2. SLA for tenant onboarding latency? (Currently instant; verify provisioning script.)
3. Strategy for cross-tenant reporting (future aggregate store).

---
Manual research artifact (speckit automation unavailable).