# Quickstart: Section & Roster Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL; Redis suggested for schedule cache.
- Azure Service Bus for roster/rollover events.

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/services/Section/Section.API.csproj -c Release
dotnet run --project Src/Foundation/services/Section/Section.AppHost/Section.AppHost.csproj
```
- Configure DB/ASB/Redis settings via environment variables or user-secrets.

## Smoke Tests
```pwsh
dotnet test tests/section-service/ -c Release
```
- Confirms conflict detection, waitlist promotion, seat capacity enforcement, and rollover dry-run behavior.

## Common Workflows
- Create section and add roster entries:
  ```pwsh
  curl -X POST https://localhost:5006/api/sections -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json" -d '{ "courseCode": "ALG1", "periodId": "1A", "capacity": 30 }'
  curl -X POST https://localhost:5006/api/sections/{id}/roster -d '{"studentIds":["..."]}' -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json"
  ```
- Trigger rollover dry-run:
  ```pwsh
  curl -X POST https://localhost:5006/api/sections/rollover -d '{"sourceYear":2025,"targetYear":2026,"dryRun":true}' -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json"
  ```

## Evidence
- Capture Red/Green outputs from `dotnet test`.
- Save events emitted during roster and rollover flows: `SectionCreated`, `StudentAddedToRoster`, `RolloverCompleted`.
