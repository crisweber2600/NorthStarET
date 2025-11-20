# Implementation Plan - Identity Migration to Entra ID

## Layer Identification
- **Target Layer**: Foundation
- **Justification**: Core identity services are part of the Foundation layer.
- **Cross-Layer Dependencies**:
  - `Src/Foundation/shared/Infrastructure` (Auth handlers)

## Technical Context
- **Feature**: Migration to Microsoft Entra ID
- **Goal**: Replace IdentityServer with Entra ID for all authentication.
- **Current State**: Legacy IdentityServer in use.
- **Architectural Impact**: High. Fundamental change to auth stack.

## Constitution Check
- [x] **Layer Compliance**: Target layer is Foundation.
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

### 1. Entra ID Setup
- [ ] Configure Entra ID Tenant.
- [ ] Register Applications (Web, API).
- [ ] Configure Claims (TenantId, Roles).

### 2. Code Migration
- [ ] Remove IdentityServer dependencies.
- [ ] Install `Microsoft.Identity.Web`.
- [ ] Configure `JwtBearer` authentication.
- [ ] Implement `SessionAuthenticationHandler`.

### 3. Data Migration
- [ ] Create `ExternalProviderLinks` table.
- [ ] Develop migration script (Email matching).
- [ ] Run migration in staging.

### 4. Verification
- [ ] Verify Login/Logout.
- [ ] Verify Token Validation.
- [ ] Verify Role Mapping.
