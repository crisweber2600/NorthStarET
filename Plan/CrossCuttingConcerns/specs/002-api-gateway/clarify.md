# Clarification Decisions — 2025-11-21

This summarizes the automatic answers chosen while running `@speckit.clarify`.

## 1. Dynamic configuration source
- **Options**
  - **A.** Keep routes/flags in static appsettings checked into git.
  - **B.** Store everything in Azure App Configuration feature management (with refresh).
  - **C.** Build a bespoke configuration database + admin UI.
- **Decision**: **B** — enables instant Strangler Fig routing flips, integrates with .NET Feature Management, and avoids building new admin tooling.

## 2. Token issuer to trust
- **Options**
  - **A.** Microsoft Entra ID (same tenant as Identity Service).
  - **B.** Legacy IdentityServer tokens while migration completes.
  - **C.** API-key based auth only.
- **Decision**: **A** — keeps gateway auth aligned with the new Entra-backed Identity Service and removes dual-stack token handling.

## 3. Rate limiting & circuit state store
- **Options**
  - **A.** Per-instance in-memory counters.
  - **B.** Redis cache shared across replicas.
  - **C.** PostgreSQL tables queried each request.
- **Decision**: **B** — Redis provides atomic LUA scripts for throttling, keeps counters consistent cluster-wide, and meets latency goals.

## 4. Backend service discovery
- **Options**
  - **A.** .NET Aspire ServiceDefaults/registry metadata.
  - **B.** Hard-coded hostnames per environment.
  - **C.** Kubernetes ingress annotations resolved at runtime.
- **Decision**: **A** — leverages existing Aspire wiring so adding a new microservice automatically registers routes/endpoints.

## 5. Hosting platform
- **Options**
  - **A.** Azure API Management.
  - **B.** nginx/Envoy container manually configured.
  - **C.** YARP project hosted via Aspire locally and Azure Container Apps in prod.
- **Decision**: **C** — retains .NET tooling, reuses shared authentication/middleware, and keeps infra consistent with the rest of CrossCuttingConcerns.
