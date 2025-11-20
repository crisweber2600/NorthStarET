# Data Model: Configuration Service
Layer: Foundation
Version: 0.1.0

## District
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid | Unique per district |
| Name | text |  |
| State | text | 2-letter |
| Timezone | text | IANA identifier |
| AcademicYearStart | date |  |
| CreatedAt | timestamptz |  |

## School
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| DistrictId | uuid |  |
| Name | text |  |
| GradeLevels | int[] |  |
| Capacity | int |  |
| CreatedAt | timestamptz |  |

## Calendar
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid |  |
| TenantId | uuid |  |
| DistrictId | uuid |  |
| SchoolId | uuid | nullable |
| AcademicYear | int | 2025 etc |
| FirstDay | date |  |
| LastDay | date |  |
| Holidays | jsonb |  |
| Breaks | jsonb |  |

## ConfigurationSetting
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid | nullable (system default if null) |
| SchoolId | uuid | nullable |
| Key | text |  |
| Value | jsonb |  |
| IsSystemDefault | bool |  |
| UpdatedAt | timestamptz |  |

## GradingScale
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| GradeRange | text | e.g. K-5 |
| ScaleDefinition | jsonb | structured descriptors |
| UpdatedAt | timestamptz |  |

## CustomAttribute
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| EntityType | text | STUDENT, STAFF |
| Name | text |  |
| DataType | text | STRING, ENUM, BOOL |
| Options | jsonb | for ENUM |
| CreatedAt | timestamptz |  |

## NotificationTemplate
| Column | Type | Notes |
|--------|------|-------|
| Id | uuid | PK |
| TenantId | uuid |  |
| Name | text |  |
| Subject | text |  |
| Body | text | may contain merge fields |
| CreatedAt | timestamptz |  |
| Version | int | optimistic concurrency |

Indexes: Settings(TenantId, Key), School(TenantId, DistrictId), Calendar(TenantId, AcademicYear), CustomAttribute(TenantId, EntityType).

---
Draft model.