Feature: API Gateway: YARP Service Orchestration and Cross-Cutting Concerns
Short Name: api-gateway
Target Layer: CrossCuttingConcerns
Business Value: Provide unified secure entry point for all clients, centralizing authentication, routing, migration (Strangler Fig), rate limiting, observability, and resiliency while decoupling clients from backend service topology.

Scenarios:
Scenario 1: Route Request to New Microservice
Given gateway configured with routes for all services
And Student Management Service deployed and healthy
When client sends GET /api/v1/students/123
Then gateway validates JWT
And routes to Student Management Service
And forwards tenant context
And returns response within 150ms total.

Scenario 2: Route Request to Legacy Monolith During Migration
Given Assessment Service not migrated
And legacy NS4.WebAPI handles assessments
When client sends GET /api/v1/assessments/456
Then gateway routes to legacy service
And translates headers if needed
And client unaware of legacy backend.

Scenario 3: Authentication Validation at Gateway
Given request includes JWT Authorization header
When request arrives at gateway
Then signature, expiration, issuer, audience validated
And tenant_id extracted
And invalid token yields 401
And check completes <20ms P95.

Scenario 4: Rate Limiting by Tenant
Given per-tenant limits configured (District A 1000 req/min)
When District A makes 1001 requests in a minute
Then first 1000 succeed
And 1001st returns 429 with Retry-After
And other tenants unaffected.

Scenario 5: Cross-Origin Resource Sharing (CORS)
Given frontend at https://app.northstar.edu and gateway at https://api.northstar.edu
When browser sends preflight OPTIONS
Then gateway returns configured CORS headers
And browser caches policy for subsequent credentialed requests.

Scenario 6: Request Logging and Correlation IDs
Given client makes request
When request enters gateway
Then correlation ID generated or propagated
And added to downstream calls and response headers
And logs include path, status, duration, correlation_id
And distributed tracing spans correlated.

Scenario 7: Health Check Aggregation
Given gateway monitors downstream health
When GET /health requested
Then gateway aggregates service statuses
And returns 200 if all healthy else 503 with details
And completes <500ms.

Scenario 8: Circuit Breaker for Failing Service
Given Assessment Service timing out
When 5 consecutive failures occur
Then circuit opens for Assessment Service
And subsequent requests return 503 immediately
And attempts recovery after 30s; closes on success.

Scenario 9: Request Transformation and Header Injection
Given downstream needs tenant and user context
When request flows through gateway
Then gateway injects X-Tenant-Id, X-User-Id, X-Correlation-Id
And removes sensitive headers
And downstream relies on these values.

Scenario 10: API Versioning Support
Given API supports v1 and v2
When client requests /api/v1/students
Then gateway routes to v1 service
When client requests /api/v2/students
Then gateway routes to v2
And v1 responds with deprecation sunset header.

Scenario 11: Load Balancing Across Service Instances
Given Student Service has 3 instances
When multiple student requests arrive
Then gateway round-robins evenly
And unhealthy instance removed until healthy again.

Scenario 12: Request Size Limits and Validation
Given request size limit 10MB configured
When client uploads >10MB file
Then gateway returns 413 Payload Too Large
And malformed JSON yields 400 with validation details
And request not forwarded.

Acceptance Criteria:
1. JWT validation central with <20ms P95 overhead.
2. Strangler Fig routing supports legacy and new seamlessly.
3. Per-tenant rate limiting enforced with proper 429 responses.
4. Correlation IDs consistent across distributed trace.
5. Health aggregation endpoint reports granular statuses.
6. Circuit breaker isolates failing services with recovery attempts.
7. Header injection standardizes tenant/user/correlation context.
8. Coexisting versioned APIs with clear deprecation metadata.
9. Load balancing policy (RoundRobin) functioning & resilient.
10. Size limit & validation guard rails stop invalid payloads early.
