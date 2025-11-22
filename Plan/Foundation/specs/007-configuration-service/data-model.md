# Data Model: Configuration Service

**Feature ID**: 007-configuration-service  
**Database**: PostgreSQL (tenant-scoped with RLS)  
**Last Updated**: 2025-11-20

## Core Entities
- **SettingDefinition**
  - Fields: `id`, `key`, `data_type`, `description`, `default_value`, `allowed_values`, `created_at`.
  - Notes: global catalog of supported settings.

- **SettingValue**
  - Fields: `id`, `definition_id`, `tenant_id`, `scope` (System|District|School), `scope_ref_id` (nullable for system), `value`, `version`, `updated_at`, `updated_by`.
  - Notes: hierarchical override resolution order System < District < School; unique constraint on (`definition_id`, `tenant_id`, `scope`, `scope_ref_id`).

- **ResolvedSetting (materialized/cache)**
  - Fields: `tenant_id`, `school_id`, `key`, `effective_value`, `version`, `resolved_at`.
  - Notes: populated after write; cached in Redis to satisfy p95 <50ms.

- **AcademicCalendar**
  - Fields: `id`, `tenant_id`, `school_id`, `name`, `year`, `status` (Draft|Published|Archived), `created_at`, `published_at`.

- **Term**
  - Fields: `id`, `calendar_id`, `name`, `start_date`, `end_date`, `sequence`, `created_at`.
  - Notes: overlaps prohibited within the same calendar.

- **Session/GradingPeriod**
  - Fields: `id`, `calendar_id`, `name`, `start_date`, `end_date`, `type` (Session|GradingPeriod).
  - Notes: conflict detection with blackout dates.

- **BlackoutDate**
  - Fields: `id`, `calendar_id`, `date`, `reason`.

- **GradingScale**
  - Fields: `id`, `tenant_id`, `school_id`, `name`, `bands (jsonb)`, `effective_date`, `expires_at`, `created_at`.
  - Notes: validation prevents overlapping thresholds in `bands`.

- **CustomAttributeDefinition**
  - Fields: `id`, `tenant_id`, `scope` (Student|Staff|Section|Intervention|Assessment), `name`, `data_type`, `validation_rules`, `created_at`.
  - Notes: unique per tenant + scope; collisions with district/school handled via validation.

- **NotificationTemplate**
  - Fields: `id`, `tenant_id`, `scope` (System|District|School), `name`, `version`, `body_html`, `body_text`, `merge_fields`, `created_at`, `published_at`.

- **AuditRecord**
  - Fields: `id`, `tenant_id`, `entity_type`, `entity_id`, `action`, `actor`, `before`, `after`, `created_at`.

## Events
- `ConfigurationChanged`, `CalendarPublished`, `GradingScaleUpdated`, `CustomAttributeUpdated`, `TemplatePublished`.
- All include `tenant_id`, `scope`, `version`, `correlation_id`.

## Access Patterns
- Read: resolved settings via Redis key `cfg:{tenant}:{school}:{key}`; DB fallback queries `ResolvedSetting`.
- Write: transactionally update SettingValue, recompute ResolvedSetting, publish ConfigurationChanged.
- Calendars: enforce non-overlap via exclusion constraints on `(daterange(start_date, end_date), school_id)`.
