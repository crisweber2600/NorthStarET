# /specify: User Authentication with OAuth 2.0

> **⚠️ OBSOLETE**: This specification document is from the WIPNorthStar research phase and references outdated Duende IdentityServer architecture.
> **Current Specification**: See `Plan/Foundation/specs/Foundation/002-identity-authentication/spec.md` for the active specification using Microsoft Entra ID with session authentication.

## Feature Title
Secure user authentication and token-based authorization for NorthStar LMS

## Goal / Why
Enable users (teachers, administrators, students) to securely access the NorthStar LMS through Microsoft Entra ID integration. The system validates JWT tokens issued by Entra ID and creates secure session-based authentication, replacing the legacy .NET Framework 4.6 IdentityServer. This establishes the foundation for all microservices security using modern cloud identity practices.

## Intended Experience / What
A user navigates to the login page, enters their email and password, and clicks "Login". The system validates credentials against the Identity database. On success, the user receives an access token (15-minute expiration) and refresh token (7-day expiration), stored securely in httpOnly cookies. The user is redirected to their role-appropriate dashboard. Invalid credentials show an error message without revealing whether email or password was wrong (security). After 5 failed attempts within 15 minutes, the account locks for 30 minutes. Users can click "Forgot Password" to receive a password reset email with a time-limited link.

## Service Boundary Outcomes
The Identity Service owns user authentication state, token issuance, and password management. It publishes `UserLoggedInEvent` and `UserLoggedOutEvent` for audit logging (consumed by Reporting Service). Token validation happens locally via JWT signature verification (no cross-service call). The service guarantees idempotent login within a 5-minute window (same session token returned for duplicate requests). Authentication SLO: p95 < 200ms for login, p99 < 500ms.

## Functional Requirements

**Must:**
- Validate email and password against Identity database using secure hash comparison (PBKDF2 with 10,000+ iterations)
- Issue JWT access token (15-min expiration) and refresh token (7-day expiration) on successful authentication
- Return same session tokens for duplicate login requests within 5 minutes (idempotency)
- Lock account for 30 minutes after 5 failed login attempts within a 15-minute window
- Send password reset email with secure one-time token (6-hour expiration) when user requests password reset
- Publish `UserLoggedInEvent` with userId, timestamp, and IP address to message bus for audit trail

## Constraints / Non-Goals

- **Not** implementing multi-factor authentication (deferred to Phase 3)
- **Not** supporting social login (Google, Microsoft) in initial release
- **Not** implementing "Remember Me" functionality (security policy: require explicit login)
- Password reset email delivery is asynchronous; service returns 200 OK immediately without waiting for email send confirmation

---

**Acceptance Signals (Seed)**:
1. ✅ User with valid credentials receives access token and is redirected to dashboard within 200ms
2. ✅ User with invalid password sees error message and account locks after 5 attempts
3. ✅ User clicks "Forgot Password", enters email, and receives reset link within 2 minutes

**Handoff to /plan**:
- Define JWT token structure (claims schema, signing algorithm)
- Specify REST API endpoints (`POST /auth/login`, `POST /auth/refresh`, `POST /auth/forgot-password`)
- Detail database schema for Users, RefreshTokens, FailedLoginAttempts tables
- Implement email template for password reset with branded HTML
- Configure Duende IdentityServer client credentials and scopes
- Set up MassTransit message bus contracts for `UserLoggedInEvent`

**Open Questions for /plan**:
- Which SMTP provider for password reset emails (SendGrid, Azure Communication Services)?
- Should we implement CAPTCHA after 3 failed login attempts?
- Password complexity requirements (length, special characters)?
