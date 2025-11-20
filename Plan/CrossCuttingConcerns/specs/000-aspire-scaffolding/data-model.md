# Data Model: Aspire Orchestration & Cross-Cutting Scaffolding

**Feature**: aspire-scaffolding
**Date**: 2025-11-20

## Shared Domain Primitives

These primitives are defined in `Src/Foundation/shared/Domain/` and used by all services.

### 1. EntityBase
Base class for all entities.

```csharp
public abstract class EntityBase
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; } // Soft delete
    
    // Domain Events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### 2. ITenantEntity
Interface for multi-tenant entities.

```csharp
public interface ITenantEntity
{
    public Guid TenantId { get; set; }
}
```

### 3. AuditLog
Schema for audit logging (cross-cutting).

| Column | Type | Description |
|--------|------|-------------|
| Id | Guid | PK |
| TenantId | Guid | Tenant Context |
| UserId | Guid | User performing action |
| Action | String | e.g., "Create", "Update" |
| EntityType | String | e.g., "Student" |
| EntityId | Guid | ID of affected entity |
| Changes | JsonB | Serialized changes |
| Timestamp | DateTime | UTC time |

## Configuration Schemas

### 1. Aspire AppHost Configuration (`appsettings.json`)

```json
{
  "Postgres": {
    "Password": "[SECRET]"
  },
  "RabbitMq": {
    "User": "guest",
    "Password": "[SECRET]"
  },
  "Redis": {
    "ConnectionString": "..."
  }
}
```

### 2. Service Defaults Configuration

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "http://dashboard:18889",
  "Identity": {
    "Authority": "https://login.microsoftonline.com/...",
    "Audience": "api://northstar-api"
  }
}
```
