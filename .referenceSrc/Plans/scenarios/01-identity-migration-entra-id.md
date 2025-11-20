# Identity Service Migration to Microsoft Entra ID

**Feature**: Migrate Identity Service from IdentityServer to Microsoft Entra ID  
**Epic**: Phase 1 - Foundation Services  
**Service**: Identity & Authentication Service  
**Business Value**: Modernize authentication, enable SSO, reduce maintenance overhead

---

## Scenario 1: Staff Member Logs In Using Entra ID SSO

**Given** the Identity Service is migrated to Microsoft Entra ID  
**And** a staff member has an active Entra ID account  
**And** the staff member is assigned to a school district tenant  
**When** the staff member navigates to the NorthStar login page  
**And** clicks "Sign in with Microsoft"  
**Then** they are redirected to Microsoft Entra ID login  
**And** after successful authentication, they are redirected back to NorthStar  
**And** their session is established with proper tenant context  
**And** they see their district-specific dashboard  
**And** their role-based permissions are loaded from the Identity Service

---

## Scenario 2: Administrator Logs In Using Entra ID with Multi-Factor Authentication

**Given** the Identity Service requires MFA for administrator accounts  
**And** an administrator has Entra ID account with MFA enabled  
**When** the administrator attempts to log in  
**Then** they are prompted for their username and password  
**And** they are prompted for their second factor (authenticator app or SMS)  
**And** after successful MFA verification, they receive a JWT token  
**And** the token includes admin claims and tenant context  
**And** they can access administrative features across all assigned districts

---

## Scenario 3: Legacy User Account Migration to Entra ID

**Given** a user exists in the legacy IdentityServer database  
**And** the user has an email address matching an Entra ID account  
**When** the user migration script runs  
**Then** their user profile is created in the new Identity Service database  
**And** their Entra ID external provider link is established  
**And** their existing roles and permissions are preserved  
**And** their tenant associations are maintained  
**And** they can log in using their Entra ID credentials  
**And** their legacy password is marked as deprecated

---

## Scenario 4: Token Refresh and Session Management

**Given** a staff member is logged in with an active session  
**And** their JWT access token is nearing expiration (within 5 minutes)  
**When** the client application detects the approaching expiration  
**Then** the application automatically requests a token refresh  
**And** the Identity Service validates the refresh token  
**And** a new access token is issued with updated claims  
**And** the user's session continues without interruption  
**And** the refresh is logged for security audit purposes

---

## Scenario 5: Cross-District Access with Tenant Switching

**Given** a staff member is assigned to multiple school districts  
**And** they are currently authenticated in District A context  
**When** they select District B from the tenant switcher  
**Then** a new token is issued with District B tenant context  
**And** their dashboard refreshes with District B data  
**And** their permissions are re-evaluated for District B  
**And** they can only access data belonging to District B  
**And** the tenant switch is logged for audit purposes

---

## Scenario 6: Password Reset Flow via Entra ID

**Given** a user has forgotten their password  
**When** they click "Forgot Password" on the login page  
**Then** they are redirected to Microsoft's password reset flow  
**And** they receive a password reset email from Microsoft  
**And** after completing the reset in Entra ID, they can log in with new credentials  
**And** the Identity Service logs the password reset event

---

## Scenario 7: Role-Based Authorization Check

**Given** a teacher is logged in with authenticated session  
**And** they attempt to access the "Student Enrollment" feature  
**When** the application checks their permissions via Identity Service  
**Then** the Identity Service validates their JWT token  
**And** retrieves their role assignments from the database  
**And** checks if "Student Enrollment" permission is granted  
**And** returns authorization decision within 50ms (P95 SLO)  
**And** the decision is cached for subsequent requests in the same session

---

## Scenario 8: Session Termination and Logout

**Given** a staff member has an active authenticated session  
**When** they click the "Logout" button  
**Then** their access token is marked as revoked in the Identity Service  
**And** their refresh token is invalidated  
**And** they are redirected to the Entra ID logout endpoint  
**And** their session cookie is cleared  
**And** they are redirected to the public login page  
**And** any subsequent requests with the old token are rejected

---

## Scenario 9: Service-to-Service Authentication

**Given** the Student Management Service needs to call the Assessment Service  
**When** Student Management Service makes an API request  
**Then** it includes its service principal token from Entra ID  
**And** the API Gateway validates the token with Identity Service  
**And** the Identity Service confirms the service identity  
**And** the service-to-service call is authorized  
**And** the request includes tenant context for data isolation

---

## Scenario 10: Failed Authentication Handling

**Given** a user attempts to log in with invalid credentials  
**When** Entra ID rejects the authentication  
**Then** the user is shown an appropriate error message  
**And** the failed attempt is logged in the Identity Service  
**And** after 5 failed attempts, the account is temporarily locked  
**And** the user receives notification about the locked account  
**And** an administrator can unlock the account via admin portal

---

## Technical Implementation Notes

**Migration Steps**:
1. Configure Microsoft Entra ID tenant and register NorthStar application
2. Implement OAuth 2.0/OIDC flow using Duende IdentityServer as proxy
3. Create ExternalProviderLinks table to map legacy users to Entra ID
4. Migrate user accounts with email matching
5. Implement token validation and claims transformation
6. Setup Row-Level Security policies for tenant isolation
7. Configure MFA policies in Entra ID
8. Implement token refresh mechanism
9. Setup audit logging for security events

**Database Schema**:
- `identity.Users` (tenant_id, entra_id, email, legacy_user_id)
- `identity.Roles` (tenant_id, role_name, permissions)
- `identity.UserRoles` (user_id, role_id, tenant_id)
- `identity.RefreshTokens` (user_id, token_hash, expires_at)
- `identity.ExternalProviderLinks` (user_id, provider, provider_user_id)
- `identity.AuditLog` (user_id, event_type, tenant_id, timestamp)

**Performance SLOs**:
- Authorization decision: <50ms (P95)
- Token validation: <20ms (P95)
- Entra ID login redirect: <200ms (P95)
- Token refresh: <100ms (P95)

**Security Requirements**:
- All tokens use RS256 signing
- Refresh tokens are single-use only
- Session timeout: 8 hours for staff, 1 hour for admins
- MFA required for admin roles
- Audit all authentication events
