# LMS Web App Authentication Fix Summary

## Problem
The Web app was returning 401 Unauthorized when calling the API because:
1. The API requires an `X-Lms-Session-Id` header (handled by `SessionAuthenticationHandler`)
2. The Web app was not exchanging its Entra ID token for an LMS session
3. The HttpClient requests were not including the required session header

## Root Cause Analysis
From the distributed traces, the issue was:
- **Web Request**: `POST {controller=Home}/{action=Index}/{id?}` → calls `POST /api/districts`
- **API Response**: `AuthenticationScheme: LmsSession was challenged.` → 401 Unauthorized
- **Missing Flow**: Web app never called `/auth/exchange-token` to get LMS session ID

## Solution Implemented

### 1. Updated Configuration (`appsettings.json`)
- Added `DownstreamApi` section with API base URL and scope
- Updated `EntraId.Scopes` to include the new API scope: `api://792ef7e2-56c1-414a-a300-c071c6a4ce51/access_as_user`

### 2. Added Microsoft.Identity.Web.DownstreamApi Package
- Updated `Directory.Packages.props` with version 4.0.0
- Added package reference to Web project

### 3. Created LMS Session Management Services

#### `ILmsSessionAccessor` & `LmsSessionAccessor`
- Stores/retrieves LMS session ID via secure HTTP-only cookies
- 8-hour expiration matching session lifetime
- Secure, SameSite=Strict settings

#### `LmsSessionHandler` (DelegatingHandler)
- Automatically injects `X-Lms-Session-Id` header into all HttpClient requests
- Uses `AuthenticationHeaders.SessionId` constant from contracts

#### `ITokenExchangeService` & `TokenExchangeService`
- Calls `/api/auth/exchange-token` endpoint using `IDownstreamApi`
- Exchanges Entra access token for LMS session ID
- Proper error handling and logging

### 4. Updated Program.cs Authentication Configuration
- Enabled `.EnableTokenAcquisitionToCallDownstreamApi()`
- Added `.AddDownstreamApi("LmsApi", ...)` for API calls
- Added `.AddInMemoryTokenCaches()` for token caching
- Registered all new services in DI container
- Added `LmsSessionHandler` to HttpClient pipelines

### 5. Created OIDC Event Handlers (`LmsAuthenticationExtensions`)
- **OnTokenValidated**: After successful Entra sign-in:
  1. Acquires access token for LMS API scope
  2. Calls token exchange service
  3. Stores session ID in secure cookie
- **OnSignedOutCallbackRedirect**: Clears session cookie on logout
- Comprehensive error handling that doesn't break auth flow

## Authentication Flow (Fixed)

1. **User signs in** → Entra ID OIDC flow
2. **OnTokenValidated fires** → Gets access token for API scope
3. **Token exchange** → POST `/api/auth/exchange-token` with Entra token
4. **Session storage** → LMS session ID stored in secure cookie
5. **API calls** → `LmsSessionHandler` adds `X-Lms-Session-Id` header
6. **API authentication** → `SessionAuthenticationHandler` validates session
7. **Success** → API calls return 200 instead of 401

## Key Files Modified

### Configuration
- `/src/NorthStarET.NextGen.Lms.Web/appsettings.json` - API scope and downstream API config
- `Directory.Packages.props` - Added DownstreamApi package

### Services
- `/src/NorthStarET.NextGen.Lms.Web/Services/LmsSessionAccessor.cs` - Session cookie management
- `/src/NorthStarET.NextGen.Lms.Web/Services/LmsSessionHandler.cs` - HTTP header injection
- `/src/NorthStarET.NextGen.Lms.Web/Services/TokenExchangeService.cs` - API token exchange

### Authentication
- `/src/NorthStarET.NextGen.Lms.Web/Authentication/LmsAuthenticationExtensions.cs` - OIDC event handlers
- `/src/NorthStarET.NextGen.Lms.Web/Program.cs` - DI registration and auth configuration

### Project Files
- `/src/NorthStarET.NextGen.Lms.Web/NorthStarET.NextGen.Lms.Web.csproj` - Package references

## Testing the Fix

1. **Start the application**: `dotnet run --project src/NorthStarET.NextGen.Lms.AppHost`
2. **Navigate to Web app**: https://localhost:7002
3. **Sign in** with Entra ID
4. **Check logs** for "Successfully created LMS session for user"
5. **Verify API calls** return 200 instead of 401
6. **Check cookies** for `LmsSessionId` in browser dev tools

## Security Considerations

- **Session cookie**: HTTP-only, Secure, SameSite=Strict
- **Token scope**: Minimal scope `access_as_user` for API access only
- **Error handling**: Auth failures don't expose sensitive information
- **Token caching**: In-memory cache clears on app restart
- **Session cleanup**: Cookie cleared on logout

## Entra ID Configuration Required

The Azure portal steps documented in `/docs/entra-web-api-setup.md` must be completed:
1. LMS API app registration with exposed scope
2. LMS Website app granted permission to API scope  
3. Admin consent provided
4. Client secret configured in user secrets

This implementation follows Microsoft's recommended patterns for web apps calling protected APIs with Microsoft.Identity.Web.