#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Playwright UI Test Runner for NorthStarET.NextGen.Lms
.DESCRIPTION
    This script runs the Playwright UI tests for the LMS application.
    It ensures browsers are installed and runs the test suite.
.PARAMETER Configuration
    The build configuration to use (Debug or Release). Default is Debug.
.PARAMETER Filter
    Optional test filter to run specific tests.
.PARAMETER Headless
    Run tests in headless mode. Default is false.
.PARAMETER Workers
    Number of parallel workers. Default is 1.
.EXAMPLE
    .\playwright.ps1
    .\playwright.ps1 -Configuration Release
    .\playwright.ps1 -Filter "SignInFlow" -Headless $false
#>

param(
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [Parameter()]
    [string]$Filter = "",
    
    [Parameter()]
    [bool]$Headless = $false,
    
    [Parameter()]
    [int]$Workers = 1
)

$ErrorActionPreference = "Stop"

# Set working directory to the UI test project
$TestProjectPath = Join-Path $PSScriptRoot "NorthStarET.NextGen.Lms.Playwright"
Set-Location $TestProjectPath

Write-Host "Playwright UI Test Runner" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Test Project: $TestProjectPath" -ForegroundColor Gray

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to restore packages"
    exit $LASTEXITCODE
}

# Build the test project
Write-Host "Building test project..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build test project"
    exit $LASTEXITCODE
}

# Install Playwright browsers if needed
Write-Host "Installing Playwright browsers..." -ForegroundColor Yellow
powershell bin/$Configuration/net9.0/playwright.ps1 install
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Playwright browsers installation had issues, but continuing..."
}

# Build test arguments
$testArgs = @(
    "test"
    "--configuration", $Configuration
    "--no-build"
    "--logger", "trx"
    "--logger", "console;verbosity=normal"
)

if ($Filter) {
    $testArgs += "--filter", $Filter
}

# Set environment variables for Playwright
$env:PLAYWRIGHT_BROWSERS_PATH = "0"  # Use default browser path
if ($Headless) {
    $env:HEADLESS = "true"
}
else {
    $env:HEADLESS = "false"
}

$env:PLAYWRIGHT_WORKERS = $Workers.ToString()

Write-Host "Running Playwright tests..." -ForegroundColor Yellow
Write-Host "Command: dotnet $($testArgs -join ' ')" -ForegroundColor Gray

# Run the tests
& dotnet @testArgs

$exitCode = $LASTEXITCODE
if ($exitCode -eq 0) {
    Write-Host "All Playwright tests passed!" -ForegroundColor Green
}
else {
    Write-Host "Some Playwright tests failed (exit code: $exitCode)" -ForegroundColor Red
}

# Show test results location
$resultsPath = Join-Path $TestProjectPath "TestResults"
if (Test-Path $resultsPath) {
    Write-Host "Test results available in: $resultsPath" -ForegroundColor Cyan
}

exit $exitCode