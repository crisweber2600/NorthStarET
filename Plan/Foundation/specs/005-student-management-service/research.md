# Research: Student Management Service
Layer: Foundation
Version: 0.1.0

## Event-Driven Integration
- MassTransit selected (already pattern in repo) for publish/subscribe.
- Azure Service Bus in production; RabbitMQ locally via Aspire.

## Duplicate Detection & Merge
- Common patterns: deterministic key (Name+DOB+School) vs ML fuzzy match (out of scope).
- Simple deterministic candidate list acceptable for first phase.

## Photo Storage
- Blob naming convention: `{tenant}/{students}/{studentId}/photo.jpg`.
- Consider CDN integration future for faster retrieval.

## Import Validation
- Pre-validate mandatory fields; accumulate errors; reject if any critical errors.
- Could adopt streaming parse for very large CSVs (future optimization).

## Dashboard Composition Patterns
| Pattern | Pros | Cons |
|---------|------|------|
| Orchestrated fan-out (Chosen) | Simple, explicit error handling | Coupled aggregator logic |
| Aggregation via GraphQL gateway | Flexible queries | Additional infra overhead |
| Async precomputed cache | Fast reads | Complexity + eventual consistency |

Fan-out chosen; may evolve to cached aggregates later.

## Observability Considerations
- High-cardinality student IDs avoided in metrics; use counts not per-id metrics.

## Open Questions
1. Need for student state machine (Active, Withdrawn, Merged)? – Domain model includes enumerated status.
2. Bulk export concurrency limits? – Define max concurrent exports to manage load.
3. Virus scanning integration timeline? – Placeholder until security tooling ready.

---
Manual research artifact.