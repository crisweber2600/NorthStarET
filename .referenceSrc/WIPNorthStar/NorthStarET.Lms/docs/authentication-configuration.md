# Authentication Configuration Guide

## Overview

This document describes the authentication configuration for the NorthStarET.NextGen.Lms application, which uses a custom session-based authentication scheme built on top of Microsoft Entra ID (Azure AD).

## Architecture

The application uses a two-tier authentication approach:

1. **Web Frontend**: Uses OpenID Connect (OIDC) to authenticate with Microsoft Entra ID
2. **API Backend**: Uses a custom session-based authentication scheme (`LmsSession`)

### Authentication Flow

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Browser   │      │  Web (MVC)   │      │   API       │
└─────────────┘      └──────────────┘      └─────────────┘
       │                     │                     │
       │  1. Login Request   │                     │
       │────────────────────>│                     │
       │                     │                     │
       │  2. Redirect to     │                     │
       │     Entra ID        │                     │
       │<────────────────────│                     │
       │                     │                     │
       │  3. User AuthN      │                     │
       │     (Entra ID)      │                     │
       │                     │                     │
       │  4. Auth Code       │                     │
       │────────────────────>│                     │
       │                     │                     │
       │                     │  5. Exchange Token  │
       │                     │    (Bearer token)   │
       │                     │────────────────────>│
       │                     │                     │
       │                     │  6. LMS Session ID  │
       │                     │<────────────────────│
       │                     │                     │
       │  7. Session Cookie  │                     │
       │     (LmsSessionId)  │                     │
       │<────────────────────│                     │
       │                     │                     │
       │  8. API Request     │                     │
       │────────────────────>│                     │
       │                     │                     │
       │                     │  9. API Call with   │
       │                     │     X-Lms-Session-Id│
       │                     │────────────────────>│
       │                     │                     │
       │                     │  10. Response       │
       │                     │<────────────────────│
       │                     │                     │
       │  11. Response       │                     │
       │<────────────────────│                     │
```

## Configuration

### Web Application (NorthStarET.NextGen.Lms.Web)

#### appsettings.json

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "{your-tenant-id}",
    "ClientId": "{your-web-app-client-id}",
    "CallbackPath": "/signin-oidc",
    "ClientSecret": "{stored-in-user-secrets}",
    "Scopes": [
      "api://{your-api-client-id}/access_as_user"
    ]
  },
  "DownstreamApi": {
    "BaseUrl": "https://localhost:7001",
    "Scopes": [
      "api://{your-api-client-id}/access_as_user"
    ]
  },
  "LmsApi": {
    "Scope": "api://{your-api-client-id}/access_as_user"
  }
}
```

#### Key Components

1. **LmsAuthenticationExtensions.cs**
   - Hooks into OIDC `OnTokenValidated` event
   - Acquires access token for API scope
   - Exchanges Entra token for LMS session ID
   - Stores session ID in secure cookie

2. **LmsSessionAccessor.cs**
   - Manages session cookie (`LmsSessionId`)
   - Cookie settings:
     - `HttpOnly`: true (prevents JavaScript access)
     - `Secure`: true (HTTPS only)
     - `SameSite`: Lax (allows OAuth redirects)
     - `IsEssential`: true (required for authentication)
     - `Expires`: 8 hours (matches session expiration)

3. **LmsSessionHandler.cs**
   - Delegating handler for HttpClient
   - Automatically adds `X-Lms-Session-Id` header to all API requests
   - Retrieves session ID from `LmsSessionAccessor`

4. **TokenExchangeService.cs**
   - Calls `/api/auth/exchange-token` endpoint
   - Sends Entra access token as Bearer token
   - Returns LMS session ID

### API Application (NorthStarET.NextGen.Lms.Api)

#### appsettings.json

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "{your-tenant-id}",
    "ClientId": "{your-api-client-id}",
    "CallbackPath": "/signin-oidc",
    "ClientSecret": "{stored-in-user-secrets}",
    "Scopes": [
      "api://northstar-lms-api/.default"
    ]
  }
}
```

#### Key Components

1. **SessionAuthenticationHandler.cs**
   - Custom authentication handler
   - Validates `X-Lms-Session-Id` header
   - Queries session repository to validate session
   - Creates ClaimsPrincipal with user claims

2. **Program.cs**
   - Registers `LmsSession` authentication scheme
   - Configures CORS to allow Web frontend
   - Sets fallback authorization policy

## CORS Configuration

The API is configured to accept credentials from the Web frontend:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        var webOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "https://localhost:7064", "http://localhost:5143" };
        
        policy.WithOrigins(webOrigins)
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Production Configuration

For production environments, configure allowed origins via environment-specific settings:

```json
{
  "AllowedOrigins": [
    "https://lms.northstaret.org"
  ]
}
```

## HTTPS Configuration

### Development

Both applications are configured for HTTPS in development:

- **API**: `https://localhost:7001`
- **Web**: `https://localhost:7064`

### launchSettings.json

Both projects include HTTPS profiles with `ASPNETCORE_HTTPS_PORT` environment variable set:

**API**:
```json
{
  "https": {
    "applicationUrl": "https://localhost:7001;http://localhost:5259",
    "environmentVariables": {
      "ASPNETCORE_HTTPS_PORT": "7001"
    }
  }
}
```

