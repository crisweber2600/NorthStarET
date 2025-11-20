Feature: Aspire Orchestration & Cross-Cutting Scaffolding
Short Name: aspire-scaffolding
Target Layer: CrossCuttingConcerns
Business Value: Establish a consistent, reproducible scaffolding baseline so every slice (Identity, Student, Assessment, etc.) inherits enforced tenant isolation, diagnostics, messaging, resiliency, and deployment orchestration from day one. Reduces integration friction, accelerates feature delivery, and ensures migration safety under the Strangler Fig strategy.

Scenarios:
Scenario 1: AppHost Boots Full Stack with Dependency Readiness
Given AppHost defines PostgreSQL, Redis, and RabbitMQ resources per constitution
And each microservice declares .WaitFor(postgres).WaitFor(redis) where required
When dotnet run --project Src/Foundation/AppHost is executed
Then all containers start in dependency order
And the Aspire dashboard lists healthy resources
And service logs stream with unified correlation IDs.

Scenario 2: New Microservice Scaffolding in Under 2 Minutes
Given a developer needs to add Intervention Management Service
When they run an internal scaffold script (future automation)
Then a project is created with Application/Domain/Infrastructure/API folders
And DependencyInjection.cs stubs for Application & Infrastructure
And Aspire AppHost references the new service
And baseline tests (unit + BDD feature placeholder + health) are added.

Scenario 3: Tenant Isolation Enforced Automatically
Given TenantInterceptor and global query filters are registered
And every entity has TenantId
When a repository queries Students
Then only rows with current context TenantId are returned
And attempts to override the filter require explicit opt-out with reviewed justification
And audit logging captures any opt-out usage.

Scenario 4: Event Publication on Domain Changes
Given a StudentCreated domain event is raised in Domain layer
When the Application handler commits the transaction
Then Infrastructure publishes an integration event via MassTransit (RabbitMQ locally)
And subscribers receive the event within 500ms P95
And idempotency ensures duplicate publishes inside a 10-minute window are ignored.

Scenario 5: Redis Caching Accelerates Session & Idempotency Lookups
Given Redis resource is provisioned by Aspire
And session validation logic is executed
When the same session is validated multiple times in <5 minutes
Then lookup cost stays <20ms P95 (cache hit)
And fallback to PostgreSQL only occurs on cache miss
And cache entries slide expiration per configured policy.

Scenario 6: Unified Observability – Traces, Metrics, Logs
Given OpenTelemetry is configured in every service
And correlation ID middleware sets X-Correlation-ID
When a request flows API Gateway → Student Service → Message Bus → Assessment Service
Then a single trace appears in dashboard with spans per hop
And logs include correlation scope
And metrics expose request duration, rate, error counts.

Scenario 7: Strangler Fig Legacy Routing Toggle
Given certain endpoints still reside in legacy monolith
And routing rules exist in API Gateway
When configuration flips a feature flag AssessmentService:Migrated=true
Then traffic shifts seamlessly to new microservice
And rollback can occur by toggling flag off
And no client URL changes are required.

Scenario 8: Resilient Messaging with Retry & DLQ
Given MassTransit configured with retry and dead-letter queues
When AssessmentCalculated consumer throws a transient exception
Then message is retried with exponential backoff
And after max attempts it lands in DLQ
And DLQ metrics surface in observability dashboard.

Scenario 9: Onboarding a New Entity with Idempotent Create
Given CreateDistrictCommand includes idempotency key
When a client submits the same payload twice within 10 minutes
Then Redis idempotency envelope deduplicates the second call
And response returns original entity identifier
And audit trail notes idempotent replay.

Scenario 10: Performance Budget Verification at Build Time
Given performance SLO budgets defined (e.g., token exchange <200ms P95)
When integration test suite runs in CI
Then tests assert metrics thresholds via exported OpenTelemetry data
And build fails if budgets are exceeded
And failure artifacts attach Red evidence.

Scenario 11: Multi-Tenant Migration Safety
Given legacy data migration executes into per-service PostgreSQL schemas
When ETL jobs load student records
Then each record is stamped with correct TenantId
And cross-tenant contamination attempts are blocked by RLS
And validation scripts confirm row counts per tenant.

Scenario 12: Rapid Local Developer Feedback Loop
Given developer modifies a command handler
When they run dotnet test followed by dotnet run --project AppHost
Then Aspire restarts only impacted services
And updated spans appear live
And BDD scenario transitions from Red to Green within minutes.

Acceptance Criteria:
1. AppHost boots baseline resources with health signals.
2. New service scaffolding template produces DI, tracing, tests.
3. TenantInterceptor & global filters enforced automatically.
4. Domain events publish with retry/DLQ and idempotency.
5. Redis caching + idempotency service operational with metrics.
6. Unified traces across gateway, services, messaging.
7. Feature flag routing toggles legacy vs new endpoints.
8. Performance budget tests fail when thresholds exceeded.
9. Red→Green evidence captured for initial test suites.
10. Security hooks (token validation, RLS, audit logging) verified.
