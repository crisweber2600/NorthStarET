# Clarification Decisions — 2025-11-21

Automatic run of `@speckit.clarify` recorded the choices below for future review.

## 1. Entra ID tenant topology
- **Options**
  - **A.** Single Microsoft Entra tenant with custom claims for district context.
  - **B.** Per-district Entra tenants linked via B2B/B2C trust.
  - **C.** Hybrid (shared tenant for HQ, district-owned tenants for locals).
- **Decision**: **A** — centralizes governance, simplifies app registration, and still supports multi-district admins by carrying tenant context in claims.

## 2. Role synchronization model
- **Options**
  - **A.** Manage all roles/permissions in Entra ID only.
  - **B.** Manage all roles inside NorthStar only.
  - **C.** Hybrid: Entra App Roles for coarse access, NorthStar DB for granular permissions.
- **Decision**: **C** — Entra App Roles drive high-level access (teacher, admin, support) while NorthStar keeps fine-grained permissions per tenant for least-privilege controls.

## 3. Session storage source of truth
- **Options**
  - **A.** Redis-as-authority (no database persistence).
  - **B.** PostgreSQL only (no cache).
  - **C.** PostgreSQL canonical store with Redis cache mirroring TTL.
- **Decision**: **C** — keeps durable audit/compliance trail while maintaining <20ms validation latency via Redis.

## 4. Multi-region replication approach
- **Options**
  - **A.** Azure Cache for Redis Enterprise active-active + PostgreSQL logical replication + region-affinity routing.
  - **B.** Independent region stacks with eventual manual sync.
  - **C.** Force all auth traffic through a single primary region.
- **Decision**: **A** — preserves availability objectives without sacrificing latency or consistency for cross-region deployments.

## 5. Custom claims required from Entra ID
- **Options**
  - **A.** `district_id`, `school_ids[]`, `northstar_role` custom claims emitted via App Roles.
  - **B.** Use default `groups` claim plus directory extensions as-needed.
  - **C.** Encode tenant context in JWT audience per district.
- **Decision**: **A** — explicit claims keep downstream services deterministic and reduce per-request directory lookups.
