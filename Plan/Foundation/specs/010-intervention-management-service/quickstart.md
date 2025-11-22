# Quickstart: Intervention Management Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL; Redis optional for attendance rollups.
- Azure Service Bus for events; access to Configuration templates for communications.

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/services/Intervention/Intervention.API.csproj -c Release
dotnet run --project Src/Foundation/services/Intervention/Intervention.AppHost/Intervention.AppHost.csproj
```
- Set connection strings and ASB settings via environment variables or user-secrets.

## Smoke Tests
```pwsh
dotnet test tests/intervention-service/ -c Release
```
- Verifies creation, enrollment, conflict detection, attendance logging, and event emission.

## Demo Flow
1. Create an intervention template and intervention:
   ```pwsh
   curl -X POST https://localhost:5005/api/interventions/templates -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json" -d '{ "name": "Reading Support", "frequency": "Weekly", "durationMinutes": 45 }'
   ```
2. Enroll students (manual):
   ```pwsh
   curl -X POST https://localhost:5005/api/interventions/{id}/enrollments -d '{"studentIds":["..."]}' -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json"
   ```
3. Record attendance for generated sessions:
   ```pwsh
   curl -X POST https://localhost:5005/api/interventions/sessions/{sessionId}/attendance -d '{"studentId":"...","status":"Present"}' -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json"
   ```

## Evidence
- Capture Red/Green outputs from `dotnet test`.
- Save event traces for `InterventionCreated`, `InterventionEnrollmentChanged`, and `AttendanceRecorded` produced during the demo flow.
