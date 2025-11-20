# Aspire Testing Strategy

This document explains how Aspire is used in tests to properly validate the distributed application.

## Test Categories

### 1. Configuration Tests (Fast - No Containers)
**Location**: `tests/aspire/NorthStarET.NextGen.Lms.AspireTests/`

These tests verify the AppHost configuration without starting containers:
- Validate resource definitions (PostgreSQL, Redis, API, Web)
- Check resource dependencies and relationships
- Verify service configurations

**Run with**: `dotnet test tests/aspire/NorthStarET.NextGen.Lms.AspireTests/`

**Duration**: ~10 seconds

### 2. Integration Tests (Slow - Requires Containers)
**Locations**:
- `tests/integration/NorthStarET.NextGen.Lms.IntegrationTests/`
- `tests/integration/NorthStarET.NextGen.Lms.Identity.IntegrationTests/`

These tests start the full distributed application with Docker containers:
- PostgreSQL 16 database
- Redis Stack cache
- API service
- Web service

**Prerequisites**:
- Docker Desktop running
- ~5GB free disk space for container images
- Network access to pull images from Docker Hub

**Run with**: `dotnet test tests/integration/`

**Duration**: 2-5 minutes (first run pulls images)

### 3. E2E Tests (Slow - Requires Containers + Playwright)
**Location**: `tests/ui/NorthStarET.NextGen.Lms.Playwright/`

These tests use Playwright for browser automation against the full stack:
- All integration test requirements
- Playwright browsers installed (`pwsh tests/ui/playwright.ps1`)

**Prerequisites**:
- All integration test prerequisites
- Playwright browsers installed

**Run with**: `pwsh tests/ui/playwright.ps1`

**Duration**: 2-5 minutes

## CI/CD Considerations

### GitHub Actions
The provided workflow should:
1. Always run unit tests and Aspire configuration tests (fast)
2. Optionally run integration/E2E tests based on:
   - PR label (e.g., `test:integration`)
   - File changes (e.g., changes to `src/` trigger integration tests)
   - Manual workflow dispatch

### Local Development
For fast feedback during development:
```bash
# Quick validation (seconds)
dotnet test tests/unit/ tests/aspire/

# Full validation before PR (minutes)
dotnet test
pwsh tests/ui/playwright.ps1
```

## Troubleshooting

### Timeout Issues
If integration/E2E tests timeout:
1. Ensure Docker Desktop is running and healthy
2. Pre-pull images: `docker pull postgres:16` and `docker pull redis/redis-stack:latest`
3. Increase timeout in test fixtures (default: 120s)
4. Check Docker resource limits (memory, CPU)

### Container Startup Failures
If containers fail to start:
1. Check Docker logs: `docker logs <container-id>`
2. Verify ports 5432 (PostgreSQL) and 6379 (Redis) are not in use
3. Clean up stale containers: `docker system prune`

### Resource Name Mismatches
All Aspire resources use lowercase-with-hyphens naming:
- `identity-postgres` - PostgreSQL server
- `identity-db` - PostgreSQL database
- `identity-redis` - Redis cache
- `northstaret-nextgen-lms-api` - API service
- `northstaret-nextgen-lms-web` - Web service

## Future Improvements

1. **Test Containers**: Consider using [Testcontainers](https://dotnet.testcontainers.org/) for better container lifecycle management
2. **Resource Pooling**: Share container instances across test runs to reduce startup time
3. **Selective Testing**: Use test categories/traits to run subsets: `dotnet test --filter Category=Integration`
4. **Parallel Execution**: Configure xUnit collection behavior to optimize test execution time
