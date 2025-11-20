FROM mcr.microsoft.com/devcontainers/dotnet:10.0

# Install Docker CLI for Docker-in-Docker support
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends \
    ca-certificates \
    curl \
    gnupg \
    lsb-release \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null \
    && apt-get update \
    && apt-get -y install --no-install-recommends docker-ce-cli docker-compose-plugin \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

# Install global .NET tools (Aspire CLI)
RUN dotnet tool install -g aspire

# Ensure tools are in PATH
ENV PATH="${PATH}:/root/.dotnet/tools:/home/vscode/.dotnet/tools"
ENV AGENT_TOOLSDIRECTORY=/opt/hostedtoolcache

# Install Node.js packages for MCP servers
RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g @modelcontextprotocol/server-sequential-thinking @modelcontextprotocol/server-filesystem @playwright/mcp" 2>&1