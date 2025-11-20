# Research Findings — Tenant-Isolated District Access

## Event Dispatch Mechanism

- **Decision**: Publish domain events to Azure Event Grid via the Aspire eventing extension.
- **Rationale**: Event Grid provides durable fan-out with built-in retry policies and decouples downstream subscribers, aligning with the requirement to keep external systems informed without tight coupling. Aspire can provision local emulation through the existing AppHost for developer parity.
- **Alternatives Considered**:
  - Azure Service Bus Topics — strong ordering guarantees but added management complexity for multiple downstream consumers; unnecessary for current fan-out pattern.
  - Direct HTTP webhooks — simpler but introduces tight coupling and reliability concerns for tenant isolation events.

## Audit Log Persistence Strategy

- **Decision**: Store audit entries in an append-only PostgreSQL table backed by EF Core and expose them via the Application layer, with an outbox process shipping relevant records to Event Grid.
- **Rationale**: PostgreSQL already hosts authoritative data, ensuring ACID semantics and simple query access for compliance reviews. The outbox pattern keeps audit publishing consistent with domain event processing.
- **Alternatives Considered**:
  - Dedicated audit microservice — overkill for initial slice; adds deployment complexity.
  - Redis-backed audit cache — lacks durability requirements for compliance and legal discovery.

## Redis Hosting Model Under Aspire

- **Decision**: Run Redis Stack as an Aspire-managed container for local development and integration tests, mirroring Azure Cache for Redis in production.
- **Rationale**: Aspire already orchestrates containerized dependencies and can inject connection strings via Service Defaults. Using Redis Stack locally ensures module availability (JSON, Bloom) should future idempotency rules need them.
- **Alternatives Considered**:
  - Embedded in-memory cache — insufficient for idempotency windows across scaled instances.
  - Managed Redis emulator outside Aspire — complicates developer setup and diverges from pipeline provisioning scripts.

## Tenant Isolation Enforcement

- **Decision**: Apply tenant-scoped query filters in the Application layer using MediatR pipelines plus EF Core global query filters to bind `DistrictId` from the authenticated principal; enforce additional guard clauses inside command handlers.
- **Rationale**: Combining pipeline behaviors with global filters ensures every query respects the scoped `DistrictId`, preventing accidental cross-tenant reads while keeping Domain aggregates unaware of infrastructure details.
- **Alternatives Considered**:
  - Database row-level security — powerful but would add migration and role complexity during the initial slice.
  - Manual where clauses per repository call — error-prone and easy to miss in future changes.

## Idempotent Command Handling

- **Decision**: Persist idempotency tokens in Redis keyed by `(ActorId, Action, ResourceId)` and cross-check payload hashes before invoking handlers; align with EF Core upsert patterns and unique constraints on district suffix and admin email.
- **Rationale**: Redis provides fast expiry-based storage aligned to the 10-minute window and supports distributed instances; pairing with database uniqueness guarantees consistency if Redis misses occur.
- **Alternatives Considered**:
  - Database-only unique indexes with retry — covers duplicates but provides poorer feedback when identical payloads arrive within the window.
  - In-memory cache — fails in multi-instance deployments and during restarts.

## Invitation Expiration & Resend Flow

- **Decision**: Track `InvitationSentAtUtc` and `InvitationExpiresAtUtc` in the aggregate, set to seven days on each send, and generate a deterministic verification token hashed into the invite link for idempotent resends.
- **Rationale**: Storing expiration inside the domain keeps business rules centralized and ensures resends refresh the window without duplicating invites or breaking existing links.
- **Alternatives Considered**:
  - Stateless JWT invite links — harder to revoke/resend cleanly and would require key rotation strategy upfront.
  - Email service-managed expiration — couples business rules to external infrastructure.
