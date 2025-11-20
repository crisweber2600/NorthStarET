# API Gateway Configuration Guide

## Overview

The API Gateway serves as the single entry point for all client requests to the NorthStar microservices. This guide covers the configuration and setup of the API Gateway using YARP (Yet Another Reverse Proxy) or Ocelot.

## Gateway Responsibilities

1. **Request Routing** - Route requests to appropriate microservices
2. **Authentication** - Validate JWT tokens
3. **Authorization** - Enforce access policies
4. **Rate Limiting** - Protect services from overload
5. **Load Balancing** - Distribute requests across service instances
6. **API Composition** - Aggregate responses from multiple services
7. **Request/Response Transformation** - Modify requests and responses
8. **Caching** - Cache responses for improved performance
9. **Logging & Monitoring** - Centralized request logging
10. **CORS** - Handle cross-origin requests

## Technology Choice: YARP vs Ocelot

### YARP (Recommended)
- **Pros**: Microsoft-supported, high performance, actively maintained, flexible configuration
- **Cons**: Newer, less third-party documentation
- **Use When**: Building a new gateway or need high performance

### Ocelot
- **Pros**: Feature-rich, mature, good documentation, .NET ecosystem
- **Cons**: Lower performance than YARP, less active development
- **Use When**: Need specific features like request aggregation out-of-the-box

**Recommendation**: Use YARP for NorthStar migration due to performance and Microsoft support.

## YARP Configuration

### Installation

```bash
dotnet new web -n NorthStar.Gateway
cd NorthStar.Gateway
dotnet add package Yarp.ReverseProxy
```

### Basic Configuration (appsettings.json)

```json
{
  "ReverseProxy": {
    "Routes": {
      "student-route": {
        "ClusterId": "student-cluster",
        "Match": {
          "Path": "/api/v1/students/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/api/v1/students/{**catch-all}"
          }
        ]
      },
      "assessment-route": {
        "ClusterId": "assessment-cluster",
        "Match": {
          "Path": "/api/v1/assessments/{**catch-all}"
        }
      },
      "staff-route": {
        "ClusterId": "staff-cluster",
        "Match": {
          "Path": "/api/v1/staff/{**catch-all}"
        }
      },
      "intervention-route": {
        "ClusterId": "intervention-cluster",
        "Match": {
          "Path": "/api/v1/interventions/{**catch-all}"
        }
      },
      "section-route": {
        "ClusterId": "section-cluster",
        "Match": {
          "Path": "/api/v1/sections/{**catch-all}"
        }
      },
      "reporting-route": {
        "ClusterId": "reporting-cluster",
        "Match": {
          "Path": "/api/v1/reports/{**catch-all}"
        }
      },
      "configuration-route": {
        "ClusterId": "configuration-cluster",
        "Match": {
          "Path": "/api/v1/configuration/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "student-cluster": {
        "Destinations": {
          "student-destination": {
            "Address": "http://student-service"
          }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:30",
            "Timeout": "00:00:10",
            "Policy": "ConsecutiveFailures",
            "Path": "/health"
          }
        },
        "LoadBalancingPolicy": "RoundRobin"
      },
      "assessment-cluster": {
        "Destinations": {
          "assessment-destination": {
            "Address": "http://assessment-service"
          }
        },
        "LoadBalancingPolicy": "RoundRobin"
      },
      "staff-cluster": {
        "Destinations": {
          "staff-destination": {
            "Address": "http://staff-service"
          }
        }
      },
      "intervention-cluster": {
        "Destinations": {
          "intervention-destination": {
            "Address": "http://intervention-service"
          }
        }
      },
      "section-cluster": {
        "Destinations": {
          "section-destination": {
            "Address": "http://section-service"
          }
        }
      },
      "reporting-cluster": {
        "Destinations": {
          "reporting-destination": {
            "Address": "http://reporting-service"
          }
        }
      },
      "configuration-cluster": {
        "Destinations": {
          "configuration-destination": {
            "Address": "http://configuration-service"
          }
        }
      }
    }
  }
}
```

### Program.cs Setup

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("NorthStarPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"].Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Add response caching
builder.Services.AddResponseCaching();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure middleware pipeline
app.UseCors("NorthStarPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseResponseCaching();

// Map health checks
app.MapHealthChecks("/health");

// Map reverse proxy
app.MapReverseProxy();

app.Run();
```

## Authentication Flow

### 1. Client Login
```
Client -> Gateway -> Identity Service
    <- JWT Token <-
```

### 2. Authenticated Requests
```
Client (with JWT) -> Gateway (validates JWT) -> Microservice
                  <- Response <-
```

## Rate Limiting Configuration

### Per-User Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("user-api", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});
```

### Apply to Routes

```json
{
  "ReverseProxy": {
    "Routes": {
      "student-route": {
        "ClusterId": "student-cluster",
        "Match": {
          "Path": "/api/v1/students/{**catch-all}"
        },
        "RateLimiterPolicy": "user-api"
      }
    }
  }
}
```

## Request Transformation

### Add Custom Headers

```csharp
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use((context, next) =>
    {
        // Add correlation ID
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();
        
        // Add user info
        if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Request.Headers["X-User-Id"] = context.User.FindFirst("sub")?.Value;
        }
        
        return next();
    });
});
```

### Response Transformation

```csharp
proxyPipeline.Use(async (context, next) =>
{
    await next();
    
    // Add custom response headers
    context.Response.Headers["X-Gateway-Version"] = "1.0";
    context.Response.Headers["X-Response-Time"] = 
        DateTime.UtcNow.ToString("o");
});
```

## API Composition

For complex UI views that need data from multiple services:

```csharp
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public DashboardController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentDashboard(Guid studentId)
    {
        var tasks = new[]
        {
            GetStudentInfo(studentId),
            GetStudentAssessments(studentId),
            GetStudentInterventions(studentId)
        };
        
        await Task.WhenAll(tasks);
        
        return Ok(new
        {
            Student = tasks[0].Result,
            Assessments = tasks[1].Result,
            Interventions = tasks[2].Result
        });
    }
    
    private async Task<object> GetStudentInfo(Guid studentId)
    {
        var client = _httpClientFactory.CreateClient("student-service");
        var response = await client.GetAsync($"/api/v1/students/{studentId}");
        return await response.Content.ReadFromJsonAsync<object>();
    }
    
    // Similar methods for assessments and interventions...
}
```

## Caching Strategy

### Response Caching

```csharp
[HttpGet]
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "page", "pageSize" })]
public async Task<IActionResult> GetStudents([FromQuery] int page, [FromQuery] int pageSize)
{
    // Implementation
}
```

### Distributed Caching

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"];
    options.InstanceName = "NorthStar:";
});

