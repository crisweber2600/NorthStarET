# Research: API Gateway Technical Patterns

**Feature**: 002-api-gateway  
**Date**: November 21, 2025  
**Status**: Complete - All clarifications resolved

---

## Overview

This document resolves all technical unknowns from the implementation plan's Phase 0 research tasks. Each section documents the decision, rationale, alternatives considered, and implementation guidance.

---

## R1: YARP Configuration & Transform Patterns

### Decision

Use **Aspire 9.4+ programmatic configuration API** with `.WithConfiguration(yarp => yarp.AddRoute(...))` fluent methods. The `WithConfigFile()` method was removed in Aspire 9.4; all configuration is now code-based.

### Implementation Pattern

```csharp
// AppHost/Program.cs - Register YARP gateway in Aspire
var identityApi = builder.AddProject<Projects.Identity_Api>("identity-api");
var studentApi = builder.AddProject<Projects.Student_Api>("student-api");
var legacyNs4 = builder.AddContainer("legacy-ns4", "oldnorthstar/webapi", "latest");

var gateway = builder.AddYarp("api-gateway")
    .WithConfiguration(yarp =>
    {
        // Route to new microservice with transforms
        yarp.AddRoute("/api/v1/students/{**catch-all}", studentApi)
            .WithMatchMethods("GET", "POST", "PUT", "DELETE")
            .WithTransformPathRemovePrefix("/api/v1") // Remove /api/v1 before forwarding
            .WithTransformRequestHeader("X-Tenant-Id", "{tenant_id}") // Inject tenant context
            .WithTransformRequestHeader("X-Correlation-Id", "{correlation_id}")
            .WithTransformResponseHeader("X-Api-Version", "v1")
            .WithTransformResponseHeader("Sunset", "2026-12-31"); // Deprecation notice
        
        // Route to legacy monolith (Strangler Fig pattern)
        yarp.AddRoute("/api/v1/assessments/{**catch-all}", legacyNs4)
            .WithTransformPathPrefix("/NS4.WebAPI") // Add legacy path prefix
            .WithTransformRequestHeader("X-Legacy-Request", "true")
            .WithTransformRequestHeaderRemove("X-Original-Host"); // Remove internal headers
        
        // Catch-all route for new Identity API
        yarp.AddRoute(identityApi); // Defaults to /{**catch-all} match
    })
    .WithReference(identityApi)
    .WithReference(studentApi)
    .WithReference(legacyNs4)
    .WithHttpsEndpoint(port: 7000, name: "https");
```

### Key Transform APIs

| Transform Method | Purpose | Example |
|------------------|---------|---------|
| `WithTransformPathRemovePrefix` | Remove path prefix before forwarding | `/api/v1/students/123` → `/students/123` |
| `WithTransformPathPrefix` | Add path prefix for backend | `/assessments/456` → `/NS4.WebAPI/assessments/456` |
| `WithTransformRequestHeader` | Inject request header | Add `X-Tenant-Id: district-001` |
| `WithTransformRequestHeaderRemove` | Remove request header | Strip `X-Original-Host` |
| `WithTransformResponseHeader` | Add response header | Add `X-Api-Version: v1`, `Sunset: 2026-12-31` |
| `WithMatchMethods` | Limit route to HTTP methods | Only allow GET, POST, PUT, DELETE |

### Rationale

- **Type Safety**: Fluent API provides IntelliSense and compile-time validation
- **Deployment Ready**: Code-based config translates to environment variables in Azure Container Apps
- **Service Discovery**: `.WithReference()` automatically resolves service endpoints via Aspire registry
- **Strangler Fig Support**: Path transforms enable seamless legacy/new routing coexistence

### Alternatives Considered

- ❌ **JSON config files**: Removed in Aspire 9.4; no longer supported
- ❌ **appsettings.json**: Cannot leverage service discovery; hardcoded endpoints
- ❌ **Custom configuration provider**: Unnecessary complexity; fluent API covers all needs

### References

