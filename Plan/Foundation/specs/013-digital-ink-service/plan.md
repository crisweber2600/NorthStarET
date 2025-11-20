Technical Plan (Digital Ink Service)

Slices:
1. Session creation API + Blob SAS retrieval.
2. Stroke batch save endpoint (append JSONB -> merge strategy, size guard, per-page segmentation array).
3. Audio upload endpoint linking asset URL.
4. PlaybackQuery assembling timeline structure (points + audio offsets).
5. Feedback annotation model (FeedbackAnnotations table) + overlay retrieval.
6. Auto-save scheduler (client responsibility + server idempotent path).
7. Offline sync guidelines (client SDK design doc) – stroke queue flush.
8. Archival job (age > academic year end) moving assets to cold tier & marking IsArchived.
9. ExportLLMQuery producing compact normalized JSON.
10. Security: SAS token generator, scope validation.

Performance: JSONB partial update vs full replacement—store strokes per page segment to minimize write cost.
Testing: Serialization integrity, playback ordering, undo semantics, permission boundaries.
