# GitHub Copilot Coding Agent Setup - Complete

This document describes the complete setup for GitHub Copilot Coding Agent to execute code on self-hosted Actions Runner Controller (ARC) runners with .NET 10, Aspire, and MCP servers.

## Overview

GitHub Copilot Coding Agent can now execute code in this repository using custom self-hosted runners that match the devcontainer environment.

### Architecture

```
┌─────────────────────────────────────────────────────────┐
│ GitHub Copilot Coding Agent                            │
│ (Executes code via @copilot chat commands)            │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ↓ Triggers GitHub Actions Workflow
┌─────────────────────────────────────────────────────────┐
│ GitHub Actions                                          │
│ runs-on: arc-runner-set                                │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ↓ Scales runner pod
┌─────────────────────────────────────────────────────────┐
│ ARC Runner Pod (Kubernetes)                            │
│                                                         │
│ ┌─────────────────────┐  ┌──────────────────────────┐ │
│ │ Runner Container    │  │ Docker-in-Docker (DinD)  │ │
│ │                     │  │                          │ │
│ │ • .NET 10 SDK       │  │ • Docker daemon          │ │
│ │ • Aspire workload   │  │ • Docker Compose         │ │
│ │ • Node.js + MCP     │  │ • Container isolation    │ │
│ │ • Git, gh, tools    │  │                          │ │
│ └─────────────────────┘  └──────────────────────────┘ │
│           │                         │                   │
│           └─────────┬───────────────┘                   │
│                     │ Shared /home/runner/_work         │
└─────────────────────┴───────────────────────────────────┘
                      │
                      ↓ Clones repository
┌─────────────────────────────────────────────────────────┐
│ Repository: NorthStarET.Lms                            │
│ • Full filesystem access via MCP                       │
│ • Can execute: dotnet, docker, npm, git, gh           │
│ • Access to MCP servers for enhanced capabilities      │
└─────────────────────────────────────────────────────────┘
```

## Components

### 1. ARC Custom Runner Image

**Image**: `ghcr.io/crisweber2600/arc-runner-aspire:latest`  
**Size**: 4.25GB  
**Base**: `ghcr.io/actions/actions-runner:latest`

**Includes**:
- .NET 10.0 SDK (10.0.100)
- .NET Aspire workload
- Docker CLI & Docker Compose
- Node.js LTS (v22.x)
- GitHub CLI (gh)
- Git & Git LFS
- Python uv package manager
- .NET global tools: dotnet-ef, dotnet-serve

**Dockerfile**: `D:\arc-setup\custom-runner-image\Dockerfile`

### 2. ARC Runner Scale Set

**Name**: `arc-runner-set`  
**Namespace**: `arc-runners`  
**Controller Namespace**: `arc-systems`  
**Scaling**: 0-10 replicas (auto-scale on demand)

**Features**:
- Docker-in-Docker sidecar for container workloads
- Shared workspace volume
- Privileged mode for container execution
- TLS-secured Docker socket

**Configuration**: `D:\arc-setup\custom-runner-image\runner-values.yaml`

### 3. MCP Server Configuration

Located in `.github/copilot/mcp-servers.json`:

| Server | Package | Purpose |
|--------|---------|---------|
| sequential-thinking | `@modelcontextprotocol/server-sequential-thinking` | Planning and reasoning |
| filesystem | `@modelcontextprotocol/server-filesystem` | Code navigation and file operations |
| microsoft-docs | `@microsoft/mcp-server-docs` | .NET and Azure documentation |
| github | `@modelcontextprotocol/server-github` | GitHub API operations |

### 4. Repository Configuration Files

```
.github/
├── copilot/
│   ├── README.md              # This documentation
│   ├── environment.yml        # Runner environment spec
│   └── mcp-servers.json       # MCP server configurations
├── workflows/
│   └── copilot-environment.yml  # Verification workflow
└── copilot-instructions.md    # Enhanced with Coding Agent section
```

## How Copilot Coding Agent Works

### Execution Flow

1. **User Request**: User asks Copilot to execute code in chat
   ```
   @copilot Run dotnet build and show me warnings
   ```

2. **Workflow Generation**: Copilot generates a GitHub Actions workflow
   ```yaml
   jobs:
     execute:
       runs-on: arc-runner-set  # Uses custom runner
       steps:
         - uses: actions/checkout@v4
         - run: dotnet build --no-restore
   ```

3. **Runner Scaling**: ARC controller provisions runner pod
   - Runner container starts (with .NET 10, Aspire)
   - DinD sidecar starts (Docker daemon)
   - MCP servers available via npx