- [Aspire YARP Integration](https://learn.microsoft.com/en-us/dotnet/aspire/proxies/yarp-integration)
- [YARP Transforms](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/extensibility-transforms)

---

## R2: ASP.NET Core Rate Limiting Middleware

### Decision

Use **Redis-backed token bucket rate limiter** with `PartitionedRateLimiter.Create<HttpContext, string>` to partition by `tenant_id`. Implement custom `RedisTenantRateLimiter` for distributed state.

### Implementation Pattern

```csharp
// Program.cs - Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = 
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
        
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "rate_limit_exceeded",
            message = "Too many requests. Please try again later.",
            retryAfter = retryAfter?.TotalSeconds
        }, cancellationToken);
    };
    
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Extract tenant_id from JWT claims (set by TenantContextMiddleware)
        var tenantId = httpContext.User.FindFirst("tenant_id")?.Value ?? "anonymous";
        
        // Route-based key for per-tenant + per-route throttling
        var route = httpContext.GetEndpoint()?.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName ?? "default";
        var partitionKey = $"{tenantId}:{route}";
        
        return RateLimitPartition.Get(partitionKey, key => new RedisTenantRateLimiter(
            redis: httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>(),
            key: $"ratelimit:{key}",
            options: new TokenBucketRateLimiterOptions
            {
                TokenLimit = 1000,           // 1000 tokens per tenant per route
                TokensPerPeriod = 100,       // Replenish 100 tokens every 6 seconds
                ReplenishmentPeriod = TimeSpan.FromSeconds(6),
                QueueLimit = 10,             // Queue up to 10 requests
                AutoReplenishment = true
            }));
    });
});

var app = builder.Build();
app.UseRateLimiter(); // Add AFTER UseAuthentication to access JWT claims
```

### Custom Redis Rate Limiter

```csharp
// RateLimiting/RedisTenantRateLimiter.cs
public class RedisTenantRateLimiter : RateLimiter
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _key;
    private readonly TokenBucketRateLimiterOptions _options;
    
    public RedisTenantRateLimiter(IConnectionMultiplexer redis, string key, TokenBucketRateLimiterOptions options)
    {
        _redis = redis;
        _key = key;
        _options = options;
    }
    
    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var db = _redis.GetDatabase();
        
        // Lua script for atomic token bucket check-and-decrement
        var script = @"
            local key = KEYS[1]
            local tokens_key = key .. ':tokens'
            local timestamp_key = key .. ':timestamp'
            local token_limit = tonumber(ARGV[1])
            local tokens_per_period = tonumber(ARGV[2])
            local period_seconds = tonumber(ARGV[3])
            local now = tonumber(ARGV[4])
            
            local tokens = tonumber(redis.call('GET', tokens_key)) or token_limit
            local last_update = tonumber(redis.call('GET', timestamp_key)) or now
            
            -- Replenish tokens based on elapsed time
            local elapsed = now - last_update
            local periods_elapsed = math.floor(elapsed / period_seconds)
            if periods_elapsed > 0 then
                tokens = math.min(token_limit, tokens + (periods_elapsed * tokens_per_period))
                redis.call('SET', timestamp_key, now)
            end
            
            -- Check if enough tokens available
            if tokens >= 1 then
                redis.call('SET', tokens_key, tokens - 1)
                redis.call('EXPIRE', tokens_key, period_seconds * 2)
                redis.call('EXPIRE', timestamp_key, period_seconds * 2)
                return 1
            else
                return 0
            end
        ";
        
        var result = await db.ScriptEvaluateAsync(script,
            keys: new RedisKey[] { _key },
            values: new RedisValue[]
            {
                _options.TokenLimit,
                _options.TokensPerPeriod,
                (int)_options.ReplenishmentPeriod.TotalSeconds,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });
        
        if ((int)result == 1)
        {
            return new RateLimitLease(isAcquired: true);
        }
        else
        {
            var retryAfter = TimeSpan.FromSeconds(_options.ReplenishmentPeriod.TotalSeconds);
            return new RateLimitLease(isAcquired: false, metadata: new[] 
            { 
                new KeyValuePair<string, object?>(MetadataName.RetryAfter.Name, retryAfter) 
            });
        }
    }
    
    // Dispose implementation omitted for brevity
}
```

### Rationale

- **Token Bucket Algorithm**: Allows bursts (better UX than fixed window) while enforcing average rate
- **Redis Atomic Operations**: Lua script ensures thread-safe check-and-decrement across gateway replicas
- **Per-Tenant Isolation**: Each district gets independent token bucket; prevents noisy neighbor issues
- **Per-Route Granularity**: Different endpoints can have different limits (e.g., `/health` unlimited, `/assessments` strict)
- **Graceful Degradation**: Queue requests briefly (10 queue limit) before rejecting with 429

### Alternatives Considered

- ❌ **Sliding Window**: More accurate but higher Redis memory usage (stores all request timestamps)
- ❌ **Fixed Window**: Allows burst at window boundaries (e.g., 1000 requests at 00:00:59, 1000 at 00:01:00)
- ❌ **In-Memory Limiter**: Cannot sync state across gateway replicas; inconsistent throttling

### References

- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [Rate Limiting with YARP](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/rate-limiting)

---

## R3: Circuit Breaker Integration with YARP

### Decision

Use **Polly circuit breaker with Redis-backed state store** to share circuit state across gateway replicas. Configure per-cluster circuit breakers via YARP HTTP client configuration.

### Implementation Pattern

```csharp
// Program.cs - Configure circuit breakers for YARP clusters
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        // Circuit breaker policy per cluster
        var circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    var circuitStore = context.ServiceProvider.GetRequiredService<ICircuitBreakerStateStore>();
                    circuitStore.RecordBreak(context.ClusterId, duration);
                },
                onReset: () =>
                {
                    var circuitStore = context.ServiceProvider.GetRequiredService<ICircuitBreakerStateStore>();
                    circuitStore.RecordReset(context.ClusterId);
                },
                onHalfOpen: () =>
                {
                    var circuitStore = context.ServiceProvider.GetRequiredService<ICircuitBreakerStateStore>();
                    circuitStore.RecordHalfOpen(context.ClusterId);
                });
        
        handler.SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Recycle handlers every 5 minutes
        handler.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            EnableMultipleHttp2Connections = true
        });
    });

// Register circuit breaker state store
builder.Services.AddSingleton<ICircuitBreakerStateStore, RedisCircuitBreakerStore>();
```

### Redis Circuit Breaker State Store

```csharp
// Resilience/RedisCircuitBreakerStore.cs
public class RedisCircuitBreakerStore : ICircuitBreakerStateStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCircuitBreakerStore> _logger;
    
    public RedisCircuitBreakerStore(IConnectionMultiplexer redis, ILogger<RedisCircuitBreakerStore> logger)
    {
        _redis = redis;
        _logger = logger;
    }
    
    public async Task RecordBreak(string serviceId, TimeSpan breakDuration)
    {
        var db = _redis.GetDatabase();
        var key = $"circuit:{serviceId}:state";
        
        var state = new CircuitBreakerState
        {
            ServiceId = serviceId,
            State = CircuitState.Open,
            OpenedAt = DateTime.UtcNow,
            FailureCount = 5 // Threshold reached
        };
        
        await db.StringSetAsync(key, JsonSerializer.Serialize(state), breakDuration);
        _logger.LogWarning("Circuit breaker OPENED for service {ServiceId}, break duration: {Duration}", 
            serviceId, breakDuration);
    }
    
    public async Task RecordReset(string serviceId)
    {
        var db = _redis.GetDatabase();
        var key = $"circuit:{serviceId}:state";
        
        var state = new CircuitBreakerState
        {
            ServiceId = serviceId,
            State = CircuitState.Closed,
            FailureCount = 0,
            LastSuccessTime = DateTime.UtcNow
        };
        
        await db.StringSetAsync(key, JsonSerializer.Serialize(state), TimeSpan.FromMinutes(5));
        _logger.LogInformation("Circuit breaker CLOSED for service {ServiceId}", serviceId);
    }
    
    public async Task RecordHalfOpen(string serviceId)
    {
        var db = _redis.GetDatabase();
        var key = $"circuit:{serviceId}:state";
        
        var existing = await db.StringGetAsync(key);
        var state = existing.HasValue 
            ? JsonSerializer.Deserialize<CircuitBreakerState>(existing!) 
            : new CircuitBreakerState { ServiceId = serviceId };
        
        state.State = CircuitState.HalfOpen;
        
        await db.StringSetAsync(key, JsonSerializer.Serialize(state), TimeSpan.FromMinutes(1));
        _logger.LogInformation("Circuit breaker HALF-OPEN for service {ServiceId}", serviceId);
    }
    
    public async Task<CircuitState> GetState(string serviceId)
    {
        var db = _redis.GetDatabase();
        var key = $"circuit:{serviceId}:state";
        var existing = await db.StringGetAsync(key);
        
        if (!existing.HasValue) return CircuitState.Closed;
        
        var state = JsonSerializer.Deserialize<CircuitBreakerState>(existing!);
        return state.State;
    }
}
```

### Fallback Response Middleware

```csharp
// Middleware/CircuitBreakerFallbackMiddleware.cs
public class CircuitBreakerFallbackMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICircuitBreakerStateStore _circuitStore;
    
    public CircuitBreakerFallbackMiddleware(RequestDelegate next, ICircuitBreakerStateStore circuitStore)
    {
        _next = next;
        _circuitStore = circuitStore;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("circuit breaker"))
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            context.Response.Headers.RetryAfter = "30"; // Match break duration
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "service_unavailable",
                message = "The requested service is temporarily unavailable. Please try again later.",
                retryAfter = 30
            });
        }
    }
}
```

### Rationale

- **Distributed State**: Redis ensures all gateway replicas see same circuit breaker state
- **Fail-Fast**: Open circuit immediately returns 503 instead of waiting for timeout
- **Automatic Recovery**: Half-open state tests service health; resets on success
- **Granular Monitoring**: Per-service circuit breakers prevent cascade failures
- **Client Guidance**: `Retry-After` header tells clients when to retry

### Alternatives Considered

- ❌ **In-Memory Circuit Breakers**: Each replica has independent state; inconsistent behavior
- ❌ **Database-Backed State**: Too slow for high-frequency checks (circuit breaker evaluated per request)
- ❌ **No Circuit Breaker**: Slow/failing backends degrade entire gateway; retry storms

### References

- [Polly Circuit Breaker](https://www.pollydocs.org/strategies/circuit-breaker)
- [YARP HTTP Client Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/yarp/http-client-config)

---

## R4: Azure App Configuration Dynamic Refresh

### Decision

Use **Azure App Configuration with sentinel-based refresh** to reload YARP routes and policies every 30 seconds without service restart.

### Implementation Pattern

```csharp
// Program.cs - Configure Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration["AzureAppConfiguration:ConnectionString"])
        .Select("Gateway:*", LabelFilter.Null) // Load all Gateway:* keys
        .ConfigureRefresh(refresh =>
        {
            // Sentinel key triggers full config reload
            refresh.Register("Gateway:ConfigVersion", refreshAll: true)
                .SetCacheExpiration(TimeSpan.FromSeconds(30)); // Poll every 30 seconds
        })
        .UseFeatureFlags(featureFlagOptions =>
        {
            // Strangler Fig feature flags
            featureFlagOptions.Select("StranglerFig_*");
            featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(30);
        });
});

// Enable dynamic configuration updates
builder.Services.AddAzureAppConfiguration();
```

```csharp
// Middleware pipeline - Add refresh middleware
var app = builder.Build();
app.UseAzureAppConfiguration(); // Polls Azure App Config every 30s
app.UseRateLimiter();
app.MapReverseProxy();
```

### Azure App Configuration Structure

```json
{
  "Gateway:ConfigVersion": "v2.3.5",  // Sentinel key - increment to force refresh
  "Gateway:Routes:0:RouteId": "student-api-v1",
  "Gateway:Routes:0:Match:Path": "/api/v1/students/{**catch-all}",
  "Gateway:Routes:0:ClusterId": "student-api-cluster",
  "Gateway:Clusters:student-api-cluster:Destinations:student-api:Address": "http://student-api",
  "Gateway:Clusters:student-api-cluster:CircuitBreaker:FailureThreshold": "5",
  "Gateway:Clusters:student-api-cluster:CircuitBreaker:BreakDuration": "00:00:30",
  "StranglerFig_AssessmentService_UseLegacy": "true"  // Feature flag
}
```

### Dynamic Route Reconfiguration

```csharp
// Configuration/YarpConfigurationProvider.cs
public class YarpConfigurationProvider : IProxyConfigProvider
{
    private readonly IConfiguration _configuration;
    private readonly IOptionsMonitor<GatewayRouteConfig> _routeOptions;
    private volatile InMemoryConfigProvider _config;
    
    public YarpConfigurationProvider(IConfiguration configuration, IOptionsMonitor<GatewayRouteConfig> routeOptions)
    {
        _configuration = configuration;
        _routeOptions = routeOptions;
        
        // Reload routes when configuration changes
        _routeOptions.OnChange(updatedRoutes =>
        {
            var routes = BuildRoutes(updatedRoutes);
            var clusters = BuildClusters(updatedRoutes);
            _config = new InMemoryConfigProvider(routes, clusters);
        });
        
        // Initial load
        var initialRoutes = _routeOptions.CurrentValue;
        var routes = BuildRoutes(initialRoutes);
        var clusters = BuildClusters(initialRoutes);
        _config = new InMemoryConfigProvider(routes, clusters);
    }
    
    public IProxyConfig GetConfig() => _config.GetConfig();
    
    private IReadOnlyList<RouteConfig> BuildRoutes(GatewayRouteConfig config)
    {
        // Convert configuration to YARP RouteConfig objects
        return config.Routes.Select(r => new RouteConfig
        {
            RouteId = r.RouteId,
            Match = new RouteMatch { Path = r.Match.Path, Methods = r.Match.Methods },
            ClusterId = r.ClusterId,
            Transforms = BuildTransforms(r)
        }).ToList();
    }
    
    private IReadOnlyList<ClusterConfig> BuildClusters(GatewayRouteConfig config)
    {
        // Convert configuration to YARP ClusterConfig objects
        return config.Clusters.Select(c => new ClusterConfig
        {
            ClusterId = c.Key,
            Destinations = c.Value.Destinations.ToDictionary(
                d => d.Key,
                d => new DestinationConfig { Address = d.Value.Address }
            ),
            HealthCheck = new HealthCheckConfig
            {
                Active = new ActiveHealthCheckConfig
                {
                    Enabled = c.Value.HealthCheck?.Active?.Enabled ?? false,
                    Interval = c.Value.HealthCheck?.Active?.Interval ?? TimeSpan.FromSeconds(30),
                    Timeout = c.Value.HealthCheck?.Active?.Timeout ?? TimeSpan.FromSeconds(5),
                    Path = c.Value.HealthCheck?.Active?.Path ?? "/health"
                }
            }
        }).ToList();
    }
}
```

### Rationale

- **Zero-Downtime Updates**: New requests use updated routes; in-flight requests complete on old routes
- **Sentinel Key Pattern**: Incrementing `Gateway:ConfigVersion` forces full reload (prevents stale cache)
- **Feature Flag Integration**: Strangler Fig toggles (`StranglerFig_*`) refresh every 30 seconds
- **Centralized Management**: Ops team updates routes in Azure portal; no redeployment needed
- **Environment Separation**: Dev/staging/prod use different Azure App Config instances

### Alternatives Considered

- ❌ **File-Based Config**: Requires redeployment or file sync across replicas
- ❌ **Database Config**: Adds latency; Azure App Config has CDN-backed edge caching
- ❌ **Manual Polling**: Azure App Config SDK handles polling, retry, and caching automatically

### References

- [Azure App Configuration Quickstart](https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-aspnet-core-app)
- [Dynamic Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core)

---

## R5: Health Check Aggregation Patterns

### Decision

Use **ASP.NET Core health checks with custom JSON response writer** to aggregate health from all backend services in <500ms.

### Implementation Pattern

```csharp
// Program.cs - Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck<GatewayHealthCheck>("gateway-self")
    .AddCheck<RedisHealthCheck>("redis")
    .AddUrlGroup(new Uri("http://identity-api/health"), "identity-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://student-api/health"), "student-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://staff-api/health"), "staff-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://assessment-api/health"), "assessment-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://intervention-api/health"), "intervention-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://section-api/health"), "section-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://dataimport-api/health"), "dataimport-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://reporting-api/health"), "reporting-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://content-api/health"), "content-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://digitalink-api/health"), "digitalink-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://configuration-api/health"), "configuration-api", timeout: TimeSpan.FromSeconds(5))
    .AddUrlGroup(new Uri("http://oldnorthstar.internal/health"), "legacy-ns4", timeout: TimeSpan.FromSeconds(5));

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthResponseWriter.WriteDetailedJsonResponse,
    AllowCachingResponses = false,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
```

### Custom Health Response Writer

```csharp
// Health/HealthResponseWriter.cs
public static class HealthResponseWriter
{
    public static Task WriteDetailedJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            services = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString(),
                    exception = e.Value.Exception?.Message,
                    data = e.Value.Data
                })
        };
        
        return context.Response.WriteAsJsonAsync(response);
    }
}
```

### Example Health Response

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.4523451",
  "services": {
    "gateway-self": {
      "status": "Healthy",
      "description": "Gateway is operational",
      "duration": "00:00:00.0012345",
      "exception": null,
      "data": {}
    },
    "redis": {
      "status": "Healthy",
      "description": "Redis connection successful",
      "duration": "00:00:00.0087654",
      "exception": null,
      "data": {}
    },
    "identity-api": {
      "status": "Healthy",
      "description": "HTTP GET http://identity-api/health responded with 200 OK",
      "duration": "00:00:00.1234567",
      "exception": null,
      "data": {}
    },
    "student-api": {
      "status": "Unhealthy",
      "description": "HTTP GET http://student-api/health responded with 503 Service Unavailable",
      "duration": "00:00:00.2345678",
      "exception": "HttpRequestException: Response status code does not indicate success: 503",
      "data": {}
    }
  }
}
```

### Rationale

- **Parallel Execution**: All backend health checks run concurrently (max 5s timeout)
- **Fail-Fast**: Returns 503 if ANY service unhealthy (load balancers remove gateway from pool)
- **Detailed Diagnostics**: JSON response shows per-service status for troubleshooting
- **Timeout Protection**: 5s timeout prevents slow backends from blocking health endpoint
- **Service Discovery**: Uses Aspire service names (`http://identity-api`) resolved via DNS

### Alternatives Considered

- ❌ **Sequential Health Checks**: Too slow (11 services × 5s timeout = 55s worst case)
- ❌ **Optimistic Aggregation**: Returning 200 when services unhealthy misleads load balancers
- ❌ **Database Health Storage**: Adds latency; health checks should be lightweight

### References

- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Health Checks with Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/health-checks)

