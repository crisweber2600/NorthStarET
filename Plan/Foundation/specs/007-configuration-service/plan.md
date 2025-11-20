# Plan: Configuration Service Migration
Version: 0.1.0
Status: Draft (Planning)
Layer: Foundation
Spec Ref: 007-configuration-service/spec.md

## Objectives
- Provide authoritative multi-tenant configuration with hierarchical overrides.
- Enable fast cached reads; consistent cross-service consumption via events.
- Maintain comprehensive audit trail for compliance.

## Architecture Components
1. API Layer – Controllers: Districts, Schools, Calendars, Settings, GradingScales, Templates, Attributes.
2. Application – Commands (CreateDistrict, CreateSchool, UpdateCalendar, UpdateSetting, AddCustomAttribute, UpdateGradingScale), Queries (GetDistrictSettings, GetCalendar, GetGradingScale, ListSchools).
3. Domain – Entities: District, School, Calendar, ConfigurationSetting, GradingScale, CustomAttribute, NotificationTemplate; Events: DistrictCreated, SchoolCreated, ConfigurationChanged, GradingScaleChanged.
4. Infrastructure – EF Core DbContext, SettingsRepository, CacheProvider (Redis), EventPublisher, AuditInterceptor.

## Hierarchy Resolution
Order: School override (tenant_id + school_id) → District (tenant_id) → System default (no tenant).
Resolution algorithm memoized per tenant.

## Caching Strategy
- Key format: `config:{tenantId}:{scope}:{version}`.
- Version bump on ConfigurationChangedEvent.
- Calendar & grading scale cached separately.

## Data Model Highlights
- District(TenantId unique, Name, State, Timezone).
- School(DistrictId, Name, GradeLevels[]).
- ConfigurationSetting(TenantId?, SchoolId?, Key, Value, IsSystemDefault, UpdatedAt).

## Commands Example
```csharp
public record UpdateSettingCommand(string Key, JsonNode Value, Guid? SchoolId) : ICommand<Result>;
```

## Events Payload Minimal Fields
- DistrictCreated: TenantId, DistrictId, Name, Timestamp.
- ConfigurationChanged: TenantId, Key, Scope, Timestamp.

## Cache Invalidation
On ConfigurationChangedEvent: remove keys matching tenant + key; optionally rebuild warm cache.

## Audit Logging
EF interceptor captures changes on tracked entities; writes AuditRecord with before/after JSON diff.

## Testing Strategy
- Unit: hierarchy resolution, cache invalidation, grading scale conversion.
- Integration: creation flows publishing events, RLS enforcement.
- BDD: scenario mapping for creation & hierarchy behaviors.
- Performance: cache hit latency (<5ms), cache miss (<50ms).

## Risks
| Risk | Impact | Mitigation |
|------|--------|------------|
| Overlapping overrides complexity | Incorrect values | Graph tests of resolution combinations |
| Cache stampede | Latency spikes | Mutex rebuild lock & jitter TTL |
| Large attribute sets | Query slowdown | Pagination + size limits |

## Completion Criteria
- All spec scenarios green.
- Cache hit ratio ≥90% in typical read workloads.
- Audit entries present for all mutations.

---
Draft plan.