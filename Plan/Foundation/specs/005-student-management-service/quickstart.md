# Quickstart: Student Management Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL instance with tenant-aware schema; Redis (optional for dashboards).
- Azure Service Bus with MassTransit configured for publishing/subscribing student events.
- Blob storage for student photos.

## Build & Run (Aspire)
```pwsh
dotnet restore
dotnet build Src/Foundation/services/Student/Student.API.csproj -c Release
dotnet run --project Src/Foundation/services/Student/Student.AppHost/Student.AppHost.csproj
```
- The AppHost should start Student API + worker processes; configure connection strings via user-secrets or environment variables.

## Smoke Tests
```pwsh
dotnet test tests/student-service/ -c Release
```
- Ensure RLS is active by running tests with multiple tenant principals.
- Validate event publishing by asserting MassTransit harness receives `StudentCreated` and `StudentEnrolled`.

## Common Commands
- Seed demo data for a tenant:
  ```pwsh
  dotnet run --project Src/Foundation/services/Student/Student.Seeder/Student.Seeder.csproj -- --tenant DemoDistrict
  ```
- Import sample CSV:
  ```pwsh
  dotnet run --project Src/Foundation/services/Student/Student.Tools/Student.Import.csproj -- --file samples/students.csv --tenant DemoDistrict
  ```

## Evidence Capture
- Save Red/Green outputs from `dotnet test` and seeding/import scripts for the planning record.
- Keep MassTransit/ASB message traces for StudentCreated/Enrolled/Withdrawn during smoke runs.
