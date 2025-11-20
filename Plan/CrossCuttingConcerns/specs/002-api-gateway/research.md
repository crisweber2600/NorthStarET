# Research: API Gateway

## Decisions

### 1. Gateway Technology: YARP
- **Decision**: Use YARP (Yet Another Reverse Proxy).
- **Rationale**: Native .NET, high performance, highly customizable via code, integrates with Aspire.
- **Alternatives**: Ocelot (older, less active), Envoy (sidecar pattern, complex), Nginx (external).

### 2. Rate Limiting: .NET 7+ RateLimiting
- **Decision**: Use built-in `System.Threading.RateLimiting`.
- **Rationale**: Standard, performant, supports sliding window and token bucket algorithms.
- **Alternatives**: AspNetCoreRateLimit (third-party library).

### 3. Resiliency: Polly
- **Decision**: Use Polly for retries, circuit breakers, and timeouts.
- **Rationale**: Industry standard for .NET resiliency.
- **Alternatives**: Custom middleware (reinventing the wheel).

## Needs Clarification
- None.
