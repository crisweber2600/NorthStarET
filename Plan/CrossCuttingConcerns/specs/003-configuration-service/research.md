# Research - Configuration Service

## Technical Stack
- **Framework**: .NET 9 (Aspire)
- **Database**: PostgreSQL (EF Core 9)
- **Caching**: Redis (StackExchange.Redis)
- **Messaging**: MassTransit (RabbitMQ)

## Key Challenges
1. **Hierarchical Configuration**:
   - Need efficient way to resolve settings: School -> District -> System.
   - Strategy: Load all relevant scopes and merge in memory (or use recursive CTE if deep, but 3 levels is shallow).
   - Caching: Cache the *resolved* settings for a tenant/school to avoid re-computation.

2. **Multi-Tenancy**:
   - Strict isolation for writes.
   - Reads might need system defaults (cross-tenant conceptually, but system is a special tenant).

3. **Event Consistency**:
   - When a District is created, Identity Service MUST create the admin.
   - Use Outbox pattern (MassTransit) to ensure event is published if DB transaction commits.

## Existing Patterns
- **Identity Service**: Uses similar stack. Can copy `ServiceDefaults` and `Infrastructure` setup.
- **Gateway**: Needs route config update.

## References
- [Configuration Service Architecture](../../architecture/services/configuration-service.md)
- [Multi-Tenancy Pattern](../../patterns/multi-tenancy.md)
