# GitHub Actions Runner Controller (ARC) Setup

## Overview

This repository uses Actions Runner Controller (ARC) with self-hosted runners configured for:
- .NET 10 SDK
- Aspire CLI and workloads
- Docker-in-Docker support
- GitHub Copilot Coding Agent compatibility
- MCP (Model Context Protocol) servers

## Critical Configuration

### Environment Variables for Copilot Coding Agent

The following environment variables **must** be set in the runner pods to support GitHub Copilot Coding Agent:

```yaml
env:
  - name: GITHUB_HOST
    value: "github.com"  # Required for CodeQL bundle download
  - name: AGENT_TOOLSDIRECTORY
    value: "/opt/hostedtoolcache"  # Required for tools cache location
```

**Why these are required:**
- `GITHUB_HOST`: The Copilot Coding Agent's CodeQL helper needs to know which GitHub instance to query
- `AGENT_TOOLSDIRECTORY`: Specifies where pre-installed tools like CodeQL should be located

**Failure symptom if missing:**
```
Error ensuring CodeQL: Error: GitHub host is required
  at demandGitHubHost (...src/settings/demand.ts:45:15)
```

Reference: [GitHub Community Discussion #177903](https://github.com/orgs/community/discussions/177903)

## Current Setup

### Namespaces
- **arc-systems**: ARC controller and listener pods
- **arc-runners**: Runner pods (ephemeral, created on-demand)

### Scale Set Configuration

```bash
INSTALLATION_NAME="arc-runner-set"
NAMESPACE="arc-runners"
GITHUB_CONFIG_URL="https://github.com/crisweber2600/NorthStarET.Lms"
```

### Helm Values

See `.github/arc-runner-values.yaml` for the complete configuration.

## Installation Commands

### 1. Install ARC Controller

```powershell
$NAMESPACE = "arc-systems"
helm install arc `
    --namespace $NAMESPACE `
    --create-namespace `
    oci://ghcr.io/actions/actions-runner-controller-charts/gha-runner-scale-set-controller
```

### 2. Install Runner Scale Set

```powershell
$INSTALLATION_NAME = "arc-runner-set"
$NAMESPACE = "arc-runners"
$GITHUB_CONFIG_URL = "https://github.com/crisweber2600/NorthStarET.Lms"
$GITHUB_PAT = "<YOUR_PAT>"  # Replace with actual PAT

helm install $INSTALLATION_NAME `
    --namespace $NAMESPACE `
    --create-namespace `
    --set githubConfigUrl=$GITHUB_CONFIG_URL `
    --set githubConfigSecret.github_token=$GITHUB_PAT `
    --values .github/arc-runner-values.yaml `
    oci://ghcr.io/actions/actions-runner-controller-charts/gha-runner-scale-set
```

### 3. Update Existing Installation

```powershell
helm upgrade arc-runner-set `
    --namespace arc-runners `
    --set githubConfigUrl="https://github.com/crisweber2600/NorthStarET.Lms" `
    --set githubConfigSecret.github_token="<YOUR_PAT>" `
    --values .github/arc-runner-values.yaml `
    oci://ghcr.io/actions/actions-runner-controller-charts/gha-runner-scale-set
```

## Runner Image

### Custom Image: `ghcr.io/crisweber2600/arc-runner-aspire:latest`

Based on `mcr.microsoft.com/devcontainers/dotnet:10.0` with:
- .NET 10 SDK
- Aspire CLI (`dotnet tool install -g aspire`)
- Docker client
- CodeQL tools pre-installed in `/opt/hostedtoolcache/CodeQL/`
- GitHub Actions runner binaries

### Dockerfile Location
`.github/Dockerfile.arc-runner`

### Building and Pushing

```powershell
# Build
docker build -f .github/Dockerfile.arc-runner -t ghcr.io/crisweber2600/arc-runner-aspire:latest .

# Push
docker push ghcr.io/crisweber2600/arc-runner-aspire:latest
```

## Troubleshooting

### Check Pod Status

```powershell
# Controller
kubectl get pods -n arc-systems

# Runners (during job execution)
kubectl get pods -n arc-runners

# Listener
kubectl get pods -n arc-systems | Select-String listener
```

### View Logs

```powershell
# Controller logs
kubectl logs -n arc-systems deployment/arc-gha-rs-controller

# Listener logs
kubectl logs -n arc-systems -l actions.github.com/scale-set-name=arc-runner-set

# Runner logs (during job)
kubectl logs -n arc-runners <pod-name> -c runner
```

### Common Issues

#### 1. "GitHub host is required" Error
**Solution:** Ensure `GITHUB_HOST` is set in runner template (see arc-runner-values.yaml)

#### 2. CodeQL Download Failures
**Solution:** Ensure `AGENT_TOOLSDIRECTORY=/opt/hostedtoolcache` is set and CodeQL is pre-installed in the image

#### 3. Docker-in-Docker Issues
**Solution:** Verify dind container has `privileged: true` and volume mounts are correct

#### 4. Jobs Not Picking Up
**Solution:** Check listener pod logs and verify PAT has correct scopes:
- `repo` (for repository access)
- `admin:org` or `manage_runners:org` (for runner registration)

### Verify Configuration

```powershell
# Check AutoScalingRunnerSet configuration
kubectl get autoscalingrunnersets.actions.github.com -n arc-runners arc-runner-set -o yaml

# Check environment variables in template
kubectl get autoscalingrunnersets.actions.github.com -n arc-runners arc-runner-set -o yaml | Select-String -Pattern "GITHUB_HOST|AGENT_TOOLS" -Context 2,2
```

## MCP Server Configuration

The runner includes MCP servers for enhanced Copilot functionality:
- chrome-devtools
- figma
- microsoft-docs
- playwright
- sequential-thinking
- github-mcp-server

These are automatically started when the Copilot Coding Agent workflow runs.

## Workflow Usage

In your workflows, use:

```yaml
runs-on: arc-runner-set
```

This matches the `INSTALLATION_NAME` from the Helm installation.

## Resources

- [ARC Documentation](https://github.com/actions/actions-runner-controller)
- [Helm Chart Values](https://github.com/actions/actions-runner-controller/blob/master/charts/gha-runner-scale-set/values.yaml)
- [Copilot Coding Agent + Self-Hosted Runners](https://docs.github.com/en/enterprise-cloud@latest/copilot/using-github-copilot/using-github-copilot-code-review/customizing-copilot-code-review#using-self-hosted-runners)

## Maintenance

### Updating the Controller

```powershell
helm upgrade arc `
    --namespace arc-systems `
    oci://ghcr.io/actions/actions-runner-controller-charts/gha-runner-scale-set-controller
```

### Updating Runner Image

1. Build and push new image
2. Update image tag in `arc-runner-values.yaml`
3. Run helm upgrade command (see Installation Commands section)

### Scaling

Adjust in `arc-runner-values.yaml`:
```yaml
minRunners: 0  # Minimum idle runners
maxRunners: 10 # Maximum concurrent runners
```

Then run `helm upgrade` to apply changes.
