---
description: Process scenario files and execute speckit.specify and speckit.plan for each scenario to generate complete feature specifications and implementation plans. Optimized for GitHub Copilot with sequential, non-parallel processing using runSubagent tool for automated agent orchestration.
handoffs: 
  - label: Specify Scenario
    agent: speckit.specify
    prompt: Create specification for this scenario
    send: true
  - label: Plan Scenario
    agent: speckit.plan
    prompt: Create implementation plan for the specification
    send: true
---

## GitHub Copilot Optimization

This agent is **optimized for GitHub Copilot** execution with the following design principles:

1. **Sequential Processing**: Processes scenarios one at a time, never in parallel
2. **Automated Agent Orchestration**: Uses `runSubagent` tool to invoke speckit.specify and speckit.plan agents automatically
3. **Clean State Management**: Returns to original branch after each scenario to ensure clean git state
4. **Rich Error Context**: Captures and logs detailed output from runSubagent for debugging
5. **Graceful Degradation**: Distinguishes between fatal errors (stop) and partial success (continue)
6. **Artifact Tracking**: Records all generated files and branch URLs for comprehensive reporting

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
   git fetch --all --prune
   git status
   git branch --show-current > original_branch.txt
   ```
   - Record original branch name for later return (step 3j)
   - Ensure repository is in clean state (no uncommitted changes)
   - Validate required files exist:
     - `.specify/scripts/bash/create-new-feature.sh`
     - `.specify/scripts/bash/setup-plan.sh`
     - `.github/agents/speckit.specify.agent.md`
     - `.github/agents/speckit.plan.agent.md`
   - If any validation fails: STOP with fatal error (see Error Handling section)

3. **Process each scenario file sequentially** (optimized for GitHub Copilot - no parallel processing):
   
   For each scenario file found (process one at a time):
   
   a. **Read and analyze scenario content** (format: see `Plan/CrossCuttingConcerns/scenarios/00-aspire-scaffolding.md`):
      
      **Header Section** (lines 1-20 typically):
      - Extract Target Layer: Look for `**Target Layer**: LayerName` (e.g., "CrossCuttingConcerns", "Foundation")
      - Extract feature title: First H1 heading (e.g., "# Aspire Orchestration & Cross-Cutting Scaffolding")
      - Extract Service Set: Look for `**Service Set**: Value` (e.g., "AppHost + All Foundation Microservices")
      - Extract Patterns: Look for `**Patterns**: List` (comma-separated pattern names)
      - Extract Architecture References: Look for `**Architecture References**:` followed by bullet list of markdown links
      - Extract Business Value: Look for `**Business Value**:` followed by descriptive paragraph
      
      **Scenarios Section** (main body):
      - Extract all scenario blocks with format:
        ```
        ## Scenario N: Descriptive Title
        **Given** context/preconditions
        **And** additional context (optional, may repeat)
        **When** action/trigger
        **Then** expected outcome
        **And** additional outcomes (optional, may repeat)
        ```
      - Preserve scenario numbering, titles, and full Given/When/Then structure
      - Note: Scenarios are separated by `---` or by next `## Scenario` heading
      
      **Technical Sections** (optional, after scenarios):
      - "Related Architecture & Standards" section: References to patterns/standards
      - "Technical Implementation Notes" section: Code snippets and implementation details
      - "Performance & Resiliency Targets" section: Specific metrics and SLOs
      - "Security & Compliance Hooks" section: Security requirements and audit trails
      - "Expansion Roadmap" section: Future enhancements
      - "Acceptance Summary" section: Checklist of acceptance criteria
      - "Next Actions" section: Immediate next steps
      
      **Validation**:
      - If Target Layer not found: Use layer from path (step 1), log warning
      - If no scenarios found: ERROR "No scenarios found in file [path]" → Skip scenario
      - If Business Value missing: Warn and use "See scenario file for details"
   
   b. **Generate comprehensive feature description for speckit.specify**:
      - Combine all extracted content into a rich, detailed feature description
      - Preserve the structure and formatting from the scenario file
      - Format as:
        ```
        # [Feature Title]
        
        **Target Layer**: [Target Layer from scenario file]
        **Service Set**: [Service Set from scenario file]
        
        ## Business Value
        [Business Value statement from scenario file]
        
        ## Architecture References
        [List of architecture pattern references with links]
        
        ## Scenarios
        
        [All scenario blocks with full Given/When/Then structure, preserving numbering and titles]
        
        ## Technical Context
        [Technical Implementation Notes section if present]
        
        ## Performance Targets
        [Performance & Resiliency Targets section if present]
        
        ## Security Requirements
        [Security & Compliance Hooks section if present]
        
        ## Acceptance Criteria
        [Acceptance Summary section if present]
        ```
      - **CRITICAL**: Include Target Layer in description so speckit.specify agent can auto-select it
   
   c. **Validate Target Layer before invocation**:
      - Extract Target Layer from scenario file content (look for `**Target Layer**: LayerName` pattern)
      - If no Target Layer found in scenario file, use layer from path (step 1)
      - Validate layer exists by checking `Plan/{LayerName}/` directory exists
      - If layer not found:
        - ERROR with message: "Layer '[LayerName]' not found in Plan/ or Src/. Valid layers: Foundation, DigitalInk, CrossCuttingConcerns. For new layers, run Architecture Review first (Constitution Principle 6)."
        - Skip this scenario and continue to next
      - Store validated layer for use in agent invocation
   
   d. **Invoke speckit.specify agent using runSubagent** (Constitution v2.1.0 workflow, optimized for GitHub Copilot):
      - Use `runSubagent` tool to call speckit.specify agent
      - Pass the comprehensive feature description from step 3b
      - Description: "Create specification for [feature-title] in [LayerName] layer"
      - Prompt format:
        ```
        Create a specification for the following feature. The Target Layer is [LayerName from step 3c].
        
        [Full feature description from step 3b]
        
        When prompted for layer selection, use: [LayerName from step 3c]
        ```
      - **Sequential execution**: Wait for speckit.specify agent to complete before proceeding
      - The specify agent will:
        - Generate a short name for the feature
        - Create a **specification branch** with pattern: `{LayerName}/[###]-feature-name-spec`
        - Create specification directory at: `Plan/{LayerName}/specs/[###-feature-name]/`
        - Generate the specification in `Plan/{LayerName}/specs/[###-feature-name]/spec.md`
        - Record layer metadata in `.layer` file
      - **Capture agent output**: Parse the runSubagent result to extract:
        - Specification branch name (pattern: `{LayerName}/[###]-feature-name-spec`)
        - Spec file path (pattern: `Plan/{LayerName}/specs/[###]-feature-name]/spec.md`)
        - Layer assignment confirmation
        - Feature number assigned
        - Short name generated
      - **Error handling**:
        - If agent reports failure: Log detailed error, record in results tracking, skip to next scenario
        - If agent succeeds: Store branch name and spec path for next step
   
   e. **Verify specification branch**:
      ```bash
      git branch --show-current
      ```
      - Confirm current branch matches pattern: `{LayerName}/[###]-feature-name-spec`
      - Verify spec.md file exists at `Plan/{LayerName}/specs/[###-feature-name]/spec.md`
      - If verification fails: ERROR with details, mark as failed, skip to step 3h
   
   f. **Invoke speckit.plan agent using runSubagent** (Constitution v2.1.0 workflow, optimized for GitHub Copilot):
      - **IMPORTANT**: Must be on specification branch for plan agent to work
      - Use `runSubagent` tool to call speckit.plan agent
      - Description: "Create implementation plan for [feature-title] in [LayerName] layer"
      - Prompt: Empty string (plan agent auto-detects current feature from branch)
      - **Sequential execution**: Wait for speckit.plan agent to complete before proceeding
      - The plan agent will:
        - Validate current branch is specification branch (`-spec` suffix)
        - Validate layer consistency with spec.md
        - Load the spec.md created in the previous step
        - Generate plan.md with technical implementation details and layer identification
        - Generate research.md with technology decisions
        - Generate data-model.md with entity definitions
        - Generate contracts/ directory with API specifications
        - Update agent context files
      - **Capture agent output**: Parse the runSubagent result to extract:
        - Plan file path (pattern: `Plan/{LayerName}/specs/[###]-feature-name]/plan.md`)
        - Research file path
        - Data model file path
        - Contracts directory path
        - Layer consistency validation results
        - Any warnings or clarifications needed
      - **Error handling**:
        - If agent reports success: Record all generated artifacts, continue to step 3g
        - If agent reports failure: Log detailed error, mark as partial success (spec exists but no plan), continue to step 3g
   
   g. **Publish specification branch** (Constitution v2.1.0):
   ```bash
   git push -u origin {LayerName}/[###]-feature-name-spec
   ```
   - Push specification branch to remote
   - Verify push was successful (check exit code)
   - Record remote branch URL format: `https://github.com/{owner}/{repo}/tree/{LayerName}/[###]-feature-name-spec`
   - **Error handling**: If push fails, log error, mark as warning (branches exist locally), continue to step 3h
   
   h. **Create and publish proposed branch** (for review):
   ```bash
   git checkout -b {LayerName}/[###]-feature-name-proposed
   git push -u origin {LayerName}/[###]-feature-name-proposed
   git checkout {LayerName}/[###]-feature-name-spec
   ```
   - Create proposed branch from specification branch
   - Proposed branch contains spec.md, plan.md, and all planning artifacts
   - Push proposed branch to remote for stakeholder review
   - Return to specification branch
   - Record remote branch URL format: `https://github.com/{owner}/{repo}/tree/{LayerName}/[###]-feature-name-proposed`
   - **Error handling**: If any git operation fails, log error, mark as warning, continue to step 3i
   - **Note**: Implementation branch (`{LayerName}/###-feature-name`) will be created later by `/speckit.implement`
   
   i. **Record results** (for summary report in step 4):
      - Scenario file path and name
      - Target layer assignment
      - Specification branch name (with `-spec` suffix)
      - Proposed branch name (with `-proposed` suffix)
      - Feature number assigned (e.g., 003)
      - Short name generated (e.g., "aspire-scaffolding")
      - All artifacts generated:
        - spec.md path and status (✓ created / ✗ failed)
        - plan.md path and status
        - research.md path and status
        - data-model.md path and status
        - contracts/ directory path and status
        - quickstart.md path and status
      - Remote branch URLs (both spec and proposed)
      - Processing status: Complete / Partial (spec only) / Failed
      - Any errors or warnings encountered
      - Timestamp of completion
   
   j. **Return to original branch before processing next scenario**:
      ```bash
      git checkout [original-branch-from-step-2]
      ```
      - **CRITICAL for sequential processing**: Must return to original branch before starting next scenario
      - Ensures clean state for next create-new-feature.sh execution
      - **Error handling**: If checkout fails, STOP entire process (cannot continue safely)

