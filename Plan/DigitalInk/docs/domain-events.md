# Digital Ink Domain Events Specification

**Version:** 1.0  
**Last Updated:** November 20, 2025  
**Schema Version:** Follows [domain-events-schema.md](../../../docs/architecture/domain-events-schema.md)

---

## Overview

The Digital Ink Service publishes domain events when significant state changes occur (session created, strokes captured, audio recorded, session archived). Other services subscribe to these events for analytics, reporting, and compliance workflows.

**Event Bus:** Azure Service Bus (Standard tier)  
**Serialization:** JSON  
**Message TTL:** 7 days  
**Dead Letter Queue:** Enabled (manual intervention for failures)

---

## Published Events

### 1. InkSessionCreatedEvent

**When:** New ink session is created and linked to an assignment

**Purpose:** Notify downstream services (Reporting, Analytics) that a student has started working on an assignment with digital ink.

**Schema:**
```json
{
  "eventId": "uuid",
  "eventType": "InkSessionCreated",
  "version": "1.0",
  "occurredAt": "2025-11-20T14:30:00Z",
  "payload": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ownerId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "entityId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "entityType": "Assignment",
    "backgroundAssetUrl": "https://storage.blob.core.windows.net/pdfs/math-worksheet-1.pdf?sas=...",
    "createdAt": "2025-11-20T14:30:00Z"
  },
  "metadata": {
    "correlationId": "uuid",
    "causationId": "uuid",
    "serviceName": "DigitalInk.Service",
    "serviceVersion": "1.0.0"
  }
}
```

**Consumers:**
- **Reporting Service** - Track assignment engagement metrics
- **Analytics Service** - Identify assignments with digital ink usage

**Idempotency:** Consumers should deduplicate using `sessionId`

---

### 2. StrokesCapturedEvent

**When:** Student saves a batch of strokes (auto-save every 30 seconds or manual save)

**Purpose:** Track ink usage patterns and session activity for analytics.

**Schema:**
```json
{
  "eventId": "uuid",
  "eventType": "StrokesCaptured",
  "version": "1.0",
  "occurredAt": "2025-11-20T14:32:15Z",
  "payload": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "strokeCount": 42,
    "pageNumber": 1,
    "capturedAt": "2025-11-20T14:32:15Z"
  },
  "metadata": {
    "correlationId": "uuid",
    "causationId": "uuid",
    "serviceName": "DigitalInk.Service",
    "serviceVersion": "1.0.0"
  }
}
```

**Consumers:**
- **Reporting Service** - Aggregate stroke counts per session
- **Analytics Service** - Identify high-engagement assignments

**Idempotency:** Use `sessionId` + `capturedAt` timestamp for deduplication

---

### 3. AudioRecordedEvent

**When:** Audio file is successfully uploaded to Blob Storage

**Purpose:** Track multimedia assessment usage and enable content-aware analytics.

**Schema:**
```json
{
  "eventId": "uuid",
  "eventType": "AudioRecorded",
  "version": "1.0",
  "occurredAt": "2025-11-20T14:35:00Z",
  "payload": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "audioAssetUrl": "https://storage.blob.core.windows.net/audio/session-550e8400.webm?sas=...",
    "durationMs": 45000,
    "format": "webm",
    "sampleRate": 44100
  },
  "metadata": {
    "correlationId": "uuid",
    "causationId": "uuid",
    "serviceName": "DigitalInk.Service",
    "serviceVersion": "1.0.0"
  }
}
```

**Consumers:**
- **Reporting Service** - Track audio usage metrics
- **Content Moderation Service** (future) - Analyze audio for inappropriate content

**Idempotency:** Use `sessionId` (only one audio file per session)

---

### 4. SessionArchivedEvent

**When:** Session is soft-deleted or moved to cold storage (e.g., year-end rollover)

**Purpose:** Trigger data retention workflows and compliance reporting.

**Schema:**
```json
{
  "eventId": "uuid",
  "eventType": "SessionArchived",
  "version": "1.0",
  "occurredAt": "2025-08-15T00:00:00Z",
  "payload": {
    "sessionId": "550e8400-e29b-41d4-a716-446655440000",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "archivedAt": "2025-08-15T00:00:00Z",
    "retentionPolicyId": "7-year-ferpa-retention"
  },
  "metadata": {
    "correlationId": "uuid",
    "causationId": "uuid",
    "serviceName": "DigitalInk.Service",
    "serviceVersion": "1.0.0"
  }
}
```

**Consumers:**
- **Data Retention Service** - Apply cold storage policies
- **Compliance Service** - FERPA retention audit trail

**Idempotency:** Use `sessionId`

---

## Subscribed Events

### 1. AssessmentAssignedEvent

**Source:** Assessment Service

**When:** Teacher assigns an assessment/assignment to students

**Purpose:** Enable ink session creation for the assigned assessment.

