# Azure Entra Configuration & IDX20807 Troubleshooting Guide

When running the LMS Identity module locally (via `aspire run` or `dotnet run`), authentication uses Microsoft Entra ID. If tenant or client settings are not configured, you'll see errors like:

```
IDX20807: Unable to retrieve document from: 'https://login.microsoftonline.com/<tenant-id>/v2.0/.well-known/openid-configuration'
IDX20803: Unable to obtain configuration from: 'https://login.microsoftonline.com/<tenant-id>/v2.0/.well-known/openid-configuration'
```

These indicate either network connectivity issues or that Entra settings (tenant ID, client ID, secrets) are placeholders. This guide highlights the steps needed to resolve these issues.

---

## Prerequisites

- An Azure account with permissions to manage Entra ID and app registrations.
- The `az` CLI configured (`az login`).
- Local environment prepared to run the .NET application.

---

## 1. Register the Web API Application

1. Log in to Azure: `az login`
2. Navigate to Azure Active Directory > App Registrations > New Registration.
3. Name: `NorthStarET-LMS-API`.
4. Set supported account types: **Accounts in this organization directory only (Single tenant)**.
5. Redirect URI (web): `https://localhost:7002/signin-oidc` or the host your application uses.
6. Record the **Application (client) ID** and **Directory (tenant) ID**.
7. Under Certificates & secrets, create a new client secret. Save the value.

---

## 2. Configure API Permissions

Ensure the API registration has the appropriate Microsoft Graph permissions or any custom APIs. Grant admin consent if necessary.

---

## 3. Configure Redirect URIs and Logout URLs

In the API app registration, under Authentication:

- Add platform "Web" and specify the redirect URI: `https://localhost:7002/signin-oidc`
- Set the Front-channel logout URL: `https://localhost:7002/signout-oidc`
- Ensure the "ID tokens" checkbox is selected.

---

## 4. Create a Client App Registration (Optional)

If there's another client (like the web project) that calls your API, register it and configure redirect URIs, scopes, etc.

---

## 5. Set Up Configuration Values Locally

1. Store the tenant ID and client ID in `appsettings.Development.json` or `dotnet user-secrets`:

```json
{
  "EntraId": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "ClientSecret": "<client-secret>",
    "CallbackPath": "/signin-oidc"
  },
  "DistrictManagement": {
    "IdempotencyWindowMinutes": 10,
    "InvitationExpiryDays": 7,
    "MaxDistrictsPerPage": 100,
    "MaxAdminsPerDistrict": 10,
    "SuffixValidationRegex": "^[a-z0-9.-]+$"
  },
  "Idempotency": {
    "DefaultTtlMinutes": 10
  }
}
```

2. For secrets, use `dotnet user-secrets`:

```bash
cd src/NorthStarET.NextGen.Lms.Api
(dotnet user-secrets init)
dotnet user-secrets set "EntraId:ClientSecret" "<client-secret>"
```

### District Management Configuration

The `DistrictManagement` section controls:

- **IdempotencyWindowMinutes**: Time window for duplicate request prevention (default: 10 minutes)
- **InvitationExpiryDays**: How long district admin invitations remain valid (default: 7 days)
- **MaxDistrictsPerPage**: Pagination limit for district listings (default: 100)
- **MaxAdminsPerDistrict**: Maximum administrators per district (default: 10)
- **SuffixValidationRegex**: Pattern for valid district suffixes (default: `^[a-z0-9.-]+$`)

### Idempotency Configuration

The `Idempotency` section configures Redis-backed duplicate request prevention:

- **DefaultTtlMinutes**: Default time-to-live for idempotency tokens in Redis (default: 10 minutes)

This ensures that duplicate commands within the window return the same result without re-execution.

---

## 6. Networking and DNS

Ensure your local environment can reach `login.microsoftonline.com`. If proxies or firewalls block requests, configure bypasses.

---

## 7. Retry the Application

Restart the application (`aspire run`). The authentication handshake should now succeed.

---

## 8. Common Issues & Fixes

| Issue                                      | Cause                                | Fix                                                     |
| ------------------------------------------ | ------------------------------------ | ------------------------------------------------------- |
| IDX20807 errors                            | Placeholder tenant or network issues | Update real tenant ID and client ID; check connectivity |
| IDX20803 follows IDX20807                  | App can't fetch OpenId configuration | Same as above                                           |
| login.microsoftonline.com host unreachable | Network restrictions                 | Configure firewall/proxy                                |
| 500 errors from HttpDocumentRetriever      | Client secret missing                | Set secret using user secrets                           |

---

---

## 9. District Management Configuration

The District Management module requires additional configuration for invitation handling and idempotency:

```json
{
  "DistrictManagement": {
    "Schema": "districts",
    "InvitationValidityDays": 7,
    "IdempotencyWindowMinutes": 10
  }
}
```

### Configuration Settings

| Setting                    | Description                                    | Default     |
| -------------------------- | ---------------------------------------------- | ----------- |
| `Schema`                   | PostgreSQL schema for district/admin tables    | `districts` |
| `InvitationValidityDays`   | Days before a district admin invite expires    | `7`         |
| `IdempotencyWindowMinutes` | Window for idempotent district CRUD operations | `10`        |

The idempotency window ensures duplicate create/update requests within 10 minutes return the same result, preventing accidental duplicates. Invitation resends within this window update the most recent invite without creating duplicates.

---

## References

- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [Azure AD App Registration Quickstart](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/overview)
