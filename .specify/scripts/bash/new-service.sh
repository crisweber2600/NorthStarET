#!/bin/bash
set -euo pipefail

# Scaffolds a new microservice with Clean Architecture structure
# Usage: ./new-service.sh ServiceName

if [ $# -ne 1 ]; then
    echo "Usage: $0 ServiceName"
    echo "Example: $0 StudentManagement"
    exit 1
fi

SERVICE_NAME="$1"

# Get repository root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
SERVICES_DIR="$REPO_ROOT/Src/Foundation/services"
APPHOST_PATH="$REPO_ROOT/Src/Foundation/AppHost/AppHost.cs"
SOLUTION_PATH="$REPO_ROOT/NorthStarET.sln"

# Validate service name
if ! [[ "$SERVICE_NAME" =~ ^[A-Z][a-zA-Z0-9]*$ ]]; then
    echo "Error: Service name must start with an uppercase letter and contain only alphanumeric characters."
    exit 1
fi

SERVICE_PATH="$SERVICES_DIR/$SERVICE_NAME"

# Check if service already exists
if [ -d "$SERVICE_PATH" ]; then
    echo "Error: Service '$SERVICE_NAME' already exists at $SERVICE_PATH"
    exit 1
fi

echo -e "\033[0;32mCreating service: $SERVICE_NAME\033[0m"
echo -e "\033[0;36mLocation: $SERVICE_PATH\033[0m"

# Create service directory
mkdir -p "$SERVICE_PATH"

# Create Domain project
echo -e "\033[0;33mCreating Domain layer...\033[0m"
dotnet new classlib -n "$SERVICE_NAME.Domain" -o "$SERVICE_PATH/$SERVICE_NAME.Domain" -f net10.0
rm -f "$SERVICE_PATH/$SERVICE_NAME.Domain/Class1.cs"

# Create directories
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Domain/Entities"
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Domain/Events"
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Domain/ValueObjects"

# Create Application project
echo -e "\033[0;33mCreating Application layer...\033[0m"
dotnet new classlib -n "$SERVICE_NAME.Application" -o "$SERVICE_PATH/$SERVICE_NAME.Application" -f net10.0
rm -f "$SERVICE_PATH/$SERVICE_NAME.Application/Class1.cs"

# Create directories
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Application/Commands"
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Application/Queries"

# Add Application packages and references
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj" package MediatR
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj" package FluentValidation
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj" reference "$SERVICE_PATH/$SERVICE_NAME.Domain/$SERVICE_NAME.Domain.csproj"
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj" reference "$REPO_ROOT/Src/Foundation/shared/Domain/Domain.csproj"

# Create Infrastructure project
echo -e "\033[0;33mCreating Infrastructure layer...\033[0m"
dotnet new classlib -n "$SERVICE_NAME.Infrastructure" -o "$SERVICE_PATH/$SERVICE_NAME.Infrastructure" -f net10.0
rm -f "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/Class1.cs"

# Create directories
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/Persistence"
mkdir -p "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/Messaging"

# Add Infrastructure packages and references
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj" package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj" reference "$SERVICE_PATH/$SERVICE_NAME.Domain/$SERVICE_NAME.Domain.csproj"
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj" reference "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj"
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj" reference "$REPO_ROOT/Src/Foundation/shared/Infrastructure/Infrastructure.csproj"

# Create API project
echo -e "\033[0;33mCreating API layer...\033[0m"
dotnet new webapi -n "$SERVICE_NAME.Api" -o "$SERVICE_PATH/$SERVICE_NAME.Api" -f net10.0 --no-openapi
rm -f "$SERVICE_PATH/$SERVICE_NAME.Api/WeatherForecast.cs" 2>/dev/null || true
rm -f "$SERVICE_PATH/$SERVICE_NAME.Api/Controllers/WeatherForecastController.cs" 2>/dev/null || true

# Add API references
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Api/$SERVICE_NAME.Api.csproj" reference "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj"
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Api/$SERVICE_NAME.Api.csproj" reference "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj"
dotnet add "$SERVICE_PATH/$SERVICE_NAME.Api/$SERVICE_NAME.Api.csproj" reference "$REPO_ROOT/Src/Foundation/shared/ServiceDefaults/ServiceDefaults.csproj"

# Update API Program.cs to use ServiceDefaults
cat > "$SERVICE_PATH/$SERVICE_NAME.Api/Program.cs" << 'EOF'
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure pipeline
app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
EOF

# Add projects to solution
echo -e "\033[0;33mAdding projects to solution...\033[0m"
dotnet sln "$SOLUTION_PATH" add "$SERVICE_PATH/$SERVICE_NAME.Domain/$SERVICE_NAME.Domain.csproj"
dotnet sln "$SOLUTION_PATH" add "$SERVICE_PATH/$SERVICE_NAME.Application/$SERVICE_NAME.Application.csproj"
dotnet sln "$SOLUTION_PATH" add "$SERVICE_PATH/$SERVICE_NAME.Infrastructure/$SERVICE_NAME.Infrastructure.csproj"
dotnet sln "$SOLUTION_PATH" add "$SERVICE_PATH/$SERVICE_NAME.Api/$SERVICE_NAME.Api.csproj"

# Update AppHost to register the service
echo -e "\033[0;33mRegistering service in AppHost...\033[0m"
SERVICE_VAR=$(echo "$SERVICE_NAME" | tr '[:upper:]' '[:lower:]')
DB_NAME="${SERVICE_NAME}Db"

NEW_RESOURCE_CODE="// Add $SERVICE_NAME database
var ${SERVICE_VAR}Db = postgres.AddDatabase(\"$DB_NAME\");

// Add $SERVICE_NAME API
var ${SERVICE_VAR}Api = builder.AddProject<Projects.${SERVICE_NAME}_Api>(\"${SERVICE_VAR}-api\")
    .WithReference(${SERVICE_VAR}Db)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(${SERVICE_VAR}Db);

"

# Insert before builder.Build().Run()
sed -i "s|builder\.Build()\.Run();|$NEW_RESOURCE_CODE\nbuilder.Build().Run();|" "$APPHOST_PATH"

# Add AppHost reference to the API project
dotnet add "$REPO_ROOT/Src/Foundation/AppHost/AppHost.csproj" reference "$SERVICE_PATH/$SERVICE_NAME.Api/$SERVICE_NAME.Api.csproj"

echo ""
echo -e "\033[0;32m✓ Service '$SERVICE_NAME' created successfully!\033[0m"
echo ""
echo -e "\033[0;36mNext steps:\033[0m"
echo -e "1. Add domain entities to: $SERVICE_PATH/$SERVICE_NAME.Domain/Entities/"
echo -e "2. Add commands/queries to: $SERVICE_PATH/$SERVICE_NAME.Application/"
echo -e "3. Add controllers to: $SERVICE_PATH/$SERVICE_NAME.Api/Controllers/"
echo -e "4. Run: dotnet build"
echo -e "5. Run: dotnet run --project $REPO_ROOT/Src/Foundation/AppHost"
