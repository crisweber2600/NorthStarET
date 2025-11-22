# NorthStar Foundation Layer Migration Scenarios

This directory contains comprehensive Given-When-Then (Gherkin-style) scenarios for **domain services** in the Foundation layer migration from OldNorthStar monolith to microservices.

## Purpose

These scenario files provide a **business view** into the technical migration plan, making it easy for stakeholders to understand:
- What functionality is being built
- How users will interact with the system
- What success looks like for each feature
- How services integrate and communicate

## Infrastructure Scenarios

**Infrastructure scenarios (Identity, API Gateway, Configuration, Multi-Tenant Database) have been moved to**:  
📁 **[Plan/CrossCuttingConcerns/scenarios/](../../CrossCuttingConcerns/scenarios/)**

These infrastructure patterns are reusable across all mono-repo layers (Foundation, DigitalInk, future layers) and are documented as cross-cutting concerns.

---

## Domain Service Scenarios

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

### 06-12. Additional Domain Services

Additional domain service scenarios (Staff Management, Assessment, Intervention Management, Section & Roster, Data Import, Reporting, Content & Media) are documented in their respective scenario files:

- [08-staff-management-service.md](./08-staff-management-service.md)
- [09-assessment-service.md](./09-assessment-service.md)
- [10-intervention-management-service.md](./10-intervention-management-service.md)
- [11-section-roster-service.md](./11-section-roster-service.md)
- [12-data-import-service.md](./12-data-import-service.md)

---

## Cross-Layer Infrastructure

For scenarios covering infrastructure services used across all layers:
- **Identity Service**: [CrossCuttingConcerns/scenarios/01-identity-service.md](../../CrossCuttingConcerns/scenarios/01-identity-service.md)
- **API Gateway**: [CrossCuttingConcerns/scenarios/02-api-gateway.md](../../CrossCuttingConcerns/scenarios/02-api-gateway.md)
- **Configuration Service**: [CrossCuttingConcerns/scenarios/03-configuration-service.md](../../CrossCuttingConcerns/scenarios/03-configuration-service.md)
- **Multi-Tenant Database**: [CrossCuttingConcerns/patterns/multi-tenant-database.md](../../CrossCuttingConcerns/patterns/multi-tenant-database.md)

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

| Aspect | Scenarios | User Types | Services Covered | Location |
|--------|-----------|------------|------------------|----------|
| Identity Service | 10 | Staff, Admin, Users | Identity, All Services | [CrossCuttingConcerns](../../CrossCuttingConcerns/scenarios/01-identity-service.md) |
| API Gateway | 12 | All Users | Gateway, All Services | [CrossCuttingConcerns](../../CrossCuttingConcerns/scenarios/02-api-gateway.md) |
| Configuration | 12 | Admins | Configuration | [CrossCuttingConcerns](../../CrossCuttingConcerns/scenarios/03-configuration-service.md) |
| Multi-Tenant DB | 12 | All Users | All Services (data layer) | [CrossCuttingConcerns](../../CrossCuttingConcerns/patterns/multi-tenant-database.md) |
| UI Migration | 12 | Teachers, Admins | Frontend, All APIs | This directory |
| Data Migration | 12 | System | All Services (data) | This directory |
| Student Service | 12 | Teachers, Admins | Student, Assessment, Section | This directory |

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
