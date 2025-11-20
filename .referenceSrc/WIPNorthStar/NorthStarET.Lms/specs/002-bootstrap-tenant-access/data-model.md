# Data Model — Tenant-Isolated District Access

## District

- **Identifier**: `DistrictId` (UUID)
- **Attributes**:
  - `Name` (string, 3-100 chars, trimmed)
  - `Suffix` (string, email domain segment, lowercase, unique platform-wide)
  - `CreatedAtUtc` (timestamp)
  - `UpdatedAtUtc` (timestamp, nullable)
  - `IsDeleted` (bool, default false)
- **Relationships**:
  - One-to-many with `DistrictAdmin` assignments
  - One-to-many with `AuditRecord`
  - One-to-many with `DomainEventEnvelope`
- **Validation Rules**:
  - Suffix must match regex `^[a-z0-9.-]+$`
  - Suffix uniqueness enforced case-insensitively
  - Delete operation prohibited when active admins exist without explicit confirmation
- **Lifecycle / State Transitions**:
  - `Created` → optional `Updated` → `Deleted` (soft delete, triggers cascade revoke for admins)
  - Updates within 10 minutes consolidate via idempotency token

## DistrictAdmin

- **Identifier**: `DistrictAdminId` (UUID)
- **Attributes**:
  - `DistrictId` (UUID, FK to District)
  - `Email` (string, RFC 5322-compliant, must end with district suffix)
  - `Status` (enum: Unverified, Verified, Revoked)
  - `InvitationSentAtUtc` (timestamp)
  - `InvitationExpiresAtUtc` (timestamp, 7 days after send)
  - `VerifiedAtUtc` (timestamp, nullable)
  - `RevokedAtUtc` (timestamp, nullable)
- **Relationships**:
  - Belongs to one `District`
  - Emits `DistrictAdminInvited`, `DistrictAdminVerified`, `DistrictAdminRevoked` domain events
  - Audit records reference assignments
- **Validation Rules**:
  - Email must align with district suffix (case-insensitive)
  - Resend before expiry updates `InvitationSentAtUtc` and extends expiry by 7 days
  - Status transitions limited to `Unverified → Verified`, `Unverified → Revoked`, `Verified → Revoked`
- **Lifecycle / State Transitions**:
  - `Unverified` (on invite) → `Verified` (after acceptance) or `Revoked`
  - Re-invite keeps state `Unverified` but refreshes timestamps

## AuditRecord

- **Identifier**: `AuditRecordId` (UUID)
- **Attributes**:
  - `OccurredAtUtc` (timestamp)
  - `ActorId` (string/UserId)
  - `ActorRole` (enum: SystemAdmin, DistrictAdmin)
  - `DistrictId` (UUID, nullable for system-level actions)
  - `EntityType` (string)
  - `EntityId` (string/UUID)
  - `Action` (string: Created, Updated, Deleted, Invited, Verified, Revoked)
  - `BeforePayload` (JSON, nullable)
  - `AfterPayload` (JSON, nullable)
  - `CorrelationId` (UUID)
- **Relationships**:
  - Optional foreign key to `District`
  - Linked to `DomainEventEnvelope` via `CorrelationId`
- **Validation Rules**:
  - Immutable once inserted
  - `BeforePayload` required for update/delete actions

## DomainEventEnvelope

- **Identifier**: `EventId` (UUID)
- **Attributes**:
  - `OccurredAtUtc` (timestamp)
  - `EventType` (string)
  - `SchemaVersion` (integer)
  - `Payload` (JSONB)
  - `PublishedAtUtc` (timestamp, nullable)
  - `PublishAttempts` (int)
  - `CorrelationId` (UUID)
- **Relationships**:
  - Optional foreign key to `District`
  - References `AuditRecord` via `CorrelationId`
- **Validation Rules**:
  - `SchemaVersion` must match Contracts package version
  - Publish retries capped (configurable, default 5)

## IdempotencyToken

- **Identifier**: Composite key `<ActorId, Action, ResourceId>`
- **Attributes**:
  - `WindowEndsAtUtc` (timestamp, 10 minutes after first submission)
  - `PayloadHash` (string)
  - `ResultReference` (string/UUID)
- **Storage**: Redis hash entry keyed by composite identifier
- **Validation Rules**:
  - Requests within window with same payload return stored result reference
  - Divergent payload within window triggers conflict response
