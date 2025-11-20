# Authentication Fix: MSAL Token Acquisition Error

## Problem
The Web application was crashing with the following error during the token exchange process:

```
MsalUiRequiredException: No account or login hint was passed to the AcquireTokenSilent call.
Microsoft.Identity.Client.Internal.Requests.Silent.SilentRequest.ExecuteAsync(CancellationToken cancellationToken)

MicrosoftIdentityWebChallengeUserException: IDW10502: An MsalUiRequiredException was thrown due to a challenge for the user.
Microsoft.Identity.Web.TokenAcquisition.GetAuthenticationResultForUserAsync(...)

AuthenticationFailureException: An error was encountered while handling the remote login.
Microsoft.AspNetCore.Authentication.RemoteAuthenticationHandler<TOptions>.HandleRequestAsync()
```

## Root Cause Analysis

The issue occurred in the `OnTokenValidated` event handler of the OpenID Connect authentication flow:

1. **Step 1**: `LmsAuthenticationExtensions.ConfigureLmsTokenExchange()` calls `tokenAcquisition.GetAccessTokenForUserAsync()` to acquire an access token for the LMS API
2. **Step 2**: The access token is passed to `TokenExchangeService.ExchangeTokenAsync()`
3. **Step 3**: Inside `ExchangeTokenAsync()`, the original implementation called `_downstreamApi.CallApiForUserAsync()`
4. **Step 4**: `CallApiForUserAsync()` triggers a **SECOND** token acquisition attempt via MSAL
5. **Step 5**: This second acquisition fails because:
   - We're still in the `OnTokenValidated` event (authentication flow not complete)
   - The user's account information is not yet in the MSAL token cache
   - MSAL cannot perform silent token acquisition without cached user information
   - Result: `MsalUiRequiredException` is thrown

## Solution

Modified `TokenExchangeService` to use the access token directly instead of triggering a second token acquisition:

### Changes Made

#### 1. Updated Dependencies
**Before:**
```csharp
public TokenExchangeService(IDownstreamApi downstreamApi, ILogger<TokenExchangeService> logger)
```

**After:**
```csharp
public TokenExchangeService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<TokenExchangeService> logger)
```

#### 2. Changed Token Exchange Implementation
**Before:**
```csharp
var response = await _downstreamApi.CallApiForUserAsync<TokenExchangeRequest, TokenExchangeResponse>(
    "LmsApi",
    request,
    options =>
    {
        options.HttpMethod = HttpMethod.Post.Method;
        options.RelativePath = "/api/auth/exchange-token";
    }
);
```

**After:**
```csharp
// Create HttpClient and manually add the Authorization header with the access token
var httpClient = _httpClientFactory.CreateClient();
var baseUrl = _configuration["DownstreamApi:BaseUrl"] ?? "https://localhost:7001";
httpClient.BaseAddress = new Uri(baseUrl);
httpClient.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

var httpResponse = await httpClient.PostAsJsonAsync("/api/auth/exchange-token", request);

if (httpResponse.IsSuccessStatusCode)
{
    var response = await httpResponse.Content.ReadFromJsonAsync<TokenExchangeResponse>();
    // ... handle response
}
```

### Key Improvements

1. **No Second Token Acquisition**: The service now uses the access token that was already acquired in Step 1, eliminating the problematic second MSAL call
2. **Direct HTTP Call**: Using `HttpClient` directly gives us full control over the Authorization header
3. **Configuration-Based URL**: Reads the API base URL from configuration (`DownstreamApi:BaseUrl`)
4. **Maintains Compatibility**: No changes to the interface or calling code required
5. **Preserves Error Handling**: All existing error handling and logging behavior maintained

## Testing

Created comprehensive unit tests covering:
- ✅ Successful token exchange returns session ID
- ✅ Failed API calls return null
- ✅ Client IP address included in request
- ✅ HTTP exceptions handled gracefully
- ✅ Authorization header correctly set with Bearer token

All tests passing (7/7 in Web.Tests project).

## Impact

- **Breaking Changes**: None
- **Configuration Changes**: None required
- **Behavioral Changes**: None visible to end users
- **Performance**: Slight improvement by eliminating redundant token acquisition attempt

## Files Modified

1. `src/NorthStarET.NextGen.Lms.Web/Services/TokenExchangeService.cs` - Core fix implementation
2. `tests/unit/NorthStarET.NextGen.Lms.Web.Tests/Services/TokenExchangeServiceTests.cs` - New test file

## Verification Steps

1. Build solution: `dotnet build --configuration Debug`
2. Run Web tests: `dotnet test tests/unit/NorthStarET.NextGen.Lms.Web.Tests`
3. Run full test suite: `dotnet test --configuration Debug`

All builds and tests passing ✅

## Notes

- The `AddDownstreamApi()` registration in `Program.cs` could be removed as it's no longer used by any component in the Web project. However, it has been retained in this fix to minimize changes and maintain the option for future use if needed.
- This fix aligns with Microsoft Identity Web best practices for handling tokens during authentication events
- The solution maintains the original security model and error handling patterns
