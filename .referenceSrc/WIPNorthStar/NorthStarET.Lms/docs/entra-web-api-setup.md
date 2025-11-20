# Microsoft Entra Setup: LMS Website → LMS API

This guide walks through the exact Azure portal actions required to let the **LMS Website** app registration call the protected **LMS API** without triggering `401 Unauthorized`. It mirrors the Microsoft identity platform guidance for [calling a web API from a web app](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-web-api-dotnet-protect-app#expose-the-api) and for [exposing web API scopes](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-configure-app-expose-web-apis#add-a-scope).

> **Goal**: issue access tokens whose `aud` claim matches the LMS API registration and inject the LMS session header so the API accepts the request.

---

## Prerequisites

- Azure role: **Application Administrator** (or higher) in the tenant that hosts the LMS apps.
- Existing app registration named **LMS Website** (`appId: de4a31ef-4460-460f-9808-c4335159ee10`).
- Authority: `https://login.microsoftonline.com/122f92e0-c52e-45ac-b509-ec3354ca73c7` (from `TenantId`).
- Ability to run `dotnet user-secrets` locally for both the API and Web projects.

---

## Step 1 — Register the protected LMS API

1. Open [entra.microsoft.com](https://entra.microsoft.com) and switch to the correct directory if prompted.
2. Navigate to **Entra ID ▸ App registrations ▸ New registration**.
3. Name the app **LMS API** (or similar). Choose **Accounts in this organizational directory only**. Leave Redirect URI empty. Select **Register**.
4. Record the new **Application (client) ID** and **Directory (tenant) ID**. You will paste them into `src/NorthStarET.NextGen.Lms.Api/appsettings.json` later.

---

## Step 2 — Expose a delegated scope on the API

Microsoft documentation requires at least one delegated scope (for example `access_as_user`).

1. In the **LMS API** registration, open **Expose an API**.
2. If **Application ID URI** is empty, select **Add** and accept the suggested value `api://{api-client-id}`. Save.
3. Select **Add a scope** and enter:
   - **Scope name**: `access_as_user`
   - **Who can consent**: **Admins and users**
   - **Admin consent display name**: `Access LMS API as the signed-in user`
   - **Admin consent description**: `Allows the LMS Website to call the LMS API on the user's behalf.`
   - **User consent display name**: `Access LMS API as you`
   - **User consent description**: `Allows the site to call backend services using your session.`
   - **State**: **Enabled**
4. Select **Add scope**. Verify that the full scope shows as `api://{api-client-id}/access_as_user` in the list (per [Expose an API](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-configure-app-expose-web-apis#add-a-scope)).

Optional but recommended per Microsoft guidance: under **Authorized client applications**, add the LMS Website client ID and tick the new `access_as_user` scope so users will not see a consent prompt.

---

## Step 3 — Grant the LMS Website delegated permission

1. Switch to **Entra ID ▸ App registrations** and open the existing **LMS Website** registration (manifest excerpt in `Untitled-1`).
2. Go to **API permissions ▸ Add a permission ▸ My APIs**.
3. Select **LMS API** from the list and tick the **`access_as_user`** delegated permission.
4. Choose **Add permissions**, then select **Grant admin consent for northstaret.com** and confirm. This follows the admin-consent workflow described in [Quickstart: Call a web API](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-web-api-dotnet-protect-app#expose-the-api).

---

## Step 4 — Issue a client secret for the LMS Website app

Microsoft identity web uses the confidential client credentials flow; create a secret that you can store with user secrets.

1. Still inside **LMS Website**, select **Certificates & secrets ▸ New client secret**.
2. Provide a description such as `Dev-LMS-Website` and choose an expiration policy that matches your security requirements.
3. Copy the **Value** immediately—this is the secret you will add to local configuration:
   ```bash
   dotnet user-secrets set "EntraId:ClientSecret" "<secret-value>" --project src/NorthStarET.NextGen.Lms.Web
   dotnet user-secrets set "EntraId:ClientSecret" "<secret-value>" --project src/NorthStarET.NextGen.Lms.Api
   ```
   (Run the second command only if the API reuses the same secret; adjust if you prefer separate credentials.)

---

## Step 5 — Update application configuration

1. **API project (`src/NorthStarET.NextGen.Lms.Api/appsettings.json`)**

   - Set `EntraId:ClientId` to the **LMS API** client ID.
   - Optionally add `"Audience": "api://{api-client-id}"` if you prefer explicit audience validation.
   - Ensure `EntraId:Scopes` includes the new scope (for the API, this is typically not required, but it keeps settings consistent).

2. **Web project (`src/NorthStarET.NextGen.Lms.Web/appsettings.json`)**

   - Keep `EntraId:ClientId` pointing to the **LMS Website** app ID.
   - Replace the existing placeholder scope array with the new value:
     ```json
     "Scopes": [
       "api://{api-client-id}/access_as_user"
     ]
     ```

3. **Code changes (Web project)**
   - Update authentication wiring in `Program.cs` to acquire downstream tokens:
     ```csharp
     builder.Services
         .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
         .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("EntraId"))
         .EnableTokenAcquisitionToCallDownstreamApi()
         .AddDownstreamApi("LmsApi", builder.Configuration.GetSection("EntraId"))
         .AddInMemoryTokenCaches();
     ```
   - After sign-in, call `/auth/exchange-token` with the acquired access token and persist `SessionId` in a secure cookie; add a delegating handler that reads the cookie and sets the `X-Lms-Session-Id` header before sending requests via `IApiClient`.

(These code-level tasks align with [A web app that calls web APIs: Code configuration](https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-app-call-api-app-registration).)

---

## Step 6 — Validate the flow

1. Restart Aspire (`dotnet run --project src/NorthStarET.NextGen.Lms.AppHost`).
2. Sign in to the LMS Website. Use browser dev tools to confirm the `/auth/exchange-token` call returns `200` and that subsequent API calls carry the `X-Lms-Session-Id` header.
3. If you see another `401`, enable temporary PII logging per [401 troubleshooting guidance](https://learn.microsoft.com/en-us/troubleshoot/entra/entra-id/app-integration/401-unauthorized-aspnet-core-web-api#solution) to inspect token validation errors.

---

## Reference Links

- [Quickstart: Call a web API that is protected by the Microsoft identity platform](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-web-api-dotnet-protect-app#expose-the-api)
- [Configure an application to expose a web API](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-configure-app-expose-web-apis#add-a-scope)
- [401 Unauthorized errors in ASP.NET Core Web API with Microsoft Entra ID](https://learn.microsoft.com/en-us/troubleshoot/entra/entra-id/app-integration/401-unauthorized-aspnet-core-web-api)
