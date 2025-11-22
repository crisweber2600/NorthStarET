# Quickstart: Data Import & Integration Service
Layer: Foundation

## Prerequisites
- .NET 8 SDK; PostgreSQL; blob storage for file artifacts/error reports.
- Azure Service Bus for events.
- Optional: SFTP endpoint credentials for scheduled runs.

## Build & Run
```pwsh
dotnet restore
dotnet build Src/Foundation/services/DataImport/DataImport.API.csproj -c Release
dotnet run --project Src/Foundation/services/DataImport/DataImport.AppHost/DataImport.AppHost.csproj
```
- Configure DB/blob/ASB/SFTP settings via environment variables or user-secrets.

## Upload + Validate
1. Register/import template:
   ```pwsh
   curl -X POST https://localhost:5007/api/import/templates -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json" -d @samples/student-template.json
   ```
2. Upload file and run validation:
   ```pwsh
   curl -F "file=@samples/students.csv" https://localhost:5007/api/import/jobs?templateId=<templateId> -H "Authorization: Bearer <jwt>"
   ```
3. Confirm validation results; start processing (if not auto-started):
   ```pwsh
   curl -X POST https://localhost:5007/api/import/jobs/<jobId>/start -H "Authorization: Bearer <jwt>"
   ```

## Scheduling
- Configure nightly SFTP import:
  ```pwsh
  curl -X POST https://localhost:5007/api/import/schedules -H "Authorization: Bearer <jwt>" -H "Content-Type: application/json" -d '{ "templateId":"...", "cron":"0 2 * * *", "timezone":"America/Chicago", "sftp":{"host":"sftp.example.com","path":"/exports"} }'
  ```

## Rollback
- Roll back a recent job within retention window:
  ```pwsh
  curl -X POST https://localhost:5007/api/import/jobs/<jobId>/rollback -H "Authorization: Bearer <jwt>"
  ```

## Evidence
- Capture `dotnet test tests/data-import-service/ -c Release` output (Red/Green).
- Archive validation and error report artifacts emitted during sample runs.
