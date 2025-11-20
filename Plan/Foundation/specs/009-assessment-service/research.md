Research Notes (Assessment Service)

JSONB vs Join Tables: JSONB selected for flexible custom fields; measured query cost mitigated with projection view; alternative (EAV) rejected due to complexity & performance.
Linear Regression Approach: Simple least squares sufficient (low cardinality). ML forecast deferred.
State Test Format Diversity: Strategy pattern enabling plug-in parsers; avoids large conditional blocks.
Benchmark Computation: Real-time vs precompute. Real-time chosen (lightweight) with optional caching.
Event Volume Projection: Worst-case assignment events (large district) ~50k/day; Azure Service Bus throughput acceptable with batching.
Security Review: Custom field input validation prevents injection into JSONB; rely on EF parameterization.