4. **Generate summary report** (after all scenarios processed):
   
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

## Error Handling (Sequential Processing - No Parallelization)

### Fatal Errors (STOP entire process immediately):
- **Missing scenario directory**: ERROR "Scenario directory not found at [path]. Please provide a valid path." → STOP
- **No scenario files found**: ERROR "No .md files found in [directory]. Directory must contain scenario files." → STOP
- **Git checkout to original branch fails** (in step 3j): ERROR "Cannot return to original branch. Manual intervention required." → STOP
- **Scripts not available**: ERROR ".specify/scripts/bash/ not found. Repository structure invalid." → STOP

### Non-Fatal Errors (Log error, record in results, continue to next scenario):
- **Layer not found** (in step 3c): ERROR "Layer '[LayerName]' not found in Plan/ or Src/. Valid layers: Foundation, DigitalInk, CrossCuttingConcerns. For new layers, run Architecture Review first (Constitution Principle 6)." → Skip scenario
- **Target Layer extraction fails**: Fallback to layer from path (step 1), log warning → Continue
- **Scenario file parsing fails**: ERROR "Cannot parse scenario file [path]. Format may be invalid." → Skip scenario
- **Branch already exists**: Check if spec.md already exists; if yes, skip with note "Already processed"; if no, ERROR "Orphaned branch exists" → Skip scenario

