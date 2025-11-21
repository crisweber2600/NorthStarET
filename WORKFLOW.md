# NorthStar ET Development Workflow

## Overview

This repository uses a specification-driven development workflow powered by custom GitHub Copilot agents. The workflow ensures that all features are properly specified, planned, and implemented before merging to the main branch.

## Branch Strategy

### Default Branch: `main`

All implementation work is merged back to `main` through pull requests. This ensures:
- Clean, linear history on main
- Code review before integration
- Automated checks and validations

### Feature Branches

Feature branches follow the naming convention: `###-feature-name`
- `###`: Three-digit sequential number (e.g., 001, 002, 003)
- `feature-name`: Short, descriptive name (2-4 words, kebab case)

Examples:
- `001-unified-sso-auth`
- `002-bootstrap-tenant-access`
- `003-user-authentication`

## Development Workflow

### 1. Specify Phase (`speckit.specify`)

**Purpose**: Create feature specification from requirements

**Actions**:
- Creates a new feature branch (`###-feature-name`)
- Generates `specs/###-feature-name/spec.md`
- Defines user scenarios, requirements, and success criteria

**Branch**: Creates and checks out new feature branch

**Command**: `/speckit.specify <feature description>`

---

### 2. Plan Phase (`speckit.plan`)

**Purpose**: Generate technical design and implementation plan

**Actions**:
- Creates `plan.md` with tech stack and architecture
- Generates `data-model.md` for entities and relationships
- Creates API contracts in `contracts/` directory
- Updates agent context with technology choices

**Branch**: Works on current feature branch (does not create new branches)

**Command**: `/speckit.plan`

---

### 3. Task Generation (`speckit.tasks`)

**Purpose**: Break down plan into actionable tasks

**Actions**:
- Analyzes plan and specification
- Generates `tasks.md` with ordered task list
- Identifies dependencies and parallelizable tasks
- Creates task phases (Setup, Tests, Core, Integration, Polish)

**Branch**: Works on current feature branch (does not create new branches)

**Command**: `/speckit.tasks`

---

### 4. Implementation Phase (`speckit.implement`)

**Purpose**: Execute implementation plan and complete all tasks

**Actions**:
- Verifies project setup (ignore files, dependencies)
- Executes tasks in dependency order
- Follows TDD approach (tests before code)
- Reports progress after each completed task
- Validates implementation against specification

**Branch**: Works on current feature branch (does not create new branches)

**Command**: `/speckit.implement`

---

### 5. Pull Request to Main

**Target Branch**: `main` (automatic, based on repository default branch)

**Process**:
1. Implementation phase completes all tasks on feature branch
2. Agent commits and pushes changes via `report_progress` tool
3. Pull request is created automatically
4. PR targets `main` branch (GitHub default behavior)
5. Code review and automated checks run
6. Upon approval, changes merge to `main`

## Key Principles

### Branch Creation Control

- ✅ **Only** `speckit.specify` creates new branches
- ✅ `speckit.plan` works on existing branch
- ✅ `speckit.implement` works on existing branch
- ✅ No branch creation during planning or implementation phases

### PR Targeting

- ✅ All PRs automatically target `main` (repository default branch)
- ✅ No manual PR base configuration needed
- ✅ Clean merge path: `feature-branch → main`

### Workflow Isolation

Each feature follows the complete workflow on its own branch:
```
main
 ├── 001-feature-a (specify → plan → implement → PR to main)
 ├── 002-feature-b (specify → plan → implement → PR to main)
 └── 003-feature-c (specify → plan → implement → PR to main)
```

## Verification

To verify the workflow is correctly configured:

```bash
# Check default branch
git remote show origin | grep "HEAD branch"
# Should output: HEAD branch: main
# Note: Output format may vary between Git versions, but should indicate 'main' as default

# Verify current branch PRs will target main
git rev-parse --abbrev-ref --symbolic-full-name @{upstream}
# Branch tracking ensures PR targets main by default
```

## Best Practices

1. **Start with Specification**: Always run `speckit.specify` first to create feature branch and spec
2. **Complete Planning**: Run `speckit.plan` and review before implementation
3. **Generate Tasks**: Use `speckit.tasks` to break down work into manageable chunks
4. **Follow TDD**: Write tests before implementation code during `speckit.implement` phase
5. **Review PRs**: All changes to `main` go through PR review process
6. **Keep Main Stable**: Main branch should always be in a deployable state

## Troubleshooting

### Issue: Wrong PR target branch
**Solution**: Verify repository default branch is set to `main` in GitHub settings

### Issue: Multiple feature branches
**Solution**: Each feature should have its own numbered branch; complete and merge one before starting another

### Issue: Branch number conflicts
**Solution**: Scripts automatically detect highest number and increment; fetch latest branches before creating new feature

## Summary

✅ **Implementation PRs target `main` by default**
- Repository default branch: `main`
- GitHub automatically sets PR base to default branch
- No manual configuration needed

✅ **Branch creation controlled during specify phase**
- Only `speckit.specify` creates branches
- All other agents work on existing branches
- Clean separation of concerns

✅ **Complete workflow isolation**
- Each feature has dedicated branch
- Full lifecycle on branch: specify → plan → implement
- Merge to main via PR after completion
