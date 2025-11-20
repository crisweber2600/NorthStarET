# NorthStar Migration Scenarios - Business-Focused Given-When-Then

This directory contains comprehensive Given-When-Then (Gherkin-style) scenarios for each major aspect of the NorthStar migration from monolith to microservices.

## Purpose

These scenario files provide a **business view** into the technical migration plan, making it easy for stakeholders to understand:
- What functionality is being built
- How users will interact with the system
- What success looks like for each feature
- How services integrate and communicate

## Scenario Files

### 01. Identity Service Migration to Microsoft Entra ID
**File**: [01-identity-migration-entra-id.md](./01-identity-migration-entra-id.md)  
**Scenarios**: 10  
**Coverage**:
- Staff member SSO login
- Administrator MFA authentication
- Legacy user account migration
- Token refresh and session management
- Cross-district access with tenant switching
- Password reset via Entra ID
- Role-based authorization
- Session termination
- Service-to-service authentication
- Failed authentication handling

**Key Business Value**: Modern cloud authentication, reduced maintenance, enterprise SSO

---

### 02. Multi-Tenant Database Architecture
**File**: [02-multi-tenant-database-architecture.md](./02-multi-tenant-database-architecture.md)  
**Scenarios**: 12  
**Coverage**:
- Student record creation with tenant isolation
- Query filtering by tenant context
- Cross-tenant access prevention
- Database migration from per-district to multi-tenant
- LMS service schema management
- Row-Level Security enforcement
- Multi-service database access
- Backup and restore procedures
- Performance optimization
- New district onboarding
- Audit trail with tenant context
- Connection pooling with isolation

**Key Business Value**: Consolidate 100s of databases into 11, reduce costs, simplify operations

---

### 03. UI Migration with Preservation Strategy
**File**: [03-ui-migration-preservation.md](./03-ui-migration-preservation.md)  
**Scenarios**: 12  
**Coverage**:
- Student dashboard screen migration
- Teacher viewing student list (before/after comparison)
- Assessment entry form migration
- Incremental component migration
- Visual regression testing
- API integration without backend changes
- User settings preservation
- Responsive design maintenance
- Navigation structure preservation
- Search and filter functionality
- Print and export features
- Accessibility features maintained

**Key Business Value**: Zero user retraining, maintain workflows, technology modernization only

---

### 04. Data Migration ETL Process
**File**: [04-data-migration-etl.md](./04-data-migration-etl.md)  
**Scenarios**: 12  
**Coverage**:
- Student records migration with tenant tagging
- Assessment results migration (large volume)
- Foreign key relationship transformation
- Dual-write pattern during transition
- Data validation and reconciliation
- Historical data preservation
- Identity and GUID mapping
- Data type conversions
- Incremental migration by service
- Rollback procedures
- Reference data migration
- Denormalized data handling

**Key Business Value**: Zero data loss, minimal downtime, validated integrity

---

### 05. Student Management Service
**File**: [05-student-management-service.md](./05-student-management-service.md)  
**Scenarios**: 12  
**Coverage**:
- Create new student with event publishing
- Event-driven service integration
- Update student demographics
- Search with tenant isolation
- Student enrollment process
- Bulk CSV import
- Student dashboard aggregation
- Student withdrawal handling
- FERPA compliance and privacy
- Duplicate student merge
- Photo upload and management
- State reporting export

**Key Business Value**: Modern student data management, event-driven architecture, FERPA compliant

---

### 06. API Gateway and Service Orchestration
**File**: [06-api-gateway-orchestration.md](./06-api-gateway-orchestration.md)  
**Scenarios**: 12  
**Coverage**:
- Route to new microservice
- Route to legacy monolith during migration
- Authentication validation
- Rate limiting by tenant
- CORS support
- Request logging and correlation
- Health check aggregation
- Circuit breaker for failing services
- Request transformation and header injection
- API versioning support
- Load balancing across instances
- Request size limits and validation

