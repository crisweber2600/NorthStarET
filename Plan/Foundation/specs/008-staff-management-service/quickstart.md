# Quickstart: Staff Management Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL with RLS enabled; Redis optional for directory caching.
- Azure Service Bus configured for MassTransit.
- Blob storage path for import error reports (shared with Data Import Service).

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/services/Staff/Staff.API.csproj -c Release
dotnet run --project Src/Foundation/services/Staff/Staff.AppHost/Staff.AppHost.csproj
```
- Configure DB/ASB/Redis connection strings via environment variables or user-secrets before running.

## Smoke & Validation
```pwsh
dotnet test tests/staff-service/ -c Release
```
- Tests cover CRUD, assignment FTE validation, overlap detection, and event emission.
- To simulate certification reminders, run the reminder worker with a short window:
  ```pwsh
  dotnet run --project Src/Foundation/services/Staff/Staff.Workers/Staff.Reminders.csproj -- --window-days 1
  ```

## Import Flow
- Publish staff import templates via Data Import Service, then trigger import:
  ```pwsh
  dotnet run --project Src/Foundation/services/Staff/Staff.Tools/Staff.Import.csproj -- --file samples/staff.csv --tenant DemoDistrict
  ```
- Review error report URI for failed rows; re-run after fixes.

## Evidence
- Capture Red/Green output from `dotnet test` and import tool dry-runs.
- Keep MassTransit captured messages for `StaffCreated` and `StaffAssignmentChanged`.
