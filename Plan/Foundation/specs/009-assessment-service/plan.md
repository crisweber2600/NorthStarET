Technical Plan (Assessment Service) - Foundation Layer

Architecture:
- Clean Architecture (Domain, Application, Infrastructure, API).
- EF Core 9 PostgreSQL schemas: assessment schema, JSONB field_scores.
- MassTransit + Azure Service Bus events.
- Redis: hot cache for assessment definitions, benchmark lookups.
- OpenTelemetry: spans (CreateAssessment, RecordResult, CalculateTrend).

Slices / Vertical Features:
1. Core Model & DbContext (Assessments, Fields, Results, Benchmarks).
2. CreateAssessmentCommand (+ validator) + event publish.
3. AssignAssessmentCommand (batch creation) + event per student.
4. RecordResultCommand: scoring engine (strategy pattern: PointsStrategy, WeightedStrategy, RubricStrategy).
5. Benchmark CRUD & auto classification service.
6. SearchAssessmentsQuery: dynamic filters + index usage; add composite index (tenant_id, subject, assessment_type, created_at DESC).
7. StudentTrendsQuery: regression calculation (simple linear least squares) → consider incremental cache keyed by studentId.
8. StateTestImport: parser strategies (CalpadsParser, PeimsParser) → produce AssessmentResult + import event.
9. Template Library: Template entity & materialization clone utility.
10. Scheduling & Reminders: Background service (HostedService + Cron expressions stored) publishing reminder notifications.
11. Audit Interceptor: EF SaveChanges interceptor capturing before/after snapshots.

Data Migration Strategy:
- Extract legacy tables per district; transform to unified schema; load with COPY.
- Replay benchmark logic to populate benchmarkLevel for historical records (parallel batches).
- Backfill trend materialized view.

Performance Considerations:
- JSONB field_scores flattened via projection view (assessment_result_field_projection) for analytics.
- Trend queries: limit result set via date range param, degrade gracefully > 500 results (return partial + continuation token).

Security:
- Policy-based authorization: CanRecordAssessmentResult (teacher owns student or role admin).
- RLS + tenant claim enforcement middleware verifying session tenant matches header.

Error Handling:
- Scoring engine throws ValidationException for out-of-range field values (returns 400). Logs with category Assessment.Scoring.
- Import errors aggregated; partial success allowed.

Testing Strategy Alignment:
- Unit: scoring strategies, benchmark classification, regression slope.
- Integration: EF migrations, event publishing, RLS enforcement, search filtering.
- BDD: CreateAssessment.feature, RecordResult.feature, Benchmark.feature, Trends.feature.
- UI: Playwright forms & trend dashboard visual checks.

Deployment:
- Aspire: Add project reference, Postgres resource, Redis; health check endpoints /health/ready-liveness.

Observability:
- Metrics: assessment_results_total, assessment_trend_calc_duration_ms histogram.
- Logs: include assessmentId, studentId, tenantId.

Rollout Risks & Mitigations in Plan:
- High volume import → throttle + queue (dedicated background consumer).
- Regression calc hot path → memoize last N results; refresh async.
