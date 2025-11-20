# Data Model

## Entities

### District

- **Id** (Guid, key)
- **Name** (string, existing)
- **Slug** (string, existing)
- **IsActive** (bool, existing)
- **CreatedAt / UpdatedAt** (DateTimeOffset, existing audit trail)
- **DeletedAt** (DateTimeOffset?, soft delete)
- **ConcurrencyStamp** (string, existing row version)
- **Relationships**: One-to-many with `School`.

### School

- **Id** (Guid, key)
- **DistrictId** (Guid, FK to District; tenant boundary)
- **Name** (string, required, unique per district, max 200 chars)
- **Code** (string?, optional, max 50 chars)
- **Notes** (string?, optional, max 1000 chars)
- **Status** (enum: Active, Inactive; default Active)
- **GradeRangeMin** (GradeLevel?, derived helper)
- **GradeRangeMax** (GradeLevel?, derived helper)
- **ConcurrencyStamp** (string, for optimistic concurrency)
- **CreatedBy / UpdatedBy** (Guid, actor identifiers)
- **CreatedAt / UpdatedAt** (DateTimeOffset, audit timestamps)
- **DeletedAt** (DateTimeOffset?, soft delete with archiving)
- **Relationships**: One-to-many with `GradeOffering`.
- **Validation Rules**:
  - `Name` required and unique within District.
  - Optional `Code` unique within District when provided.
  - Prevent delete when already soft-deleted; confirm impact before removal.

### GradeOffering

- **Id** (Guid, key)
- **SchoolId** (Guid, FK to School)
- **GradeId** (string, FK to master grade taxonomy)
- **SchoolType** (enum: Elementary, Middle, High, Other)
- **EffectiveFrom** (DateTimeOffset, default now)
- **EffectiveTo** (DateTimeOffset?, optional for retirements)
- **CreatedBy / UpdatedBy** (Guid)
- **CreatedAt / UpdatedAt** (DateTimeOffset)
- **ConcurrencyStamp** (string)
- **Relationships**: Many-to-one with `School`; references read-only Grade taxonomy table.
- **Validation Rules**:
  - GradeId must exist in taxonomy.
  - `SchoolId` must belong to active District of the acting user.
  - At least one grade required unless admin confirms empty state.
  - Selecting a minimum and subsequent maximum grade auto-selects the contiguous range between them (per acceptance scenario 4).
  - Each school type exposes a "Select All" toggle that selects/deselects all grades of that type in a single action (per acceptance scenarios 5 and 6).

### SchoolChangeEvent (Domain Event)

- **EventId** (Guid)
- **SchoolId** (Guid)
- **DistrictId** (Guid)
- **ChangeType** (enum: Created, Updated, Deleted, GradesUpdated)
- **PerformedBy** (Guid)
- **OccurredAt** (DateTimeOffset)
- **Payload** (JSON document with before/after fields as applicable)
- **IdempotencyKey** (string, hashed payload + 10-minute window)

## Relationships & State Transitions

- `District` owns `School`; cascade soft-delete marks school and archives grade offerings.
- `School` owns `GradeOffering`; grade assignments replace existing set per save (transactional replace).
- Create School → emit `SchoolChangeEvent` (Created) → UI refresh within 2 seconds via cache invalidation.
- Update School (name/code/notes) → optimistic concurrency check → emit `SchoolChangeEvent` (Updated).
- Delete School → confirmation required → soft delete + grade offering archive → emit `SchoolChangeEvent` (Deleted).
- Configure Grades → replace grade offering rows → emit `SchoolChangeEvent` (GradesUpdated) with new set.
- System admin district switch updates tenant context via session claim and reloads School list scope.

## Derived Views & Queries

- `SchoolListItem` projection: School Id, Name, Code, Status, GradeRange (min/max), LastUpdated, HasPendingChanges flag.
- `SchoolDetail` projection: School core fields + Grades grouped by SchoolType with Select-All flags.
- Search filters by Name/Code (case-insensitive) within District; sort alphabetical ascending/descending.
- Tenant filter applied to every query via middleware or base repository extension to enforce `DistrictId`.

## Idempotency & Caching

- Redis key pattern `idempotency:school:{districtId}:{operation}:{hash}` expires after 10 minutes; returning prior response if duplicate.
- Post-write cache invalidation triggers District-specific cache keys `schools:list:{districtId}` and `school:detail:{schoolId}` to honor 2-second refresh target.
