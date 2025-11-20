# Staff Management Service

## Overview

The Staff Management Service manages staff members, teachers, administrators, and support personnel for the NorthStar LMS platform. It integrates with Microsoft Entra ID for authentication while maintaining staff-specific profiles, teams, and role assignments within the educational context.

## Service Classification

- **Type**: Core Domain Service
- **Phase**: Phase 2 (Weeks 9-16)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Staff/`
- **Priority**: High (core administrative functionality)
- **LMS Role**: Staff management for teachers, administrators, and support personnel with Entra ID integration

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (StaffController, TeamMeetingController)  
**Framework**: .NET Framework 4.6  
**Database**: Shared `NorthStar` database (tables: Staff, Teams, TeamMembers, StaffRoles)

**Key Components**:
- `StaffController.cs` - Staff CRUD operations
- `TeamMeetingController.cs` - Team and meeting management
- Shared EF6 context accessing monolithic database

**Key Features**:
- Staff profile management (name, email, hire date, position)
- Role assignment (teacher, administrator, counselor, support)
- Team creation and membership
- Team meeting scheduling and notes
- School/district assignment

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Staff.API/                      # UI Layer (REST endpoints)
├── Controllers/
│   ├── StaffController.cs
│   ├── TeamsController.cs
│   └── RolesController.cs
├── Middleware/
└── Program.cs

Staff.Application/              # Application Layer
├── Commands/
│   ├── CreateStaffMember/
│   ├── UpdateStaffProfile/
│   ├── AssignStaffToSchool/
│   ├── CreateTeam/
│   └── AssignTeamMember/
├── Queries/
│   ├── GetStaffById/
│   ├── GetStaffBySchool/
│   ├── GetTeamMembers/
│   └── GetStaffByRole/
├── DTOs/
├── Interfaces/
└── ExternalProviders/
    └── EntraIdSync/            # Sync staff from Entra ID

Staff.Domain/                   # Domain Layer
├── Entities/
│   ├── StaffMember.cs
│   ├── Team.cs
│   ├── TeamMember.cs
│   ├── StaffRole.cs
│   ├── TeamMeeting.cs
│   └── StaffAssignment.cs
├── ValueObjects/
│   ├── EmployeeId.cs
│   ├── Position.cs
│   └── Certification.cs
├── Events/
│   ├── StaffMemberCreatedEvent.cs
│   ├── StaffAssignedToSchoolEvent.cs
│   ├── TeamCreatedEvent.cs
│   └── StaffMemberDeactivatedEvent.cs
└── Aggregates/
    └── StaffAggregate.cs

Staff.Infrastructure/           # Infrastructure Layer
├── Data/
│   ├── StaffDbContext.cs
│   └── Repositories/
│       ├── StaffRepository.cs
│       └── TeamRepository.cs
├── MessageBus/
└── ExternalServices/
    └── EntraIdProvider.cs
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **External Authentication**: Microsoft Entra ID integration for staff SSO and profile sync
- **Data Access**: EF Core with SQL Server
- **Messaging**: MassTransit + Azure Service Bus for domain events
- **Caching**: Redis for staff profile lookups
- **Orchestration**: .NET Aspire hosting

### Owned Data

**Database**: `NorthStar_Staff_DB`

**Tables**:
- Staff (Id, FirstName, LastName, Email, Position, EmployeeId, HireDate, TerminationDate, IsActive, EntraIdObjectId)
- StaffSchoolAssignments (Id, StaffId, SchoolId, DistrictId, StartDate, EndDate, IsPrimary)
- StaffRoles (Id, StaffId, RoleType, SchoolId, EffectiveDate, ExpirationDate)
- Teams (Id, Name, Purpose, SchoolId, DistrictId, LeaderId, CreatedDate, IsActive)
- TeamMembers (Id, TeamId, StaffId, Role, JoinedDate, LeftDate)
- TeamMeetings (Id, TeamId, MeetingDate, Agenda, Notes, CreatedBy)
- Certifications (Id, StaffId, CertificationType, IssueDate, ExpirationDate, CertifyingBody)
- EntraIdSyncLog (Id, StaffId, EntraIdObjectId, LastSyncDate, SyncStatus)

### Service Boundaries

**Owned Responsibilities**:
- Staff member profile management (name, contact, position, credentials)
- Staff-to-school and staff-to-district assignments
- Role management (teacher, administrator, counselor, etc.)
- Team creation and membership management
- Team meeting scheduling and documentation
- Certification and credential tracking
- **Microsoft Entra ID synchronization** for staff accounts
- Staff activation/deactivation

**Not Owned** (delegated to other services):
- Authentication and authorization → Identity Service (Entra ID tokens)
- School and district definitions → Configuration Service
- Section/class teaching assignments → Section & Roster Service
- Student data → Student Management Service

**Cross-Service Coordination**:
- Subscribe to `UserRegisteredEvent` from Identity Service for new staff accounts
- Publish `StaffMemberCreatedEvent` for downstream services
- Sync with Entra ID to maintain staff profiles

### Domain Events Published

**Event Schema Version**: 1.0

- `StaffMemberCreatedEvent` - When a new staff member is added
  ```
  - StaffId: Guid
  - FirstName: string
  - LastName: string
  - Email: string
  - Position: string
  - EmployeeId: string
  - EntraIdObjectId: string (optional)
  - Timestamp: DateTime
  ```

- `StaffAssignedToSchoolEvent` - When staff is assigned to a school
  ```
  - AssignmentId: Guid
  - StaffId: Guid
  - SchoolId: Guid
  - DistrictId: Guid
  - StartDate: DateTime
  - IsPrimary: bool
  - Timestamp: DateTime
  ```

- `StaffRoleAssignedEvent` - When a role is assigned to staff
  ```
  - StaffId: Guid
  - RoleType: string (Teacher, Administrator, Counselor)
  - SchoolId: Guid
  - EffectiveDate: DateTime
  - Timestamp: DateTime
  ```

- `TeamCreatedEvent` - When a new team is created
  ```
  - TeamId: Guid
  - Name: string
  - Purpose: string
  - SchoolId: Guid
  - LeaderId: Guid
  - Timestamp: DateTime
  ```

- `StaffMemberDeactivatedEvent` - When staff leaves or is terminated
  ```
  - StaffId: Guid
  - TerminationDate: DateTime
  - Reason: string
  - Timestamp: DateTime
  ```

### Domain Events Subscribed

- `UserRegisteredEvent` (from Identity Service) - Create staff profile when Entra ID user is provisioned
- `SchoolCreatedEvent` (from Configuration Service) - Track available schools for assignment
- `DistrictCreatedEvent` (from Configuration Service) - Track available districts

### API Functional Intent

**Staff Management**:
- Create and update staff profiles
- Assign staff to schools and districts
- Manage staff roles and permissions
- Track certifications and credentials
- Deactivate/terminate staff members
- Sync staff profiles from Microsoft Entra ID

**Team Management**:
- Create and manage teams
- Assign team members and roles
- Schedule and document team meetings
- Track team leadership changes

**Queries** (read-only):
- Get staff by school or district
- Get staff by role type
- Get team members and meeting history
- Get staff certifications and expiration dates

### Service Level Objectives (SLOs)

- **Availability**: 99.5% uptime during business hours (6 AM - 6 PM local time)
- **Staff Profile Retrieval**: < 150ms p95 for profile queries
- **Entra ID Sync**: Complete within 5 minutes of Entra ID changes
- **Team Operations**: < 300ms p95 for team CRUD operations
- **Consistency Model**: Eventually consistent for cross-service data; strongly consistent for staff assignments
- **Idempotency Window**: 10 minutes for duplicate profile updates

### Security & Compliance

**Authorization**:
- **System Admin**: Full staff management across all districts
- **District Admin**: Staff management within district
- **School Admin**: View staff in their school; limited edit permissions
- **Staff**: View own profile; limited self-service updates

**Data Protection**:
- PII (names, emails, employee IDs) encrypted at rest
- FERPA compliance for staff-student relationships
- Audit logging for all role assignments and terminations
- Entra ID integration uses OAuth 2.0 with least-privilege scopes

**Secrets Management**:
- Database connection strings in Azure Key Vault
- Entra ID client secrets in Key Vault
- No secrets in configuration files or code

### Testing Requirements

**Unit Tests** (Domain & Application layers):
- Staff profile validation logic
- Team membership rules
- Role assignment business rules
- Certification expiration calculations

**Reqnroll BDD Features**:
- `staff-onboarding.feature` - Creating new staff members
- `staff-school-assignment.feature` - Assigning staff to schools
- `team-management.feature` - Creating teams and managing membership
- `entra-id-sync.feature` - Synchronizing staff from Entra ID

**Integration Tests** (Aspire):
- Event publishing to Azure Service Bus
- Entra ID API integration
- Database persistence via EF Core
- Redis caching behavior

**Playwright UI Tests**:
- Staff profile management workflow (Figma: staff-profile-flow)
- Team creation and member assignment (Figma: team-management-flow)
- Staff directory search (Figma: staff-directory-search)

**Test Coverage Target**: ≥ 80% for Domain and Application layers

### Migration Strategy

**Strangler Fig Approach**:

1. **Phase 2, Week 9-10**: Foundation
   - Set up Staff.API with Clean Architecture structure
   - Configure .NET Aspire orchestration
   - Implement EF Core DbContext with migration scripts
   - Set up Entra ID integration

2. **Phase 2, Week 11-12**: Core Features
   - Migrate staff CRUD operations
   - Implement school/district assignment
   - Set up event publishing for staff lifecycle
   - API Gateway routes new endpoints

3. **Phase 2, Week 13-14**: Advanced Features
   - Migrate team management
   - Implement Entra ID sync workflows
   - Add certification tracking
   - Performance optimization (caching, indexing)

4. **Phase 2, Week 15-16**: Cutover
   - Validate data consistency between legacy and new DB
   - Switch API Gateway to route all traffic to new service
   - Deprecate legacy staff controllers
   - Monitor performance and error rates

**Data Migration**:
- Historical staff records migrated via ETL scripts
- Active staff profiles synced with Entra ID
- Team membership and meeting history preserved
- Rollback plan: revert API Gateway routing

**Legacy-New Interoperability**:
- During transition, both legacy and new services can query staff data
- Events published to support dependent services (Section, Assessment)
- Gradual feature migration with parallel operation

### Dependencies

**Upstream Services** (this service depends on):
- Identity Service (Entra ID authentication, user accounts)
- Configuration Service (school and district definitions)

**Downstream Services** (services that depend on this):
- Section & Roster Service (teachers for class sections)
- Assessment Service (teachers for assessment assignment)
- Intervention Management Service (staff responsible for interventions)

### Implementation Checklist

**Phase 2, Weeks 9-16**:

- [ ] Set up project structure with Clean Architecture
  - [ ] Staff.API
  - [ ] Staff.Application
  - [ ] Staff.Domain
  - [ ] Staff.Infrastructure
  - [ ] Staff.Tests (unit, integration, BDD)

- [ ] Configure .NET Aspire
  - [ ] Add service to AppHost
  - [ ] Configure Aspire Service Defaults
  - [ ] Set up distributed tracing

- [ ] Implement Domain Layer
  - [ ] Staff aggregate with business rules
  - [ ] Value objects (EmployeeId, Position, Certification)
  - [ ] Domain events (Created, Assigned, Deactivated)
  - [ ] Unit tests for domain logic

- [ ] Implement Application Layer
  - [ ] CQRS commands and queries
  - [ ] DTOs and mapping
  - [ ] Entra ID sync service
  - [ ] Event handlers
  - [ ] Application service tests

- [ ] Implement Infrastructure Layer
  - [ ] EF Core DbContext and configurations
  - [ ] Repository implementations
  - [ ] MassTransit event publishing
  - [ ] Entra ID integration (Microsoft.Graph SDK)
  - [ ] Database migrations
  - [ ] Redis caching

- [ ] Implement API Layer
  - [ ] REST controllers
  - [ ] Authentication/authorization middleware
  - [ ] API documentation (Swagger)
  - [ ] Health checks

- [ ] Testing
  - [ ] Write Reqnroll BDD features
  - [ ] Execute TDD Red → Green → Refactor cycles
  - [ ] Write Playwright UI tests (with Figma designs)
  - [ ] Aspire integration tests
  - [ ] Achieve ≥ 80% code coverage

- [ ] Data Migration
  - [ ] Design ETL scripts for historical data
  - [ ] Configure Entra ID sync
  - [ ] Validate data consistency

- [ ] Deployment
  - [ ] Configure Docker container
  - [ ] Set up Kubernetes manifests
  - [ ] Configure API Gateway routing
  - [ ] Deploy to staging environment
  - [ ] Execute smoke tests

- [ ] Production Cutover
  - [ ] Monitor performance and errors
  - [ ] Gradually increase traffic to new service
  - [ ] Deprecate legacy controllers
  - [ ] Post-migration validation

### Monitoring & Observability

**Application Insights**:
- Track staff creation rate
- Monitor Entra ID sync success/failure
- Alert on failed profile updates

**Custom Metrics**:
- Staff members created per day
- Active staff count by school/district
- Entra ID sync latency
- Team creation rate

**Distributed Tracing**:
- OpenTelemetry for request tracing across services
- Trace staff assignment workflow

**Logging**:
- Structured logging to Seq
- Log all role assignments with audit trail
- Error logging for Entra ID sync failures

### Open Questions / Risks

1. **Entra ID Sync Complexity**: Need to handle large organizations with thousands of staff. Pagination and rate limiting required.
2. **Profile Data Ownership**: Determine which fields sync from Entra ID vs. managed locally (e.g., position, certifications).
3. **Team Meeting Data Volume**: Historical meeting notes may be extensive. Consider archival strategy.
4. **Multi-School Staff**: Staff assigned to multiple schools need clear primary assignment logic.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
