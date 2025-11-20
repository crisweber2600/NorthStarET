# Custom Agents for GitHub Copilot

This directory contains custom agent definitions for GitHub Copilot Coding Agent. These agents automate various development workflows in the NorthStarET project.

## Available Agents

### Specification & Planning Agents

#### `speckit.specify`
Creates or updates feature specifications from natural language descriptions.

**Usage:**
```
/speckit.specify Add user authentication with OAuth2
```

**What it does:**
- Generates a short feature name
- Creates a new feature branch
- Generates `spec.md` with user stories, acceptance criteria, and success metrics

**Handoffs:**
- Can trigger `speckit.plan` to create implementation plan
- Can trigger `speckit.clarify` to resolve ambiguities

---

#### `speckit.plan`
Executes the implementation planning workflow to generate design artifacts.

**Usage:**
```
/speckit.plan
```

**What it does:**
- Loads the current feature specification
- Generates `plan.md` with technical approach
- Generates `research.md` with technology decisions
- Generates `data-model.md` with entity definitions
- Generates `contracts/` directory with API specifications
- Updates agent context files

**Handoffs:**
- Can trigger `speckit.tasks` to break down into tasks
- Can trigger `speckit.checklist` to create validation checklists

---

#### `speckit.scenario` (NEW)
Processes scenario files and automatically generates specifications and plans for each scenario.

**Usage:**
```bash
# Process all scenarios in default directory
/speckit.scenario

# Process specific directory
/speckit.scenario Plan/Foundation/Plans/scenarios

# Process single scenario file
/speckit.scenario Plan/Foundation/Plans/scenarios/10-intervention-management-service.md
```

**What it does:**
- Reads scenario files from the specified directory
- For each scenario:
  - Extracts feature description from Given/When/Then blocks
  - Invokes `speckit.specify` to create specification
  - Switches to the created feature branch
  - Invokes `speckit.plan` to create implementation plan
- Generates a comprehensive processing report
- Returns to original branch when complete

**Output:**
- One feature branch per scenario with complete spec and plan
- Processing report in `Plan/Foundation/Plans/scenarios/PROCESSING_REPORT_[timestamp].md`

**Example workflow:**
```bash
# 1. Process all scenarios
/speckit.scenario Plan/Foundation/Plans/scenarios

# 2. Review the processing report
cat Plan/Foundation/Plans/scenarios/PROCESSING_REPORT_*.md

# 3. Switch to a generated branch to work on that feature
git checkout 001-student-management-service

# 4. Generate tasks for implementation
/speckit.tasks

# 5. Begin implementation
/speckit.implement
```

---

### Task & Implementation Agents

#### `speckit.tasks`
Generates an actionable, dependency-ordered `tasks.md` for the feature.

**Usage:**
```
/speckit.tasks
```

**What it does:**
- Loads spec.md and plan.md
- Extracts user stories with priorities
- Generates phased task breakdown
- Creates dependency graph
- Outputs tasks.md with checklist format

**Handoffs:**
- Can trigger `speckit.analyze` for consistency check
- Can trigger `speckit.implement` to start implementation

---

#### `speckit.implement`
Executes the implementation plan by processing tasks defined in `tasks.md`.

**Usage:**
```
/speckit.implement
```

**What it does:**
- Validates all checklists are complete
- Loads tasks.md and plan.md
- Executes tasks phase by phase
- Commits progress after each task
- Runs tests to validate changes

**Requirements:**
- Must have `tasks.md` in the feature directory
- All checklists must be complete (or user approves proceeding)

---

### Quality & Validation Agents

#### `speckit.analyze`
Performs cross-artifact consistency and quality analysis.

**Usage:**
```
/speckit.analyze
```

**What it does:**
- Validates spec.md, plan.md, and tasks.md consistency
- Checks for missing dependencies
- Validates test coverage requirements
- Reports quality metrics

---

#### `speckit.clarify`
Identifies underspecified areas in the feature spec by asking targeted clarification questions.

**Usage:**
```
/speckit.clarify
```

**What it does:**
- Analyzes spec.md for ambiguities
- Generates up to 5 targeted clarification questions
- Encodes answers back into spec.md
- Updates affected sections

