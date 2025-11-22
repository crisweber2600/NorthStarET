# Clarification Decisions — 2025-11-21

Automatic run of `@speckit.clarify` applied the recommended answers below.

## 1. Feature flag provider for Strangler Fig routing
- **Options**
  - **A.** Azure App Configuration feature flags with App Configuration SDK refresh.
  - **B.** LaunchDarkly SaaS workspace.
  - **C.** Custom PostgreSQL toggle table polled by the gateway.
- **Decision**: **A** — aligns with existing Azure footprint, gives built-in feature flag semantics, and supports per-environment controls with minimal ops overhead.

## 2. Observability sink for OpenTelemetry signals
- **Options**
  - **A.** Azure Monitor/Application Insights via OTLP exporter.
  - **B.** Self-hosted Grafana stack (Tempo/Loki/Prometheus).
  - **C.** AWS X-Ray / CloudWatch.
  - **D.** Elastic Observability (Elastic APM).
- **Decision**: **A** — keeps telemetry in the same Azure subscription as Aspire resources, simplifies alerting, and avoids owning additional infrastructure.

## 3. Event publication reliability pattern
- **Options**
  - **A.** Transactional outbox table in PostgreSQL with MassTransit dispatcher.
  - **B.** Best-effort publish inside the request handler (no guarantee).
  - **C.** Two-phase commit across database and broker.
  - **D.** In-memory queue flushed at the end of requests.
- **Decision**: **A** — guarantees at-least-once delivery without 2PC complexity and matches existing Postgres dependency.
