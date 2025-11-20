# NorthStar Migration Scenarios - Complete Inventory Checklist

**Purpose**: Track all Given-When-Then scenarios across the migration  
**Last Updated**: November 16, 2025  
**Total Scenarios**: 82+ across 13 files  
**Status**: ✅ Complete Coverage of All 11 Microservices + Cross-Cutting Concerns

---

## Scenario Files Overview

| # | File | Scenarios | Service/Aspect | Phase | Status |
|---|------|-----------|----------------|-------|--------|
| 1 | 01-identity-migration-entra-id.md | 10 | Identity & Authentication | Phase 1 | ✅ Complete |
| 2 | 02-multi-tenant-database-architecture.md | 12 | Database Layer (All Services) | Phase 1 | ✅ Complete |
| 3 | 03-ui-migration-preservation.md | 12 | UI Migration | Phase 4 | ✅ Complete |
| 4 | 04-data-migration-etl.md | 12 | Data Migration | All Phases | ✅ Complete |
| 5 | 05-student-management-service.md | 12 | Student Management | Phase 2 | ✅ Complete |
| 6 | 06-api-gateway-orchestration.md | 12 | API Gateway (YARP) | Phase 1 | ✅ Complete |
| 7 | 07-configuration-service.md | 12 | Configuration Service | Phase 1 | ✅ Complete |
| 8 | 08-staff-management-service.md | 12 | Staff Management | Phase 2 | 🔄 To Create |
| 9 | 09-assessment-service.md | 12 | Assessment Service | Phase 2 | 🔄 To Create |
| 10 | 10-intervention-management-service.md | 12 | Intervention Management | Phase 3 | 🔄 To Create |
| 11 | 11-section-roster-service.md | 12 | Section & Roster | Phase 3 | 🔄 To Create |
| 12 | 12-data-import-service.md | 12 | Data Import & Integration | Phase 3 | 🔄 To Create |
| 13 | 13-reporting-analytics-service.md | 12 | Reporting & Analytics | Phase 4 | 🔄 To Create |
| 14 | 14-content-media-service.md | 12 | Content & Media | Phase 4 | 🔄 To Create |
| 15 | 15-system-operations-service.md | 12 | System Operations | Phase 4 | 🔄 To Create |
| 16 | 16-testing-strategy.md | 10 | Testing & QA | All Phases | 🔄 To Create |
| 17 | 17-deployment-devops.md | 10 | Deployment & DevOps | All Phases | 🔄 To Create |
| 18 | 13-digital-ink-service.md | 12 | Digital Ink Service | Phase 4 | ✅ Complete |

**Legend**: ✅ Complete | 🔄 To Create | ⚠️ In Progress

---

## Detailed Scenario Inventory

### 01. Identity Migration to Entra ID ✅

**File**: `01-identity-migration-entra-id.md` | **Scenarios**: 10 | **Service**: Identity & Authentication

- [x] Scenario 1: Staff Member Logs In Using Entra ID SSO
- [x] Scenario 2: Administrator Logs In Using Entra ID with MFA
- [x] Scenario 3: Legacy User Account Migration to Entra ID
- [x] Scenario 4: Token Refresh and Session Management
- [x] Scenario 5: Cross-District Access with Tenant Switching
- [x] Scenario 6: Password Reset Flow via Entra ID
- [x] Scenario 7: Role-Based Authorization Check
- [x] Scenario 8: Session Termination and Logout
- [x] Scenario 9: Service-to-Service Authentication
- [x] Scenario 10: Failed Authentication Handling

**Coverage**: SSO, MFA, token management, RBAC, service auth, error handling

---

### 02. Multi-Tenant Database Architecture ✅

**File**: `02-multi-tenant-database-architecture.md` | **Scenarios**: 12 | **Aspect**: Database Layer

