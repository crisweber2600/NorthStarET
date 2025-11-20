# Research: Identity Service

## Decisions

### 1. Identity Provider: Microsoft Entra ID
- **Decision**: Migrate to Entra ID (formerly Azure AD).
- **Rationale**: Corporate standard, built-in security (MFA, Conditional Access), reduces maintenance of custom IdentityServer.
- **Alternatives**: Auth0 (cost), Duende IdentityServer (maintenance burden).

### 2. Authentication Pattern: BFF (Backend for Frontend)
- **Decision**: Use BFF pattern with secure HTTP-only cookies for session management.
- **Rationale**: Prevents XSS token theft, standard security practice for SPAs.
- **Alternatives**: Storing tokens in LocalStorage (insecure).

### 3. Token Validation: Microsoft.Identity.Web
- **Decision**: Use Microsoft.Identity.Web library.
- **Rationale**: Official library, handles key rotation, validation, and claims transformation.
- **Alternatives**: System.IdentityModel.Tokens.Jwt (manual validation).

## Needs Clarification
- None.
