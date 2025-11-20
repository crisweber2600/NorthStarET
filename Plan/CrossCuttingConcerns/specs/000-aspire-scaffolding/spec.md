# Feature Specification: Aspire Orchestration & Cross-Cutting Scaffolding

---
ado_work_item_id: 1401
ado_parent_id: 1400
ado_work_item_type: "User Story"
ado_url: "https://dev.azure.com/northstaret/NorthStarET/_workitems/edit/1401"
last_synced: "2025-11-20T19:59:53Z"
sync_status: "synced"
story_points: 89
priority: 1
tags:
  - aspire
  - orchestration
  - multi-tenancy
  - event-driven
  - observability
  - scaffolding
---

**Specification Branch**: `CrossCuttingConcerns/000-aspire-scaffolding-spec` *(planning artifacts only)*
**Implementation Branch**: `CrossCuttingConcerns/000-aspire-scaffolding` *(created after spec approval)*
**Created**: 2025-11-20
**Status**: Draft
**Input**: User description: "Aspire Orchestration & Cross-Cutting Scaffolding..."

---

## Layer Identification (MANDATORY)

**Target Layer**: CrossCuttingConcerns
*Select ONE layer where this feature will be implemented. If introducing a new layer, provide explicit name and justification.*

**Layer Validation Checklist**:
- [x] Layer explicitly identified (not "Other" or "TBD")
- [x] Layer exists in mono-repo structure (`Plan/CrossCuttingConcerns/`)
- [ ] If new layer: Architecture Review documented in `Plan/{LayerName}/README.md`
- [x] Cross-layer dependencies justified and limited to approved shared infrastructure

**Cross-Layer Dependencies**: Foundation shared infrastructure (AppHost, ServiceDefaults)
*If depending on another layer, list ONLY approved shared infrastructure components from `Src/Foundation/shared/`. Direct service-to-service dependencies across layers are prohibited.*

**Justification**: This feature establishes the cross-cutting patterns and orchestration for the entire platform, residing in the CrossCuttingConcerns layer to be referenced by all other layers.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - AppHost Boots Full Stack (Priority: P1)

As a developer, I want the AppHost to boot the full stack with all dependencies so that I can run the system locally with a single command.

**Why this priority**: Essential for developer productivity and consistent local environments.

**Independent Test**: Can be tested by running `dotnet run` and verifying dashboard health.

**Acceptance Scenarios**:

1. **Given** AppHost defines PostgreSQL, Redis, and RabbitMQ resources, **When** `dotnet run --project Src/Foundation/AppHost` is executed, **Then** all containers start in dependency order and Aspire dashboard lists healthy resources.

---

### User Story 2 - New Microservice Scaffolding (Priority: P2)

As a developer, I want to scaffold a new microservice quickly using a PowerShell/Bash script so that I can focus on business logic rather than boilerplate.

**Why this priority**: Accelerates development of new services.

**Independent Test**: Run the scaffold script and verify project structure and build.

**Acceptance Scenarios**:

1. **Given** a developer needs to add a new service, **When** they run the PowerShell or Bash scaffold script with a service name, **Then** a project is created with Application/Domain/Infrastructure/API folders, DependencyInjection.cs stubs, AppHost registration, and baseline tests (unit + health check).

---

### User Story 3 - Tenant Isolation Enforced Automatically (Priority: P1)

As a security officer, I want tenant isolation enforced automatically so that data leaks between tenants are prevented.

**Why this priority**: Critical security requirement for multi-tenant system.

**Independent Test**: Attempt to query data from another tenant and verify it returns empty/error.

**Acceptance Scenarios**:

1. **Given** `TenantInterceptor` is registered, **When** a repository queries entities, **Then** only rows with current `TenantId` are returned.
2. **Given** a method is decorated with `[IgnoreTenantFilter]` attribute, **When** a cross-tenant query is executed, **Then** the filter is bypassed and an audit log entry is created.

---

### User Story 4 - Event Publication on Domain Changes (Priority: P1)

As a system, I want to publish integration events when domain changes occur so that other services can react asynchronously.

**Why this priority**: Enables decoupled, event-driven architecture.

**Independent Test**: Trigger a domain event and verify message receipt in RabbitMQ.

**Acceptance Scenarios**:

1. **Given** a domain event is raised, **When** the transaction commits, **Then** an integration event is published via MassTransit and subscribers receive it.

---

### User Story 5 - Redis Caching & Idempotency (Priority: P2)

As a user, I want fast response times and protection against duplicate actions so that the system is performant and reliable.

**Why this priority**: Improves user experience and data integrity.

**Independent Test**: Send duplicate requests and verify only one is processed; check cache hits.

**Acceptance Scenarios**:

1. **Given** Redis is provisioned, **When** the same session is validated multiple times, **Then** lookup cost stays <20ms (cache hit).
2. **Given** an idempotent command, **When** submitted twice within the 10-minute window, **Then** the second call returns 202 Accepted with the original entity ID without reprocessing.

