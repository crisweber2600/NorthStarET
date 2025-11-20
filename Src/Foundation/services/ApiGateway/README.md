# API Gateway (YARP)

**Phase**: 1 (Weeks 1-8)  
**Priority**: Critical (Blocking - Routes traffic to all services)  
**Status**: To Be Implemented

---

## Overview

The API Gateway provides a unified entry point for all client applications using **YARP (Yet Another Reverse Proxy)** to:

- **Route requests** to appropriate microservices
- **Aggregate health checks** from all services
- **Apply cross-cutting concerns** (authentication, rate limiting, logging)
- **Enable Strangler Fig migration** (route to legacy OldNorthStar or new Foundation services)
- **Provide API versioning** (`/api/v1/`, `/api/v2/`)

**Technology**: YARP (Yet Another Reverse Proxy)  
**Database**: None (stateless gateway)

---

## Responsibilities

1. **Request Routing**
   - Route `/api/identity/*` → Identity Service
   - Route `/api/configuration/*` → Configuration Service
   - Route `/api/students/*` → Student Management Service
   - Route `/api/staff/*` → Staff Management Service
   - Route `/api/assessments/*` → Assessment Service
   - ... (all 11 services)

2. **Strangler Fig Migration**
   - Route to OldNorthStar for non-migrated features
   - Gradually shift routes to new Foundation services
   - Dual-write coordination (if needed)

3. **Authentication & Authorization**
   - Validate JWT tokens from Identity Service
   - Reject unauthenticated requests
   - Forward user claims to downstream services

4. **Cross-Cutting Concerns**
   - Rate limiting (per tenant, per endpoint)
   - Request/response logging
   - Distributed tracing correlation IDs
   - Circuit breaker (Polly)

5. **Health Check Aggregation**
   - Poll health endpoints of all services
   - Expose `/health` for overall system health
   - Expose `/ready` for Kubernetes readiness probe

---

## YARP Configuration

### Routing Rules

`appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": {
          "Path": "/api/identity/{**catch-all}"
        },
        "Transforms": [
          { "PathPattern": "/api/{**catch-all}" }
        ]
      },
      "configuration-route": {
        "ClusterId": "configuration-cluster",
        "Match": {
          "Path": "/api/configuration/{**catch-all}"
        }
      },
      "student-route": {
        "ClusterId": "student-cluster",
        "Match": {
          "Path": "/api/students/{**catch-all}"
        }
      },
      "legacy-fallback": {
        "ClusterId": "oldnorthstar-cluster",
        "Match": {
          "Path": "/{**catch-all}"
        },
        "Order": 1000
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://identity:80"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:10",
            "Timeout": "00:00:05",
            "Path": "/health"
          }
        }
      },
      "configuration-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://configuration:80"
          }
        }
      },
      "oldnorthstar-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://oldnorthstar.example.com"
          }
        }
      }
    }
  }
}
```

### Authentication Middleware

`Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        // Forward authentication headers
        builderContext.AddRequestTransform(async transformContext =>
        {
            var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                transformContext.ProxyRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        });
    });

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.Audience = "northstar-api";
    });

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapReverseProxy();

app.Run();
```

---

## Strangler Fig Pattern

### Gradual Migration Strategy

1. **Phase 1 (Weeks 1-8)**: Route identity/config to new Foundation services, rest to OldNorthStar
2. **Phase 2 (Weeks 9-16)**: Route student/staff/assessment to Foundation, rest to OldNorthStar
3. **Phase 3 (Weeks 17-22)**: Route intervention/section/data-import to Foundation
4. **Phase 4 (Weeks 23-28)**: Route reporting/content to Foundation, decommission OldNorthStar

### Route Priority

- **High Priority** (Order=1): New Foundation service routes
- **Low Priority** (Order=1000): Fallback to OldNorthStar for non-migrated features

---

## Rate Limiting Strategy

### Per-Tenant Rate Limits

```csharp
options.AddPolicy("per-tenant", httpContext =>
{
    var tenantId = httpContext.User.FindFirstValue("tenant_id");
    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: tenantId,
        factory: partition => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1000, // 1000 requests per tenant per minute
            Window = TimeSpan.FromMinutes(1)
        });
});
```

### Per-Endpoint Rate Limits

```json
{
  "RateLimiting": {
    "Policies": {
      "login-endpoint": {
        "PermitLimit": 10,
        "Window": "00:01:00"
      },
      "report-generation": {
        "PermitLimit": 5,
        "Window": "00:05:00"
      }
    }
  }
}
```

---

## Health Check Aggregation

`Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddUrlGroup(new Uri("http://identity/health"), "identity")
    .AddUrlGroup(new Uri("http://configuration/health"), "configuration")
    .AddUrlGroup(new Uri("http://student/health"), "student");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## Project Structure

```
ApiGateway/
├── ApiGateway.csproj                 # Project file
├── Program.cs                        # YARP configuration
├── appsettings.json                  # Routing rules, rate limits
├── Middleware/
│   ├── TenantResolutionMiddleware.cs # Resolve tenant from claims/headers
│   └── RequestLoggingMiddleware.cs   # Log all requests
└── README.md                         # This file
```

---

## Configuration

```json
{
  "IdentityServer": {
    "Authority": "https://identity.northstar.local"
  },
  "RateLimiting": {
    "GlobalLimit": 100,
    "WindowMinutes": 1
  },
  "Logging": {
    "LogLevel": {
      "Yarp": "Information"
    }
  }
}
```

---

## Testing Requirements

### Integration Tests (Aspire)

- Route requests to correct services
- Authentication enforcement (reject unauthenticated requests)
- Rate limiting enforcement
- Health check aggregation
- Strangler Fig routing (Foundation vs OldNorthStar)

### Load Tests

- 1000 concurrent requests
- Latency targets: p50 < 50ms, p99 < 200ms
- Rate limiting under load

---

## References

- **Specification**: [005-api-gateway](../../../Plan/Foundation/specs/Foundation/005-api-gateway/)
- **Configuration**: [api-gateway-config.md](../../../docs/standards/api-gateway-config.md)
- **Scenario**: [06-api-gateway-orchestration.md](../../../Plan/Foundation/Plans/scenarios/06-api-gateway-orchestration.md)
- **YARP Documentation**: [https://microsoft.github.io/reverse-proxy/](https://microsoft.github.io/reverse-proxy/)

---

**Status**: Specification Complete, Implementation Pending  
**Start Date**: TBD (Phase 1, Week 2)  
**Completion Target**: Week 4