- [x] Scenario 1: Student Record Created with Tenant Isolation
- [x] Scenario 2: Query Filtering by Tenant Context
- [x] Scenario 3: Cross-Tenant Access Prevention
- [x] Scenario 4: Database Migration from Per-District to Multi-Tenant
- [x] Scenario 5: LMS Service Creates Base Database Schema
- [x] Scenario 6: Row-Level Security Policy Enforcement
- [x] Scenario 7: Multi-Service Database Access with Tenant Context
- [x] Scenario 8: Database Backup and Restore with Multi-Tenancy
- [x] Scenario 9: Performance Optimization for Multi-Tenant Queries
- [x] Scenario 10: New District Onboarding
- [x] Scenario 11: Audit Trail with Tenant Context
- [x] Scenario 12: Database Connection Pooling with Tenant Isolation

**Coverage**: RLS, tenant isolation, migration, performance, security

---

### 03. UI Migration with Preservation ✅

**File**: `03-ui-migration-preservation.md` | **Scenarios**: 12 | **Aspect**: UI Migration

- [x] Scenario 1: Student Dashboard Screen Migration
- [x] Scenario 2: Teacher Viewing Student List (Before/After)
- [x] Scenario 3: Assessment Entry Form Migration
- [x] Scenario 4: Incremental Component Migration
- [x] Scenario 5: Visual Regression Testing During Migration
- [x] Scenario 6: API Integration Without Backend Changes
- [x] Scenario 7: User Settings and Preferences Preservation
- [x] Scenario 8: Responsive Design Maintenance
- [x] Scenario 9: Navigation and Menu Structure Preservation
- [x] Scenario 10: Search and Filter Functionality Preservation
- [x] Scenario 11: Print and Export Features Preservation
- [x] Scenario 12: Accessibility Features Maintained

**Coverage**: Screen migration, visual regression, no Figma required, accessibility

---

### 04. Data Migration ETL ✅

**File**: `04-data-migration-etl.md` | **Scenarios**: 12 | **Aspect**: Data Migration

- [x] Scenario 1: Student Records Migration with Tenant Tagging
- [x] Scenario 2: Assessment Results Migration (Large Volume)
- [x] Scenario 3: Foreign Key Relationship Transformation
- [x] Scenario 4: Dual-Write Pattern During Transition
- [x] Scenario 5: Data Validation and Reconciliation
- [x] Scenario 6: Historical Data Preservation
- [x] Scenario 7: Identity and GUID Mapping
- [x] Scenario 8: Handling Data Type Conversions
- [x] Scenario 9: Incremental Migration by Service
- [x] Scenario 10: Rollback Scenario for Failed Migration
- [x] Scenario 11: Reference Data Migration (Lookup Tables)
- [x] Scenario 12: Handling Denormalized Data

**Coverage**: 383 entities, 15 years data, validation, rollback, data types

---

### 05. Student Management Service ✅

**File**: `05-student-management-service.md` | **Scenarios**: 12 | **Service**: Student Management

- [x] Scenario 1: Create New Student with Event Publishing
- [x] Scenario 2: Other Services React to Student Created Event
- [x] Scenario 3: Update Student Demographics
- [x] Scenario 4: Search Students with Tenant Isolation
- [x] Scenario 5: Enroll Student in School
- [x] Scenario 6: Bulk Student Import via CSV
- [x] Scenario 7: Student Dashboard Query Optimization
- [x] Scenario 8: Handle Student Withdrawal
- [x] Scenario 9: Student Data Privacy and FERPA Compliance
- [x] Scenario 10: Student Merge After Duplicate Detection
- [x] Scenario 11: Student Photo Upload and Management
- [x] Scenario 12: Student Export for State Reporting

**Coverage**: CRUD, events, FERPA, bulk import, state reporting, photos

---

### 06. API Gateway & Orchestration ✅

**File**: `06-api-gateway-orchestration.md` | **Scenarios**: 12 | **Component**: API Gateway

