# Reqnroll Features: Schools & Grades

This directory contains Reqnroll (SpecFlow/Gherkin) BDD feature files for the Schools & Grades feature slice.

## Purpose

Feature files capture acceptance criteria from `spec.md` in executable Given/When/Then format, ensuring implementation matches functional requirements.

## Feature Files

### User Story 1: Maintain District Schools (P1)
- `district-admin-manages-schools.feature` - List, search, create, update, delete schools with tenant isolation

### User Story 2: Configure School Grades (P1)
- `district-admin-configures-grades.feature` - Manage grade offerings via checklist, range selection, select-all helpers

### User Story 3: System Admin Oversight (P2)
- `system-admin-oversees-districts.feature` - District switching, audit, cross-tenant safeguards

### User Story 4: District Admin Home (P4)
- `district-admin-home.feature` - Dashboard KPIs, invite management, access control

## Step Definitions

Step definition implementations live in:
```
tests/bdd/NorthStarET.NextGen.Lms.Bdd/StepDefinitions/SchoolCatalogSteps.cs
```

## Execution

Run all feature files:
```bash
dotnet test tests/bdd/NorthStarET.NextGen.Lms.Bdd/NorthStarET.NextGen.Lms.Bdd.csproj
```

## Constitution Compliance

- Feature files MUST exist before production code (Red→Green TDD)
- Step definition stubs committed before implementation
- Red/Green transcripts captured in `../checklists/phase-1-evidence/`
- All scenarios must link to acceptance criteria in spec.md
