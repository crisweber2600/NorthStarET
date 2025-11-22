# Contracts: Configuration Service
Layer: Foundation

## REST APIs (proposed)
- `GET /api/config/resolved?schoolId=<id>` — resolved settings for tenant/school.
- `POST /api/config/settings` — create/update setting value (district/school scope).
- `GET /api/config/settings/{id}` — fetch specific setting value with history.
- `POST /api/config/calendars` / `PUT /api/config/calendars/{id}` — manage calendars, terms, blackout dates.
- `POST /api/config/grading-scales` — create/update grading scales.
- `POST /api/config/custom-attributes` — create/update/delete custom attribute definitions.
- `POST /api/config/templates` — create/publish notification templates; preview endpoints.
- `GET /api/config/search` — filter by entity type, scope, key/name.

## Events
- `ConfigurationChanged` (setting key, scope, version, actor)
- `CalendarPublished` (calendar id, year, school/district scope)
- `GradingScaleUpdated` (scale id, scope, version)
- `CustomAttributeUpdated`
- `TemplatePublished`

## Validation/Headers
- Auth required; tenant context from gateway headers.  
- Writes generate `AuditRecord` with before/after payload; responses include `ETag`/`version` for optimistic concurrency.

## Consumers
- Assessment, Intervention, Section, Student, Staff services consume ConfigurationChanged + CalendarPublished to refresh caches and validate operations.