- [x] Scenario 1: Route Request to New Microservice
- [x] Scenario 2: Route Request to Legacy Monolith During Migration
- [x] Scenario 3: Authentication Validation at Gateway
- [x] Scenario 4: Rate Limiting by Tenant
- [x] Scenario 5: Cross-Origin Resource Sharing (CORS)
- [x] Scenario 6: Request Logging and Correlation IDs
- [x] Scenario 7: Health Check Aggregation
- [x] Scenario 8: Circuit Breaker for Failing Service
- [x] Scenario 9: Request Transformation and Header Injection
- [x] Scenario 10: API Versioning Support
- [x] Scenario 11: Load Balancing Across Service Instances
- [x] Scenario 12: Request Size Limits and Validation

**Coverage**: Routing, auth, rate limiting, CORS, circuit breakers, monitoring

---

### 07. Configuration Service ✅

**File**: `07-configuration-service.md` | **Scenarios**: 12 | **Service**: Configuration

- [x] Scenario 1: District Administrator Creates District Settings
- [x] Scenario 2: Configure Academic Calendar for School Year
- [x] Scenario 3: Create School Within District
- [x] Scenario 4: Configure Grade Levels and Subjects
- [x] Scenario 5: Multi-Tenant Configuration Isolation
- [x] Scenario 6: System-Wide Settings vs. District-Specific
- [x] Scenario 7: Custom Attributes and Fields
- [x] Scenario 8: Grading Scale Configuration
- [x] Scenario 9: State-Specific Compliance Settings
- [x] Scenario 10: Navigation Menu Customization
- [x] Scenario 11: Notification and Email Templates
- [x] Scenario 12: Configuration Change Audit Trail

**Coverage**: Districts, schools, calendars, grading scales, state compliance

---

### 08. Staff Management Service 🔄

**File**: `08-staff-management-service.md` | **Scenarios**: 12 | **Service**: Staff Management | **Status**: To Create

**Planned Scenarios**:
- [ ] Create staff member with role assignment
- [ ] Staff profile management and updates
- [ ] Team creation and member assignment
- [ ] Staff-school assignment (multi-school)
- [ ] Staff role and permission management
- [ ] Staff search and filtering
- [ ] Staff schedule and availability
- [ ] Staff certification and credential tracking
- [ ] Staff performance review workflow
- [ ] Staff import via CSV
- [ ] Staff directory and contact information
- [ ] Staff audit trail and compliance

---

### 09. Assessment Service 🔄

**File**: `09-assessment-service.md` | **Scenarios**: 12 | **Service**: Assessment | **Status**: To Create

**Planned Scenarios**:
- [ ] Create assessment definition with fields
- [ ] Assign assessment to students
- [ ] Record assessment results
- [ ] Benchmark management and grading
- [ ] Assessment scoring and calculation
- [ ] Assessment search and filtering
- [ ] Assessment analytics and trends
- [ ] Assessment result exports
- [ ] State test data integration
- [ ] Assessment template library
- [ ] Assessment scheduling
- [ ] Assessment audit and compliance

---

### 10. Intervention Management Service 🔄

**File**: `10-intervention-management-service.md` | **Scenarios**: 12 | **Service**: Intervention | **Status**: To Create

**Planned Scenarios**:
- [ ] Create intervention group
- [ ] Assign students to intervention
- [ ] Track intervention attendance
- [ ] Record intervention progress notes
- [ ] Intervention scheduling and calendar
- [ ] Intervention effectiveness tracking
- [ ] Intervention resource management
- [ ] Multi-tier intervention support (MTSS/RTI)
- [ ] Parent communication for interventions
- [ ] Intervention reporting and analytics
- [ ] Intervention plan templates
- [ ] Intervention compliance tracking

---

### 11. Section & Roster Service 🔄

**File**: `11-section-roster-service.md` | **Scenarios**: 12 | **Service**: Section & Roster | **Status**: To Create

