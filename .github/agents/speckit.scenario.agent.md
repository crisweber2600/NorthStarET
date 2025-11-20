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
   - If user provides a directory path (e.g., `Plan/Foundation/scenarios`):
     - List all `.md` files in that directory
     - Exclude `README.md` and `SCENARIO_INVENTORY.md` files
     - **Extract layer name from path**: If path matches `Plan/{LayerName}/scenarios`, extract layer name
   - If user provides a specific file path:
     - Validate the file exists and is a `.md` file
     - Process only that single file
     - **Extract layer name from path**: If path matches `Plan/{LayerName}/scenarios/*.md`, extract layer name
   - If no path provided:
     - Default to `Plan/Foundation/scenarios` directory
     - Layer defaults to `Foundation`
   - Convert all paths to absolute paths from repository root
   - **Validate layer exists**: Check that `Plan/{LayerName}/` directory exists

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
        Target Layer: [LayerName from step 1]
        
        [All scenario descriptions with Given/When/Then blocks]
        ```
   
   c. **Invoke speckit.specify agent** (Constitution v2.1.0 workflow):
      - Call the `speckit.specify` agent
      - Pass the generated feature description as the prompt argument
      - **IMPORTANT**: The specify agent will prompt for layer selection - respond with the layer extracted in step 1
      - The specify agent will:
        - Generate a short name for the feature
        - Create a **specification branch** with pattern: `{LayerName}/[###]-feature-name-spec`
        - Create specification directory at: `Plan/{LayerName}/specs/[###-feature-name]/`
        - Generate the specification in `Plan/{LayerName}/specs/[###-feature-name]/spec.md`
        - Record layer metadata in `.layer` file
      - Wait for completion and parse the output to find:
        - Specification branch name (pattern: `{LayerName}/[###]-feature-name-spec`)
        - Spec file path (pattern: `Plan/{LayerName}/specs/[###]-feature-name]/spec.md`)
        - Layer assignment
      - If the agent reports success, continue to next step
      - If the agent reports failure, log the error and skip to next scenario
   
   d. **Verify specification branch**:
      ```bash
      git branch --show-current
      ```
      - Confirm current branch matches pattern: `{LayerName}/[###]-feature-name-spec`
      - Verify spec.md file exists at `Plan/{LayerName}/specs/[###-feature-name]/spec.md`
   
   e. **Invoke speckit.plan agent** (Constitution v2.1.0 workflow):
      - **IMPORTANT**: Must be on specification branch for plan agent to work
      - Call the `speckit.plan` agent
      - Pass empty prompt (the plan agent will auto-detect the current feature from the current branch)
      - The plan agent will:
        - Validate current branch is specification branch (`-spec` suffix)
        - Validate layer consistency with spec.md
        - Load the spec.md created in the previous step
        - Generate plan.md with technical implementation details and layer identification
        - Generate research.md with technology decisions
        - Generate data-model.md with entity definitions
        - Generate contracts/ directory with API specifications
        - Update agent context files
      - Wait for completion and parse the output to find:
        - Plan file path (pattern: `Plan/{LayerName}/specs/[###]-feature-name]/plan.md`)
        - Other generated artifacts
        - Layer consistency validation results
      - If the agent reports success, record all generated artifacts
      - If the agent reports failure, log the error but mark as partial success (spec exists)
   
f. **Publish specification branch** (Constitution v2.1.0):
   ```bash
   git push -u origin {LayerName}/[###]-feature-name-spec
   ```
   - Push specification branch to remote
   - Verify push was successful
   - Record remote branch URLg. **Create and publish proposed branch** (for review):
   ```bash
   git checkout -b {LayerName}/[###]-feature-name-proposed
   git push -u origin {LayerName}/[###]-feature-name-proposed
   git checkout {LayerName}/[###]-feature-name-spec
   ```
   - Create proposed branch from specification branch
   - Proposed branch contains spec.md, plan.md, and all planning artifacts
   - Push proposed branch to remote for stakeholder review
   - Return to specification branch
   - **Note**: Implementation branch (`{LayerName}/[###]-feature-name`) will be created later by `/speckit.implement`   h. **Record results**:
      - Track the scenario file processed
      - Track the specification branch created (`-spec` suffix)
      - Track the proposed branch created (`-proposed` suffix)
      - Track target layer assignment
      - Track all artifacts generated (spec.md, plan.md, etc.)
      - Track both remote branch URLs
      - Track any errors or warnings

4. **Generate summary report**:
   
   After processing all scenarios, generate a markdown report:
   
   ```markdown
   # Scenario Processing Report
   
   **Date**: [ISO timestamp]
   **Total Scenarios Processed**: [count]
   **Successful**: [count]
   **Failed**: [count]
   **Target Layer**: [LayerName]
   
   ## Branch Strategy (Constitution v2.1.0)
   
   Each scenario generates **two branches**:
   1. **Specification Branch** (`{LayerName}/[###]-feature-name-spec`): Contains planning artifacts (spec.md, plan.md, tasks.md)
   2. **Proposed Branch** (`{LayerName}/[###]-feature-name-proposed`): Copy of spec branch for stakeholder review
   3. **Implementation Branch** (`{LayerName}/[###]-feature-name`): Created later by `/speckit.implement` when development begins
   
   ## Results by Scenario
   
   ### ✓ [Scenario File Name]
   - **Layer**: [LayerName]
   - **Specification Branch**: {LayerName}/[###]-feature-name-spec (published)
   - **Proposed Branch**: {LayerName}/[###]-feature-name-proposed (published)
   - **Spec**: Plan/[LayerName]/specs/[###]-feature-name/spec.md
   - **Plan**: Plan/[LayerName]/specs/[###]-feature-name/plan.md
   - **Research**: Plan/[LayerName]/specs/[###]-feature-name/research.md
   - **Remote URLs**:
     - Spec branch: https://github.com/{owner}/{repo}/tree/{LayerName}/[###]-feature-name-spec
     - Proposed branch: https://github.com/{owner}/{repo}/tree/{LayerName}/[###]-feature-name-proposed
   - **Status**: Complete
   
   ### ✗ [Failed Scenario File Name]
   - **Error**: [error message]
   - **Status**: Failed
   
   ## Next Steps
   
   ### For Stakeholder Review:
   1. Review proposed branches for each feature
   2. Provide feedback via PR comments on proposed branches
   3. Approve or request changes to specifications
   
   ### For Implementation (after approval):
   1. Checkout specification branch: `git checkout {LayerName}/[###]-feature-name-spec`
   2. Generate tasks: `/speckit.tasks`
   3. Begin implementation: `/speckit.implement` (creates implementation branch)
   4. Implementation branch (`{LayerName}/[###]-feature-name`) created automatically
   
   ### To work on a specific specification:
   ```bash
   git checkout {LayerName}/[###]-feature-name-spec
   ```
   
   ### To review a specific proposal:
   ```bash
   git checkout {LayerName}/[###]-feature-name-proposed
   ```
   ```
   
   Save this report to: `Plan/{LayerName}/scenarios/PROCESSING_REPORT_[timestamp].md`

5. **Return to original branch**:
   ```bash
   git checkout [original-branch]
   ```

## Error Handling

- **Missing scenario directory**: ERROR "Scenario directory not found at [path]. Please provide a valid path."
- **No scenario files found**: ERROR "No .md files found in [directory]. Directory must contain scenario files."
- **Layer not found**: ERROR "Layer '[LayerName]' not found in Plan/ or Src/. Valid layers: Foundation, DigitalInk. For new layers, run Architecture Review first (Constitution Principle 6)."
- **speckit.specify fails**: Log error, continue with next scenario if in batch mode
- **speckit.plan fails**: Log error, specification branch already created with spec.md, mark as partial success
- **Git push fails**: Log error, branches exist locally but not published, mark as partial success with warning
- **Git operations fail**: STOP and report git error with instructions to resolve manually
- **Branch already exists**: Skip scenario and note in report (use `--force` to reprocess)

## Important Notes

- **Constitution v2.1.0 Workflow**: Each scenario creates **two published branches**:
  1. **Specification branch** (`{LayerName}/[###]-feature-name-spec`): Working branch for planning artifacts
  2. **Proposed branch** (`{LayerName}/[###]-feature-name-proposed`): Review branch for stakeholders
  3. **Implementation branch** (`{LayerName}/[###]-feature-name`): Created later by `/speckit.implement`
- **Layer Organization**: Specifications are placed in `Plan/{LayerName}/specs/[###-feature-name]/`
- **Layer Identification**: Each spec, plan, and task explicitly declares target layer (Constitution Principle 6)
- All branches are independent and can be worked on in parallel
- The original branch remains unchanged; all work happens in feature branches
- If a scenario has already been processed (specification branch exists), skip it and note in report
- Use `--force` flag to reprocess existing scenarios (will create new branch numbers)
- Both specification and proposed branches are published to remote for collaboration
- Always validate that agents (`speckit.specify`, `speckit.plan`) are available before starting

## Usage Examples

Process all scenarios in default directory (Foundation layer):
```
/speckit.scenario
```

Process all scenarios in Foundation layer:
```
/speckit.scenario Plan/Foundation/scenarios
```

Process all scenarios in DigitalInk layer:
```
/speckit.scenario Plan/DigitalInk/scenarios
```

Process a single scenario file (layer extracted from path):
```
/speckit.scenario Plan/Foundation/scenarios/10-intervention-management-service.md
```

Reprocess a scenario (force new branch numbers):
```
/speckit.scenario --force Plan/Foundation/scenarios/05-student-management-service.md
```

## Branch Naming Convention

For a scenario creating feature "user-authentication" as feature #003 in Foundation layer:

- **Specification branch**: `Foundation/003-user-authentication-spec` (planning artifacts)
- **Proposed branch**: `Foundation/003-user-authentication-proposed` (review copy)
- **Implementation branch**: `Foundation/003-user-authentication` (created later by `/speckit.implement`)

All branches are published to remote:
- `origin/Foundation/003-user-authentication-spec`
- `origin/Foundation/003-user-authentication-proposed`
- `origin/Foundation/003-user-authentication` (created later)
