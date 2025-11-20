# Data Model: UI Migration Preservation
Layer: Foundation
Version: 0.1.0

(No new persistent domain entities introduced; UI parity migration.)

## Preference Entity (Existing)
| Field | Type | Notes |
|-------|------|-------|
| Id | uuid | PK |
| UserId | uuid |  |
| Key | text | e.g. DASHBOARD_COLUMN_ORDER |
| Value | jsonb | Serialized preference |
| CreatedAt | timestamptz |  |
| UpdatedAt | timestamptz |  |

## Migration Mapping (Local Storage)
| Legacy Key | New Handling |
|------------|--------------|
| ns.dashboard.columns | Imported into Preferences service on first load |
| ns.student.filters | Converted to standardized filter DTO |
| ns.shortcuts.custom | Preserved verbatim |

## Data Flow
Legacy local storage -> Angular 18 bootstrap -> Preference ingestion -> Single source of truth via API.

---
Minimal model impact.