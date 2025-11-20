# GitHub Actions Runner with .NET 10, Aspire, and Docker-in-Docker support
FROM ghcr.io/actions/actions-runner:latest

# Switch to root for installations
USER root

# Install .NET 10 SDK
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    ca-certificates \
    curl \
    gnupg \
    lsb-release \
    wget \
    apt-transport-https \
    && wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get -y install --no-install-recommends dotnet-sdk-10.0 \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# Install Aspire CLI
RUN dotnet tool install -g aspire

# Install Node.js and npm (for MCP servers)
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && npm install -g @modelcontextprotocol/server-sequential-thinking @modelcontextprotocol/server-filesystem @playwright/mcp \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# Set environment variables
ENV PATH="${PATH}:/root/.dotnet/tools:/home/runner/.dotnet/tools"
ENV AGENT_TOOLSDIRECTORY=/opt/hostedtoolcache
ENV GITHUB_HOST=github.com
ENV DOTNET_ROOT=/usr/share/dotnet
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Create tools directory
RUN mkdir -p /opt/hostedtoolcache

# Switch back to runner user
USER runner
