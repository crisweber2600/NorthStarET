---
description: Execute the implementation planning workflow using the plan template to generate design artifacts.
handoffs: 
  - label: Create Tasks
    agent: speckit.tasks
    prompt: Break the plan into tasks
    send: true
  - label: Create Checklist
    agent: speckit.checklist
    prompt: Create a checklist for the following domain...
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

1. **Verify Specification Branch**: Ensure current branch is `proposed-specs`. If not, ERROR with message: "Must be on `proposed-specs` branch to create plan. Use `/speckit.specify` first."

2. **Setup**: Run `.specify/scripts/bash/setup-plan.sh --json` from repo root and parse JSON for FEATURE_SPEC, IMPL_PLAN, SPECS_DIR, BRANCH, LAYER. For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot").

3. **Load context**: Read FEATURE_SPEC and `.specify/memory/constitution.md`. Load IMPL_PLAN template (already copied).

4. **Layer Consistency Validation**: Extract Target Layer from FEATURE_SPEC (spec.md) and validate:
   - Layer is explicitly identified (not "Other" or "TBD")
   - Layer exists in mono-repo structure (`Plan/{LayerName}/` or `Src/{LayerName}/`)
   - Cross-layer dependencies match approved shared infrastructure only
   - If validation fails, ERROR with specific issue and reference to Constitution Principle 6

5. **Execute plan workflow**: Follow the structure in IMPL_PLAN template to:
   - Fill Layer Identification section matching layer from spec.md (MANDATORY)
   - Fill Technical Context (mark unknowns as "NEEDS CLARIFICATION")
   - Fill Constitution Check section from constitution
   - Evaluate gates (ERROR if violations unjustified)
   - Phase 0: Generate research.md (resolve all NEEDS CLARIFICATION)
   - Phase 1: Generate data-model.md, contracts/, quickstart.md
   - Phase 1: Update agent context by running the agent script
   - Re-evaluate Constitution Check post-design
   - Verify layer consistency throughout (Target Layer in plan.md MUST match spec.md)

6. **Commit Planning Artifacts**:
   - Stage and commit all planning files:
     ```bash
     git add .
     git commit -m "Complete planning for [###]-<short-name>"
     ```
   - Push to remote:
     ```bash
     git push origin proposed-specs
     ```

7. **Stop and report**: Command ends after Phase 2 planning. Report:
   - Current branch: `proposed-specs`
   - Target layer (verified against spec.md)
   - IMPL_PLAN path within layer structure
   - Generated artifacts
   - Note: Pull request to merge `proposed-specs` into `Specs` was created by `/speckit.specify`

## Phases

### Phase 0: Outline & Research

1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:

   ```text
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

### Phase 1: Design & Contracts

**Prerequisites:** `research.md` complete

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Agent context update**:
   - Run `.specify/scripts/bash/update-agent-context.sh copilot`
   - These scripts detect which AI agent is in use
   - Update the appropriate agent-specific context file
   - Add only new technology from current plan
   - Preserve manual additions between markers

**Output**: data-model.md, /contracts/*, quickstart.md, agent-specific file

## Key rules

- Use absolute paths
- ERROR on gate failures or unresolved clarifications
