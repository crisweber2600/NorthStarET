# Quickstart — Tenant-Isolated District Access

1. **Restore & Build**

   - `dotnet restore NorthStarET.NextGen.Lms.sln`
   - `dotnet build NorthStarET.NextGen.Lms.sln --configuration Debug --verbosity normal /warnaserror`

2. **Launch Aspire Host**

   - `dotnet run --project src/NorthStarET.NextGen.Lms.AppHost`
   - Services started: API (ApplicationFacade v1), background worker (outbox publisher), PostgreSQL 16, Redis Stack, Event Grid emulator.

3. **Seed System Admins**

   - Ensure environment variable `SYSTEM_ADMIN_ALLOWED_DOMAINS` set to `northstaret.com;weber.center`.
   - Run seeding command: `dotnet run --project src/NorthStarET.NextGen.Lms.Api -- --seed-admins` (idempotent).

4. **Authenticate via Entra**

   - Use existing Entra test tenant credentials. Assign users with the seeded domains to the `SystemAdmin` app role, others to `DistrictAdmin`.
   - `appsettings.Development.json` should reference the shared Entra configuration defined in `docs/azure-entra-setup.md`.
   - Ensure `DistrictManagement` configuration is present with `InvitationValidityDays: 7` and `IdempotencyWindowMinutes: 10`.

5. **Exercise System Admin Journeys**

   - Navigate to `https://localhost:5001/admin/districts` in the Web project.
   - Create a new district with a unique suffix (e.g., "demo" for demo.example.edu). The system validates:
     - District name is 3-100 characters
     - Suffix matches pattern `^[a-z0-9.-]+$`
     - Suffix is case-insensitively unique across all districts
   - Verify district appears in the list at `/admin/districts`
   - Update district name or suffix, confirm changes persist
   - Invite a district admin with email matching the suffix pattern (e.g., `admin@demo.example.edu`)
   - Check the audit trail at `/admin/audit` to see all actions logged with actor, timestamps, and payloads

6. **Verify District Admin Invitation Flow**

   - After inviting a district admin, verify invitation record shows status "Unverified" with 7-day expiration
   - Invitations can be resent within the idempotency window (10 minutes), refreshing the expiration
   - Attempt to invite duplicate email to same district, confirm rejection
   - Revoke an admin, verify status changes to "Revoked" and appears in audit log
   - Attempt to delete district with active admins, confirm rejection with error message

7. **Verify Tenant Isolation & Idempotency**

   - Test creating district with same name twice using same idempotency key within 10 minutes
     - First request: Returns 201 Created with district details
     - Second request: Returns 201 Created with same district ID (idempotent)
   - Test creating district with different payload but same idempotency key
     - Should return 409 Conflict with idempotency error
   - Verify SystemAdmin can access all districts, while DistrictAdmin (once implemented) can only access their assigned district

8. **API Endpoint Validation**

   - Test district CRUD via REST API:
     - `POST /api/districts` - Create district (requires SystemAdmin role)
     - `GET /api/districts` - List all districts
     - `GET /api/districts/{id}` - Get specific district
     - `PUT /api/districts/{id}` - Update district (validates suffix uniqueness)
     - `DELETE /api/districts/{id}` - Soft delete district (checks active admin count)
   - Test district admin management:
     - `POST /api/districts/{districtId}/admins` - Invite admin (validates email suffix alignment)
     - `GET /api/districts/{districtId}/admins` - List district admins
     - `POST /api/districts/{districtId}/admins/{adminId}/resend` - Resend invitation
     - `DELETE /api/districts/{districtId}/admins/{adminId}` - Revoke admin access
   - Test audit trail:
     - `GET /api/audit` - Get recent audit records (default 100)
     - `GET /api/audit?districtId={id}` - Get district-specific audit trail
     - `GET /api/audit?count=50` - Limit results

9. **Run Automated Tests**

   - Unit: `dotnet test tests/unit/NorthStarET.NextGen.Lms.Application.Tests/NorthStarET.NextGen.Lms.Application.Tests.csproj`
   - Domain: `dotnet test tests/unit/NorthStarET.NextGen.Lms.Domain.Tests/NorthStarET.NextGen.Lms.Domain.Tests.csproj`
   - API: `dotnet test tests/unit/NorthStarET.NextGen.Lms.Api.Tests/NorthStarET.NextGen.Lms.Api.Tests.csproj`
   - BDD: `dotnet test tests/bdd/NorthStarET.NextGen.Lms.Bdd/NorthStarET.NextGen.Lms.Bdd.csproj`
   - Playwright: `pwsh tests/ui/playwright.ps1`
   - Aspire integration: `dotnet test tests/aspire/NorthStarET.NextGen.Lms.AspireTests/NorthStarET.NextGen.Lms.AspireTests.csproj`

10. **Observe Events & Audit Trail**

- Query the audit trail in PostgreSQL:
  ```sql
  SELECT * FROM districts."AuditRecords"
  ORDER BY "OccurredAtUtc" DESC
  LIMIT 20;
  ```
- Verify audit records contain:
  - Actor ID and role (e.g., "SystemAdmin")
  - Entity type and ID
  - Action (Created, Updated, Deleted, Invited, Revoked)
  - Before/After payloads in JSON format
  - Correlation IDs for tracking event chains
- Check Redis for idempotency keys:
  ```bash
  redis-cli KEYS "idempotency:*"
  redis-cli GET "idempotency:result:{key}"
  ```
- Inspect Event Grid emulator dashboard (if configured) to ensure domain events are published

11. **Data Validation Checks**

- Verify database constraints:

  ```sql
  -- Unique suffix constraint
  SELECT "Suffix", COUNT(*) FROM districts."Districts"
  GROUP BY "Suffix" HAVING COUNT(*) > 1;

  -- Unique email per district constraint
  SELECT "DistrictId", "Email", COUNT(*)
  FROM districts."DistrictAdmins"
  GROUP BY "DistrictId", "Email"
  HAVING COUNT(*) > 1;
  ```

- Verify soft delete behavior:
  ```sql
  -- Should return only non-deleted districts by default
  SELECT * FROM districts."Districts" WHERE "IsDeleted" = false;
  ```
- Verify invitation expiration logic:
  ```sql
  SELECT "Email", "InvitationExpiresAtUtc",
         CASE WHEN "InvitationExpiresAtUtc" < NOW()
         THEN 'Expired' ELSE 'Valid' END as Status
  FROM districts."DistrictAdmins"
  WHERE "Status" = 'Unverified';
  ```

12. **Performance & Scalability Checks**

- Create 100+ districts rapidly, verify suffix uniqueness check remains fast (<10ms)
- Test idempotency with concurrent requests (same key), verify only one district created
- Verify audit query performance with pagination (should return 100 records in <100ms)
- Check Redis cache hit rate for idempotency (should be >90% for duplicate requests)

13. **Security Validation**

- Verify all endpoints require authentication (401 Unauthorized without token)
- Verify district endpoints require SystemAdmin role (403 Forbidden for non-admins)
- Verify tenant isolation prevents cross-district access
- Verify idempotency keys are scoped per-actor (different users can use same key)
- Verify audit trail captures all administrative actions with actor information
