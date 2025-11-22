# Data Model - Configuration Service

## Entities

### District
- `Id` (Guid, PK)
- `Name` (string)
- `TenantId` (string, unique)
- `State` (string)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)

### School
- `Id` (Guid, PK)
- `DistrictId` (Guid, FK)
- `Name` (string)
- `GradeLevels` (string[])
- `Capacity` (int)

### ConfigurationEntry
- `Id` (Guid, PK)
- `Scope` (enum: System, District, School)
- `ScopeId` (Guid, nullable) - Null for System, DistrictId or SchoolId otherwise.
- `Key` (string)
- `Value` (string, JSON)
- `DataType` (string)
- `IsEncrypted` (bool)

### AcademicCalendar
- `Id` (Guid, PK)
- `DistrictId` (Guid, FK)
- `Name` (string)
- `StartDate` (Date)
- `EndDate` (Date)
- `Events` (JSON) - Holidays, breaks.

## Relationships
- District 1:N School
- District 1:N ConfigurationEntry (Scope=District)
- School 1:N ConfigurationEntry (Scope=School)
- District 1:N AcademicCalendar
