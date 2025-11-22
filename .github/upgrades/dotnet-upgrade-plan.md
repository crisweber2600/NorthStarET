# .NET 10.0 (Preview) Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade src\NorthStarET.NextGen.Lms.Contracts\NorthStarET.NextGen.Lms.Contracts.csproj
4. Upgrade src\NorthStarET.NextGen.Lms.Domain\NorthStarET.NextGen.Lms.Domain.csproj
5. Upgrade src\NorthStarET.NextGen.Lms.Application\NorthStarET.NextGen.Lms.Application.csproj
6. Upgrade src\NorthStarET.NextGen.Lms.ServiceDefaults\NorthStarET.NextGen.Lms.ServiceDefaults.csproj
7. Upgrade src\NorthStarET.NextGen.Lms.Infrastructure\NorthStarET.NextGen.Lms.Infrastructure.csproj
8. Upgrade src\NorthStarET.NextGen.Lms.Web\NorthStarET.NextGen.Lms.Web.csproj
9. Upgrade src\NorthStarET.NextGen.Lms.Api\NorthStarET.NextGen.Lms.Api.csproj
10. Upgrade src\NorthStarET.NextGen.Lms.AppHost\NorthStarET.NextGen.Lms.AppHost.csproj
11. Upgrade tests\unit\NorthStarET.NextGen.Lms.Web.Tests\NorthStarET.NextGen.Lms.Web.Tests.csproj
12. Upgrade tests\unit\NorthStarET.NextGen.Lms.Infrastructure.Tests\NorthStarET.NextGen.Lms.Infrastructure.Tests.csproj
13. Upgrade tests\ui\NorthStarET.NextGen.Lms.Playwright\NorthStarET.NextGen.Lms.Playwright.csproj
14. Upgrade tests\bdd\NorthStarET.NextGen.Lms.Bdd\NorthStarET.NextGen.Lms.Bdd.csproj
15. Upgrade tests\aspire\NorthStarET.NextGen.Lms.AspireTests\NorthStarET.NextGen.Lms.AspireTests.csproj
16. Upgrade tests\unit\NorthStarET.NextGen.Lms.Domain.Tests\NorthStarET.NextGen.Lms.Domain.Tests.csproj
17. Upgrade tests\unit\NorthStarET.NextGen.Lms.Api.Tests\NorthStarET.NextGen.Lms.Api.Tests.csproj
18. Upgrade tests\unit\NorthStarET.NextGen.Lms.Application.Tests\NorthStarET.NextGen.Lms.Application.Tests.csproj
19. Run unit and integration tests across the solution to validate the upgrade (discovered test projects will be used).

## Settings

### Excluded projects

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

