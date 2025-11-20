Data Model (Digital Ink Service)

Entities:
- InkSession(Id, TenantId, OwnerId, EntityId, EntityType, BackgroundAssetUrl, AudioAssetUrl, StrokeData JSONB, CreatedAt, UpdatedAt, IsArchived)
- SessionMetadata(Id, TenantId, SessionId, TotalStrokes, TotalPages, AudioDurationMs, LastSyncedAt)
- FeedbackAnnotation(Id, TenantId, SessionId, TeacherId, FeedbackStrokeData JSONB, CreatedAt)

Indexes: session by entityId; Owner queries; archived filter.
RLS: tenant isolation.

JSONB Structure: pages[ { pageNumber, strokes[ { strokeId, tool, color, width, points[[x,y,p,t]], deleted } ], audioStartOffset } ], audioMetadata.
