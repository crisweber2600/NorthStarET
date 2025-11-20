# Research - Entra ID Migration

## Technical Stack
- **Provider**: Microsoft Entra ID (OIDC)
- **Library**: Microsoft.Identity.Web
- **Storage**: PostgreSQL (User profiles, Links)

## Key Decisions
1. **Auth Flow**: Authorization Code Flow with PKCE for SPA/Web.
2. **Token Validation**: Validate Audience, Issuer, and Signature.
3. **User Matching**: Match legacy users by Email Address.
4. **Role Mapping**: Map Entra ID App Roles to NorthStar Roles.

## References
- [Microsoft.Identity.Web Documentation](https://github.com/AzureAD/microsoft-identity-web)
- [Entra ID OIDC Spec](https://learn.microsoft.com/en-us/entra/identity-platform/v2-protocols-oidc)
