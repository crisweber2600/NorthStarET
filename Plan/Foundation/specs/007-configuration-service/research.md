# Research: Configuration Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Hierarchical resolution (system -> district -> school) with materialized view/cache**  
  - Rationale: meets p95 <50ms read requirement; cache invalidated on write with fallback to source.  
  - Alternatives: on-the-fly resolution at read time (slower, more DB load).

- **PostgreSQL + Redis cache**  
  - Rationale: strong consistency for writes with tenant isolation; Redis accelerates reads and stores effective configuration snapshots.  
  - Alternatives: Cosmos DB (new dependency), in-memory cache only (single-node only).

- **Event notifications via MassTransit (ConfigurationChanged, CalendarUpdated, TemplateUpdated)**  
  - Rationale: downstream services refresh derived data; aligns with event-driven principle.  
  - Alternatives: polling (laggy), direct HTTP callbacks (tight coupling).

- **Audit-first approach using immutable audit table**  
  - Rationale: required by constitution and FR-010; ensures visibility into who changed which setting.  
  - Alternatives: change tracking tables only (less context).

## Open Questions
1. Cache TTL strategy for read-mostly APIs—prefer write-through with immediate invalidation and short TTL (e.g., 5 min) for safety?
2. Required merge fields for notification templates; confirm HTML vs text storage approach.
3. Do calendar conflicts block publish or allow draft with warnings? Specs assume block; confirm with stakeholders.
