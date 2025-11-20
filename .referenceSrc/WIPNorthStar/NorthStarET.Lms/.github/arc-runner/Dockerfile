# GitHub Actions Runner Controller - Runner Image
# Custom image with .NET 10, Aspire CLI, Docker-in-Docker, and MCP servers

FROM mcr.microsoft.com/devcontainers/dotnet:10.0

# Set environment variables for Copilot Coding Agent
# CRITICAL: These must be set at the container level for the agent to see them
ENV GITHUB_HOST=github.com \
    AGENT_TOOLSDIRECTORY=/opt/hostedtoolcache \
    DOTNET_VERSION=10.0 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    ASPIRE_DASHBOARD_PORT=15000

# Install Node.js LTS for MCP servers
RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - && \
    apt-get install -y nodejs

# Install Docker CLI for Docker-in-Docker
RUN apt-get update && \
    apt-get install -y ca-certificates curl && \
    install -m 0755 -d /etc/apt/keyrings && \
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc && \
    chmod a+r /etc/apt/keyrings/docker.asc && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
    $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
    tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && \
    apt-get install -y docker-ce-cli docker-compose-plugin

# Install GitHub CLI
RUN curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg && \
    chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg && \
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | tee /etc/apt/sources.list.d/github-cli.list > /dev/null && \
    apt-get update && \
    apt-get install -y gh

# Create tool cache directory (for CodeQL and other tools)
RUN mkdir -p /opt/hostedtoolcache && \
    chmod -R 777 /opt/hostedtoolcache

# Install Aspire CLI globally
RUN dotnet tool install -g aspire && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /etc/profile.d/dotnet-tools.sh

# Pre-install common .NET workloads (if aspire is still a workload in future versions)
# For .NET 10, aspire is now a separate CLI tool, not a workload
RUN dotnet workload update || true

# Create workspace directory
RUN mkdir -p /workspace && \
    chmod -R 777 /workspace

WORKDIR /workspace

# Verify installations
RUN echo "=== Environment Verification ===" && \
    echo ".NET SDK: $(dotnet --version)" && \
    echo "Node.js: $(node --version)" && \
    echo "Docker CLI: $(docker --version)" && \
    echo "GitHub CLI: $(gh --version)" && \
    echo "Aspire CLI: $(aspire --version || echo 'will be installed via dotnet tool')" && \
    echo "GITHUB_HOST: $GITHUB_HOST" && \
    echo "AGENT_TOOLSDIRECTORY: $AGENT_TOOLSDIRECTORY"
