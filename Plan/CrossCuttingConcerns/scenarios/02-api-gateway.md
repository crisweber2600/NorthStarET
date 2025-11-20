# API Gateway: YARP Service Orchestration and Cross-Cutting Concerns

**Service**: API Gateway (YARP)  
**Pattern**: Backend-for-Frontend (BFF), Strangler Fig  
**Architecture Reference**: [API Gateway Configuration](../standards/api-gateway-config.md)  
**Business Value**: Unified entry point, authentication, rate limiting, request routing

---

## Scenario 1: Route Request to New Microservice

**Given** the API Gateway is configured with routes for all services  
**And** the Student Management Service is deployed and healthy  
**When** a client sends `GET /api/v1/students/123`  
**Then** the API Gateway receives the request  
**And** validates the JWT token with Identity Service  
**And** routes the request to Student Management Service  
**And** forwards the tenant context from the token  
**And** returns the response to the client  
**And** the entire flow completes within 150ms

---

## Scenario 2: Route Request to Legacy Monolith During Migration

**Given** the Assessment Service is not yet migrated  
**And** legacy NS4.WebAPI handles assessment endpoints  
**When** a client sends `GET /api/v1/assessments/456`  
**Then** the API Gateway routes to legacy NS4.WebAPI  
**And** translates any header/token formats if needed  
**And** returns the legacy response  
**And** the client is unaware it's calling legacy vs. new service

---

## Scenario 3: Authentication Validation at Gateway

**Given** a request includes a JWT token in the Authorization header  
**When** the request arrives at the API Gateway  
**Then** the gateway validates the token signature  
**And** checks token expiration  
**And** validates the issuer and audience claims  
**And** extracts tenant_id from claims  
**And** if valid, forwards request with tenant context  
**And** if invalid, returns HTTP 401 Unauthorized  
**And** authentication check completes within 20ms

---

## Scenario 4: Rate Limiting by Tenant

**Given** the API Gateway has rate limits configured per tenant  
**And** District A is limited to 1000 requests/minute  
**When** District A makes 1001 requests in one minute  
**Then** the first 1000 requests are processed normally  
**And** the 1001st request is rejected with HTTP 429 Too Many Requests  
**And** the response includes Retry-After header  
**And** the rate limit resets after one minute  
**And** other districts are unaffected by District A's limit

---

## Scenario 5: Cross-Origin Resource Sharing (CORS)

**Given** the frontend is hosted at `https://app.northstar.edu`  
**And** the API Gateway is at `https://api.northstar.edu`  
**When** the browser makes a preflight OPTIONS request  
**Then** the gateway responds with CORS headers:
  - Access-Control-Allow-Origin: https://app.northstar.edu
  - Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH
  - Access-Control-Allow-Headers: Authorization, Content-Type
  - Access-Control-Max-Age: 86400
**And** the browser caches the CORS policy  
**And** subsequent requests include credentials

---

## Scenario 6: Request Logging and Correlation IDs

**Given** a client makes a request  
**When** the request enters the API Gateway  
**Then** a correlation ID is generated (or extracted from X-Correlation-ID header)  
**And** the correlation ID is added to all downstream service calls  
**And** the request is logged with: timestamp, path, method, status, duration, correlation_id  
**And** all services use the same correlation ID for logging  
**And** the correlation ID is returned in response headers  
**And** distributed tracing is enabled across all services

---

## Scenario 7: Health Check Aggregation

**Given** the API Gateway monitors health of all downstream services  
**When** a monitoring system queries `GET /health`  
**Then** the gateway checks health of: Identity, Student, Staff, Assessment, etc.  
**And** aggregates the health status  
**And** returns HTTP 200 if all services are healthy  
**And** returns HTTP 503 if any critical service is unhealthy  
**And** includes details about each service's health status  
**And** the health check completes within 500ms

---

## Scenario 8: Circuit Breaker for Failing Service

**Given** the Assessment Service is experiencing issues  
**And** requests to Assessment Service are timing out  
**When** 5 consecutive requests fail  
**Then** the circuit breaker opens for Assessment Service  
**And** subsequent requests immediately return HTTP 503 Service Unavailable  
**And** the circuit breaker attempts recovery after 30 seconds  
**And** if recovery succeeds, the circuit closes  
**And** if recovery fails, the circuit remains open  
**And** other services are unaffected by the circuit breaker

---

## Scenario 9: Request Transformation and Header Injection

**Given** downstream services need tenant context  
**When** a request flows through the gateway  
**Then** the gateway extracts tenant_id from JWT claims  
**And** injects `X-Tenant-Id` header for downstream services  
**And** injects `X-User-Id` header with user identifier  
**And** injects `X-Correlation-Id` for request tracing  
**And** removes sensitive headers before forwarding  
**And** downstream services can rely on these headers

