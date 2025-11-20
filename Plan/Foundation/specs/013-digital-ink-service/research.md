Research Notes (Digital Ink)

Stroke Storage Options: Separate table per stroke vs JSONB aggregate; chose JSONB pages for batch efficiency; future sharding if size surpasses threshold.
Audio Sync: Timestamp relative sessionStart; drift correction by normalizing final duration ratio.
LLM Export: Keep vector fidelity; exclude raster for size; consider delta compression.
Offline Strategy: IndexedDB queue replay; conflict minimal (append-only). Potential CRDT if collaborative editing introduced.
