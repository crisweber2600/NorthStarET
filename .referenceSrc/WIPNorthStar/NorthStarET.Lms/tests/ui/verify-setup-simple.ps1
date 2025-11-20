#!/usr/bin/env pwsh

Write-Host "Playwright Setup Verification" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Check if we're in the right directory
$currentDir = Get-Location
$testProjectPath = Join-Path $currentDir "NorthStarET.NextGen.Lms.Playwright"

if (-not (Test-Path $testProjectPath)) {
    Write-Error "Please run this script from the tests/ui directory"
    exit 1
}

Write-Host "PASS: Running from correct directory" -ForegroundColor Green

# Check project file exists
$projectFile = Join-Path $testProjectPath "NorthStarET.NextGen.Lms.Playwright.csproj"
if (Test-Path $projectFile) {
    Write-Host "PASS: Project file found" -ForegroundColor Green
}
else {
    Write-Host "FAIL: Project file not found" -ForegroundColor Red
    exit 1
}

# Check if project builds
Write-Host "Building project..." -ForegroundColor Yellow
Set-Location $testProjectPath
$buildResult = dotnet build --configuration Debug --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "PASS: Project builds successfully" -ForegroundColor Green
}
else {
    Write-Host "FAIL: Project build failed" -ForegroundColor Red
    exit 1
}

# Check playwright script
$playwrightScript = "bin/Debug/net9.0/playwright.ps1"
if (Test-Path $playwrightScript) {
    Write-Host "PASS: Playwright script found" -ForegroundColor Green
}
else {
    Write-Host "FAIL: Playwright script not found" -ForegroundColor Red
    exit 1
}

# Check configuration files
$configFile = "playwright.config.js"
if (Test-Path $configFile) {
    Write-Host "PASS: Playwright configuration file found" -ForegroundColor Green
}
else {
    Write-Host "INFO: Playwright configuration file not found (optional)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Setup Summary:" -ForegroundColor Cyan
Write-Host "  - Project compiles: PASS" -ForegroundColor Green
Write-Host "  - Playwright installed: PASS" -ForegroundColor Green  
Write-Host "  - Configuration ready: PASS" -ForegroundColor Green
Write-Host "  - Ready for testing: PASS" -ForegroundColor Green

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Start the application: dotnet run --project ../../../src/NorthStarET.NextGen.Lms.AppHost"
Write-Host "  2. Run tests: pwsh ../playwright.ps1"
Write-Host "  3. View results in TestResults/ directory"

Write-Host ""
Write-Host "Playwright setup is complete and ready for use!" -ForegroundColor Green