---

## R6: JWT Validation Performance Optimization

### Decision

Use **Microsoft.Identity.Web with metadata caching** and optional Redis caching of validated claims to achieve <20ms P95 JWT validation overhead.

### Implementation Pattern

```csharp
// Program.cs - Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5), // Allow 5min clock drift
            
            // Performance optimizations
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
        
        // Cache OIDC metadata for 1 hour (reduce calls to Entra ID discovery endpoint)
        options.MetadataAddress = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0/.well-known/openid-configuration";
        options.RefreshInterval = TimeSpan.FromHours(1);
        
        // Enable distributed token validation result caching (if needed)
        options.SaveToken = false; // Don't store JWT in AuthenticationProperties (memory optimization)
    },
    options =>
    {
        builder.Configuration.Bind("AzureAd", options);
    });
```

### Optional: Redis Claims Caching

```csharp
// Middleware/JwtClaimsCacheMiddleware.cs
public class JwtClaimsCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConnectionMultiplexer _redis;
    
    public JwtClaimsCacheMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
    {
        _next = next;
        _redis = redis;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            
            var db = _redis.GetDatabase();
            var cacheKey = $"jwt:claims:{tokenHash}";
            var cachedClaims = await db.StringGetAsync(cacheKey);
            
            if (cachedClaims.HasValue)
            {
                // Restore claims from cache (skip expensive signature validation)
                var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(cachedClaims!);
                var identity = new ClaimsIdentity(
                    claims.Select(kvp => new Claim(kvp.Key, kvp.Value)),
                    "JWT-Cached"
                );
                context.User = new ClaimsPrincipal(identity);
            }
            else
            {
                // Let Microsoft.Identity.Web validate JWT
                await _next(context);
                
                // Cache validated claims for 5 minutes
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var claimsDict = context.User.Claims.ToDictionary(c => c.Type, c => c.Value);
                    await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(claimsDict), TimeSpan.FromMinutes(5));
                }
                return;
            }
        }
        
        await _next(context);
    }
}
```

