# NorthStarET Specification Workflow Constitution

## Branch Strategy

### Three-Branch Workflow

The specification and implementation workflow uses three distinct branch types:

1. **`main` branch**: Production-ready code, single source of truth
2. **`Specs` branch**: Approved specifications (created from `main`)
3. **`proposed-specs` branch**: Active specification work (created from `Specs`)
4. **`{LayerName}/###-feature-name` branches**: Implementation branches (created from `Specs`)

### Specification Phase Workflow

**Branch**: `proposed-specs` (all specification work happens here)

1. **`/speckit.specify`**:
   - Creates `Specs` branch from `main` (if it doesn't exist)
   - Creates `proposed-specs` branch from `Specs` (if it doesn't exist)
   - Checks out `proposed-specs` branch
   - Creates specification files in `Plan/{LayerName}/specs/###-feature-name/`
   - Commits and pushes to `proposed-specs`
   - Creates pull request: `proposed-specs` → `Specs`

2. **`/speckit.plan`**:
   - Verifies current branch is `proposed-specs`
   - Generates planning artifacts (plan.md, research.md, data-model.md, contracts/)
   - Commits and pushes to `proposed-specs`
   - Updates existing pull request

3. **`/speckit.tasks`**:
   - Verifies current branch is `proposed-specs`
   - Generates task breakdown (tasks.md)
   - Commits and pushes to `proposed-specs`
   - Updates existing pull request

**Approval Gate**: Merge `proposed-specs` → `Specs` when specification is complete and approved

### Implementation Phase Workflow

**Branch**: `{LayerName}/###-feature-name` (layer-prefixed implementation branches)

1. **`/speckit.implement {LayerName} ###`**:
   - Requires layer name and spec number as parameters
   - Creates implementation branch from `Specs`: `{LayerName}/###-feature-name`
   - Branch naming: `Foundation/001-identity-migration`, `DigitalInk/002-ink-capture`, etc.
   - Copies specification artifacts from Specs branch
   - Implements features in `Src/{LayerName}/` directories
   - Commits and pushes to implementation branch
   - Creates pull request: `{LayerName}/###-feature-name` → `main`

**Approval Gate**: Merge `{LayerName}/###-feature-name` → `main` when implementation is complete and tested

### Layer Organization

All features must be assigned to a layer in the mono-repo architecture:

- **Foundation**: Core LMS migration services (Identity, Configuration, Student Management, Assessment, etc.)
- **DigitalInk**: Digital ink capture, audio recording, and AI-ready data export
- **Custom**: New layers (requires Architecture Review)

Each layer has:
- Planning directory: `Plan/{LayerName}/`
- Specifications: `Plan/{LayerName}/specs/###-feature-name/`
- Implementation: `Src/{LayerName}/services/`, `Src/{LayerName}/shared/`

## Core Principles

### I. Specification-First Development

- All features begin with specification on `proposed-specs` branch
- Specifications are technology-agnostic, focused on WHAT and WHY (not HOW)
- Planning artifacts generated before implementation begins
- No implementation branches created until specification is approved

### II. Layer-Based Organization

- Every feature assigned to a specific layer (Foundation, DigitalInk, etc.)
- Layer-prefixed implementation branches maintain organizational boundaries
- Cross-layer dependencies must be documented and justified
- New layers require Architecture Review approval

### III. Pull Request Workflow

- Specifications: `proposed-specs` → `Specs` (approval before implementation)
- Implementation: `{LayerName}/###-feature-name` → `main` (merge when complete)
- All changes require pull request review
- No direct commits to `main` or `Specs` branches

## Development Workflow

### Specification Workflow

1. Run `/speckit.specify` with feature description
2. Agent creates/updates `proposed-specs` branch
3. Agent generates spec.md with requirements
4. Agent creates PR to `Specs` branch
5. Run `/speckit.plan` to generate planning artifacts
6. Run `/speckit.tasks` to generate task breakdown
7. Review and approve PR to merge into `Specs`

### Implementation Workflow

1. Ensure specification is approved (merged into `Specs`)
2. Run `/speckit.implement {LayerName} ###` with layer and spec number
3. Agent creates implementation branch from `Specs`
4. Agent implements features per tasks.md
5. Agent creates PR to `main`
6. Review, test, and approve PR to merge into `main`

## Governance

- **Branch Protection**: `main` and `Specs` branches require pull request reviews
- **No Direct Commits**: All changes must go through pull request workflow
- **Specification Approval**: Implementation cannot begin until specification is merged into `Specs`
- **Layer Consistency**: Implementation branch must match layer specified in specification
- **Agent Compliance**: All speckit agents must follow the three-branch workflow

**Version**: 3.0.0 | **Ratified**: 2025-11-20 | **Last Amended**: 2025-11-20

## Version History

### 3.0.0 (2025-11-20) - BREAKING CHANGE
- Introduced three-branch workflow: `main` → `Specs` → `proposed-specs`
- Removed automatic branch creation in `/speckit.specify`
- Deferred implementation branch creation to `/speckit.implement`
- Added layer-prefixed implementation branches: `{LayerName}/###-feature-name`
- All specification work now happens on `proposed-specs` branch
- Implementation branches created from `Specs` (not from layer-prefixed spec branches)
