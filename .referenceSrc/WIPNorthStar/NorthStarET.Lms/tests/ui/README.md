# Playwright Setup Guide for NorthStarET.NextGen.Lms

## Overview

Playwright is now successfully installed and configured for UI testing. This guide covers how to run the tests and understand the setup.

## Prerequisites

✅ **COMPLETED**:

- .NET 9.0 SDK
- Playwright browsers (Chromium, Firefox, WebKit)
- NuGet packages restored
- Project compiled successfully

## Project Structure

```
tests/ui/
├── playwright.ps1                          # Main test runner script
├── NorthStarET.NextGen.Lms.Playwright/
│   ├── playwright.config.js               # Playwright configuration
│   ├── NorthStarET.NextGen.Lms.Playwright.csproj
│   └── Tests/
│       ├── DistrictManagementTests.cs
│       ├── DistrictAdminDelegationTests.cs
│       ├── SessionExpirationTests.cs
│       ├── SignInFlowTests.cs
│       └── TenantSwitchingTests.cs
```

## Running Tests

### 1. Run All Tests

From the repository root:

```bash
pwsh tests/ui/playwright.ps1
```

### 2. Run Specific Tests

```bash
# Run with filter
pwsh tests/ui/playwright.ps1 -Filter "DistrictManagement"

# Run in headless mode (CI-friendly)
pwsh tests/ui/playwright.ps1 -Headless $true

# Run with specific configuration
pwsh tests/ui/playwright.ps1 -Configuration Release
```

### 3. Run Individual Test via dotnet

```bash
cd tests/ui/NorthStarET.NextGen.Lms.Playwright
dotnet test --filter "DistrictManagementPage_LoadsSuccessfully"
```

## Application Dependency

⚠️ **IMPORTANT**: The UI tests use Aspire to orchestrate the full application stack (PostgreSQL, Redis, API, Web).

### Running Tests

#### Prerequisites

- **.NET 9.0 SDK** with Aspire workload: `dotnet workload install aspire`
- **Docker Desktop** running and accessible
- **Playwright browsers** installed

#### Option 1: Run via dotnet test (Recommended)

```bash
# From repository root
dotnet test tests/ui/NorthStarET.NextGen.Lms.Playwright/
```

This will:

1. Start Aspire with PostgreSQL, Redis, API, and Web containers
2. Wait for services to become healthy
3. Run all Playwright tests against the running application
4. Clean up containers after completion

**Note**: First run may take several minutes to pull Docker images.

#### Option 2: Skip when Docker unavailable

```bash
# Set environment variable to skip Aspire tests
export SKIP_ASPIRE_TESTS=true
dotnet test tests/ui/NorthStarET.NextGen.Lms.Playwright/
```

Tests will skip gracefully with a message explaining the requirement.

#### Option 3: Manual Application Start (Advanced)

```bash
# Terminal 1: Start via Aspire
dotnet run --project src/NorthStarET.NextGen.Lms.AppHost

# Terminal 2: Run tests (requires manual setup)
pwsh tests/ui/playwright.ps1
```

## Application Dependency

The tests automatically start the application via Aspire orchestration. You don't need to manually start the application unless using Option 3 above.

## Test Status

### ✅ Working

- Playwright browser installation
- Project compilation
- Test discovery (39 tests found)
- Browser automation (browsers start correctly)
- Aspire orchestration integration
- Graceful skip when Docker/DCP unavailable

### ⚠️ Environment Requirements

**Tests will skip automatically if:**

- Docker is not running or accessible
- Aspire DCP (Developer Control Plane) cannot start
- Network constraints prevent container orchestration

**To explicitly skip** (useful in CI environments):

```bash
export SKIP_ASPIRE_TESTS=true
dotnet test tests/ui/NorthStarET.NextGen.Lms.Playwright/
```

### 🔄 Awaiting Figma Assets

Some tests are marked as `[Ignore]` pending design assets:

- SignInFlowTests (awaiting Figma frames)

## Configuration

### Environment Variables

- `NORTHSTARET_LMS_WEB_BASE_URL`: Override base URL (default: https://localhost:7002)
- `HEADLESS`: Run browsers in headless mode (default: false)
- `PLAYWRIGHT_WORKERS`: Number of parallel workers (default: 1)

### Browser Configuration

Tests use the base `PageTest` class from Playwright.NUnit with:

- HTTPS error ignoring (for development)
- Base URL configuration
- Context options for consistent testing

## Troubleshooting

### Tests Skip with "Aspire/DCP cannot start"

**Cause**: Docker is not running, DCP has configuration issues, or network constraints prevent container orchestration.

**Solutions**:

1. Ensure Docker Desktop is running: `docker ps`
2. Verify Aspire workload is installed: `dotnet workload list | grep aspire`
3. Check DCP version: `~/.nuget/packages/aspire.hosting.orchestration.linux-x64/*/tools/dcp version`
4. In CI/constrained environments: `export SKIP_ASPIRE_TESTS=true`

### Timeout During Aspire Startup

**Cause**: Container images need to be pulled or system resources are constrained.

**Solutions**:

1. Pre-pull images: `docker pull postgres:16` and `docker pull redis/redis-stack:latest`
2. Increase Docker resource limits (4GB+ RAM recommended)
3. First run takes longer; subsequent runs are faster with cached images

### Test Discovery Issues

Rebuild the test project:

```bash
cd tests/ui/NorthStarET.NextGen.Lms.Playwright
dotnet clean && dotnet build
```

## Next Steps

1. **Start the application** using Aspire
2. **Run the test suite** to verify end-to-end functionality
3. **Review failing tests** for implementation gaps
4. **Update tests** as Figma assets become available

## Architecture Integration

The Playwright tests integrate with your Clean Architecture:

- Tests target the **Web** project (Razor Pages)
- Tests validate **Application** services via UI interactions
- Tests verify **Infrastructure** integrations (auth, data persistence)
- Tests use **Aspire** for complete stack testing

This follows the Constitution's requirement for Red→Green evidence and UI testing with Figma backing.
