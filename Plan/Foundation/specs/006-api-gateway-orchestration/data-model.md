# Data Model: API Gateway and Service Orchestration
Layer: Foundation
Version: 0.1.0

(Gateway primarily configuration; minimal persistence.)

## Route Configuration (Mapped In-Memory)
| Field | Description |
|-------|-------------|
| RouteId | Unique route key |
| PathPattern | Incoming path matcher |
| ClusterId | Target cluster |
| Version | API version segment |
| Transforms | Header/path transforms |

## Cluster Configuration
| Field | Description |
|-------|-------------|
| ClusterId | Unique id |
| Destinations | List of destination addresses |
| HealthPolicy | Active check config |
| LoadBalancingPolicy | RoundRobin / PowerOfTwo |

## Rate Limit Policy (In Config)
| Field | Description |
|-------|-------------|
| TenantId | Partition key |
| PermitLimit | Requests per window |
| Window | Time span |

## Metrics (Exposed)
- gateway.request.count
- gateway.request.duration
- gateway.rate_limit.rejections
- gateway.circuit.open.count

---
Draft model (config-oriented).