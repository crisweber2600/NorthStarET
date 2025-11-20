# GitHub Copilot Coding Agent Configuration

This directory contains configuration for [GitHub Copilot Coding Agent](https://docs.github.com/en/enterprise-cloud@latest/copilot/how-tos/use-copilot-agents/coding-agent/customize-the-agent-environment) to execute code on self-hosted Actions Runner Controller (ARC) runners.

## Overview

The Coding Agent runs on custom ARC runners with:

- **Runs-on Label**: `arc-runner-set`
- **.NET 10 SDK** with Aspire workload
- **Docker-in-Docker** for Aspire container orchestration
- **MCP Servers** for enhanced capabilities
- **Full repository access** via filesystem MCP

## Configuration Files

### `environment.yml`
Defines the runner environment, including:
- Runner labels (`arc-runner-set`)
- Software versions (.NET 10, Node.js LTS, Docker)
- MCP server configurations
- Aspire AppHost settings
- Resource limits

### `mcp-servers.json`
MCP (Model Context Protocol) server configurations:

| Server | Purpose | Command |
|--------|---------|---------|
| `sequential-thinking` | Planning and reasoning | `@modelcontextprotocol/server-sequential-thinking` |
| `filesystem` | Code navigation and file access | `@modelcontextprotocol/server-filesystem` |
| `microsoft-docs` | .NET and Azure documentation | `@microsoft/mcp-server-docs` |
| `github` | Repository operations | `@modelcontextprotocol/server-github` |

## How It Works

When GitHub Copilot Coding Agent executes code:

1. **Runner Selection**: Jobs run on `arc-runner-set` runners
2. **Environment Setup**: Runner provides .NET 10, Aspire, Docker-in-Docker
3. **MCP Servers**: Agent can invoke MCP tools for:
   - Sequential reasoning (`#think`)
   - File system operations
   - .NET documentation lookup
   - GitHub API interactions
4. **Code Execution**: Full access to `dotnet`, `docker`, `npm`, `gh` commands
5. **Aspire Support**: Can run and test Aspire applications

## Usage in Copilot Chat

### Available Commands

```plaintext
# Use sequential thinking for complex planning
@copilot #think "How should I implement the district CRUD feature?"

# Execute .NET code
@copilot Run dotnet build and show me the output

# Test Aspire application
@copilot Start the Aspire AppHost and verify all services are running

# Run tests with coverage
@copilot Run all tests and generate coverage report

# Work with Docker
@copilot Build the Docker image and run docker compose up
```

### Example Workflows

#### Build and Test
```plaintext
@copilot Can you:
1. Restore the solution
2. Build with no warnings
3. Run all unit tests
4. Show me the coverage report
```

#### Aspire Development
```plaintext
@copilot I need to:
1. Start the Aspire AppHost
2. Verify PostgreSQL and Redis are running
3. Check the API health endpoint
4. Show me the Aspire dashboard URL
```

#### Research .NET APIs
```plaintext
@copilot #microsoft.docs.mcp
Search for examples of using Entity Framework Core 9 with PostgreSQL
and Aspire integration
```

## Runner Image

The custom runner image (`ghcr.io/crisweber2600/arc-runner-aspire:latest`) includes:

✅ .NET 10.0 SDK  
✅ .NET Aspire workload  
✅ Docker-in-Docker (DinD)  
✅ Docker Compose  
✅ Node.js LTS (for MCP servers)  
✅ GitHub CLI  
✅ Git & Git LFS  
✅ Python uv  
✅ .NET global tools (dotnet-ef, dotnet-serve)  

**Image Size**: 4.25GB  
**Build Script**: `D:\arc-setup\custom-runner-image\build-runner-image.ps1`

## Testing the Configuration

Run the test workflow to verify everything works:

```bash
# Trigger the verification workflow
gh workflow run copilot-environment.yml

# Watch the run
gh run watch
```

Or use the GitHub UI:
1. Go to **Actions** tab
2. Select **Enable Copilot Coding Agent** workflow
3. Click **Run workflow**
4. View logs to confirm all tools are available

## Updating the Configuration

### Adding New MCP Servers

1. Edit `mcp-servers.json`
2. Add new server configuration:
   ```json
   "my-server": {
     "command": "npx",
     "args": ["-y", "@org/my-mcp-server"]
   }
   ```
3. Update `environment.yml` to document it
4. Commit and push changes

### Modifying Environment

1. Edit `environment.yml`
2. Update runner labels, tools, or resource limits
3. If changing runner image, update in ARC:
   ```powershell
   cd D:\arc-setup\custom-runner-image
   .\update-runner-image.ps1 -ImageName "ghcr.io/crisweber2600/arc-runner-aspire:v2"
   ```

## Troubleshooting

### Copilot Can't Execute Code

**Symptom**: "No runners available" or timeout waiting for runner

**Solution**:
```bash
# Check runner status
kubectl get autoscalingrunnersets -n arc-runners

# View runner logs
kubectl logs -n arc-systems -l app.kubernetes.io/component=runner-scale-set-listener
```

### MCP Server Not Found

**Symptom**: "Command 'npx' not found" or MCP server fails to start

**Solution**: Verify Node.js is available in runner:
```yaml
- run: node --version && npm --version
```

### Docker Not Available

**Symptom**: "Cannot connect to Docker daemon"

**Solution**: Add startup wait in workflow:
```yaml
- run: |
    timeout 30 sh -c 'until docker info > /dev/null 2>&1; do sleep 1; done'
    docker info
```

### Aspire AppHost Won't Start

**Symptom**: "Could not find project or assembly"

**Solution**: Ensure workload is restored:
```yaml
- run: dotnet workload restore
- run: dotnet build src/NorthStarET.NextGen.Lms.AppHost
```

## Security Considerations

- **Runner Isolation**: Each job runs in ephemeral pods (destroyed after job completes)
- **Docker-in-Docker**: Runs in privileged sidecar container (isolated from runner)
- **Secrets**: Use GitHub Secrets, never commit tokens to `mcp-servers.json`
- **MCP Server Trust**: Only use official `@modelcontextprotocol` and `@microsoft` packages

## Resources

- [Copilot Coding Agent Docs](https://docs.github.com/en/enterprise-cloud@latest/copilot/how-tos/use-copilot-agents/coding-agent)
- [ARC Documentation](https://github.com/actions/actions-runner-controller)
- [MCP Specification](https://modelcontextprotocol.io/)
- [.NET Aspire Docs](https://learn.microsoft.com/en-us/dotnet/aspire/)

## Maintenance

This configuration is maintained alongside:
- Custom runner image: `D:\arc-setup\custom-runner-image\`
- ARC setup scripts: `D:\arc-setup\`
- Repository instructions: `.github/copilot-instructions.md`

**Last Updated**: November 12, 2025  
**Runner Image**: ghcr.io/crisweber2600/arc-runner-aspire:latest  
**ARC Version**: 0.13.0