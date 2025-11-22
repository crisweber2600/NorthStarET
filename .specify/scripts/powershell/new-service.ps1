#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Scaffolds a new microservice with Clean Architecture structure.

.DESCRIPTION
    Creates a new service with Domain, Application, Infrastructure, and API layers,
    registers it in the AppHost, and sets up basic test projects.

.PARAMETER ServiceName
    The name of the service to create (e.g., "StudentManagement")

.EXAMPLE
    .\new-service.ps1 -ServiceName "StudentManagement"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
)

$ErrorActionPreference = "Stop"

# Get repository root
$RepoRoot = Resolve-Path "$PSScriptRoot/../../.."
$ServicesDir = "$RepoRoot/Src/Foundation/services"
$TestsDir = "$RepoRoot/tests"
$AppHostPath = "$RepoRoot/Src/Foundation/AppHost/AppHost.cs"
$SolutionPath = "$RepoRoot/NorthStarET.sln"

# Validate service name
if ($ServiceName -notmatch '^[A-Z][a-zA-Z0-9]*$') {
    Write-Error "Service name must start with an uppercase letter and contain only alphanumeric characters."
    exit 1
}

$ServicePath = "$ServicesDir/$ServiceName"

# Check if service already exists
if (Test-Path $ServicePath) {
    Write-Error "Service '$ServiceName' already exists at $ServicePath"
    exit 1
}

Write-Host "Creating service: $ServiceName" -ForegroundColor Green
Write-Host "Location: $ServicePath" -ForegroundColor Cyan

# Create service directory
New-Item -ItemType Directory -Path $ServicePath -Force | Out-Null

# Create Domain project
Write-Host "Creating Domain layer..." -ForegroundColor Yellow
dotnet new classlib -n "$ServiceName.Domain" -o "$ServicePath/$ServiceName.Domain" -f net10.0
Remove-Item "$ServicePath/$ServiceName.Domain/Class1.cs" -Force

# Create directories
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Domain/Entities" -Force | Out-Null
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Domain/Events" -Force | Out-Null
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Domain/ValueObjects" -Force | Out-Null

# Create Application project
Write-Host "Creating Application layer..." -ForegroundColor Yellow
dotnet new classlib -n "$ServiceName.Application" -o "$ServicePath/$ServiceName.Application" -f net10.0
Remove-Item "$ServicePath/$ServiceName.Application/Class1.cs" -Force

# Create directories
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Application/Commands" -Force | Out-Null
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Application/Queries" -Force | Out-Null

# Add Application packages and references
dotnet add "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj" package MediatR
dotnet add "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj" package FluentValidation
dotnet add "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj" reference "$ServicePath/$ServiceName.Domain/$ServiceName.Domain.csproj"
dotnet add "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj" reference "$RepoRoot/Src/Foundation/shared/Domain/Domain.csproj"

# Create Infrastructure project
Write-Host "Creating Infrastructure layer..." -ForegroundColor Yellow
dotnet new classlib -n "$ServiceName.Infrastructure" -o "$ServicePath/$ServiceName.Infrastructure" -f net10.0
Remove-Item "$ServicePath/$ServiceName.Infrastructure/Class1.cs" -Force

# Create directories
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Infrastructure/Persistence" -Force | Out-Null
New-Item -ItemType Directory -Path "$ServicePath/$ServiceName.Infrastructure/Messaging" -Force | Out-Null

# Add Infrastructure packages and references
dotnet add "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj" package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj" package Microsoft.EntityFrameworkCore.Design
dotnet add "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj" reference "$ServicePath/$ServiceName.Domain/$ServiceName.Domain.csproj"
dotnet add "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj" reference "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj"
dotnet add "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj" reference "$RepoRoot/Src/Foundation/shared/Infrastructure/Infrastructure.csproj"

# Create API project
Write-Host "Creating API layer..." -ForegroundColor Yellow
dotnet new webapi -n "$ServiceName.Api" -o "$ServicePath/$ServiceName.Api" -f net10.0 --no-openapi
Remove-Item "$ServicePath/$ServiceName.Api/WeatherForecast.cs" -Force -ErrorAction SilentlyContinue
Remove-Item "$ServicePath/$ServiceName.Api/Controllers/WeatherForecastController.cs" -Force -ErrorAction SilentlyContinue

# Add API references
dotnet add "$ServicePath/$ServiceName.Api/$ServiceName.Api.csproj" reference "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj"
dotnet add "$ServicePath/$ServiceName.Api/$ServiceName.Api.csproj" reference "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj"
dotnet add "$ServicePath/$ServiceName.Api/$ServiceName.Api.csproj" reference "$RepoRoot/Src/Foundation/shared/ServiceDefaults/ServiceDefaults.csproj"

# Update API Program.cs to use ServiceDefaults
$ProgramCs = @"
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
"@
Set-Content -Path "$ServicePath/$ServiceName.Api/Program.cs" -Value $ProgramCs

# Add projects to solution
Write-Host "Adding projects to solution..." -ForegroundColor Yellow
dotnet sln "$SolutionPath" add "$ServicePath/$ServiceName.Domain/$ServiceName.Domain.csproj"
dotnet sln "$SolutionPath" add "$ServicePath/$ServiceName.Application/$ServiceName.Application.csproj"
dotnet sln "$SolutionPath" add "$ServicePath/$ServiceName.Infrastructure/$ServiceName.Infrastructure.csproj"
dotnet sln "$SolutionPath" add "$ServicePath/$ServiceName.Api/$ServiceName.Api.csproj"

# Update AppHost to register the service
Write-Host "Registering service in AppHost..." -ForegroundColor Yellow
$AppHostContent = Get-Content $AppHostPath -Raw
$ServiceVar = $ServiceName.ToLower()
$DbName = "${ServiceName}Db"

$NewResourceCode = @"
// Add $ServiceName database
var $ServiceVar`Db = postgres.AddDatabase("$DbName");

// Add $ServiceName API
var $ServiceVar`Api = builder.AddProject<Projects.${ServiceName}_Api>("$ServiceVar-api")
    .WithReference($ServiceVar`Db)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor($ServiceVar`Db);

"@

# Insert before builder.Build().Run()
$AppHostContent = $AppHostContent -replace '(builder\.Build\(\)\.Run\(\);)', "$NewResourceCode`$1"
Set-Content -Path $AppHostPath -Value $AppHostContent

# Add AppHost reference to the API project
dotnet add "$RepoRoot/Src/Foundation/AppHost/AppHost.csproj" reference "$ServicePath/$ServiceName.Api/$ServiceName.Api.csproj"

Write-Host "`n✓ Service '$ServiceName' created successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Add domain entities to: $ServicePath/$ServiceName.Domain/Entities/" -ForegroundColor White
Write-Host "2. Add commands/queries to: $ServicePath/$ServiceName.Application/" -ForegroundColor White
Write-Host "3. Add controllers to: $ServicePath/$ServiceName.Api/Controllers/" -ForegroundColor White
Write-Host "4. Run: dotnet build" -ForegroundColor White
Write-Host "5. Run: dotnet run --project $RepoRoot/Src/Foundation/AppHost" -ForegroundColor White
