# GitHub Copilot Coding Agent Setup for ARC Runners

## Overview
This repository is configured to use GitHub Copilot Coding Agent with self-hosted Actions Runner Controller (ARC) on a Kubernetes cluster with .NET 10, Aspire, and Docker-in-Docker support.

## Prerequisites

### 1. ARC Runner Configuration
- **Runner Scale Set**: `arc-runner-set`
- **Base Image**: `mcr.microsoft.com/devcontainers/dotnet:10.0`
- **Docker-in-Docker**: Enabled via sidecar or privileged mode
- **Node.js**: v22.x for MCP servers
- **.NET SDK**: 10.0.x
- **Aspire**: Installed via `dotnet tool install -g aspire`

### 2. Repository Settings

#### Disable Copilot Agent Firewall
⚠️ **CRITICAL**: You must disable the Copilot Agent Firewall to use self-hosted runners.

**Steps**:
1. Go to repository Settings
2. Navigate to: **Code security and analysis** → **Copilot**
3. Find **"Copilot coding agent firewall"**
4. Click **Disable**

Without this, you'll see the error:
```
You must disable the agent firewall in the repository's settings to use self-hosted runners.
```

#### Required Secrets
Add these to Repository Secrets (Settings → Secrets and variables → Actions):

- `GITHUB_TOKEN` - Automatically provided by Actions
- `GITHUB_PAT` - Personal Access Token with:
  - `repo` scope
  - `workflow` scope
  - `read:org` scope (if using org-level runners)

### 3. Fix CodeQL "GitHub host is required" Error

The error occurs because the CodeQL action doesn't have the GitHub host configured.

**Solution**: Add environment variable to workflow:

```yaml
env:
  GITHUB_HOST: github.com
```

This is already configured in `.github/workflows/copilot-setup-steps.yml`.

## Workflow Configuration

### copilot-setup-steps.yml
This workflow:
- Runs on `arc-runner-set` runners
- Sets up .NET 10 environment
- Installs Aspire CLI
- Configures MCP servers for Copilot
- Verifies Docker and Aspire setup

### copilot-environment.yml
This workflow:
- Tests the complete environment
- Validates .NET, Docker, Node.js, and Aspire
- Builds the solution
- Displays environment summary

## MCP Server Configuration

MCP (Model Context Protocol) servers are configured at `~/.config/mcp/mcp.json`:

```json
{
  "mcpServers": {
    "sequential-thinking": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"]
    },
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "${workspaceFolder}"]
    },
    "microsoft-docs": {
      "type": "http",
      "url": "https://learn.microsoft.com/api/mcp"
    },
    "github": {
      "type": "http",
      "url": "https://api.githubcopilot.com/mcp/readonly"
    }
  }
}
```

### Available MCP Servers:
- **sequential-thinking**: Planning and reasoning
- **filesystem**: Code navigation and file operations
- **microsoft-docs**: .NET documentation access
- **github**: Repository operations

## Aspire Configuration

### AppHost Project
Location: `src/NorthStarET.NextGen.Lms.AppHost`

### Dashboard
- Default URL: http://localhost:15000
- Port configured via: `ASPIRE_DASHBOARD_PORT=15000`

### Docker-in-Docker Support
Aspire requires Docker to run containers. The ARC runners are configured with:
- Docker daemon accessible
- Docker Compose v2
- Network access to container registry

## Troubleshooting

### Error: "GitHub host is required"
**Cause**: Missing `GITHUB_HOST` environment variable.

**Fix**: Already added to workflow. If you see this:
1. Check that `.github/workflows/copilot-setup-steps.yml` has:
   ```yaml
   env:
     GITHUB_HOST: github.com
   ```
2. Re-run the workflow

### Error: "Must disable agent firewall"
**Cause**: Copilot Agent Firewall is enabled.

**Fix**: Follow steps in "Disable Copilot Agent Firewall" above.

### Error: ".NET SDK 10.0.0 not found"
**Cause**: Runner doesn't have .NET 10 installed.

