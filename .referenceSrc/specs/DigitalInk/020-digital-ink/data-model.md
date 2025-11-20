# Digital Ink Data Model

## Database Schema (PostgreSQL)

### Table: `ink_sessions`
Stores the metadata for a user's annotation session.

| Column | Type | Description |
| :--- | :--- | :--- |
| `id` | `UUID` | Primary Key. |
| `tenant_id` | `UUID` | Multi-tenancy isolation. |
| `owner_id` | `UUID` | The user (Teacher/Student) who created the ink. |
| `entity_id` | `UUID` | The associated entity (e.g., AssignmentId, AssessmentId). |
| `entity_type` | `VARCHAR(50)` | Discriminator for the entity (e.g., "Assignment", "Assessment"). |
| `background_asset_url` | `TEXT` | URL to the PDF/Image in Blob Storage. |
| `audio_asset_url` | `TEXT` | URL to the Audio recording in Blob Storage (nullable). |
| `created_at` | `TIMESTAMPTZ` | Session start time. |
| `updated_at` | `TIMESTAMPTZ` | Last modification time. |
| `stroke_data` | `JSONB` | The raw vector data of the ink strokes. |

---

## JSON Data Structures

### Stroke Data Format (`stroke_data` column)
We use a custom JSON structure optimized for time-series analysis and playback.

```json
{
  "version": "1.0",
  "canvas_width": 1920,
  "canvas_height": 1080,
  "strokes": [
    {
      "tool": "pen",          // "pen", "highlighter", "eraser"
      "color": "#000000",     // Hex color
      "width": 2.5,           // Base stroke width
      "start_time": 1050,     // Offset in ms from session start
      "end_time": 2100,       // Offset in ms from session start
      "points": [
        // [x, y, pressure, timestamp_offset]
        [100.5, 200.2, 0.5, 0],
        [102.0, 205.1, 0.6, 15],
        [105.5, 210.8, 0.8, 32]
        // ... more points
      ]
    }
  ]
}
```

### Field Definitions

#### Point Vector `[x, y, p, t]`
*   **x**: X coordinate (relative to canvas width).
*   **y**: Y coordinate (relative to canvas height).
*   **p**: Pressure (0.0 to 1.0).
*   **t**: Timestamp offset (milliseconds) relative to the stroke's `start_time`.

#### Stroke Object
*   **tool**: The instrument used.
*   **color**: Visual color.
*   **width**: Thickness of the stroke.
*   **start_time**: Absolute offset (ms) from the beginning of the `Ink Session` (0 = session creation). This is crucial for syncing with audio.
*   **points**: Array of Point Vectors.

## LLM Readiness
This format allows an LLM to:
1.  **Replay** the creation of the document chronologically.
2.  **Analyze** hesitation (gaps between timestamps) or emphasis (pressure).
3.  **Segment** handwriting based on spatial and temporal proximity.
