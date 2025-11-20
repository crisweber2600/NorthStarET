# Digital Ink API Specification

## Base URL
`/api/v1/ink`

## Endpoints

### 1. Create Ink Session
Initializes a new session for a user on a specific document.

*   **POST** `/sessions`
*   **Request Body**:
    ```json
    {
      "entityId": "uuid",       // e.g., Assignment ID
      "entityType": "string",   // e.g., "Assignment"
      "backgroundAssetUrl": "string" // URL to the PDF
    }
    ```
*   **Response**: `201 Created`
    ```json
    {
      "sessionId": "uuid",
      "uploadUrl": "string" // SAS URL for uploading audio (optional)
    }
    ```

### 2. Save Ink Data
Updates the stroke data for a session. This can be called periodically (auto-save) or on close.

*   **PUT** `/sessions/{sessionId}/strokes`
*   **Request Body**:
    ```json
    {
      "strokes": [ ... ], // Full JSON array of strokes
      "canvasWidth": 1920,
      "canvasHeight": 1080
    }
    ```
*   **Response**: `200 OK`

### 3. Get Ink Session
Retrieves the full session data, including strokes and asset URLs.

*   **GET** `/sessions/{sessionId}`
*   **Response**: `200 OK`
    ```json
    {
      "id": "uuid",
      "ownerId": "uuid",
      "backgroundAssetUrl": "string",
      "audioAssetUrl": "string",
      "strokeData": { ... } // The JSON stroke data
    }
    ```

### 4. Get Sessions by Entity
Finds all ink sessions associated with a specific entity (e.g., all student submissions for an assignment).

*   **GET** `/sessions?entityId={uuid}&entityType={string}`
*   **Response**: `200 OK`
    ```json
    [
      {
        "id": "uuid",
        "ownerId": "uuid",
        "createdAt": "datetime"
      },
      ...
    ]
    ```

### 5. Upload Audio
(Optional) If the client uploads audio separately, this endpoint confirms the upload is complete and links it.

*   **POST** `/sessions/{sessionId}/audio`
*   **Request Body**:
    ```json
    {
      "audioUrl": "string" // Or handle via direct Blob upload + webhook
    }
    ```
