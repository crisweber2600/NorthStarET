# Implementation Plan - Aspire Orchestration & Cross-Cutting Scaffolding

## Layer Identification
- **Target Layer**: CrossCuttingConcerns
- **Justification**: This feature establishes the foundational scaffolding, orchestration, and shared patterns used by all other layers. It belongs in CrossCuttingConcerns as it defines the architecture and shared infrastructure.
- **Cross-Layer Dependencies**:
  - `Src/Foundation/AppHost` (Orchestration)
  - `Src/Foundation/shared/Infrastructure` (Shared libraries)

## Technical Context
- **Feature**: Aspire Orchestration & Cross-Cutting Scaffolding
- **Goal**: Establish a consistent, reproducible scaffolding baseline so every slice inherits enforced tenant isolation, diagnostics, messaging, resiliency, and deployment orchestration.
- **Current State**: Initial project setup exists, but comprehensive scaffolding and enforced patterns are needed.
- **Architectural Impact**: High. Sets the standard for all microservices.

## Constitution Check
- [x] **Layer Compliance**: Target layer is CrossCuttingConcerns.
- [x] **Dependency Rule**: Only depends on shared infrastructure and AppHost.
- [x] **Pattern Compliance**: Follows Clean Architecture, Multi-Tenancy, and Aspire patterns.

## Phase 1: Design & Contracts

### Data Model
See `data-model.md`.

### API Contracts
See `contracts/` directory.

### Research
See `research.md`.

## Phase 2: Implementation Tasks

### 1. AppHost & Service Defaults
- [ ] Configure AppHost with PostgreSQL, Redis, RabbitMQ resources.
- [ ] Implement `ServiceDefaults` project with OpenTelemetry, HealthChecks, and Service Discovery.
- [ ] Ensure all services reference `ServiceDefaults`.

### 2. Shared Infrastructure
- [ ] Implement `TenantInterceptor` for EF Core.
- [ ] Implement Global Query Filters for Multi-Tenancy.
- [ ] Implement `IdempotencyService` using Redis.
- [ ] Configure MassTransit with RabbitMQ and Retry/DLQ policies.

### 3. API Gateway
- [ ] Configure YARP for route management.
- [ ] Implement Token Validation and Tenant Context propagation.
- [ ] Configure Rate Limiting per tenant.

### 4. Scaffolding Template
- [ ] Create a template/script for generating new microservices with all patterns pre-configured.

### 5. Verification
- [ ] Verify AppHost startup with all dependencies.
- [ ] Verify Tenant Isolation in a sample service.
- [ ] Verify Event Publication and Consumption.
- [ ] Verify Distributed Tracing in Aspire Dashboard.