---

### User Story 6 - Unified Observability (Priority: P2)

As an operator, I want unified traces, metrics, and logs so that I can debug issues across service boundaries.

**Why this priority**: Essential for troubleshooting distributed systems.

**Independent Test**: Generate a request chain and view the trace in the dashboard.

**Acceptance Scenarios**:

1. **Given** OpenTelemetry is configured, **When** a request flows across services, **Then** a single trace appears in the dashboard with spans per hop.
And logs include correlation scope
And metrics expose request duration, rate, error counts.

---

### User Story 7 - Strangler Fig Legacy Routing (Priority: P2)

As a product owner, I want to route traffic between legacy and new services via feature flags so that we can migrate safely.

**Why this priority**: Enables safe, incremental migration.

**Independent Test**: Toggle feature flag and verify traffic routing changes.

**Acceptance Scenarios**:

1. **Given** routing rules exist, **When** a feature flag is flipped, **Then** traffic shifts seamlessly to the new microservice.

---

### User Story 8 - Resilient Messaging (Priority: P2)

As a system, I want messaging to be resilient with retries and DLQ so that transient failures don't lose data.

**Why this priority**: Ensures reliability of background processing.

**Independent Test**: Force a consumer failure and verify retry/DLQ behavior.

**Acceptance Scenarios**:

1. **Given** MassTransit is configured, **When** a consumer throws a transient exception, **Then** the message is retried and eventually moved to DLQ if it fails repeatedly.

---

### Edge Cases

- **Database Connection Failure**: AppHost should report unhealthy status; services should retry connection.
- **Redis Unavailable**: Caching should fail gracefully (fallback to DB); idempotency might block or fail safe.
- **RabbitMQ Down**: Event publishing should buffer or fail; consumers should stop.
- **Idempotency Window Expiration**: Requests arriving after 10-minute window are processed as new operations.

## Requirements *(mandatory)*

### Functional Requirements

- **FR1**: AppHost must define PostgreSQL, Redis, and RabbitMQ resources.
- **FR2**: All services must use `TenantInterceptor` for automatic tenant filtering; opt-out requires `[IgnoreTenantFilter]` attribute with automatic audit logging.
- **FR3**: Domain events must be published as integration events via MassTransit.
- **FR4**: Idempotency must be enforced using Redis with a 10-minute window; duplicate requests return 202 Accepted with the original entity ID.
- **FR5**: API Gateway must support Strangler Fig routing via feature flags.
- **FR6**: All services must export OpenTelemetry traces, metrics, and logs.
- **FR7**: Scaffolding scripts (PowerShell for Windows, Bash for Linux/macOS) must generate service structure with Application/Domain/Infrastructure/API projects, baseline tests, and AppHost registration.

### Non-Functional Requirements

- **NFR1 (Performance)**: AppHost startup < 15s.
- **NFR2 (Latency)**: API request overhead < 50ms P95.
- **NFR3 (Reliability)**: Event delivery < 500ms P95.
- **NFR4 (Security)**: No cross-tenant data access without explicit audit; all `[IgnoreTenantFilter]` usages must be logged with user context, timestamp, and affected query.

## Success Criteria *(mandatory)*

1. **AppHost Health**: All defined resources (Postgres, Redis, RabbitMQ) start and report healthy in Aspire dashboard.
2. **Scaffolding Efficiency**: New service scaffolding takes < 2 minutes and passes baseline tests.
3. **Tenant Isolation**: 100% of queries filter by TenantId by default; `[IgnoreTenantFilter]` bypasses are audited with full context (user, timestamp, query details).
4. **Event Reliability**: 99.9% of domain events result in published integration events.
5. **Observability Coverage**: 100% of inter-service requests generate a unified trace.
6. **Migration Safety**: Routing can be toggled instantly via feature flags with zero downtime.
7. **Performance Compliance**: CI builds fail if performance budgets (e.g., token exchange < 200ms) are exceeded.

## Assumptions

- Docker Desktop or compatible container runtime is installed.
- .NET 10 SDK and Aspire workload are available.
- Developers have access to NuGet feeds for dependencies.

## Clarifications

### Session 2025-11-20

- Q: Scaffolding Automation Approach - The spec mentions "scaffold script" but doesn't specify implementation. Options: dotnet new template, PowerShell/Bash script, CLI tool, VS extension. → A: PowerShell/Bash script for cross-platform compatibility and immediate usability
- Q: Idempotency Window Behavior on Duplicate - When a duplicate request arrives within the 10-minute window, how should the system respond? Options: return original success response, 409 Conflict, 429 Too Many Requests, silently accept 200 OK. → A: Return original success response (202 Accepted with original entity ID)
- Q: TenantInterceptor Opt-Out Mechanism - The spec mentions "explicit opt-out with reviewed justification" but doesn't specify the technical mechanism. Options: DbContext parameter, method attribute, separate DbContext, config flag. → A: Method attribute `[IgnoreTenantFilter]` with mandatory audit log entry
