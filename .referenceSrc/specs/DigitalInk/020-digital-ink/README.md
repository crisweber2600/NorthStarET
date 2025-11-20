# Digital Ink Service Specification

## Overview
The Digital Ink Service is responsible for managing "Ink Sessions" — collections of digital handwriting (strokes), synchronized audio recordings, and background assets (PDFs). This service enables teachers and students to annotate documents using a stylus, recording pressure and timing data for future playback and analysis by LLMs.

## Goals
1.  **High-Fidelity Capture**: Store pen strokes with pressure, tilt, and precise timestamps.
2.  **Audio Synchronization**: Link audio recordings to the exact moment strokes were created.
3.  **LLM Readiness**: Store data in a structured, time-series JSON format (not opaque binary blobs) to allow AI analysis of handwriting dynamics.
4.  **Platform Agnostic**: While the primary client is Avalonia, the data format should be consumable by web or mobile clients.

## Architecture
*   **Service Type**: ASP.NET Core Microservice
*   **Database**: PostgreSQL (using JSONB for stroke data)
*   **File Storage**: Azure Blob Storage (for PDFs and Audio files)
*   **Clients**: Avalonia UI (Desktop/Mobile)

## Key Concepts
*   **Ink Session**: A user's interaction with a specific document. Contains metadata, links to assets, and the collection of strokes.
*   **Background Asset**: The PDF or Image being annotated.
*   **Stroke**: A continuous line drawn by the user, consisting of multiple points.
*   **Point**: A single data sample containing X, Y, Pressure, and Timestamp.
