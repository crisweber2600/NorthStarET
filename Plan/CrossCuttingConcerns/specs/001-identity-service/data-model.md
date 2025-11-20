# Data Model: Identity Service

## Entities

### User (Extended)
- **Id**: Guid
- **Email**: String
- **EntraObjectId**: String (New)
- **TenantId**: Guid

### ExternalProviderLink
- **UserId**: Guid
- **Provider**: String (e.g., "EntraID")
- **ProviderKey**: String (Subject ID)
- **LinkedAt**: DateTimeOffset

### Session
- **Id**: Guid
- **UserId**: Guid
- **Token**: String (Encrypted)
- **ExpiresAt**: DateTimeOffset
- **IpAddress**: String
