# Implementation Plan - Identity Service: Microsoft Entra ID Authentication & Authorization

## Layer Identification
- **Target Layer**: CrossCuttingConcerns
- **Justification**: Identity is a cross-cutting concern used by all services. While the service itself might reside in Foundation, the *patterns* and *shared infrastructure* for auth are cross-cutting.
- **Cross-Layer Dependencies**:
  - `Src/Foundation/shared/Infrastructure` (Auth handlers)

## Technical Context
- **Feature**: Identity Service with Entra ID
- **Goal**: Modernize authentication using Entra ID, enabling SSO, MFA, and secure session management.
- **Current State**: Legacy IdentityServer needs migration.
- **Architectural Impact**: High. Replaces the entire auth stack.

## Constitution Check
- [x] **Layer Compliance**: Target layer is CrossCuttingConcerns (for the pattern/spec).
- [x] **Dependency Rule**: Only depends on shared infrastructure.
- [x] **Pattern Compliance**: Follows Entra ID / OIDC patterns.

## Phase 1: Design & Contracts

### Data Model
See `data-model.md`.

### API Contracts
See `contracts/` directory.

### Research
See `research.md`.

## Phase 2: Implementation Tasks

### 1. Entra ID Configuration
- [ ] Register App in Entra ID.
- [ ] Configure Redirect URIs.
- [ ] Define App Roles.

### 2. Shared Auth Infrastructure
- [ ] Implement `SessionAuthenticationHandler`.
- [ ] Implement `TokenExchangeService`.
- [ ] Configure `Microsoft.Identity.Web`.

### 3. Identity Service (Foundation)
- [ ] Implement `ExternalProviderLinks` table.
- [ ] Implement User Migration logic (Email matching).
- [ ] Implement Session Management (Redis + DB).

### 4. API Gateway Integration
- [ ] Configure Gateway to validate Entra Tokens.
- [ ] Implement Token Refresh logic.

### 5. Verification
- [ ] Verify SSO Login flow.
- [ ] Verify MFA enforcement.
- [ ] Verify Token Refresh.
- [ ] Verify Tenant Switching.
