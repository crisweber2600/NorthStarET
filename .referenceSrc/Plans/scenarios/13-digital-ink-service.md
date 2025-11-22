# Digital Ink Service Scenarios

**Feature**: Digital Ink and Audio Capture  
**Epic**: Phase 4 - Supporting Services  
**Service**: Digital Ink Service  
**Business Value**: Enables rich, interactive student assessment and feedback with AI-ready data capture.

---

## Scenario 1: Student Starts New Ink Session on Assignment

**Given** a student "Liam" opens an assignment "Math Worksheet 1"  
**And** the assignment has a PDF background  
**When** Liam taps "Start Annotation"  
**Then** the Digital Ink Service creates a new `InkSession` linked to Liam and the Assignment  
**And** retrieves the SAS URL for the PDF background from Blob Storage  
**And** initializes an empty stroke collection  
**And** the UI renders the PDF on the canvas ready for input

---

## Scenario 2: Capturing High-Fidelity Stroke Data

**Given** Liam is writing on the canvas with a stylus  
**When** he draws a circle around an answer  
**Then** the client captures the stroke as a series of points `[x, y, pressure, timestamp]`  
**And** the sampling rate is at least 60Hz  
**And** the pressure data (0.0 to 1.0) is preserved  
**And** the stroke is added to the local session buffer  
**And** the stroke is rendered immediately on screen with variable width based on pressure

---

## Scenario 3: Synchronized Audio Recording

**Given** Liam wants to explain his reasoning  
**When** he taps the "Record Audio" button  
**Then** the client begins capturing audio from the microphone  
**And** records the `audio_start_time` relative to the session start  
**And** subsequent ink strokes are tagged with timestamps relative to the audio  
**And** a visual indicator shows recording is active  
**And** the audio file is streamed or uploaded to Blob Storage upon completion

---

## Scenario 4: Teacher Reviews Student Work with Playback

**Given** a teacher "Ms. Johnson" opens Liam's submitted assignment  
**When** she presses the "Play" button  
**Then** the Digital Ink Service retrieves the stroke data and audio  
**And** the client plays the audio recording  
**And** the ink strokes appear on the canvas in real-time synchronization with the audio  
**And** Ms. Johnson can see exactly when and how Liam wrote his answer  
**And** she can pause, rewind, or scrub through the timeline

---

## Scenario 5: Offline Mode and Sync

**Given** Liam has poor internet connectivity  
**When** he continues working on the assignment offline  
**Then** the client saves all strokes and audio locally  
**And** queues the data for synchronization  
**When** connectivity is restored  
**Then** the client automatically uploads the pending stroke batches and audio files  
**And** the Digital Ink Service updates the session without data loss  
**And** a "Synced" indicator appears in the UI

---

## Scenario 6: PDF Background Rendering

**Given** the assignment is a 5-page PDF  
**When** the session loads  
**Then** the client renders the PDF pages as the background layer  
**And** the ink canvas is overlaid on top of the PDF  
**And** the coordinate system of the ink matches the PDF dimensions  
**And** zooming or panning the PDF keeps the ink perfectly aligned  
**And** the PDF itself is never modified, only overlaid

---

## Scenario 7: Multi-Page Document Navigation

**Given** Liam is working on Page 1  
**When** he swipes to go to Page 2  
**Then** the UI switches the background to Page 2 of the PDF  
**And** the ink canvas clears (visually) to show only strokes for Page 2  
**And** the Digital Ink Service tracks which page each stroke belongs to  
**And** saving the session preserves the page association for all strokes

---

## Scenario 8: Eraser and Undo Functionality

**Given** Liam makes a mistake  
**When** he uses the stylus eraser or taps "Undo"  
**Then** the specific strokes are removed from the visual canvas  
**And** the "Undo" action is recorded in the session history (soft delete)  
**And** the `stroke_data` sent to the server marks these strokes as `deleted` or removes them  
**And** playback respects the deletion (showing the stroke then disappearing, or not showing it at all depending on mode)

---

## Scenario 9: LLM Data Export

**Given** an AI researcher wants to analyze handwriting dynamics  
**When** they request the "Raw Data" export for a session  
**Then** the Digital Ink Service returns the full JSON structure  
**And** it includes the time-series vectors `[x, y, p, t]`  
**And** it includes the audio file URL  
**And** the format is optimized for ingestion by multimodal LLMs  
**And** no raster images are included, only vector data

---

## Scenario 10: Session Auto-Save

**Given** Liam is working for an extended period  
**When** 30 seconds elapse since the last save  
**Then** the client automatically sends a `PUT` request to the Digital Ink Service  
**And** the payload contains the new strokes since the last save  
**And** the service appends these strokes to the `stroke_data` in the database  
**And** the `updated_at` timestamp is refreshed  
**And** Liam sees a subtle "Saved" notification

---

## Scenario 11: Teacher Annotation on Student Work

**Given** Ms. Johnson is reviewing Liam's submission  
**When** she selects the "Red Pen" tool  
**Then** she can write on top of Liam's work  
**And** her strokes are saved to the same session (or a linked "Feedback" session)  
**And** her strokes are visually distinct (different color/layer)  
**And** Liam can view her feedback overlaid on his original work  
**And** he cannot erase her strokes

---

## Scenario 12: Archiving Old Sessions

**Given** the school year has ended  
**When** the system runs the "Year End Rollover" process  
**Then** old Ink Sessions are marked as `archived`  
**And** the raw stroke data is moved to cold storage (if configured)  
**And** the PDF/Audio assets are retained in Blob Storage (Archive Tier)  
**And** the data remains accessible for historical reporting but is read-only  
**And** the database index is optimized for active sessions

---

## Technical Implementation Notes

**Database Schema (PostgreSQL)**:
```sql
CREATE TABLE ink_sessions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    owner_id UUID NOT NULL,
    entity_id UUID NOT NULL, -- AssignmentId
    entity_type VARCHAR(50),
    background_asset_url TEXT,
    audio_asset_url TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    stroke_data JSONB -- The heavy lifting
);

CREATE INDEX idx_ink_sessions_entity ON ink_sessions(entity_id);
```

**JSON Structure**:
```json
{
  "pages": [
    {
      "pageNumber": 1,
      "strokes": [
        {
          "tool": "pen",
          "color": "#000000",
          "width": 2.0,
          "points": [[10, 10, 0.5, 0], [12, 12, 0.6, 16], ...]
        }
      ]
    }
  ]
}
```

**API Endpoints**:
- `POST /api/v1/ink/sessions`
- `PUT /api/v1/ink/sessions/{id}/strokes`
- `POST /api/v1/ink/sessions/{id}/audio`
- `GET /api/v1/ink/sessions/{id}`

**Client-Side (Avalonia)**:
- Use `InkCanvas` or custom `SkiaSharp` control for rendering.
- Implement `PointerPressed`, `PointerMoved`, `PointerReleased` events to capture high-frequency data.
- Use `NAudio` or similar for cross-platform audio recording.
