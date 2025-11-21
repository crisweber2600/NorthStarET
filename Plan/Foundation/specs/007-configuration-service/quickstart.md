# Quickstart: Configuration Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL and Redis.
- Azure Service Bus for change notification events.

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/services/Configuration/Configuration.API.csproj -c Release
dotnet run --project Src/Foundation/services/Configuration/Configuration.AppHost/Configuration.AppHost.csproj
```
- AppHost wires API + background publisher; configure DB/Redis/ASB via environment variables or user-secrets.

## Seed & Resolve
1. Seed setting definitions and defaults:
   ```pwsh
   dotnet run --project Src/Foundation/services/Configuration/Configuration.Seeder/Configuration.Seeder.csproj -- --seed defaults
   ```
2. Fetch resolved settings for a tenant/school:
   ```pwsh
   curl https://localhost:5001/api/config/resolved?schoolId=<schoolId> -H "Authorization: Bearer <jwt>"
   ```

## Tests
```pwsh
dotnet test tests/configuration-service/ -c Release
```
- Includes BDD for hierarchy overrides and cache invalidation; integration tests verify RLS and conflict detection for calendars/grades.

## Cache Invalidation Check
- Update a setting and confirm Redis cache refreshes:
  ```pwsh
  curl -X PUT https://localhost:5001/api/config/settings/<id> -d '{"value":"true"}' -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json"
  ```
- Observe `ConfigurationChanged` event on ASB and check Redis key `cfg:{tenant}:{school}:{key}` updates.