**Fix**: Update your ARC runner image to include .NET 10:
```dockerfile
FROM mcr.microsoft.com/devcontainers/dotnet:10.0
```

### Error: "Aspire workload not found"
**Cause**: .NET 10 doesn't use workloads for Aspire; use CLI instead.

**Fix**: Install Aspire CLI:
```bash
dotnet tool install -g aspire
```

### Job Doesn't Start
**Symptoms**: 
```
Waiting for a runner to pick up this job...
Job is about to start running on the runner: arc-runner-set
```
Then nothing happens or fails quickly.

**Causes**:
1. ARC runner not healthy
2. Missing environment variables
3. Firewall blocking
4. CodeQL download failing

**Fixes**:
```bash
# Check ARC runner status
kubectl get pods -n arc-runners

# Check runner logs
kubectl logs -n arc-runners <pod-name>

# Verify GitHub connection
curl -H "Authorization: token $GITHUB_TOKEN" https://api.github.com/user
```

## Testing the Setup

### 1. Run Setup Workflow
```bash
# Trigger via GitHub UI
Actions → Copilot Setup Steps → Run workflow

# Or via gh CLI
gh workflow run copilot-setup-steps.yml
```

### 2. Verify Environment
```bash
# Check .NET
dotnet --version  # Should show 10.0.x

# Check Aspire
aspire --version

# Check Docker
docker info

# Check Node.js for MCP
node --version  # Should show v22.x
```

### 3. Test Copilot Coding Agent
1. Create a PR or issue
2. Comment: `@copilot /plan implement feature X`
3. Check that the job starts and runs
4. Verify in Actions tab that it runs on `arc-runner-set`

## Environment Variables Reference

| Variable | Value | Purpose |
|----------|-------|---------|
| `GITHUB_HOST` | `github.com` | Fix CodeQL error |
| `DOTNET_VERSION` | `10.0.x` | .NET SDK version |
| `ASPIRE_DASHBOARD_PORT` | `15000` | Aspire dashboard port |
| `NODE_VERSION` | `22.x` | Node.js for MCP servers |

## Architecture

```
┌─────────────────────────────────────────┐
│  GitHub Copilot Coding Agent            │
│  (Cloud-based AI)                       │
└────────────┬────────────────────────────┘
             │
             │ via GitHub Actions API
             │
┌────────────▼────────────────────────────┐
│  ARC Runner (Kubernetes Pod)            │
│  ┌────────────────────────────────┐     │
│  │  Main Container                │     │
│  │  - .NET 10 SDK                 │     │
│  │  - Aspire CLI                  │     │
│  │  - Node.js 22                  │     │
│  │  - MCP Servers                 │     │
│  └────────────────────────────────┘     │
│  ┌────────────────────────────────┐     │
│  │  Docker-in-Docker Sidecar      │     │
│  │  - Docker daemon               │     │
│  │  - Container runtime           │     │
│  └────────────────────────────────┘     │
└─────────────────────────────────────────┘
             │
             │ runs Aspire containers
             │
┌────────────▼────────────────────────────┐
│  Aspire Application                     │
│  - AppHost (orchestration)              │
│  - Dashboard (http://localhost:15000)   │
│  - Service containers                   │
└─────────────────────────────────────────┘
```

## References

- [GitHub Copilot Coding Agent Docs](https://docs.github.com/en/copilot/using-github-copilot/copilot-coding-agent)
- [Actions Runner Controller](https://github.com/actions/actions-runner-controller)
- [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Model Context Protocol (MCP)](https://modelcontextprotocol.io/)
- [Disable Copilot Firewall for Self-Hosted Runners](https://gh.io/cca-self-hosted-disable-firewall)

## Support

If issues persist:
1. Check runner logs: `kubectl logs -n arc-runners <pod-name>`
2. Verify workflow runs in Actions tab
3. Review environment variables in workflow
4. Ensure firewall is disabled in repository settings
5. Confirm .NET 10 and Aspire are properly installed in runner image
