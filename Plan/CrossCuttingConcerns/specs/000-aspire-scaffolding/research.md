# Research: Aspire Orchestration & Cross-Cutting Scaffolding

## Decisions

### 1. Orchestration: .NET Aspire
- **Decision**: Use .NET Aspire for local orchestration and cloud deployment.
- **Rationale**: Native .NET integration, built-in dashboard, simplified service discovery, and easy provisioning of dependencies (Postgres, Redis, RabbitMQ).
- **Alternatives**: Docker Compose (manual), Kubernetes (too complex for local dev).

### 2. Messaging: MassTransit
- **Decision**: Use MassTransit as the message bus abstraction.
- **Rationale**: Industry standard for .NET, supports multiple transports (RabbitMQ, Azure Service Bus), built-in patterns (Saga, Retry, Outbox).
- **Alternatives**: Raw RabbitMQ client (too low level), Azure Service Bus SDK (vendor lock-in).

### 3. API Gateway: YARP
- **Decision**: Use YARP (Yet Another Reverse Proxy) as the API Gateway.
- **Rationale**: Highly customizable, .NET based, integrates well with Aspire service discovery.
- **Alternatives**: Ocelot (less active), Nginx (external config).

### 4. Multi-Tenancy: Database-per-Service + RLS
- **Decision**: Use Row-Level Security (RLS) in PostgreSQL for tenant isolation within shared databases per service.
- **Rationale**: Strong isolation without the overhead of database-per-tenant.
- **Alternatives**: Database-per-tenant (too many DBs), Discriminator column only (prone to developer error).

## Needs Clarification
- None.