**Key Business Value**: Unified API entry point, seamless legacy-to-new migration, cross-cutting concerns

---

## How to Use These Scenarios

### For Product Managers
- **Validate Requirements**: Ensure scenarios match business needs
- **Acceptance Criteria**: Use scenarios as acceptance criteria for features
- **Stakeholder Communication**: Share scenarios with stakeholders to explain functionality
- **Sprint Planning**: Break scenarios into user stories for sprint planning

### For Developers
- **Reqnroll Implementation**: Convert scenarios to Reqnroll feature files
- **TDD Guidance**: Write failing tests based on scenarios (Red state)
- **Integration Testing**: Use scenarios to guide integration test cases
- **API Design**: Design APIs that support the scenarios

### For QA Engineers
- **Test Case Development**: Create detailed test cases from scenarios
- **BDD Testing**: Implement Reqnroll step definitions
- **Manual Testing**: Use scenarios as manual test scripts
- **Acceptance Testing**: Verify all scenarios pass before release

### For Architects
- **Design Validation**: Ensure architecture supports all scenarios
- **Service Boundaries**: Verify scenarios respect service boundaries
- **Event Flow**: Validate event-driven flows match scenarios
- **Performance**: Check SLOs in scenarios are achievable

### For Business Analysts
- **Process Documentation**: Use scenarios to document business processes
- **Training Materials**: Create training from scenario descriptions
- **User Documentation**: Write user guides based on scenarios
- **Change Management**: Communicate changes using scenario differences

## Scenario Format

Each scenario follows the **Given-When-Then** format:

```
## Scenario N: [Descriptive Title]

**Given** [Initial state / preconditions]
**And** [Additional preconditions]
**When** [Action or event occurs]
**And** [Additional actions]
**Then** [Expected outcome]
**And** [Additional outcomes]
**And** [Verification points]
```

### Components of Each File

1. **Header**: Feature name, epic, business value
2. **Scenarios**: 10-12 Given-When-Then scenarios per file
3. **Technical Notes**: Implementation details, code examples, schemas
4. **Performance SLOs**: Response time targets, throughput requirements
5. **Security Requirements**: Authentication, authorization, data protection

## Scenario Coverage Map

| Aspect | Scenarios | User Types | Services Covered |
|--------|-----------|------------|------------------|
| Identity Migration | 10 | Staff, Admin, Users | Identity, All Services |
| Multi-Tenant DB | 12 | All Users | All Services (data layer) |
| UI Migration | 12 | Teachers, Admins | Frontend, All APIs |
| Data Migration | 12 | System | All Services (data) |
| Student Service | 12 | Teachers, Admins | Student, Assessment, Section |
| API Gateway | 12 | All Users | Gateway, All Services |

**Total Scenarios**: 70+  
**Total Services Covered**: 11 (all microservices)  
**Total User Roles**: Staff, Teachers, Administrators, System

## Integration with Migration Plan

These scenarios directly support:
- **MASTER_MIGRATION_PLAN.md**: Implementation steps and success criteria
- **Constitutional Compliance**: TDD Red→Green workflow
- **Phase Deliverables**: Acceptance criteria for each phase
- **Testing Strategy**: BDD test requirements

## Contributing New Scenarios

When adding new scenarios:
1. Follow the Given-When-Then format
2. Focus on business value and user perspective
3. Include technical implementation notes
4. Specify performance SLOs
5. Document security requirements
6. Cross-reference with master plan

## Tools and Frameworks

**BDD Testing**:
- Reqnroll (formerly SpecFlow) for .NET
- Gherkin syntax for scenario descriptions
- xUnit for test execution

**UI Testing**:
- Playwright for visual regression
- Screenshot comparison tools

**Integration Testing**:
- .NET Aspire integration test projects
- TestContainers for database testing

---

**Version**: 1.0  
**Created**: November 16, 2025  
**Purpose**: Business-focused view of migration scenarios  
**Audience**: PMs, BAs, Developers, QA, Architects, Stakeholders
