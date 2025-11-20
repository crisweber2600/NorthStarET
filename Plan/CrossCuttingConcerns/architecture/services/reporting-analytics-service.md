# Reporting & Analytics Service

## Overview

The Reporting & Analytics Service provides data visualization, dashboards, and analytical reporting for the NorthStar LMS platform using CQRS read models and aggregated data from domain services.

## Service Classification

- **Type**: Supporting Service
- **Phase**: Phase 4 (Weeks 25-30)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Reporting/`
- **Priority**: Medium (supports decision-making)
- **LMS Role**: Analytics dashboards and reports for administrators and teachers

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (*Graph, Export, Print controllers)  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database with direct queries

**Key Components**:
- Various graph/chart controllers
- Export to Excel functionality
- Print report generation
- Dashboard aggregations

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Reporting.API/                  # UI Layer (REST endpoints, GraphQL)
Reporting.Application/          # Application Layer (queries, projections)
Reporting.Domain/               # Domain Layer (read models, aggregates)
Reporting.Infrastructure/       # Infrastructure Layer (CQRS read DB, event projections)
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Query API**: GraphQL (Hot Chocolate) for flexible reporting
- **Data Access**: EF Core with SQL Server (read-optimized schema)
- **Messaging**: MassTransit + Azure Service Bus (event subscriptions)
- **Caching**: Redis for dashboard data
- **Orchestration**: .NET Aspire hosting
- **Reporting**: Export to Excel (EPPlus), PDF generation

### Owned Data

**Database**: `NorthStar_Reporting_DB` (CQRS Read Models)

**Tables** (denormalized for query performance):
- StudentPerformanceSummary (StudentId, SubjectArea, AvgScore, BenchmarkPeriod, GradeLevel, SchoolId)
- InterventionEffectiveness (InterventionId, TotalStudents, SuccessRate, AvgDuration)
- AttendanceSummary (StudentId, SchoolYear, TotalDays, PresentDays, AbsentDays)
- AssessmentResults (StudentId, AssessmentId, Score, Percentile, BenchmarkPeriod, SchoolId, DistrictId)
- SchoolMetrics (SchoolId, SchoolYear, TotalStudents, AvgScore, InterventionRate, updated daily)
- DistrictDashboard (DistrictId, SchoolYear, TotalSchools, TotalStudents, DistrictAvgScore, updated daily)

### Service Boundaries

**Owned Responsibilities**:
- CQRS read model maintenance (event projections)
- Dashboard data aggregation
- Report generation (Excel, PDF)
- Data visualization endpoints
- GraphQL query API for flexible reporting
- Caching for performance

**Not Owned**:
- Source data (students, assessments, interventions) → Domain services
- Real-time operational data → Domain services

**Cross-Service Coordination**:
- Subscribe to events from all domain services
- Build denormalized read models
- No direct database access to domain service DBs

### Domain Events Subscribed

- `StudentEnrolledEvent`, `AssessmentScoredEvent`, `InterventionCompletedEvent`, `ProgressRecordedEvent`, etc.
- Subscribes to events from: Student, Assessment, Intervention, Section services

### API Functional Intent

**Dashboard Queries**:
- District-level performance dashboard
- School-level performance dashboard
- Teacher dashboard (class performance)
- Student performance overview

**Reports**:
- Assessment results by school/grade
- Intervention effectiveness reports
- Student progress reports
- Benchmark comparison reports

**Export**:
- Export reports to Excel
- Export to PDF
- Scheduled report generation

### Service Level Objectives (SLOs)

- **Availability**: 99% uptime during business hours
- **Dashboard Load**: < 500ms p95
- **Report Generation**: < 5 seconds for 1,000 records
- **Event Processing**: < 1 minute latency for dashboard updates
- **Consistency Model**: Eventually consistent (asynchronous event projections)

### Security & Compliance

**Authorization**:
- **District Admin**: All district reports
- **School Admin**: School-specific reports
- **Teachers**: Class/section reports
- **Students**: Own progress reports (future)

**Data Protection**:
- Aggregated data, PII minimized
- FERPA compliance
- Audit logging for report access

### Testing Requirements

**Reqnroll BDD Features**:
- `dashboard-performance.feature`
- `report-generation.feature`
- `event-projection.feature`

**Test Coverage Target**: ≥ 80%

### Migration Strategy

**Phase 4, Weeks 25-30**:
1. Foundation and read models (Weeks 25-26)
2. Event projections (Weeks 27-28)
3. Reports and dashboards (Weeks 29-30)

### Dependencies

**Upstream**: All domain services (event subscriptions)  
**Downstream**: None

### Implementation Checklist

- [ ] CQRS read model database design
- [ ] Event projection handlers
- [ ] GraphQL API setup
- [ ] Dashboard endpoints
- [ ] Report generation (Excel, PDF)
- [ ] Caching layer
- [ ] Testing and deployment

### Open Questions / Risks

1. **Data Volume**: Large districts generate massive event streams. Need efficient projection strategy.
2. **Historical Data**: Backfilling read models from legacy data requires ETL.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
