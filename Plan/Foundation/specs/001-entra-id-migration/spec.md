Feature: Migration to Microsoft Entra ID
Short Name: entra-id-migration
Target Layer: Foundation
Business Value: Modernize authentication, enable SSO, reduce maintenance overhead by migrating to Microsoft Entra ID.

Scenarios:
Scenario 1: Staff Member Logs In Using Microsoft Entra ID SSO
Given NorthStar is configured to use Microsoft Entra ID exclusively
And staff member has active Entra ID account
And assigned to school district tenant
When navigating to login page and clicking "Sign in with Microsoft"
Then redirected to Entra ID login
And redirected back after success
And session established with tenant context
And district dashboard visible
And roles loaded from Entra ID claims.

Scenario 2: Administrator Logs In Using Entra ID with Multi-Factor Authentication
Given Entra ID requires MFA for admins
And admin has MFA enabled
When attempting login
Then prompted for username/password and second factor
And receive JWT token with admin claims and tenant context
And access administrative features.

Scenario 3: Microsoft Entra ID Configuration and Migration from Legacy IdentityServer
Given legacy IdentityServer usage
And Entra ID tenant configured
And users exist in legacy DB
When migration runs
Then users matched by email to Entra ID
And profiles created in new DB with Entra ID subject
And ExternalProviderLinks established
And roles/permissions preserved
And legacy passwords deprecated.

Scenario 4: Token Refresh and Session Management
Given staff logged in via Entra ID
And token nearing expiration
When client detects expiration
Then request refresh from Entra ID
And NorthStar validates refresh token
And new access token issued
And session continues uninterrupted.

Scenario 5: Cross-District Access with Tenant Switching
Given staff assigned to multiple districts
And authenticated via Entra ID
And in District A context
When selecting District B
Then new token issued with District B context
And dashboard refreshes
And permissions re-evaluated
And access restricted to District B.

Scenario 6: Password Reset Flow via Microsoft Entra ID
Given user forgot password
When clicking "Forgot Password"
Then redirected to Microsoft password reset
And receive email from Microsoft
And log in with new credentials after reset.

Scenario 7: Role-Based Authorization Check
Given teacher logged in via Entra ID
And attempts to access "Student Enrollment"
When application checks permissions
Then validate JWT token
And retrieve role assignments
And check permission
And return decision within 50ms.

Layer Identification:
Target Layer: Foundation
Layer Validation Checklist:
- [x] Layer exists in mono-repo
- [x] Meets constitutional requirements
Cross-Layer Dependencies:
- Src/Foundation/shared/Infrastructure (Auth handlers)
Justification: Core identity migration is a Foundation service capability.
Specification Branch: Foundation/001-entra-id-migration-spec
Implementation Branch: Foundation/001-entra-id-migration
