# Digital Ink Integration Strategy

## Identity Service Integration
The Digital Ink Service relies on the Foundation Identity Service for authentication and authorization.

*   **Authentication**: All API requests must bear a valid JWT issued by the Identity Service.
*   **User Context**: The service extracts `UserId` and `TenantId` from the JWT claims to populate the `owner_id` and `tenant_id` fields in the database.
*   **Authorization**:
    *   **Teachers**: Can view all sessions for entities (Assignments) they own.
    *   **Students**: Can only view/edit their own sessions.

## Blob Storage Integration
Large binary assets are offloaded to Azure Blob Storage to keep the database lightweight.

### Container Structure
*   `digital-ink-backgrounds/`: Stores the source PDFs or Images.
    *   Path: `{tenantId}/{entityId}/{filename}`
*   `digital-ink-audio/`: Stores the synchronized audio recordings.
    *   Path: `{tenantId}/{sessionId}/audio.mp4`

### Upload Flow
1.  **Backgrounds**: Uploaded by Admins/Teachers via the Content Service (or directly if permitted). The URL is passed to the Ink Service upon session creation.
2.  **Audio**:
    *   Client requests an upload URL (SAS Token) from the Ink Service.
    *   Client uploads directly to Azure Blob Storage.
    *   Client notifies Ink Service upon completion to update the `audio_asset_url`.

## LLM & Analytics Integration
The structured JSON data allows for advanced downstream processing.

*   **Playback**: The `start_time` and `points` array allow a web or desktop client to "replay" the drawing process in real-time.
*   **Vector Analysis**: An AI worker can fetch the `stroke_data` JSON to:
    *   Convert handwriting to text (OCR).
    *   Analyze pressure/speed for behavioral insights.
    *   Correlate audio timestamps with specific strokes (e.g., "What was the student saying while drawing this circle?").
