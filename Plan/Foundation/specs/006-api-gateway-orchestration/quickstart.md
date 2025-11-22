# Quickstart: API Gateway and Service Orchestration
Layer: Foundation

## Prerequisites
- .NET 8 SDK.
- Redis for distributed rate limiting counters.
- Access to JWKS/metadata endpoints for Entra ID (configured via Microsoft.Identity.Web).

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/gateway/Gateway.API.csproj -c Release
dotnet run --project Src/Foundation/gateway/Gateway.AppHost/Gateway.AppHost.csproj
```
- Configure `appsettings.Development.json` or environment variables for destination clusters, rate limit options, and allowed audiences.
- Ensure `X-Tenant-Id` and `X-User-Id` transforms are enabled on routes that proxy authenticated requests.

## Validate Routing
1. Hit a migrated service route:
   ```pwsh
   curl -I https://localhost:5443/api/students -H "Authorization: Bearer <jwt>"
   ```
2. Hit a legacy monolith passthrough route to confirm Strangler Fig coexistence.
3. Verify aggregated health:
   ```pwsh
   curl https://localhost:5443/health/aggregate
   ```

## Tests
```pwsh
dotnet test tests/gateway/ -c Release
```
- Includes BDD scenarios for auth, rate limiting, header transforms, and circuit breaking; perf harness (k6/NBomber) scripted under `tests/gateway/performance`.

## Config Deployment
- Store route/cluster/rate-limit config in the shared configuration store; gateway watches for changes and reloads.
- Validate config in staging before promotion using the config validation tool:
  ```pwsh
  dotnet run --project Src/Foundation/gateway/Gateway.Tools/Gateway.ConfigValidator.csproj -- --config path/to/routes.json
  ```