---

## Scenario 10: API Versioning Support

**Given** the API supports both v1 and v2 endpoints  
**When** a client requests `/api/v1/students`  
**Then** the gateway routes to v1 Student Service  
**When** a client requests `/api/v2/students`  
**Then** the gateway routes to v2 Student Service  
**And** both versions can coexist during migration  
**And** v1 is deprecated with sunset date in response headers  
**And** clients are encouraged to migrate to v2

---

## Scenario 11: Load Balancing Across Service Instances

**Given** the Student Service has 3 instances running  
**And** the instances are at: student-1, student-2, student-3  
**When** multiple requests arrive for student data  
**Then** the gateway uses round-robin load balancing  
**And** distributes requests evenly across all instances  
**And** monitors instance health continuously  
**And** removes unhealthy instances from the pool  
**And** reintroduces instances when they recover

---

## Scenario 12: Request Size Limits and Validation

**Given** the gateway has request size limits configured  
**When** a client uploads a file larger than 10MB  
**Then** the gateway rejects the request with HTTP 413 Payload Too Large  
**And** returns an error message about the size limit  
**When** a client sends malformed JSON  
**Then** the gateway rejects with HTTP 400 Bad Request  
**And** includes validation error details  
**And** the request never reaches downstream services

---

## Related Architecture

**Gateway Configuration**: [API Gateway Configuration Standard](../standards/api-gateway-config.md)  
**Observability**: [Observability Pattern](../patterns/observability.md)  
**Aspire Integration**: [Aspire Orchestration](../patterns/aspire-orchestration.md)  
**API Contracts**: [API Contracts Specification](../standards/API_CONTRACTS_SPECIFICATION.md)

---

## Technical Implementation Notes

**YARP Configuration**:
```json
{
  "ReverseProxy": {
    "Routes": {
      "students-route": {
        "ClusterId": "students-cluster",
        "Match": {
          "Path": "/api/v1/students/{**catch-all}"
        },
        "Transforms": [
          { "RequestHeader": "X-Tenant-Id", "Set": "{tenant_id}" },
          { "RequestHeader": "X-User-Id", "Set": "{user_id}" }
        ]
      },
      "legacy-route": {
        "ClusterId": "legacy-cluster",
        "Match": {
          "Path": "/api/v1/assessments/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "students-cluster": {
        "Destinations": {
          "destination1": { "Address": "http://students-api:8080" },
          "destination2": { "Address": "http://students-api-2:8080" }
        },
        "HealthCheck": {
          "Active": {
            "Enabled": true,
            "Interval": "00:00:30",
            "Path": "/health"
          }
        },
        "LoadBalancingPolicy": "RoundRobin"
      },
      "legacy-cluster": {
        "Destinations": {
          "legacy": { "Address": "http://ns4-webapi:5000" }
        }
      }
    }
  }
}
```

**Authentication Middleware**:
```csharp
app.Use(async (context, next) =>
{
    // Extract and validate JWT
    var token = context.Request.Headers["Authorization"]
        .FirstOrDefault()?.Split(" ").Last();
    
    if (string.IsNullOrEmpty(token))
    {
        context.Response.StatusCode = 401;
        return;
    }
    
    var principal = await _tokenValidator.ValidateAsync(token);
    if (principal == null)
    {
        context.Response.StatusCode = 401;
        return;
    }
    
    // Extract claims
    var tenantId = principal.FindFirst("tenant_id")?.Value;
    var userId = principal.FindFirst("sub")?.Value;
    
    // Add to request headers for downstream
    context.Request.Headers["X-Tenant-Id"] = tenantId;
    context.Request.Headers["X-User-Id"] = userId;
    
    await next();
});
```

**Rate Limiting**:
```csharp
services.AddRateLimiter(options =>
{
    options.AddPolicy("per-tenant", context =>
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"];
        
        return RateLimitPartition.GetFixedWindowLimiter(
            tenantId,
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 1000
            });
    });
});
```

**Circuit Breaker**:
```csharp
services.AddHttpClient("AssessmentService")
    .AddPolicyHandler(Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30)));
```

**Correlation ID Middleware**:
```csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"]
        .FirstOrDefault() ?? Guid.NewGuid().ToString();
    
    context.Request.Headers["X-Correlation-ID"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;
    
    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId
    }))
    {
        await next();
    }
});
```

**Performance Targets**:
- Authentication validation: <20ms (P95)
- Routing decision: <10ms (P95)
- Total gateway overhead: <50ms (P95)
- Health check aggregation: <500ms
- Circuit breaker decision: <1ms

**Security Requirements**:
- TLS 1.3 only
- JWT signature validation (RS256)
- Token expiration checks
- Rate limiting per tenant
- Request size limits
- Header sanitization
- CORS policy enforcement
- SQL injection prevention in query params
