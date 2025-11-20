---
description: Process scenario files and execute speckit.specify and speckit.plan for each scenario to generate complete feature specifications and implementation plans.
handoffs: 
  - label: Specify Scenario
    agent: speckit.specify
    prompt: Create specification for this scenario
    send: false
  - label: Plan Scenario
    agent: speckit.plan
    prompt: Create implementation plan for the specification
    send: false
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

The text the user typed after `/speckit.scenario` in the triggering message **is** the path to the scenarios directory or a specific scenario file. Assume you always have it available in this conversation even if `$ARGUMENTS` appears literally below.

Given the scenario path, do this:

1. **Parse and validate input**:
   - If user provides a directory path (e.g., `Plan/Foundation/Plans/scenarios`):
     - List all `.md` files in that directory
     - Exclude `README.md` and `SCENARIO_INVENTORY.md` files
   - If user provides a specific file path:
     - Validate the file exists and is a `.md` file
     - Process only that single file
   - If no path provided:
     - Default to `Plan/Foundation/Plans/scenarios` directory
   - Convert all paths to absolute paths from repository root

2. **Pre-flight validation**:
   ```bash
   cd /home/runner/work/NorthStarET/NorthStarET
   git fetch --all --prune
   git status
   ```
   - Ensure repository is in clean state
   - Check that `.specify/scripts/bash/` scripts are available
   - Verify `.github/agents/speckit.specify.agent.md` and `.github/agents/speckit.plan.agent.md` exist

3. **Process each scenario file**:
   
   For each scenario file found:
   
   a. **Read and analyze scenario content**:
      - Read the entire scenario file
      - Extract the feature title (first H1 heading)
      - Extract business value and service information
      - Extract all scenario descriptions (Given/When/Then blocks)
   
   b. **Generate feature description for speckit.specify**:
      - Combine the feature title, business value, and all scenarios into a coherent feature description
      - Format as: 
        ```
        Feature: [Feature Title]
        Business Value: [Business Value]
        
        [All scenario descriptions with Given/When/Then blocks]
        ```
   
   c. **Invoke speckit-specify agent**:
      - Call the `speckit-specify` custom agent tool
      - Pass the generated feature description as the prompt argument
      - The specify agent will:
        - Generate a short name for the feature
        - Create a new feature branch
        - Generate the specification in `specs/[###-feature-name]/spec.md`
      - Wait for completion and parse the output to find:
        - Branch name created (pattern: `[###]-feature-name`)
        - Spec file path (pattern: `specs/[###]-feature-name]/spec.md`)
      - If the agent reports success, continue to next step
      - If the agent reports failure, log the error and skip to next scenario
   
   d. **Switch to the created feature branch**:
      ```bash
      cd /home/runner/work/NorthStarET/NorthStarET
      git checkout [branch-name]
      git status
      ```
      - Verify the branch switch was successful
      - Confirm the spec.md file exists
   
   e. **Invoke speckit-plan agent**:
      - Call the `speckit-plan` custom agent tool
      - Pass empty prompt (the plan agent will auto-detect the current feature from the current branch)
      - The plan agent will:
        - Load the spec.md created in the previous step
        - Generate plan.md with technical implementation details
        - Generate research.md with technology decisions
        - Generate data-model.md with entity definitions
        - Generate contracts/ directory with API specifications
        - Update agent context files
      - Wait for completion and parse the output to find:
        - Plan file path (pattern: `specs/[###]-feature-name]/plan.md`)
        - Other generated artifacts
      - If the agent reports success, record all generated artifacts
      - If the agent reports failure, log the error but mark as partial success (spec exists)
   
   f. **Record results**:
      - Track the scenario file processed
      - Track the branch created
      - Track all artifacts generated (spec.md, plan.md, etc.)
      - Track any errors or warnings

4. **Generate summary report**:
   
   After processing all scenarios, generate a markdown report:
   
   ```markdown
   # Scenario Processing Report
   
   **Date**: [ISO timestamp]
   **Total Scenarios Processed**: [count]
   **Successful**: [count]
   **Failed**: [count]
   
   ## Results by Scenario
   
   ### ✓ [Scenario File Name]
   - **Branch**: [branch-name]
   - **Spec**: specs/[###]-feature-name/spec.md
   - **Plan**: specs/[###]-feature-name/plan.md
   - **Research**: specs/[###]-feature-name/research.md
   - **Status**: Complete
   
   ### ✗ [Failed Scenario File Name]
   - **Error**: [error message]
   - **Status**: Failed
   
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
   
   Save this report to: `Plan/Foundation/Plans/scenarios/PROCESSING_REPORT_[timestamp].md`

5. **Return to original branch**:
   ```bash
   git checkout [original-branch]
   ```

## Error Handling

- **Missing scenario directory**: ERROR "Scenario directory not found at [path]. Please provide a valid path."
- **No scenario files found**: ERROR "No .md files found in [directory]. Directory must contain scenario files."
- **speckit-specify fails**: Log error, continue with next scenario if in batch mode
- **speckit-plan fails**: Log error, branch already created with spec, mark as partial success
- **Git operations fail**: STOP and report git error with instructions to resolve manually

## Important Notes

- Each scenario will create its own feature branch
- All branches are independent and can be worked on in parallel
- The original branch remains unchanged; all work happens in feature branches
- If a scenario has already been processed (branch exists), skip it and note in report
- Use `--force` flag to reprocess existing scenarios (will create new branch numbers)
- Always validate that custom agent tools (`speckit-specify`, `speckit-plan`) are available before starting

## Usage Examples

Process all scenarios in default directory:
```
/speckit.scenario
```

Process all scenarios in specific directory:
```
/speckit.scenario Plan/Foundation/Plans/scenarios
```

Process a single scenario file:
```
/speckit.scenario Plan/Foundation/Plans/scenarios/10-intervention-management-service.md
```

Reprocess a scenario (force new branch):
```
/speckit.scenario --force Plan/Foundation/Plans/scenarios/05-student-management-service.md
```