**Schema (from Assessment Service):**
```json
{
  "eventId": "uuid",
  "eventType": "AssessmentAssigned",
  "version": "1.0",
  "occurredAt": "2025-11-20T09:00:00Z",
  "payload": {
    "assessmentId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "assignedTo": ["studentId1", "studentId2", "studentId3"],
    "dueDate": "2025-11-25T23:59:59Z",
    "hasDigitalInkEnabled": true
  },
  "metadata": {
    "correlationId": "uuid",
    "causationId": "uuid",
    "serviceName": "Assessment.Service",
    "serviceVersion": "2.0.0"
  }
}
```

**Handler:** `AssessmentAssignedConsumer` (DigitalInk.Infrastructure)

**Actions:**
- Check if `hasDigitalInkEnabled` is true
- If true, mark assessment as eligible for ink session creation
- Update internal cache of eligible assessments

---

### 2. StudentWithdrawnEvent

**Source:** Student Management Service

**When:** Student is withdrawn from a school/district

**Purpose:** Archive all ink sessions for the withdrawn student (FERPA compliance).

**Schema (from Student Management Service):**
```json
{
  "eventId": "uuid",
  "eventType": "StudentWithdrawn",
  "version": "1.0",
  "occurredAt": "2025-11-20T10:00:00Z",
  "payload": {
    "studentId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "withdrawalDate": "2025-11-20T00:00:00Z",
    "reason": "Transfer"
  },
  "metadata": {
    "correlationId": "uuid",
    "causationId": "uuid",
    "serviceName": "StudentManagement.Service",
    "serviceVersion": "1.5.0"
  }
}
```

**Handler:** `StudentWithdrawnConsumer` (DigitalInk.Infrastructure)

**Actions:**
- Query all ink sessions where `owner_id = studentId`
- Set `is_archived = true` for all matching sessions
- Publish `SessionArchivedEvent` for each archived session
- Move associated audio files to Blob Archive tier

---

## Event Versioning

### Version Compatibility

- **Schema Version:** Included in `version` field of each event
- **Backward Compatibility:** New fields added as optional, never remove required fields
- **Breaking Changes:** Increment version (e.g., `1.0` → `2.0`) and support both for 6 months

### Version Migration Example

**v1.0 → v2.0 (hypothetical):**

If we need to add `strokeStyles` to `StrokesCapturedEvent`:

```json
{
  "version": "2.0",
  "payload": {
    "sessionId": "...",
    "strokeCount": 42,
    "strokeStyles": {        // NEW FIELD (optional)
      "pen": 30,
      "highlighter": 8,
      "eraser": 4
    }
  }
}
```

**Consumer Handling:**
```csharp
public class StrokesCapturedConsumer : IConsumer<StrokesCapturedEvent>
{
    public async Task Consume(ConsumeContext<StrokesCapturedEvent> context)
    {
        var evt = context.Message;
        
        if (evt.Version == "2.0" && evt.Payload.StrokeStyles != null)
        {
            // Use new field
            _analytics.TrackStrokeStyles(evt.Payload.StrokeStyles);
        }
        
        // Common processing for both v1.0 and v2.0
        _analytics.TrackStrokeCount(evt.Payload.StrokeCount);
    }
}
```

---

## Testing Events

### Integration Tests

**Test:** Publish event and verify consumer receives it

```csharp
[Fact]
public async Task InkSessionCreatedEvent_Should_Be_Consumed_By_ReportingService()
{
    // Arrange
    var evt = new InkSessionCreatedEvent
    {
        SessionId = Guid.NewGuid(),
        TenantId = _testTenantId,
        OwnerId = _testStudentId,
        EntityId = _testAssignmentId,
        EntityType = "Assignment",
        CreatedAt = DateTime.UtcNow
    };
    
    // Act
    await _publisher.Publish(evt);
    
    // Assert
    await _testHarness.Consumed.Any<InkSessionCreatedEvent>(x => 
        x.Context.Message.SessionId == evt.SessionId);
}
```

### BDD Tests

**Feature:** Event publishing (Reqnroll)

```gherkin
Feature: Digital Ink Event Publishing

Scenario: Session creation publishes InkSessionCreatedEvent
  Given a student "Liam" is logged in
  And an assignment "Math Worksheet 1" exists with digital ink enabled
  When Liam creates an ink session for the assignment
  Then an InkSessionCreatedEvent should be published to the message bus
  And the event should include the session ID, tenant ID, and assignment ID
```

---

## Monitoring & Observability

### Event Metrics (Azure Application Insights)

- **Published Events:** Counter by event type
- **Failed Publications:** Alert if > 5 failures in 5 minutes
- **Consumer Lag:** Alert if message age > 1 minute
- **Dead Letter Queue:** Alert if any messages in DLQ

### Logging

```csharp
_logger.LogInformation(
    "Published {EventType} for SessionId={SessionId}, TenantId={TenantId}",
    nameof(InkSessionCreatedEvent),
    evt.SessionId,
    evt.TenantId
);
```

---

## Related Documentation

- **Service Overview:** [../SERVICE_OVERVIEW.md](../SERVICE_OVERVIEW.md)
- **General Event Schema:** [../../../docs/architecture/domain-events-schema.md](../../../docs/architecture/domain-events-schema.md)
- **API Specification:** [./api-specification.md](./api-specification.md)

---

**Last Updated:** November 20, 2025  
**Version:** 1.0  
**Schema Version:** 1.0
