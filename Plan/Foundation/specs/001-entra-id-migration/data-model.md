# Data Model - Identity Migration

## Entities

### User (Existing/Modified)
- `Id` (Guid, PK)
- `Email` (string)
- `FirstName` (string)
- `LastName` (string)
- `ExternalId` (string, nullable) - Entra ID Subject ID
- `AuthProvider` (string) - "EntraID"

### ExternalProviderLink
- `Id` (Guid, PK)
- `UserId` (Guid, FK)
- `Provider` (string)
- `ProviderSubjectId` (string)
- `CreatedAt` (DateTime)

## Migration Mapping
- Legacy `SubjectId` -> `ExternalProviderLink.ProviderSubjectId` (if keeping legacy history)
- Entra ID `oid` -> `User.ExternalId`