| Package Name                                    | Current Version | New Version   | Description                                   |
|:-----------------------------------------------|:---------------:|:-------------:|:----------------------------------------------|
| Aspire.Hosting.Testing                          | 9.3.0           | 13.0.0        | Required for Aspire testing in .NET 10        |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 9.0.8         | 10.0.0        | Authentication package aligned with .NET 10   |
| Microsoft.AspNetCore.Authentication.JwtBearer   | 9.0.8           | 10.0.0        | JWT bearer aligned with .NET 10               |
| Microsoft.AspNetCore.OpenApi                    | 9.0.8           | 10.0.0        | OpenAPI integration aligned with .NET 10      |
| Microsoft.EntityFrameworkCore                   | 9.0.10          | 10.0.0        | EF Core aligned with .NET 10                  |
| Microsoft.EntityFrameworkCore.Design            | 9.0.10          | 10.0.0        | EF Core design aligned with .NET 10           |
| Microsoft.EntityFrameworkCore.InMemory          | 9.0.10          | 10.0.0        | Test provider aligned with .NET 10            |
| Microsoft.EntityFrameworkCore.Relational        | 9.0.10          | 10.0.0        | EF Core relational provider for .NET 10       |
| Microsoft.EntityFrameworkCore.Sqlite            | 9.0.0           | 10.0.0        | SQLite provider aligned with .NET 10         |
| Microsoft.Extensions.Caching.StackExchangeRedis | 9.0.0           | 10.0.0        | Redis caching aligned with .NET 10            |
| Microsoft.Extensions.Configuration.EnvironmentVariables | 9.0.0 | 10.0.0 | Configuration alignment for .NET 10           |
| Microsoft.Extensions.Configuration.Json         | 9.0.0           | 10.0.0        | JSON configuration provider aligned with .NET 10 |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.8    | 10.0.0        | DI abstractions aligned with .NET 10          |
| Microsoft.Extensions.Hosting.Abstractions       | 9.0.11          | 10.0.0        | Hosting abstractions aligned with .NET 10     |
| Microsoft.Extensions.Http.Polly                 | 9.0.8           | 10.0.0        | Http Polly extensions aligned with .NET 10    |
| Microsoft.Extensions.Http.Resilience            | 8.10.0          | 10.0.0        | New resilience package for HTTP (net10)       |
| Microsoft.Extensions.Logging.Abstractions       | 9.0.0           | 10.0.0        | Logging abstractions aligned with .NET 10     |
| Microsoft.Extensions.Options                    | 9.0.8           | 10.0.0        | Options abstractions aligned with .NET 10     |
| Microsoft.Extensions.Options.ConfigurationExtensions | 9.0.8  | 10.0.0        | Options configuration extensions for .NET 10  |
| Microsoft.Extensions.Options.DataAnnotations    | 9.0.8           | 10.0.0        | Options data annotations aligned with .NET 10 |
| Microsoft.Extensions.ServiceDiscovery           | 9.5.2           | 10.0.0        | Service discovery aligned with .NET 10        |
| Microsoft.IdentityModel.Protocols               | 7.4.0           | 8.14.0        | IdentityModel LTS update                      |
| Microsoft.IdentityModel.Protocols.OpenIdConnect | 7.4.0           | 8.14.0        | IdentityModel LTS update                      |
| Microsoft.IdentityModel.Tokens                  | 7.4.0           | 8.14.0        | IdentityModel LTS update                      |
| OpenTelemetry.Instrumentation.AspNetCore        | 1.9.0           | 1.14.0-rc.1   | OpenTelemetry updates for .NET 10             |
| OpenTelemetry.Instrumentation.Http              | 1.9.0           | 1.14.0-rc.1   | OpenTelemetry updates for .NET 10             |
| Polly.Extensions.Http                            | 3.0.0           | 3.0.0         | Deprecated recommendation recorded            |
| System.IdentityModel.Tokens.Jwt                 | 7.4.0           | 8.14.0        | IdentityModel LTS update                      |

### Project upgrade details

#### src\NorthStarET.NextGen.Lms.Contracts\NorthStarET.NextGen.Lms.Contracts.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - None

Other changes:
  - None

#### src\NorthStarET.NextGen.Lms.Domain\NorthStarET.NextGen.Lms.Domain.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - None

Other changes:
  - None

#### src\NorthStarET.NextGen.Lms.Application\NorthStarET.NextGen.Lms.Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.DependencyInjection.Abstractions update from `9.0.8` to `10.0.0`
  - Microsoft.Extensions.Logging.Abstractions update from `9.0.0` to `10.0.0`
  - Microsoft.Extensions.Options update from `9.0.8` to `10.0.0`
  - MediatR.Extensions.Microsoft.DependencyInjection removal (functionality folded into main MediatR package)

Other changes:
  - Update MediatR registration to use `services.AddMediatR()` without the extensions package if needed.

#### src\NorthStarET.NextGen.Lms.ServiceDefaults\NorthStarET.NextGen.Lms.ServiceDefaults.csproj modifications

The assistant truncated due to length; full plan persisted to dotnet-upgrade-plan.md
