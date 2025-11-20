# Implementation Plan - API Gateway: YARP Service Orchestration

## Layer Identification
- **Target Layer**: CrossCuttingConcerns
- **Justification**: The API Gateway is the entry point for all services and handles cross-cutting concerns like routing, auth, and rate limiting.
- **Cross-Layer Dependencies**:
  - `Src/Foundation/shared/Infrastructure` (Auth, Logging)

## Technical Context
- **Feature**: API Gateway with YARP
- **Goal**: Provide a unified, secure, and observable entry point for the microservices architecture.
- **Current State**: No gateway exists; services are exposed directly or via legacy means.
- **Architectural Impact**: High. Decouples clients from backend topology.

## Constitution Check
- [x] **Layer Compliance**: Target layer is CrossCuttingConcerns.
- [x] **Dependency Rule**: Only depends on shared infrastructure.
- [x] **Pattern Compliance**: Follows BFF / Gateway pattern.

## Phase 1: Design & Contracts

### Data Model
See `data-model.md`.

### API Contracts
See `contracts/` directory.

### Research
See `research.md`.

## Phase 2: Implementation Tasks

### 1. YARP Configuration
- [ ] Configure Routes and Clusters in `appsettings.json`.
- [ ] Configure Load Balancing policies.
- [ ] Configure Health Checks.

### 2. Cross-Cutting Middleware
- [ ] Implement Correlation ID Middleware.
- [ ] Implement Global Exception Handling.
- [ ] Implement Request Logging.

### 3. Authentication & Authorization
- [ ] Integrate with Identity Service (Token Validation).
- [ ] Implement Tenant Context Extraction.
- [ ] Configure CORS policies.

### 4. Resiliency
- [ ] Configure Rate Limiting (per tenant).
- [ ] Configure Circuit Breakers (Polly).
- [ ] Configure Timeouts and Retries.

### 5. Verification
- [ ] Verify Routing to Microservices.
- [ ] Verify Legacy Routing (Strangler Fig).
- [ ] Verify Rate Limiting behavior.
- [ ] Verify Circuit Breaker behavior.