// Usage in controller
public class StudentsController : ControllerBase
{
    private readonly IDistributedCache _cache;
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(Guid id)
    {
        var cacheKey = $"student:{id}";
        var cached = await _cache.GetStringAsync(cacheKey);
        
        if (cached != null)
        {
            return Ok(JsonSerializer.Deserialize<StudentDto>(cached));
        }
        
        var student = await _repository.GetByIdAsync(id);
        
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(student),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        
        return Ok(student);
    }
}
```

## Load Balancing

### Round Robin (Default)

```json
{
  "LoadBalancingPolicy": "RoundRobin"
}
```

### Least Requests

```json
{
  "LoadBalancingPolicy": "LeastRequests"
}
```

### Random

```json
{
  "LoadBalancingPolicy": "Random"
}
```

### Custom Load Balancing

```csharp
builder.Services.AddSingleton<ILoadBalancingPolicy, WeightedRoundRobinPolicy>();
```

## Health Checks

### Active Health Checks

```json
{
  "HealthCheck": {
    "Active": {
      "Enabled": true,
      "Interval": "00:00:30",
      "Timeout": "00:00:10",
      "Policy": "ConsecutiveFailures",
      "Path": "/health"
    }
  }
}
```

### Passive Health Checks

```json
{
  "HealthCheck": {
    "Passive": {
      "Enabled": true,
      "Policy": "TransportFailureRate",
      "ReactivationPeriod": "00:01:00"
    }
  }
}
```

## Circuit Breaker Pattern

```csharp
builder.Services.AddHttpClient("student-service")
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromSeconds(30)));
```

## Monitoring & Logging

### Structured Logging

```csharp
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation(
        "Gateway request: {Method} {Path} from {RemoteIp}",
        context.Request.Method,
        context.Request.Path,
        context.Connection.RemoteIpAddress);
    
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    
    logger.LogInformation(
        "Gateway response: {StatusCode} in {ElapsedMs}ms",
        context.Response.StatusCode,
        sw.ElapsedMilliseconds);
});
```

### Application Insights Integration

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

## Security Headers

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", 
        "max-age=31536000; includeSubDomains");
    
    await next();
});
```

## Error Handling

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<IExceptionHandlerFeature>();
        
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception in API Gateway");
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An error occurred processing your request",
                requestId = context.TraceIdentifier
            });
        }
    });
});
```

## Environment-Specific Configuration

### Development (appsettings.Development.json)

```json
{
  "ReverseProxy": {
    "Clusters": {
      "student-cluster": {
        "Destinations": {
          "student-destination": {
            "Address": "http://localhost:5002"
          }
        }
      }
    }
  }
}
```

### Production (appsettings.Production.json)

```json
{
  "ReverseProxy": {
    "Clusters": {
      "student-cluster": {
        "Destinations": {
          "student-instance-1": {
            "Address": "http://student-service-1.northstar.svc.cluster.local"
          },
          "student-instance-2": {
            "Address": "http://student-service-2.northstar.svc.cluster.local"
          },
          "student-instance-3": {
            "Address": "http://student-service-3.northstar.svc.cluster.local"
          }
        }
      }
    }
  }
}
```

## Testing the Gateway

### Local Testing

```bash
# Start gateway
cd NorthStar.Gateway
dotnet run

# Test routing
curl -X GET http://localhost:5000/api/v1/students \
  -H "Authorization: Bearer <your-jwt-token>"
```

### Load Testing

```bash
# Using Apache Bench
ab -n 1000 -c 10 -H "Authorization: Bearer <token>" \
  http://localhost:5000/api/v1/students

# Using k6
k6 run load-test.js
```

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "NorthStar.Gateway.dll"]
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-gateway
spec:
  replicas: 3
  selector:
    matchLabels:
      app: api-gateway
  template:
    metadata:
      labels:
        app: api-gateway
    spec:
      containers:
      - name: gateway
        image: northstar/api-gateway:latest
        ports:
        - containerPort: 80
---
apiVersion: v1
kind: Service
metadata:
  name: api-gateway
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 80
  selector:
    app: api-gateway
```

## Best Practices

1. **Always validate JWT tokens at the gateway**
2. **Implement rate limiting to protect backend services**
3. **Use health checks to detect unhealthy instances**
4. **Enable distributed tracing for debugging**
5. **Cache responses where appropriate**
6. **Use circuit breakers for fault tolerance**
7. **Log all requests for security and debugging**
8. **Keep gateway stateless for easy scaling**
9. **Version your APIs explicitly**
10. **Implement proper error handling and user-friendly error messages**

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Maintained By**: Platform Team
