# Scenario Processing Agent Walkthrough

This document demonstrates how to use the `speckit.scenario` agent to batch-process scenario files and generate complete feature specifications and implementation plans.

## Overview

The scenario agent automates the process of converting scenario files (written in Given/When/Then format) into:
1. Feature specifications with user stories and acceptance criteria
2. Implementation plans with technical details
3. Ready-to-implement feature branches

## Prerequisites

- Repository cloned and up to date
- Clean working directory (no uncommitted changes)
- GitHub Copilot Coding Agent enabled

## Scenario Files Location

Scenario files are located in: `Plan/Foundation/Plans/scenarios/`

Example scenario files:
- `05-student-management-service.md`
- `10-intervention-management-service.md`
- `08-staff-management-service.md`
- etc.

Each scenario file contains:
- Feature title and business value
- Multiple scenarios in Given/When/Then format
- Service descriptions and integration points

## Basic Usage

### Process All Scenarios

To process all scenario files in the default directory:

```
/speckit.scenario
```

This will:
1. Read all `.md` files from `Plan/Foundation/Plans/scenarios/`
2. Skip `README.md` and `SCENARIO_INVENTORY.md`
3. For each scenario file:
   - Create a specification
   - Create a feature branch
   - Generate an implementation plan
4. Generate a processing report

### Process Specific Directory

To process scenarios from a different directory:

```
/speckit.scenario Plan/Foundation/Plans/scenarios
```

### Process Single Scenario

To process just one scenario file:

```
/speckit.scenario Plan/Foundation/Plans/scenarios/05-student-management-service.md
```

## What Happens During Processing

### Step 1: Scenario Analysis

The agent reads each scenario file and extracts:
- Feature title (first H1 heading)
- Business value statement
- Service name
- All Given/When/Then scenario blocks

Example from `05-student-management-service.md`:
```
Feature: Migrate Student Management from Monolith to Microservice
Business Value: Modern student data management with event-driven architecture

Scenario 1: Create New Student with Event Publishing
Given a school administrator is authenticated...
When they enter student details...
Then the Student Service validates...
```

### Step 2: Specification Generation

The agent invokes `speckit.specify` with the extracted content, which:
- Generates a short name: `student-management-service`
- Finds the next available feature number: `001`
- Creates feature branch: `001-student-management-service`
- Generates `specs/001-student-management-service/spec.md` with:
  - Feature description
  - User stories (extracted from scenarios)
  - Acceptance criteria
  - Success metrics
  - Key entities

### Step 3: Branch Switch

The agent switches to the newly created feature branch:
```bash
git checkout 001-student-management-service
```

### Step 4: Plan Generation

The agent invokes `speckit.plan` (automatically detects current feature), which generates:
- `plan.md` - Technical implementation approach
- `research.md` - Technology decisions and rationale
- `data-model.md` - Entity definitions and relationships
- `contracts/` - API specifications (OpenAPI/GraphQL schemas)

Updates:
- `.github/copilot-instructions.md` - Project-specific guidelines

### Step 5: Result Recording

The agent records:
- Branch name created
- All artifacts generated
- Any errors or warnings
- Processing timestamp

### Step 6: Return to Original Branch

After processing all scenarios, the agent returns to the original branch (e.g., `main`)

## Processing Report

After completion, the agent generates a report at:
`Plan/Foundation/Plans/scenarios/PROCESSING_REPORT_[timestamp].md`

Example report:

```markdown
# Scenario Processing Report

**Date**: 2025-11-20T12:40:00Z
**Total Scenarios Processed**: 3
**Successful**: 3
**Failed**: 0

## Results by Scenario

### ✓ 05-student-management-service.md
- **Branch**: 001-student-management-service
- **Spec**: specs/001-student-management-service/spec.md
- **Plan**: specs/001-student-management-service/plan.md
- **Research**: specs/001-student-management-service/research.md
- **Data Model**: specs/001-student-management-service/data-model.md
- **Contracts**: specs/001-student-management-service/contracts/
- **Status**: Complete

### ✓ 08-staff-management-service.md
- **Branch**: 002-staff-management-service
- **Spec**: specs/002-staff-management-service/spec.md
- **Plan**: specs/002-staff-management-service/plan.md
- **Research**: specs/002-staff-management-service/research.md
- **Data Model**: specs/002-staff-management-service/data-model.md
- **Contracts**: specs/002-staff-management-service/contracts/
- **Status**: Complete

### ✓ 10-intervention-management-service.md
- **Branch**: 003-intervention-management-service
- **Spec**: specs/003-intervention-management-service/spec.md
- **Plan**: specs/003-intervention-management-service/plan.md
- **Research**: specs/003-intervention-management-service/research.md
- **Data Model**: specs/003-intervention-management-service/data-model.md
- **Contracts**: specs/003-intervention-management-service/contracts/
- **Status**: Complete

## Next Steps

For each successful feature:
1. Review the generated specification in specs/[###]-feature-name/spec.md
2. Review the implementation plan in specs/[###]-feature-name/plan.md
3. Use `/speckit.tasks` to generate implementation tasks
4. Use `/speckit.implement` to begin implementation

To continue with a specific feature:
```bash
git checkout [branch-name]
```
```

## Working with Generated Features

After the scenario agent completes, you have multiple feature branches to work with.

### Review a Feature

```bash
# Switch to feature branch
git checkout 001-student-management-service

# Review the specification
cat specs/001-student-management-service/spec.md

# Review the implementation plan
cat specs/001-student-management-service/plan.md

# Review technical decisions
cat specs/001-student-management-service/research.md
```

