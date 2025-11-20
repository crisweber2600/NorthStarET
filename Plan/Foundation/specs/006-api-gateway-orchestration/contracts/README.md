# Contracts: API Gateway and Service Orchestration
Layer: Foundation

## Public Surface
Gateway forwards existing service endpoints; adds:
- GET /health (aggregate)
- GET /health/raw (raw downstream statuses)

## Headers Injected
- X-Tenant-Id
- X-User-Id
- X-Correlation-Id
- X-API-Version
- Optional: X-API-Sunset

## Error Responses (Standardized)
```json
{ "error": { "code": "RATE_LIMIT_EXCEEDED", "message": "Too many requests", "retryAfterSeconds": 60 } }
```

## Deprecation Header Example
`X-API-Sunset: 2026-06-01`

---