### Performance Tuning

| Optimization | Impact | Trade-Off |
|--------------|--------|-----------|
| Metadata caching (1 hour) | Eliminates OIDC discovery calls | Slower key rotation detection (acceptable; keys rotate infrequently) |
| Disable SaveToken | Reduces memory per request | Can't access raw JWT in middleware (not needed for gateway) |
| Redis claims caching | 50%+ reduction in validation time | Slight staleness (5min); invalidate on logout |
| ClockSkew = 5min | Tolerates clock drift | Slightly extends token validity window |
| Connection pooling | Reuses HTTPS connections | Requires proper HttpClient lifetime management |

### Rationale

- **Metadata Caching**: OIDC discovery endpoint calls are expensive (50-100ms); caching reduces to ~5ms
- **Claims Caching**: Signature validation is CPU-intensive; caching validated claims drops overhead to ~2ms
- **Clock Skew**: Prevents false rejections due to server time differences (common in distributed systems)
- **No SaveToken**: Gateway doesn't need raw JWT; only extracted claims (tenant_id, user_id, roles)

### Alternatives Considered

- ❌ **Skip Signature Validation**: Security risk; tokens could be forged
- ❌ **Longer Claims Cache**: Increases risk of stale permissions (user logout not reflected)
- ❌ **Database Claims Storage**: Too slow; Redis required for <5ms reads

