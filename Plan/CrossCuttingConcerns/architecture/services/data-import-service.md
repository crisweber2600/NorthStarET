# Data Import Service

## Overview

The Data Import Service manages manual and automated data entry for the NorthStar LMS platform, including CSV/Excel imports for student/staff data and automated integration with state test data systems.

## Service Classification

- **Type**: Secondary Domain Service
- **Phase**: Phase 3 (Weeks 17-24)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/DataImport/`
- **Priority**: Medium (supports data entry workflows)
- **LMS Role**: Manual CSV/Excel import and automated state test data integration

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (DataEntryController, ImportStateTestDataController) + Batch Processors  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database

**Key Components**:
- Manual CSV/Excel upload and validation
- State test data import from external systems
- Batch processors for scheduled imports
- Data validation and error reporting

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
DataImport.API/                 # UI Layer (file upload endpoints)
DataImport.Application/         # Application Layer (import orchestrator, validators)
DataImport.Domain/              # Domain Layer (ImportJob, ValidationRule, Mapping aggregates)
DataImport.Infrastructure/      # Infrastructure Layer (file parsers, external APIs, Worker Service)
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **File Processing**: EPPlus (Excel), CsvHelper (CSV)
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus
- **Storage**: Azure Blob Storage for uploaded files
- **Orchestration**: .NET Aspire hosting
- **Background Jobs**: .NET Worker Service for automated imports

### Owned Data

**Database**: `NorthStar_DataImport_DB`

**Tables**:
- ImportJobs (Id, Type, FileName, UploadedBy, StartDate, CompletionDate, Status, TotalRows, SuccessRows, ErrorRows)
- ImportErrors (Id, JobId, RowNumber, ErrorMessage, FieldName, FieldValue)
- ImportMappings (Id, DistrictId, ImportType, SourceField, TargetField, TransformationRule)
- StateTestConfigurations (Id, DistrictId, StateCode, ApiEndpoint, ApiKey, ScheduledTime, IsActive)
- StateTestImportJobs (Id, ConfigId, ImportDate, TestYear, Status, RecordsImported)

### Service Boundaries

**Owned Responsibilities**:
- CSV/Excel file upload and parsing
- Data validation against business rules
- Field mapping and transformation
- Import job orchestration and monitoring
- State test data integration via external APIs
- Scheduled/automated import jobs
- Error reporting and correction workflows

**Not Owned**:
- Student/staff data storage → Student Management, Staff Management Services
- Assessment data storage → Assessment Service
- Data validation rules (domain-specific) → Respective domain services

**Cross-Service Coordination**:
- Publish events to trigger student/staff/assessment creation
- Subscribe to validation responses from domain services
- Call domain service APIs for data persistence

### Domain Events Published

**Event Schema Version**: 1.0

- `ImportJobStartedEvent` - New import job initiated
- `ImportJobCompletedEvent` - Import job finished
- `StateTestDataImportedEvent` - External test data imported
- `ImportErrorsDetectedEvent` - Validation errors found

### Domain Events Subscribed

- None (initiates workflows via file uploads or scheduled jobs)

### API Functional Intent

**Manual Import**:
- Upload CSV/Excel files
- Validate file structure and data
- Map fields to target entities
- Execute import and persist data
- Generate error reports

**Automated Import**:
- Configure state test integration
- Schedule automated import jobs
- Monitor job execution
- Retry failed imports

**Mapping Management**:
- Define field mappings per district
- Configure transformation rules
- Version mapping configurations

**Queries**:
- Get import job status and history
- Get import errors for correction
- Get mapping configurations

### Service Level Objectives (SLOs)

- **Availability**: 99% uptime during business hours
- **File Upload**: < 30 seconds for 10MB files
- **Import Processing**: < 5 minutes for 1,000 records
- **State Test Import**: Complete within 1 hour (scheduled nightly)
- **Consistency Model**: Eventually consistent (asynchronous processing)
- **Idempotency Window**: 24 hours (prevent duplicate imports)

### Security & Compliance

**Authorization**:
- **District Admin**: Full import management
- **School Admin**: Limited to school-specific imports
- **Data Entry Staff**: Upload and monitor jobs

**Data Protection**:
- Files encrypted at rest in Azure Blob Storage
- PII in import data encrypted
- Audit logging for all imports

**Secrets Management**:
- State test API keys in Azure Key Vault
- Storage account keys in Key Vault

### Testing Requirements

**Reqnroll BDD Features**:
- `manual-csv-import.feature` - CSV upload and validation
- `state-test-integration.feature` - Automated state test import
- `import-error-handling.feature` - Error detection and reporting
- `field-mapping.feature` - Mapping configuration

**Test Coverage Target**: ≥ 80%

### Migration Strategy

**Phase 3, Weeks 17-24**:
1. Foundation setup (Weeks 17-18)
2. Manual import features (Weeks 19-20)
3. Automated imports (Weeks 21-22)
4. Cutover and batch processor migration (Weeks 23-24)

**Data Migration**:
- Import job history preserved
- Mapping configurations migrated
- Legacy batch processors replaced with Worker Services

### Dependencies

**Upstream**: Identity, Configuration  
**Downstream**: Student Management, Staff Management, Assessment (publish events to trigger data creation)

### Implementation Checklist

**Phase 3, Weeks 17-24**:
- [ ] Clean Architecture project structure
- [ ] .NET Aspire orchestration
- [ ] Domain Layer (ImportJob, Validation, Mapping aggregates)
- [ ] Application Layer (file parsers, orchestrator)
- [ ] Infrastructure Layer (Azure Blob, external APIs, Worker Service)
- [ ] API Layer
- [ ] Reqnroll BDD features
- [ ] TDD Red → Green cycles
- [ ] Playwright UI tests (file upload workflows)
- [ ] Deploy and migrate batch processors

### Monitoring & Observability

**Metrics**:
- Import jobs per day
- Average processing time
- Error rate by import type
- State test integration success rate

**Logging**: Structured logging to Seq with import audit trail

### Open Questions / Risks

1. **File Size Limits**: Large districts may import files with 10,000+ rows. Need chunked processing.
2. **State Test APIs**: External APIs may have rate limits or downtime. Need robust retry logic.
3. **Data Quality**: Invalid data in imports can cascade to multiple services. Comprehensive validation critical.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
