# Foundation Layer: Shared ServiceDefaults

**Purpose**: .NET Aspire service defaults for consistent orchestration, logging, telemetry, and observability across all Foundation layer services.

**Version**: 1.0.0  
**Last Updated**: 2025-11-20

---

## Overview

The `ServiceDefaults` project provides common .NET Aspire configuration that every Foundation service applies to ensure:

- Consistent service orchestration patterns
- Structured logging with correlation IDs
- Distributed tracing (OpenTelemetry)
- Standardized health checks
- Resilience policies (Polly)
- Metrics collection

**Consumed By**: All Foundation services (Identity, ApiGateway, Configuration, Student, Staff, Assessment, etc.)

---

## Key Responsibilities

### 1. Aspire Service Registration

Extension methods to register Aspire defaults for all services:

```csharp
public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
{
    builder.ConfigureOpenTelemetry();
    builder.AddDefaultHealthChecks();
    builder.Services.AddServiceDiscovery();
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler();
        http.AddServiceDiscovery();
    });
    return builder;
}
```

### 2. Structured Logging

Serilog configuration with JSON output for production:

```csharp
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.WithCorrelationId()
    .Enrich.WithMachineName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces));
```

### 3. Distributed Tracing

OpenTelemetry configuration for Azure Application Insights:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("MassTransit")
        .AddAzureMonitorTraceExporter(options =>
        {
            options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
        }));
```

### 4. Health Checks

Default health checks for all services:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<TDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddAzureServiceBusTopic(builder.Configuration.GetConnectionString("ServiceBus"), "health");
```

### 5. Resilience Policies

Polly resilience handlers for HTTP clients:

```csharp
services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.Timeout.Timeout = TimeSpan.FromSeconds(30);
    });
});
```

---

## Project Structure

```
ServiceDefaults/
├── ServiceDefaults.csproj           # Project file
├── Extensions.cs                     # Extension methods
│   └── AddServiceDefaults()          # Main registration method
├── OpenTelemetryExtensions.cs        # Telemetry configuration
├── HealthCheckExtensions.cs          # Health check registration
└── README.md                         # This file
```

---

## Usage in Services

Every Foundation service applies ServiceDefaults in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (logging, tracing, health checks, resilience)
builder.AddServiceDefaults();

// Add service-specific registrations
builder.AddApplicationServices();
builder.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Map default health check endpoint
app.MapDefaultEndpoints();

app.Run();
```

---

## Configuration

Configuration is read from `appsettings.json` and environment variables:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=..."
  },
  "HealthChecks": {
    "UI": {
      "PollingInterval": 10
    }
  }
}
```

---

## Dependencies

- `Aspire.Hosting` - .NET Aspire hosting abstractions
- `Microsoft.Extensions.ServiceDiscovery` - Service discovery for Aspire
- `OpenTelemetry.Exporter.AzureMonitor` - Azure Application Insights
- `Serilog.AspNetCore` - Structured logging
- `Polly` - Resilience and transient fault handling
- `AspNetCore.HealthChecks.*` - Health check providers

---

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [Serilog](https://serilog.net/)
- [Polly](https://www.pollydocs.org/)
- [Constitution: Principle 1 - Clean Architecture & Aspire Orchestration](../../../../.specify/memory/constitution.md)
- [LAYERS.md: Shared Infrastructure](../../../../Plan/Foundation/LAYERS.md#shared-infrastructure)

---

**Status**: To Be Implemented (Phase 1 - Weeks 1-2)  
**Related Spec**: [001-phase1-foundation-services](../../../../Plan/Foundation/specs/Foundation/001-phase1-foundation-services/)
