# Research: Data Migration ETL
Layer: Foundation
Version: 0.1.0

## Bulk Ingest (PostgreSQL COPY)
- COPY significantly faster than multi-row INSERT.
- Binary format reduces parsing overhead.
- Use staging table when transformations require constraint relaxation.

## Resumability Patterns
| Pattern | Notes |
|---------|-------|
| Checkpoint last legacy PK | Simple; assumes immutable ordering |
| High-water mark per tenant | Robust for multi-tenant parallelism |
| Event sourcing log | Overkill for one-time migration |

Chosen: checkpoint last legacy PK + batch status per tenant.

## UUID Mapping
- Use deterministic mapping? (Not required; random v4 fine) if no external deterministic references.
- LegacyIdMapping ensures external systems referencing old IDs can resolve new UUID.

## Dual-Write Strategies
| Approach | Pros | Cons |
|----------|------|------|
| Synchronous transactional | Strong consistency | Latency overhead |
| Outbox + async consumer | Performance | Temporary divergence |

Chosen: synchronous for limited transition window.

## Data Quality
- Pre-migration profiling: null anomalies, invalid dates, FK orphans.
- Cleansing rules applied in mapper layer.

## Open Questions
1. Retention of legacy audit tables? (All audit classic entries needed?)
2. Compression of historical assessments beyond certain age? (Future optimization.)
3. Need for data masking in non-prod rehearsals?

---
Manual research artifact.