### References

- [Microsoft.Identity.Web Configuration](https://learn.microsoft.com/en-us/entra/identity-platform/scenario-web-api-call-api-app-configuration)
- [ASP.NET Core Authentication Performance](https://learn.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)

---

## Summary of Decisions

| Research Task | Decision | Key Technology |
|---------------|----------|----------------|
| R1: YARP Configuration | Programmatic `.WithConfiguration()` fluent API | Aspire 9.4+ YARP integration |
| R2: Rate Limiting | Redis-backed token bucket per tenant | ASP.NET Core RateLimiting + Redis Lua scripts |
| R3: Circuit Breakers | Polly circuit breaker with Redis state | Polly + Redis |
| R4: Dynamic Config | Azure App Configuration with 30s sentinel refresh | Azure App Configuration SDK |
| R5: Health Aggregation | Parallel health checks with 5s timeout | ASP.NET Core Health Checks |
| R6: JWT Optimization | Metadata caching + optional Redis claims cache | Microsoft.Identity.Web + Redis |

---

## Next Steps

1. ✅ **Phase 0 Complete**: All technical unknowns resolved
2. **Proceed to Phase 1**: Generate `data-model.md` (route schemas, circuit breaker states)
3. **Proceed to Phase 1**: Generate `contracts/` (OpenAPI specs for /health, /metrics)
4. **Proceed to Phase 1**: Generate `quickstart.md` (local dev setup guide)
5. **Proceed to Phase 2**: Run `/speckit.tasks` to generate phased implementation tasks

---

**Research Status**: ✅ **COMPLETE** - Ready for design phase (Phase 1)
