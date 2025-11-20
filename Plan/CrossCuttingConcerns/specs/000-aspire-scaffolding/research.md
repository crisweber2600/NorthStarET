# Research: Aspire Orchestration & Cross-Cutting Scaffolding

**Feature**: aspire-scaffolding
**Date**: 2025-11-20
**Status**: Complete

## Decisions & Rationale

### 1. Orchestration: .NET Aspire 9.5
- **Decision**: Use .NET Aspire 9.5 (GA) for orchestration.
- **Rationale**: Aspire 9.5 supports .NET 10 GA (LTS) and includes file-based AppHost support, improved CLI tooling, and enhanced project templates. Provides native support for container orchestration, service discovery, and observability.
- **Alternatives Considered**: Docker Compose (manual), Kubernetes (too complex for local dev).
- **Package Version**: `Aspire.Hosting` 9.5.x, `Aspire.Hosting.PostgreSQL` 9.5.x, `Aspire.Hosting.Redis` 9.5.x, `Aspire.Hosting.RabbitMQ` 9.5.x

### 2. Multi-Tenancy: EF Core Interceptors & Global Query Filters
- **Decision**: Implement `TenantInterceptor` (SaveChangesInterceptor) with `[IgnoreTenantFilter]` attribute for opt-out scenarios.
- **Rationale**: Enforces tenant isolation at the database level automatically. Interceptors handle `TenantId` assignment on write, filters handle isolation on read. Opt-out via attribute ensures explicit intent with automatic audit logging.
- **Alternatives Considered**: Repository-level filtering (prone to error), DbContext parameter (not auditable), Separate databases per tenant (too many resources).
- **Package Version**: `Microsoft.EntityFrameworkCore` 10.0.x, `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.x

### 3. Messaging: MassTransit with RabbitMQ
- **Decision**: Use MassTransit as the abstraction layer over RabbitMQ.
- **Rationale**: Industry standard for .NET. Provides built-in support for retries, DLQ, and idempotency. Abstracts the underlying broker, allowing easier switch to Azure Service Bus in production.
- **Alternatives Considered**: Raw RabbitMQ Client (too much boilerplate), Dapr (adds sidecar complexity).

### 4. Idempotency: Redis-backed Envelope
- **Decision**: Implement `IdempotencyService` using Redis to store processed message hashes with 10-minute TTL, returning 202 Accepted with original entity ID on duplicates.
- **Rationale**: Redis provides fast, distributed locking and storage with TTL. Returning 202 with original ID ensures clients receive consistent responses without reprocessing.
- **Alternatives Considered**: Database table (slower), In-memory (not distributed), 409 Conflict response (misleading), 429 Too Many Requests (wrong semantic).
- **Package Version**: `StackExchange.Redis` 2.x

### 5. Observability: OpenTelemetry
- **Decision**: Use OpenTelemetry for traces, metrics, and logs.
- **Rationale**: Native integration with Aspire and .NET. Vendor-neutral standard.
- **Alternatives Considered**: Serilog only (lacks tracing), Application Insights SDK (vendor lock-in).

### 6. Scaffolding Scripts: PowerShell & Bash
- **Decision**: Implement cross-platform scaffolding scripts (PowerShell for Windows, Bash for Linux/macOS) that generate service structure with AppHost registration.
- **Rationale**: Immediate usability without additional tooling. Cross-platform support. Easy to maintain and customize. No distribution/versioning overhead compared to dotnet new templates or CLI tools.
- **Alternatives Considered**: `dotnet new` template (complex distribution), .NET CLI tool (NuGet packaging overhead), VS extension (Windows-only).
- **Script Features**: Service name parameter, Application/Domain/Infrastructure/API project generation, DependencyInjection.cs stubs, AppHost registration, baseline tests.

## Verification Steps (Phase 0)

- [x] **Aspire 9.5 GA**: Confirmed GA release with .NET 10 support via official Microsoft documentation.
- [x] **EF Core 10 GA**: Confirmed GA release (LTS) compatible with .NET 10.
- [x] **MassTransit Compatibility**: Version 8.x supports .NET 10 and Aspire DI patterns.
- [ ] **Package Version Testing**: Verify all packages install and work together without conflicts.

## References

- [Aspire Orchestration Pattern](../../../patterns/aspire-orchestration.md)
- [Multi-Tenancy Pattern](../../../patterns/multi-tenancy.md)
- [Constitution v2.2.0](../../../../.specify/memory/constitution.md)
