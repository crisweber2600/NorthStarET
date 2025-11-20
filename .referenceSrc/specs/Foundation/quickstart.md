# Quick Start: Phase 1 Foundation Services

**Version**: 1.0  
**Last Updated**: 2025-11-19  
**Services**: Identity & Authentication, API Gateway (YARP), Configuration

---

## Prerequisites

### Required Software

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **.NET Aspire Workload** - Install with: `dotnet workload install aspire`
- **Docker Desktop** - For PostgreSQL, Redis containers
- **Visual Studio 2022 17.12+** or **VS Code** with C# DevKit
- **PostgreSQL 16+** - Via Docker or local installation
- **Redis 7+** - Via Docker or local installation
- **Azure CLI** - For Key Vault and Service Bus (development)

### Azure Resources (Development)

- **Azure Key Vault** - For secrets management
- **Azure Service Bus** - For event messaging (can use RabbitMQ locally)
- **Microsoft Entra ID Tenant** - For SSO authentication

### Verify Installation

```bash
# Check .NET version
dotnet --version
# Should be 10.0.x

# Check Aspire workload
dotnet workload list
# Should show "aspire" installed

# Check Docker
docker --version
docker ps
```

---

## Project Setup

### 1. Clone Repository

```bash
cd /path/to/workspace
git clone https://github.com/northstar/NorthStarPlan.git
cd NorthStarPlan
git checkout 001-phase1-foundation-services
```

### 2. Restore NuGet Packages

```bash
cd Src/UpgradedNorthStar
dotnet restore
```

### 3. Configure Secrets

**Identity Service - Entra ID**:

```bash
cd src/Identity/NorthStar.Identity.Api

dotnet user-secrets init
dotnet user-secrets set "AzureAd:TenantId" "<your-tenant-id>"
dotnet user-secrets set "AzureAd:ClientId" "<your-client-id>"
dotnet user-secrets set "AzureAd:ClientSecret" "<your-client-secret>"
```

**Configuration Service - Database Connection**:

```bash
cd ../../Configuration/NorthStar.Configuration.Api

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ConfigurationDb" "Host=localhost;Database=northstar_configuration;Username=postgres;Password=<your-password>"
```

**AppHost - Azure Service Bus**:

```bash
cd ../../AppHost/NorthStar.AppHost

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ServiceBus" "<your-service-bus-connection-string>"
```

---

## Database Setup

### Start PostgreSQL Container

```bash
docker run -d \
  --name northstar-postgres \
  -e POSTGRES_PASSWORD=P@ssw0rd \
  -e POSTGRES_DB=postgres \
  -p 5432:5432 \
  postgres:16-alpine
```

### Create Databases

```bash
docker exec -it northstar-postgres psql -U postgres -c "CREATE DATABASE northstar_identity;"
docker exec -it northstar-postgres psql -U postgres -c "CREATE DATABASE northstar_configuration;"
```

### Run Migrations

```bash
# Identity Service
cd src/Identity/NorthStar.Identity.Api
dotnet ef database update

# Configuration Service
cd ../../Configuration/NorthStar.Configuration.Api
dotnet ef database update
```

---

## Start Development Environment

### Option 1: Visual Studio 2022

1. Open `NorthStar.sln` in Visual Studio
2. Set `NorthStar.AppHost` as startup project
3. Press **F5** to run

### Option 2: Command Line

```bash
cd src/AppHost/NorthStar.AppHost
dotnet run
```

### Option 3: VS Code

1. Open workspace in VS Code
2. Press **F5** (launch configuration auto-detected)

---

## Verify Services

### Access Aspire Dashboard

Once AppHost is running, open:

```
http://localhost:15888
```

**Verify**:
- ✅ Identity API - Green status
- ✅ Configuration API - Green status
- ✅ Gateway - Green status
- ✅ PostgreSQL - Connected
- ✅ Redis - Connected

### Test API Endpoints

**Health Checks**:

```bash
# Identity Service
curl http://localhost:5001/health

# Configuration Service
curl http://localhost:5002/health

# API Gateway
curl http://localhost:5000/health
```

**Authentication** (Local Fallback):

```bash
curl -X POST http://localhost:5000/api/identity/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@test.edu",
    "password": "Test@1234"
  }'
```

**Configuration** (Authenticated):

```bash
TOKEN="<your-jwt-token>"

curl http://localhost:5000/api/configuration/districts \
  -H "Authorization: Bearer $TOKEN"
```

---

## Development Workflow

### 1. Create Feature Branch

```bash
git checkout -b feature/001-user-registration
```

### 2. Write Failing Test (RED)

```bash
cd tests/NorthStar.Identity.UnitTests
# Add test file: RegisterUserCommandHandlerTests.cs

dotnet test > ../../docs/evidence/001-user-registration-red.txt
```

**Expected**: Test fails (RED)

### 3. Implement Feature (GREEN)

```csharp
// src/Identity/NorthStar.Identity.Application/Commands/RegisterUserCommand.cs
public class RegisterUserCommand : IRequest<User>
{
    // Implementation
}
```

```bash
dotnet test > ../../docs/evidence/001-user-registration-green.txt
```

**Expected**: Test passes (GREEN)