4. **Code Execution**: Workflow runs on runner
   - Clones repository
   - Executes commands
   - Returns output to Copilot

5. **Results**: Copilot shows results in chat
   - Build output
   - Test results
   - Error messages
   - Suggestions for fixes

### MCP Server Integration

MCP servers enhance Copilot's capabilities:

**Sequential Thinking** (`#think`):
```
@copilot #think "How should I implement tenant isolation in the repository layer?"
```
- Plans multi-step solutions
- Breaks down complex problems
- Provides reasoning chains

**Microsoft Docs** (`#microsoft.docs.mcp`):
```
@copilot #microsoft.docs.mcp
Search for Entity Framework Core 9 with PostgreSQL examples
```
- Searches official Microsoft documentation
- Finds code samples filtered by language
- Retrieves complete documentation pages

**Filesystem** (automatic):
- Navigates repository structure
- Reads and writes files
- Understands project relationships

**GitHub** (automatic):
- Creates issues and PRs
- Manages labels and milestones
- Searches code and commits

## Usage Examples

### Building and Testing

**Basic Build**:
```
@copilot Build the solution and show me any errors
```

**With Tests**:
```
@copilot Can you:
1. Restore dependencies
2. Build the solution  
3. Run all unit tests
4. Show me the coverage summary
```

**Specific Project**:
```
@copilot Build only the API project and check for warnings
```

### Aspire Development

**Start Aspire AppHost**:
```
@copilot Start the Aspire AppHost and verify all services are running
```

**Check Services**:
```
@copilot Can you:
1. Build the AppHost project
2. List all Aspire resources
3. Check PostgreSQL and Redis health
4. Show me the dashboard URL
```

**Test Service Integration**:
```
@copilot Run the Aspire integration tests and show me any failures
```

### Docker Operations

**Build Image**:
```
@copilot Build the Docker image for the API project
```

**Docker Compose**:
```
@copilot Run docker compose up and show me the container logs
```

**Multi-stage Build**:
```
@copilot Build the production Docker image with multi-stage build
```

### Database Migrations

**Add Migration**:
```
@copilot Add an Entity Framework migration named "AddSoftDelete"
```

**Apply Migrations**:
```
@copilot Apply all pending migrations to the database
```

**Check Status**:
```
@copilot Show me the migration history
```

### Research and Planning

**Plan Implementation**:
```
@copilot #think "I need to add soft delete to the District entity.
What changes are needed across the layers following Clean Architecture?"
```

**Find Documentation**:
```
@copilot #microsoft.docs.mcp
How do I configure Redis caching in a .NET Aspire application?
```

**Search Repository**:
```
@copilot #github
Find all places where we query Districts and need to add soft delete filtering
```

## Configuration Details

### Environment Variables

Available in workflows and MCP servers:

| Variable | Value | Purpose |
|----------|-------|---------|
| `DOTNET_ROOT` | `/usr/share/dotnet` | .NET SDK location |
| `DOCKER_HOST` | `tcp://localhost:2376` | Docker daemon endpoint |
| `DOCKER_TLS_VERIFY` | `1` | Use TLS for Docker |
| `DOCKER_CERT_PATH` | `/certs/client` | Docker TLS certificates |
| `GITHUB_TOKEN` | (auto-provided) | GitHub API authentication |

### Resource Limits

**Runner Container**:
- CPU: 2-4 cores
- Memory: 4-8 GB
- Storage: Ephemeral (destroyed after job)

**DinD Container**:
- CPU: 1-2 cores
- Memory: 2-4 GB
- Storage: Ephemeral Docker storage

**Total Pod**:
- Max CPU: 6 cores
- Max Memory: 12 GB
- Startup time: ~30 seconds (includes DinD initialization)

### Network Configuration

- **Cluster Network**: Internal Kubernetes networking
- **Docker Network**: Bridge network in DinD container
- **Aspire Services**: Accessible via localhost in runner
- **External Access**: GitHub.com API, npm registry, NuGet.org

## Verification

### Test the Setup

1. **Trigger Verification Workflow**:
   ```bash
   gh workflow run copilot-environment.yml --ref main
   ```

2. **Watch Execution**:
   ```bash
   gh run watch
   ```

3. **Check Runner Status**:
   ```bash
   kubectl get pods -n arc-runners -w
   ```

### Verify Components

**In Workflow**:
```yaml
- name: Verify Environment
  run: |
    dotnet --version          # Should show 10.0.100
    dotnet workload list      # Should include aspire
    docker --version          # Should show Docker 26.x
    node --version            # Should show v22.x
    gh --version              # Should show gh 2.x
```

