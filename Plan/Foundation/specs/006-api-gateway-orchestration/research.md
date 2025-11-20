# Research: API Gateway and Service Orchestration
Layer: Foundation
Version: 0.1.0

## YARP Advantages
- Native ASP.NET Core integration.
- Dynamic routing & transforms.
- Extensible middleware pipeline.

## Alternatives Considered
| Option | Pros | Cons |
|--------|------|------|
| NGINX + Lua | Mature, performant | Less .NET native extensibility |
| Envoy | Rich features | Higher ops complexity |
| Azure APIM | Managed features | Cost, slower local iteration |
| YARP (Chosen) | Developer velocity | Requires custom resilience policies |

## Circuit Breaker Libraries
- Polly integrated via HttpClient handlers.

## Rate Limiting
- .NET built-in rate limiting (since .NET 7); good partition abstraction.

## JWT Validation
- Use Microsoft.IdentityModel.Tokens with caching of metadata.

## Observability
- OpenTelemetry instrumentation custom spans around proxy invocation.

## Open Questions
1. Dynamic config reload needed in Phase 1? – Likely Phase 2.
2. Canary routing support? – Add after base stability.
3. Response shaping for legacy endpoints uniformity? – Consider wrapper transform.

---
Manual research artifact.