**Planned Scenarios**:
- [ ] Create class section with teacher
- [ ] Add students to section roster
- [ ] Section scheduling and periods
- [ ] Automated year-end rollover
- [ ] Section capacity management
- [ ] Co-teaching and multiple instructors
- [ ] Section search and filtering
- [ ] Drop/add student from section
- [ ] Section attendance integration
- [ ] Section gradebook integration
- [ ] Section reports and rosters
- [ ] Historical section data preservation

---

### 12. Data Import & Integration Service 🔄

**File**: `12-data-import-service.md` | **Scenarios**: 12 | **Service**: Data Import | **Status**: To Create

**Planned Scenarios**:
- [ ] CSV file upload and validation
- [ ] Excel file import processing
- [ ] State test data import
- [ ] Data mapping and transformation
- [ ] Import error handling and reporting
- [ ] Scheduled/automated imports
- [ ] Import template management
- [ ] Field validation rules
- [ ] Duplicate detection during import
- [ ] Import audit trail
- [ ] Rollback failed imports
- [ ] Import job monitoring and status

---

### 13. Reporting & Analytics Service 🔄

**File**: `13-reporting-analytics-service.md` | **Scenarios**: 12 | **Service**: Reporting | **Status**: To Create

**Planned Scenarios**:
- [ ] Generate student report card
- [ ] Create custom report with filters
- [ ] Schedule recurring reports
- [ ] Report export to PDF/Excel
- [ ] Dashboard with real-time metrics
- [ ] Trend analysis and visualization
- [ ] State reporting templates
- [ ] Cross-district reporting (for multi-district)
- [ ] Report sharing and distribution
- [ ] Report caching and performance
- [ ] CQRS read model updates
- [ ] Report compliance and audit

---

### 14. Content & Media Service 🔄

**File**: `14-content-media-service.md` | **Scenarios**: 12 | **Service**: Content & Media | **Status**: To Create

**Planned Scenarios**:
- [ ] Upload file to Azure Blob Storage
- [ ] Upload and transcode video
- [ ] File metadata management
- [ ] File access control and permissions
- [ ] File download and streaming
- [ ] Help documentation management
- [ ] File search and categorization
- [ ] File versioning
- [ ] Bulk file operations
- [ ] File storage quota management
- [ ] Media thumbnail generation
- [ ] File cleanup and archival

---

### 15. System Operations Service 🔄

**File**: `15-system-operations-service.md` | **Scenarios**: 12 | **Service**: System Operations | **Status**: To Create

**Planned Scenarios**:
- [ ] Health check monitoring
- [ ] Application Insights integration
- [ ] Prometheus metrics collection
- [ ] Log aggregation and search
- [ ] Performance monitoring
- [ ] Error tracking and alerting
- [ ] Diagnostic endpoints
- [ ] System status dashboard
- [ ] Incident management
- [ ] Service dependency monitoring
- [ ] Capacity planning metrics
- [ ] SLO compliance tracking

---

### 16. Testing Strategy 🔄

**File**: `16-testing-strategy.md` | **Scenarios**: 10 | **Aspect**: Testing & QA | **Status**: To Create

**Planned Scenarios**:
- [ ] Unit test execution (TDD Red-Green)
- [ ] Reqnroll BDD feature implementation
- [ ] Aspire integration testing
- [ ] Playwright UI automation
- [ ] Visual regression testing
- [ ] Performance testing
- [ ] Load testing with tenant isolation
- [ ] Security testing (penetration)
- [ ] Accessibility testing (WCAG 2.1)
- [ ] Test data management

---

### 17. Deployment & DevOps 🔄

**File**: `17-deployment-devops.md` | **Scenarios**: 10 | **Aspect**: Deployment | **Status**: To Create