### Generate Implementation Tasks

```bash
# On the feature branch
/speckit.tasks
```

This generates `specs/001-student-management-service/tasks.md` with:
- Phase-based task breakdown
- Dependency ordering
- Test requirements
- File paths for each task

### Create Validation Checklists

```bash
/speckit.checklist Create a security checklist for student data
```

Generates domain-specific checklists in `specs/001-student-management-service/checklists/`

### Begin Implementation

```bash
/speckit.implement
```

This will:
1. Validate all checklists are complete
2. Execute tasks phase by phase
3. Commit progress after each task
4. Run tests to validate changes

### Parallel Development

Since each scenario creates an independent feature branch, you can:

1. **Work on multiple features simultaneously** (different team members)
2. **Prioritize features independently**
3. **Merge features in any order**
4. **Maintain clean separation of concerns**

Example parallel workflow:
```bash
# Terminal 1 - Team member A
git checkout 001-student-management-service
/speckit.implement

# Terminal 2 - Team member B  
git checkout 002-staff-management-service
/speckit.implement

# Terminal 3 - Team member C
git checkout 003-intervention-management-service
/speckit.implement
```

## Error Handling

### Scenario File Not Found

If a scenario file doesn't exist:
```
ERROR: Scenario file not found at [path]. Please check the path and try again.
```

**Solution**: Verify the file exists and path is correct

### No Scenario Files in Directory

If directory is empty or contains no `.md` files:
```
ERROR: No .md files found in [directory]. Directory must contain scenario files.
```

**Solution**: Check the directory path and ensure it contains scenario files

### Specification Generation Fails

If `speckit.specify` fails:
- Error is logged in the report
- Agent continues with next scenario
- Partial results may be available

**Solution**: Review the error message and scenario content for issues

### Plan Generation Fails

If `speckit.plan` fails:
- Error is logged in the report
- Branch and specification are still created (partial success)
- Agent continues with next scenario

**Solution**: You can manually invoke `/speckit.plan` on the feature branch

### Git Conflicts

If branch already exists:
- Agent skips the scenario
- Notes in report that branch exists

**Solution**: Use `--force` flag to reprocess (will create new branch number)

## Advanced Usage

### Reprocess Existing Scenarios

To force reprocessing of scenarios that already have branches:

```
/speckit.scenario --force Plan/Foundation/Plans/scenarios/05-student-management-service.md
```

This will create a new branch with the next available number.

### Custom Processing Order

To process scenarios in a specific order:

```bash
# Process high-priority scenarios first
/speckit.scenario Plan/Foundation/Plans/scenarios/05-student-management-service.md
/speckit.scenario Plan/Foundation/Plans/scenarios/08-staff-management-service.md

# Then process others
/speckit.scenario Plan/Foundation/Plans/scenarios
```

### Integration with Project Management

After batch processing:

```bash
# Convert all tasks to GitHub issues
for branch in $(git branch | grep -E '^  [0-9]+-'); do
  git checkout $branch
  /speckit.taskstoissues
done
```

This creates trackable GitHub issues for all generated features.

## Best Practices

1. **Process in batches**: Use the directory mode to process multiple scenarios at once
2. **Review before implementing**: Check the processing report and review specs/plans
3. **Prioritize features**: Work on generated branches in priority order
4. **Use checklists**: Create validation checklists before implementation
5. **Test incrementally**: Each feature branch is independently testable
6. **Update scenarios**: If specs need changes, update the original scenario files
7. **Document decisions**: Research and decision artifacts are auto-generated

## Troubleshooting

### Agent Not Available

**Symptom**: `/speckit.scenario` not recognized

**Solution**: 
- Ensure file exists at `.github/agents/speckit.scenario.agent.md`
- Check file has correct front matter
- Restart Copilot session

### Processing Takes Long Time

**Symptom**: Agent processing seems stuck

**Solution**:
- Each scenario takes 2-5 minutes (specification + plan generation)
- Processing 10 scenarios may take 20-50 minutes
- Check the terminal for progress updates
- Consider processing one scenario first to verify setup

### Generated Branches Missing

**Symptom**: Processing report shows success but branches don't exist

**Solution**:
```bash
git fetch --all --prune
git branch -a
```

Branches may be remote-only; fetch and switch to them.

## Example: Complete Workflow

Here's a complete example of processing scenarios and implementing features:

```bash
# 1. Start from clean main branch
git checkout main
git pull origin main

# 2. Process all scenarios
/speckit.scenario

# 3. Review the processing report
cat Plan/Foundation/Plans/scenarios/PROCESSING_REPORT_*.md

# 4. Work on first feature
git checkout 001-student-management-service
cat specs/001-student-management-service/spec.md
cat specs/001-student-management-service/plan.md

# 5. Generate tasks
/speckit.tasks

# 6. Create checklists
/speckit.checklist Create security checklist
/speckit.checklist Create testing checklist

# 7. Analyze for consistency
/speckit.analyze

# 8. Begin implementation
/speckit.implement

# 9. After implementation, create PR
gh pr create --base main --title "Implement Student Management Service" --body "Closes #001"

# 10. Move to next feature
git checkout main
git checkout 002-staff-management-service
# Repeat steps 4-9
```

## Summary

The `speckit.scenario` agent provides:
- ✅ Automated batch processing of scenario files
- ✅ Complete specification generation
- ✅ Technical implementation plans
- ✅ Independent feature branches
- ✅ Comprehensive processing reports
- ✅ Error handling and recovery
- ✅ Parallel development support

This enables efficient migration of scenarios into production-ready feature implementations.
