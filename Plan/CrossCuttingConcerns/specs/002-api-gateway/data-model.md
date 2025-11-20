# Data Model: API Gateway

## Configuration Entities (appsettings.json structure)

### RouteConfig
- **RouteId**: String
- **ClusterId**: String
- **Match**: Path/Method/Header
- **Transforms**: Header/Path modifications
- **Authorization**: Policy name

### ClusterConfig
- **ClusterId**: String
- **Destinations**: Address list
- **HealthCheck**: Path/Interval
- **LoadBalancing**: Policy (RoundRobin, etc.)

## Runtime Entities

### RequestContext
- **CorrelationId**: Guid
- **TenantId**: Guid
- **UserId**: Guid
- **ClientIp**: String
