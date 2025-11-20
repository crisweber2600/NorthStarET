# Contracts: Data Migration ETL
Layer: Foundation

Primary functionality is internal; no new public API endpoints required for migration.

## Internal Console Arguments (Proposed)
- `--manifest path` : JSON/YAML specifying entity order & batch sizes.
- `--resume` : Continue from checkpoints.
- `--dry-run` : Execute mapping & validation without writes.

## Possible Future Public Endpoints (Post-Migration Ops)
- GET /api/migration/status/{tenantId}
- GET /api/migration/report/{entityType}/{tenantId}

---