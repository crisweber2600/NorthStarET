# Research: Student Management Service Migration
Layer: Foundation
Version: 0.1.0

## Decisions
- **Adopt EF Core 9 + Npgsql for persistence**  
  - Rationale: aligns with multi-tenant posture (feature 002) with RLS and tenant_id columns; first-class support for PostgreSQL UUIDs.  
  - Alternatives considered: Dapper-only (faster but loses change tracking for rich aggregates), MongoDB (not aligned with shared infra).

- **Use MassTransit + Azure Service Bus for domain events**  
  - Rationale: consistent with other Foundation services and Constitution Principle 4 event-driven discipline; enables StudentCreated/Enrolled/Withdrawn/Merged notifications.  
  - Alternatives: direct HTTP callbacks (tight coupling, harder to scale), native Azure Service Bus SDK (less symmetry with other services).

- **Store photos in blob storage with metadata in PostgreSQL**  
  - Rationale: keeps binaries out of the DB, supports tenancy via container path prefix, and leverages existing blob helpers.  
  - Alternatives: database bytea (bloats DB, slower backup), third-party CDN (adds dependency not in current infra).

- **Dashboard aggregation via cached read models**  
  - Rationale: meets p95 latency goals (<200ms) by precomputing aggregates in Redis-backed read models refreshed by events.  
  - Alternatives: live fan-out calls to other services (fragile, slower).

## Open Questions
1. Do we require photo moderation/virus scanning hooks before upload completion? (Assume yes; integrate with shared scanning job.)
2. Which downstream services subscribe to StudentMerged events for reconciliation (Assessment, Section, Reports)? Confirm contract expectations.
3. Preferred uniqueness keys for duplicate detection during import (email + DOB? state ID?). Document final rule set.
