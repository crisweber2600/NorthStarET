# Data Model: Cross-Cutting Concerns

## Shared Entities (Infrastructure)

### Tenant
- **Id**: Guid (PK)
- **Name**: String
- **Identifier**: String (Unique)
- **Status**: Enum (Active, Inactive)

### AuditRecord
- **Id**: Guid (PK)
- **TenantId**: Guid (FK)
- **UserId**: Guid
- **Timestamp**: DateTimeOffset
- **Action**: String
- **Resource**: String
- **Details**: Json

### OutboxMessage
- **Id**: Guid (PK)
- **OccurredOn**: DateTimeOffset
- **Type**: String
- **Data**: Json
- **ProcessedDate**: DateTimeOffset?
- **Error**: String?

## Value Objects
- **TenantId**: Strongly typed ID
- **UserId**: Strongly typed ID