---

#### `speckit.checklist`
Generates custom validation checklists for the current feature.

**Usage:**
```
/speckit.checklist Create a security checklist
```

**What it does:**
- Analyzes feature requirements
- Generates domain-specific checklist items
- Saves to `checklists/` directory
- Used by `speckit.implement` for validation

---

### Project Management Agents

#### `speckit.constitution`
Creates or updates the project constitution from principle inputs.

**Usage:**
```
/speckit.constitution
```

**What it does:**
- Defines project principles and non-negotiables
- Ensures all templates stay in sync
- Updates .specify/memory/constitution.md

---

#### `speckit.taskstoissues`
Converts tasks from `tasks.md` into actionable GitHub issues.

**Usage:**
```
/speckit.taskstoissues
```

**What it does:**
- Reads tasks.md
- Creates GitHub issues with proper labels
- Maintains dependency links
- Assigns to milestones

---

#### `speckit.coach`
Provides guidance and best practices for feature development.

**Usage:**
```
/speckit.coach How should I structure this API?
```

**What it does:**
- Provides contextual guidance
- Suggests best practices
- Reviews architecture decisions
- Links to relevant documentation

---

## Agent Workflow

### Typical Development Flow

1. **Start with scenarios** (if you have scenario files):
   ```
   /speckit.scenario Plan/Foundation/Plans/scenarios
   ```

2. **OR start with a feature description** (if building from scratch):
   ```
   /speckit.specify Build a student management dashboard
   ```

3. **Create implementation plan**:
   ```
   /speckit.plan
   ```

4. **Generate tasks**:
   ```
   /speckit.tasks
   ```

5. **Create validation checklists**:
   ```
   /speckit.checklist
   ```

6. **Analyze for consistency** (optional):
   ```
   /speckit.analyze
   ```

7. **Begin implementation**:
   ```
   /speckit.implement
   ```

8. **Convert to issues** (optional, for team tracking):
   ```
   /speckit.taskstoissues
   ```

### Scenario-Based Batch Processing Flow

For migrating multiple services from scenarios:

1. **Batch process all scenarios**:
   ```
   /speckit.scenario Plan/Foundation/Plans/scenarios
   ```

2. **Review the processing report** to see all generated branches and artifacts

3. **For each feature branch**, continue with tasks and implementation:
   ```bash
   git checkout 001-student-management-service
   /speckit.tasks
   /speckit.implement
   ```

This flow is ideal for:
- Migrating from legacy systems with documented scenarios
- Batch processing multiple related features
- Setting up multiple parallel development streams

## Agent Configuration

Agents are defined using front matter and markdown structure:

```markdown
---
description: Agent purpose and functionality
handoffs: 
  - label: Next Action
    agent: speckit.next
    prompt: Default prompt for handoff
    send: true/false
---

## User Input
[Captures $ARGUMENTS from invocation]

## Outline
[Step-by-step workflow]
```

## Development Guidelines

When creating new agents:

1. **Follow the existing structure** with front matter and sections
2. **Use absolute paths** for all file operations
3. **Handle errors gracefully** with clear error messages
4. **Validate prerequisites** before starting work
5. **Generate actionable reports** with next steps
6. **Respect git workflows** (branch management, commits)
7. **Document handoffs** to other agents for workflow continuity

## Troubleshooting

### Agent not found
- Ensure the file is in `.github/agents/` directory
- File must have `.agent.md` extension
- Check that front matter is valid YAML

### Agent fails to execute
- Check git status (repository must be clean for some operations)
- Verify required scripts exist in `.specify/scripts/bash/`
- Check that dependencies (spec.md, plan.md) exist when required

### Handoffs not working
- Verify the target agent exists
- Check handoff configuration in front matter
- Ensure `send: true` if you want automatic handoff

## Contributing

To add a new agent:

1. Create a new file: `.github/agents/speckit.[name].agent.md`
2. Follow the structure of existing agents
3. Document the agent in this README
4. Test with a sample workflow
5. Update workflow diagrams if adding new connections

## References

- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [Speckit Framework](../.specify/README.md) (if exists)
- [Repository Custom Instructions](../copilot-instructions.md)