**Web**:
```json
{
  "https": {
    "applicationUrl": "https://localhost:7064;http://localhost:5143",
    "environmentVariables": {
      "ASPNETCORE_HTTPS_PORT": "7064"
    }
  }
}
```

## Cookie Security

### Session Cookie Settings

| Setting | Value | Purpose |
|---------|-------|---------|
| Name | `LmsSessionId` | Cookie identifier |
| HttpOnly | `true` | Prevents JavaScript access (XSS protection) |
| Secure | `true` | HTTPS only |
| SameSite | `Lax` | Allows OAuth redirects while preventing CSRF |
| IsEssential | `true` | Required for authentication |
| Expires | 8 hours | Matches session expiration |

### Why SameSite.Lax?

We use `SameSite.Lax` instead of `Strict` because:
- OAuth flows redirect back to the application from Entra ID
- `Strict` cookies are not sent on top-level navigation (redirects)
- `Lax` allows cookies on safe HTTP methods (GET) during top-level navigation
- `Lax` still provides CSRF protection for state-changing operations

## Entra ID App Registrations

### Web Application Registration

1. **Application Type**: Web
2. **Redirect URIs**: `https://localhost/signin-oidc` (development)
3. **Client Secret**: Required (stored in user secrets)
4. **API Permissions**:
   - Microsoft Graph: `openid`, `profile`, `email`, `User.Read`
   - LMS API: `access_as_user` (delegated)
5. **Token Configuration**: ID tokens enabled

### API Application Registration

1. **Application Type**: Web API
2. **Identifier URI**: `api://{api-client-id}`
3. **Exposed API**:
   - Scope: `access_as_user`
   - Admin consent required
4. **Authorized Client Applications**: Add Web app client ID
5. **App Roles**: None (using delegated permissions)

## Troubleshooting

### 401 Unauthorized Errors

**Symptom**: API returns 401 for authenticated requests from Web

**Possible Causes**:
1. Session cookie not being set
   - Check browser dev tools → Application → Cookies
   - Verify `LmsSessionId` cookie exists
2. Session header not being sent
   - Check browser dev tools → Network → Request Headers
   - Verify `X-Lms-Session-Id` header is present
3. Token exchange failed
   - Check Web application logs for token exchange errors
   - Verify Entra ID scopes are configured correctly
4. CORS issues
   - Check browser console for CORS errors
   - Verify API CORS policy includes Web origin

### Token Exchange Failures

**Symptom**: MsalUiRequiredException or token exchange returns null

**Possible Causes**:
1. Missing or incorrect scope configuration
   - Verify `LmsApi:Scope` in Web appsettings.json
   - Ensure scope matches API app registration
2. API app not exposing scope
   - Check API app registration → Expose an API
   - Verify scope is enabled
3. Admin consent not granted
   - Check Entra ID → Enterprise Applications
   - Grant admin consent for API permissions

### Session Cookie Not Set

**Symptom**: Session cookie does not appear in browser after login

**Possible Causes**:
1. Token exchange failed silently
   - Check Web application logs
   - Look for errors in `LmsAuthenticationExtensions`
2. Cookie security settings
   - Ensure HTTPS is enabled in development
   - Verify certificate is trusted
3. Cookie SameSite issues
   - Check browser console for cookie warnings
   - Ensure SameSite is set to Lax for OAuth flows

## Testing

### Manual Testing

1. **Login Flow**:
   ```
   1. Navigate to https://localhost:7064
   2. Click "Sign In"
   3. Enter Entra ID credentials
   4. After redirect, check cookies:
      - Should have OIDC cookies (.AspNetCore.Cookies)
      - Should have LmsSessionId cookie
   ```

2. **API Call**:
   ```
   1. Open browser dev tools
   2. Navigate to protected page (e.g., Districts)
   3. Check Network tab for API call
   4. Verify request includes:
      - Cookie: LmsSessionId={guid}
      - X-Lms-Session-Id: {guid}
   5. Verify response is 200 OK
   ```

3. **CORS**:
   ```
   1. Open browser console
   2. Navigate to any page making API calls
   3. Should not see CORS errors
   4. API responses should include:
      - Access-Control-Allow-Origin: https://localhost:7064
      - Access-Control-Allow-Credentials: true
   ```

### Automated Testing

Unit tests for authentication components:
- `TokenExchangeServiceTests.cs` - Token exchange service
- Additional tests for session handler and accessor as needed

## Security Considerations

1. **Never log session IDs or access tokens**
   - Session IDs are sensitive and can be used to impersonate users
   - Access tokens contain user claims and API permissions

2. **Use HTTPS in all environments**
   - Session cookies must be marked `Secure`
   - Prevents session hijacking over unsecured networks

3. **Implement session expiration**
   - Sessions expire after 8 hours of inactivity
   - Expired sessions are automatically refreshed if Entra token is valid

4. **Rate limiting**
   - API implements rate limiting for authentication endpoints
   - Prevents brute force attacks and token abuse

5. **CORS restrictions**
   - Only allow specific origins
   - Never use `AllowAnyOrigin()` with `AllowCredentials()`

## Additional Resources

- [Microsoft Identity Platform documentation](https://learn.microsoft.com/en-us/entra/identity-platform/)
- [ASP.NET Core Authentication documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Cookie Authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)
- [CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
