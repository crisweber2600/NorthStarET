# Quickstart: Assessment Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL; Redis optional for trend caching.
- Azure Service Bus for event publishing/subscription.
- Access to Configuration service for grading scale references.

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/services/Assessment/Assessment.API.csproj -c Release
dotnet run --project Src/Foundation/services/Assessment/Assessment.AppHost/Assessment.AppHost.csproj
```
- Configure DB/ASB connection strings and feature flags (imports/exports) via environment variables.

## Smoke Tests
```pwsh
dotnet test tests/assessment-service/ -c Release
```
- Validates definition creation, assignment idempotency, benchmark validation, result recording, and event emission.

## Import/Export
- Trigger a state test import (leveraging Data Import Service templates):
  ```pwsh
  dotnet run --project Src/Foundation/services/Assessment/Assessment.Tools/Assessment.Import.csproj -- --file samples/state-test.csv --tenant DemoDistrict
  ```
- Request an export:
  ```pwsh
  curl -X POST https://localhost:5003/api/assessments/exports -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json" -d '{ "filter": { "subject": "Math" } }'
  ```

## Evidence
- Capture Red/Green outputs from `dotnet test`.
- Persist event logs for `AssessmentCreated`, `AssessmentAssigned`, and `AssessmentResultRecorded` produced during smoke runs.
