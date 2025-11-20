Feature: Identity Service: Microsoft Entra ID Authentication & Authorization
Short Name: identity-service
Target Layer: CrossCuttingConcerns
Business Value: Modernize authentication, enable SSO, reduce maintenance overhead by consolidating identity flows under Microsoft Entra ID with secure session, MFA, tenant isolation, and audit capabilities.

Scenarios:
Scenario 1: Staff Member Logs In Using Microsoft Entra ID SSO
Given NorthStar is configured to use Microsoft Entra ID exclusively for authentication
And a staff member has an active Entra ID account
And the staff member is assigned to a school district tenant
When the staff member navigates to the NorthStar login page
And clicks "Sign in with Microsoft"
Then they are redirected to Microsoft Entra ID login
And after successful authentication, they are redirected back to NorthStar
And their session is established with proper tenant context
And they see their district-specific dashboard
And their role-based permissions are loaded from Entra ID claims and NorthStar role mapping

Scenario 2: Administrator Logs In Using Entra ID with Multi-Factor Authentication
Given Microsoft Entra ID is configured to require MFA for administrator accounts
And an administrator has an Entra ID account with MFA enabled
When the administrator attempts to log in
Then they are prompted for credentials and second factor
And after successful MFA verification, they receive a JWT token
And the token includes admin claims and tenant context
And they can access administrative features across all assigned districts

Scenario 3: Microsoft Entra ID Configuration and User Provisioning
Given NorthStar previously used IdentityServer for authentication
And Entra ID tenant is configured and apps registered
And users exist in legacy IdentityServer database
When the migration process runs
Then user accounts are matched by email to Entra ID
And profiles created with Entra subject references
And ExternalProviderLinks established
And roles, permissions, tenant associations preserved
And users log in using Entra ID only
And legacy passwords marked deprecated

Scenario 4: Token Refresh and Session Management (Entra ID Only)
Given a staff member is logged in with active session via Entra ID
And their JWT access token is nearing expiration (within 5 minutes)
When the client detects approaching expiration
Then it requests token refresh from Entra ID
And NorthStar validates refresh token
And a new access token is issued with updated claims
And session continues uninterrupted and refresh is audited

Scenario 5: Cross-District Access with Tenant Switching (Entra ID)
Given a staff member is assigned to multiple districts
And authenticated via Entra ID in District A context
When they select District B
Then a new token issued with District B context
And dashboard refreshes with District B data
And permissions re-evaluated
And only District B data accessible
And tenant switch audited

Scenario 6: Password Reset Flow via Microsoft Entra ID
Given a user forgot their password
When they click "Forgot Password"
Then they are redirected to Entra ID reset flow
And receive reset email
And after completing reset they log in with new credentials
And password reset event logged

Scenario 7: Role-Based Authorization Check (Entra ID)
Given a teacher authenticated via Entra ID
And they attempt to access Student Enrollment
When application checks permissions
Then JWT token validated
And role assignments loaded
And permission evaluated <50ms P95
And decision cached for session

Scenario 8: Session Termination and Logout (Entra ID)
Given a staff member has an active session
When they click Logout
Then access token marked revoked
And refresh token invalidated
And redirected to Entra ID logout
And session cookie cleared
And subsequent requests rejected

Scenario 9: Service-to-Service Authentication (Entra ID)
Given Student Management Service calls Assessment Service
When it makes an API request
Then includes service principal token
And gateway validates token
And service identity confirmed
And tenant context forwarded

Scenario 10: Failed Authentication Handling (Entra ID)
Given a user attempts login with invalid credentials
When Entra ID rejects authentication
Then user sees error
And failed attempt logged
And after 5 failures account temporarily locked (policy)
And user notified
And admin can unlock in portal

Acceptance Criteria:
1. Entra ID replaces legacy IdentityServer flows.
2. Secure SSO + MFA enforced per admin policy.
3. Migration establishes ExternalProviderLinks and preserves roles.
4. Robust session lifecycle: exchange, refresh (<200ms P95), revoke.
5. Tenant switch re-issues scoped token & audits action.
6. Password reset entirely delegated to Entra ID with audit.
7. Authorization decisions cached & performant (<50ms P95).
8. Logout revokes tokens server-side and clears client session.
9. Service-to-service tokens validated centrally at gateway.
10. Failed auth attempts audited with lockout policy respected.