**In Copilot Chat**:
```
@copilot Show me the installed .NET workloads and SDK version
```

## Troubleshooting

### Issue: Copilot Can't Execute Code

**Symptom**: "No runners available" or execution timeout

**Check**:
```bash
# Verify runners are available
kubectl get autoscalingrunnersets -n arc-runners

# Check listener status  
kubectl get pods -n arc-systems -l app.kubernetes.io/component=runner-scale-set-listener

# View controller logs
kubectl logs -n arc-systems deployment/arc-gha-rs-controller
```

**Fix**: Ensure ARC controller and listener are running

### Issue: MCP Server Not Found

**Symptom**: "Cannot find module '@modelcontextprotocol/...'"

**Check**: Node.js and npm are available in runner
```yaml
- run: node --version && npm --version
```

**Fix**: MCP servers install on-demand via `npx -y`. First run may be slower.

### Issue: Docker Not Available

**Symptom**: "Cannot connect to Docker daemon"

**Check**: DinD container status
```bash
kubectl logs -n arc-runners <pod-name> -c dind
```

**Fix**: Add wait for Docker in workflow:
```yaml
- run: timeout 30 sh -c 'until docker info > /dev/null 2>&1; do sleep 1; done'
```

### Issue: Aspire AppHost Won't Start

**Symptom**: "Could not find a part of the path"

**Check**: Workload is installed
```yaml
- run: dotnet workload list | grep aspire
```

**Fix**: Restore workloads first:
```yaml
- run: dotnet workload restore
```

### Issue: Low Code Coverage

**Symptom**: Coverage below 80% threshold

**Fix**: Generate detailed coverage report:
```yaml
- run: |
    dotnet test --collect:"XPlat Code Coverage"
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:Html
```

## Security

### Runner Isolation

- Each job runs in fresh, ephemeral pod
- Pods destroyed immediately after job completion
- No data persists between jobs
- Network isolation via Kubernetes namespaces

### Docker Security

- DinD runs in privileged sidecar (isolated from runner)
- TLS-secured Docker socket
- No direct access to host Docker daemon
- Container images scanned before use

### Secrets Management

- Never commit secrets to `mcp-servers.json`
- Use GitHub Secrets for sensitive data
- MCP servers use `${GITHUB_TOKEN}` environment variable
- Secrets not logged or persisted

### Access Control

- Runner uses `arc-gha-rs-no-permission` service account
- Minimal Kubernetes RBAC permissions
- No cluster-admin access
- GitHub PAT scoped to repository

## Maintenance

### Updating Runner Image

When you need to update the image:

```powershell
cd D:\arc-setup\custom-runner-image

# Edit Dockerfile with changes

# Rebuild
.\build-runner-image.ps1 -ImageName "crisweber2600/arc-runner-aspire" -Tag "v2.0"

# Update runners
.\update-runner-image.ps1 -ImageName "ghcr.io/crisweber2600/arc-runner-aspire:v2.0" -UseValuesFile
```

### Adding MCP Servers

1. Edit `.github/copilot/mcp-servers.json`
2. Add new server configuration
3. Test with Copilot chat
4. Document in README

### Monitoring

**Runner Scaling**:
```bash
watch kubectl get autoscalingrunnersets -n arc-runners
```

**Active Jobs**:
```bash
kubectl get pods -n arc-runners
```

**Resource Usage**:
```bash
kubectl top pods -n arc-runners
```

## Resources

- [GitHub Copilot Coding Agent Docs](https://docs.github.com/en/enterprise-cloud@latest/copilot/how-tos/use-copilot-agents/coding-agent)
- [ARC Documentation](https://github.com/actions/actions-runner-controller)
- [MCP Specification](https://modelcontextprotocol.io/)
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Docker-in-Docker](https://github.com/docker-library/docker/blob/master/dind.md)

## Support

**ARC Setup**: See `D:\arc-setup\README.md` and `D:\arc-setup\SETUP-COMPLETE.md`  
**Runner Image**: See `D:\arc-setup\custom-runner-image\README.md`  
**Copilot Config**: See `.github/copilot/README.md`  
**Troubleshooting**: Run `D:\arc-setup\troubleshooting.ps1`

---

**Last Updated**: November 12, 2025  
**ARC Version**: 0.13.0  
**Runner Image**: ghcr.io/crisweber2600/arc-runner-aspire:latest  
**Repository**: https://github.com/crisweber2600/NorthStarET.Lms