### 4. Refactor & Verify

```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet tool run reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage
```

**Verify**: ≥80% code coverage

### 5. Commit with Evidence

```bash
git add .
git commit -m "Feature: User registration [Phase1]

- Added RegisterUserCommand with validation
- Implements FR-005 from spec
- Test coverage: 85%
- Evidence: docs/evidence/001-user-registration-{red,green}.txt"
```

### 6. Push to Review Branch

```bash
git push origin HEAD:001review-Phase1
```

---

## Running Tests

### Unit Tests

```bash
# All unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Specific service
dotnet test tests/NorthStar.Identity.UnitTests
```

### Integration Tests (Aspire)

```bash
# Starts test containers automatically
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### BDD Tests (Reqnroll)

```bash
dotnet test tests/NorthStar.Identity.BddTests
```

### Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage -reporttypes:Html
```

Open `coverage/index.html` in browser.

---

## Common Tasks

### Add New API Endpoint

1. **Define command/query** in Application layer
2. **Write test** in UnitTests project (RED)
3. **Implement handler** (GREEN)
4. **Add controller endpoint** in API layer
5. **Update OpenAPI docs** (automatic via Swagger)

### Add Database Entity

1. **Define entity** in Domain layer
2. **Configure in DbContext** (Infrastructure)
3. **Create migration**: `dotnet ef migrations add AddUserEntity`
4. **Apply migration**: `dotnet ef database update`
5. **Write repository tests** (TDD)

### Publish Domain Event

```csharp
// In command handler
await _eventPublisher.PublishAsync(new UserRegisteredEvent
{
    UserId = user.Id,
    Email = user.Email,
    TenantId = user.TenantId,
    RegisteredAt = DateTime.UtcNow
});
```

### Subscribe to Event

```csharp
// Create consumer in Infrastructure
public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        // Handle event
    }
}

// Register in Program.cs
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
});
```

---

## Troubleshooting

### Issue: "Aspire workload not found"

**Solution**:
```bash
dotnet workload update
dotnet workload install aspire
```

### Issue: "Database connection failed"

**Solution**:
```bash
# Verify PostgreSQL is running
docker ps | grep postgres

# Check connection string
dotnet user-secrets list

# Test connection
docker exec -it northstar-postgres psql -U postgres -l
```

### Issue: "Port already in use"

**Solution**:
```bash
# Find process using port
lsof -i :5000  # or :5001, :5002

# Kill process
kill -9 <PID>
```

### Issue: "Entra ID authentication fails"

**Solution**:
1. Verify Entra ID app registration settings
2. Check redirect URIs: `https://localhost:5001/signin-oidc`
3. Verify client secret in Key Vault
4. Check token claims in JWT debugger: https://jwt.ms

### Issue: "Tests fail with database errors"

**Solution**:
```bash
# Use TestContainers - already configured
# Tests will automatically start PostgreSQL container

# Or use in-memory database for unit tests
services.AddDbContext<IdentityDbContext>(options =>
    options.UseInMemoryDatabase("IdentityTestDb"));
```

---

## API Documentation

### Swagger UI

**Identity Service**:
```
http://localhost:5001/swagger
```

**Configuration Service**:
```
http://localhost:5002/swagger
```

**API Gateway**:
```
http://localhost:5000/swagger
```

### OpenAPI Specifications

- [Identity API](./contracts/identity/openapi.yaml)
- [Configuration API](./contracts/configuration/openapi.yaml)
- [Gateway Routes](./contracts/gateway/routes-config.json)

---

## Next Steps

1. **Complete authentication flows** - Entra ID + local fallback
2. **Implement user management** - CRUD operations with RBAC
3. **Setup Configuration Service** - Districts, schools, calendars
4. **Configure API Gateway** - Routing, rate limiting, circuit breakers
5. **Add integration tests** - Full end-to-end user journeys
6. **Performance testing** - Load testing with K6 or JMeter

---

## Resources

### Documentation

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Microsoft Identity Web](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [MassTransit Documentation](https://masstransit.io/)
- [Master Migration Plan](../../../Plans/MASTER_MIGRATION_PLAN.md)
- [Phase 1 Spec](./spec.md)

### Team Contacts

- **Architecture Questions**: Architecture Team
- **Entra ID Issues**: Identity Team
- **Database Issues**: DBA Team
- **DevOps/CI/CD**: DevOps Team

---

## Constitutional Compliance Checklist

Before committing:

- [ ] Tests written first (TDD Red → Green)
- [ ] Test evidence captured (`dotnet test > evidence.txt`)
- [ ] ≥80% code coverage verified
- [ ] Clean Architecture boundaries respected
- [ ] No UI → Infrastructure coupling
- [ ] Secrets in Key Vault only
- [ ] Domain events published for state changes
- [ ] Multi-tenant isolation verified
- [ ] Aspire integration tests passing
- [ ] Swagger/OpenAPI updated

Before merging:

- [ ] All tests passing
- [ ] Code review completed
- [ ] Constitution compliance verified
- [ ] Security audit passed (if auth/security changes)
- [ ] Performance benchmarks met

---

**Ready to develop Phase 1 Foundation Services!**