### Partial Success (Continue with warnings):
- **speckit.specify fails**: Log detailed error from runSubagent output, record as failed, skip to next scenario
- **speckit.specify succeeds but speckit.plan fails**: Mark as "Partial Success - Spec Only", log plan error, continue to publish branches (spec exists)
- **Git push fails** (step 3g or 3h): Log error "Branches exist locally but not published", mark with warning, continue to record results
- **Proposed branch creation fails**: Log warning, specification branch still valid, continue

### Recovery Actions:
- Before each scenario: Verify we're on original branch
- After each error: Log full context (scenario file, layer, branch names, error message, stack trace from runSubagent if available)
- After non-fatal error: Ensure git state is clean before next scenario (checkout original branch)
- On fatal error: Provide clear instructions for manual resolution

### Validation Checks (performed before agent invocations):
- Target Layer exists in Plan/ directory
- Scenario file is readable and non-empty
- Git repository is in clean state (no uncommitted changes)
- Required agent files exist (.github/agents/speckit.specify.agent.md, speckit.plan.agent.md)
- Original branch is recorded for return checkout

## Important Notes

### Constitution v2.1.0 Workflow
Each scenario creates **two published branches**:
1. **Specification branch** (`{LayerName}/[###]-feature-name-spec`): Working branch for planning artifacts
2. **Proposed branch** (`{LayerName}/[###]-feature-name-proposed`): Review branch for stakeholders
3. **Implementation branch** (`{LayerName}/[###]-feature-name`): Created later by `/speckit.implement`

### Layer Organization & Validation
- **Layer Identification**: Target Layer extracted from scenario file content (`**Target Layer**: LayerName` pattern)
- **Fallback**: If not found in content, use layer from path (`Plan/{LayerName}/scenarios`)
- **Validation**: Layer must exist in `Plan/{LayerName}/` directory before processing (Constitution Principle 6)
- **Specifications**: Placed in `Plan/{LayerName}/specs/[###-feature-name]/`
- **Explicit Declaration**: Each spec, plan, and task declares target layer

### Sequential Processing (GitHub Copilot Optimized)
- **One at a time**: Process scenarios sequentially, NEVER in parallel
- **Clean state**: Return to original branch after each scenario (critical for next create-new-feature.sh)
- **runSubagent**: Automated agent orchestration (no manual handoffs)
- **Error isolation**: Failures in one scenario don't stop processing of others
- **Rich logging**: Capture full runSubagent output for debugging

### Branch Management
- Original branch remains unchanged; all work happens in feature branches
- If scenario already processed (spec branch exists with spec.md), skip with note in report
- Use `--force` flag to reprocess existing scenarios (will create new branch numbers)
- Both specification and proposed branches published to remote for collaboration

### Pre-flight Requirements
- Validate agents exist: `.github/agents/speckit.specify.agent.md`, `.github/agents/speckit.plan.agent.md`
- Validate scripts exist: `.specify/scripts/bash/create-new-feature.sh`, `setup-plan.sh`
- Repository in clean state (no uncommitted changes on original branch)
- Target layer directories exist in `Plan/`

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