**Planned Scenarios**:
- [ ] GitHub Actions CI/CD pipeline
- [ ] Docker container build and push
- [ ] Kubernetes deployment to AKS
- [ ] Blue-green deployment
- [ ] Database migration execution
- [ ] Configuration management
- [ ] Secret rotation and management
- [ ] Monitoring and alerting setup
- [ ] Rollback procedure
- [ ] Production incident response

---

## Scenario Coverage by Phase

### Phase 1: Foundation (Weeks 1-8)
- ✅ Identity & Authentication (10 scenarios)
- ✅ API Gateway (12 scenarios)
- ✅ Configuration (12 scenarios)
- ✅ Multi-Tenant Database (12 scenarios)
- **Total**: 46 scenarios | **Status**: ✅ Complete

### Phase 2: Core Domain (Weeks 9-16)
- ✅ Student Management (12 scenarios)
- 🔄 Staff Management (12 scenarios) - To Create
- 🔄 Assessment (12 scenarios) - To Create
- **Total**: 36 scenarios | **Status**: 33% Complete (12/36)

### Phase 3: Secondary Domain (Weeks 17-22)
- 🔄 Intervention Management (12 scenarios) - To Create
- 🔄 Section & Roster (12 scenarios) - To Create
- 🔄 Data Import (12 scenarios) - To Create
- **Total**: 36 scenarios | **Status**: 0% Complete (0/36)

### Phase 4: Supporting (Weeks 23-28)
- 🔄 Reporting & Analytics (12 scenarios) - To Create
- 🔄 Content & Media (12 scenarios) - To Create
- 🔄 System Operations (12 scenarios) - To Create
- ✅ UI Migration (12 scenarios)
- **Total**: 48 scenarios | **Status**: 25% Complete (12/48)

### Cross-Cutting Concerns
- ✅ Data Migration ETL (12 scenarios)
- 🔄 Testing Strategy (10 scenarios) - To Create
- 🔄 Deployment & DevOps (10 scenarios) - To Create
- **Total**: 32 scenarios | **Status**: 38% Complete (12/32)

---

## Overall Progress

**Total Planned Scenarios**: 210 scenarios across 18 files  
**Completed**: 94 scenarios (45%)  
**To Create**: 116 scenarios (55%)

**Completed Files**: 8/18 (44%)  
**Services Covered**: 8/12 microservices (67%)

### Completion Checklist

**Foundation Services (Phase 1)**:
- [x] Identity & Authentication Service
- [x] API Gateway (YARP)
- [x] Configuration Service

**Core Services (Phase 2)**:
- [x] Student Management Service
- [ ] Staff Management Service
- [ ] Assessment Service

**Secondary Services (Phase 3)**:
- [ ] Intervention Management Service
- [ ] Section & Roster Service
- [ ] Data Import & Integration Service

**Supporting Services (Phase 4)**:
- [ ] Reporting & Analytics Service
- [ ] Content & Media Service
- [ ] System Operations Service
- [x] Digital Ink Service

**Cross-Cutting**:
- [x] Multi-Tenant Database Architecture
- [x] Data Migration ETL
- [x] UI Migration & Preservation
- [ ] Testing Strategy
- [ ] Deployment & DevOps

---

## Usage Instructions

### For Tracking Progress
1. Mark scenarios as complete using [x] when implemented
2. Update file status: ✅ Complete | 🔄 To Create | ⚠️ In Progress
3. Update completion percentages after each file is completed
4. Review this checklist weekly during migration

### For Implementation
1. Convert scenarios to Reqnroll .feature files
2. Implement step definitions for BDD testing
3. Use scenarios as acceptance criteria
4. Reference scenario file numbers in commit messages

### For Validation
1. Ensure all scenarios have test coverage
2. Verify all services have scenario files
3. Check that scenarios align with MASTER_MIGRATION_PLAN.md
4. Validate scenario technical notes match implementation

---

**Last Review**: November 16, 2025  
**Next Review**: Start of Phase 2 Implementation  
**Maintained By**: Architecture & QA Teams
