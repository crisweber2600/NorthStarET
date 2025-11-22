# Digital Ink Technology Stack

**Version:** 1.0  
**Last Updated:** November 20, 2025

---

## Backend Technologies

### Core Framework

| Technology | Version | Purpose |
|-----------|---------|---------|
| **.NET** | 10.0 | Runtime and framework |
| **ASP.NET Core** | 10.0 | Web API hosting |
| **C#** | 12.0 | Programming language |

### Data & Storage

| Technology | Version | Purpose | Rationale |
|-----------|---------|---------|-----------|
| **PostgreSQL** | 16 | Primary database | JSONB support for stroke data, proven scalability |
| **Entity Framework Core** | 9.0 | ORM | Code-first migrations, LINQ queries, JSONB support |
| **Azure Blob Storage** | Latest | PDF/audio assets | Cost-effective, SAS token security, Archive tier for cold storage |

**JSONB for Stroke Data:**
- Variable-length stroke arrays (10-10,000 points per stroke)
- Flexible schema for future tool types (highlighter, shapes)
- GIN indexing for fast queries on nested properties
- No need for separate stroke table joins

**Example JSONB Query:**
```sql
-- Get all strokes on page 2
SELECT stroke_data->'pages'->1->'strokes'
FROM ink_sessions
WHERE id = '550e8400-e29b-41d4-a716-446655440000';

-- Count total strokes across all pages
SELECT jsonb_array_length(stroke_data->'pages') AS page_count,
       SUM(jsonb_array_length(page->'strokes')) AS total_strokes
FROM ink_sessions, jsonb_array_elements(stroke_data->'pages') AS page
WHERE tenant_id = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
```

### Messaging & Events

| Technology | Version | Purpose |
|-----------|---------|---------|
| **MassTransit** | 8.x | Message bus abstraction |
| **Azure Service Bus** | Standard | Event publishing/consumption |

**Why MassTransit:**
- Transport-agnostic (can switch from Service Bus to RabbitMQ)
- Built-in retry policies and error handling
- Saga pattern support (future: multi-session workflows)

### Orchestration & Monitoring

| Technology | Purpose |
|-----------|---------|
| **.NET Aspire** | Service orchestration, local development |
| **Azure Application Insights** | Telemetry, logging, performance monitoring |
| **Azure Monitor** | Alerts, availability checks |

---

## Client Technologies

### Cross-Platform UI Framework

**Primary Choice: Avalonia 11**

| Aspect | Details |
|--------|---------|
| **Platforms** | Windows, macOS, Linux, iOS, Android, WebAssembly |
| **Rendering** | Skia-based (SkiaSharp integration) |
| **MVVM Support** | ReactiveUI, Prism, CommunityToolkit.Mvvm |
| **Stylus Input** | Full pointer event support (pressure, tilt) |

**Alternative: .NET MAUI**
- Better iOS/Android performance
- Limited desktop support compared to Avalonia
- Use if mobile-first priority

**Decision:** Avalonia for desktop-first pilot, migrate to MAUI if mobile usage > 60%

---

### Ink Rendering

**SkiaSharp 2.88+**

```csharp
public class InkCanvasControl : SKCanvasView
{
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
        
        foreach (var stroke in _strokes)
        {
            using var paint = new SKPaint
            {
                Color = SKColor.Parse(stroke.Color),
                StrokeWidth = stroke.Width,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };
            
            var path = new SKPath();
            for (int i = 0; i < stroke.Points.Count; i++)
            {
                var point = stroke.Points[i];
                if (i == 0)
                    path.MoveTo(point.X, point.Y);
                else
                    path.LineTo(point.X, point.Y);
            }
            
            canvas.DrawPath(path, paint);
        }
    }
}
```

**Why SkiaSharp:**
- Hardware-accelerated rendering (GPU)
- Variable stroke width based on pressure
- Smooth curves with anti-aliasing
- Platform-agnostic (works on all Avalonia targets)

---

### Audio Recording

**NAudio (Windows) + Platform-Specific (iOS/Android)**

| Platform | Library | Format |
|----------|---------|--------|
| **Windows** | NAudio | WebM (Opus codec) |
| **macOS/iOS** | AVFoundation (P/Invoke) | AAC |
| **Android** | MediaRecorder (P/Invoke) | AAC |
| **Web** | MediaRecorder API (WASM) | WebM |

**Audio Capture Example (NAudio - Windows):**
```csharp
public class AudioRecordingService
{
    private WaveInEvent _waveIn;
    private WaveFileWriter _writer;
    
    public void StartRecording(string outputPath)
    {
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1) // 44.1kHz, mono
        };
        
        _writer = new WaveFileWriter(outputPath, _waveIn.WaveFormat);
        _waveIn.DataAvailable += (s, e) =>
        {
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
        };
        
        _waveIn.StartRecording();
    }
    
    public void StopRecording()
    {
        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        _writer?.Close();
    }
}
```

---

### Offline Storage

**IndexedDB (Web) / SQLite (Desktop/Mobile)**

| Platform | Technology | Purpose |
|----------|-----------|---------|
| **Web (WASM)** | Blazored.LocalStorage | Session cache, stroke buffer |
| **Desktop** | SQLite-net | Offline session storage |
| **Mobile** | SQLite-net | Offline session storage |

**Offline Sync Queue:**
```csharp
public class OfflineSyncService
{
    private readonly Queue<InkSessionUpdate> _syncQueue = new();
    
    public void QueueUpdate(InkSessionUpdate update)
    {
        _syncQueue.Enqueue(update);
        SaveQueueToStorage();
    }
    
    public async Task SyncWhenOnline()
    {
        while (_syncQueue.TryDequeue(out var update))
        {
            try
            {
                await _apiClient.SaveStrokeBatchAsync(update.SessionId, update.Strokes);
            }
            catch (HttpRequestException)
            {
                _syncQueue.Enqueue(update); // Retry later
                break;
            }
        }
    }
}
```

---

### HTTP Client

**Refit 7.x**

```csharp
public interface IDigitalInkApi
{
    [Post("/api/v1/ink/sessions")]
    Task<InkSessionResponse> CreateSessionAsync([Body] CreateSessionRequest request);
    
    [Put("/api/v1/ink/sessions/{sessionId}/strokes")]
    Task<SaveStrokesResponse> SaveStrokesAsync(Guid sessionId, [Body] SaveStrokesRequest request);
    
    [Multipart]
    [Post("/api/v1/ink/sessions/{sessionId}/audio")]
    Task<UploadAudioResponse> UploadAudioAsync(Guid sessionId, [AliasAs("file")] StreamPart file);
}
```

**Why Refit:**
- Type-safe HTTP client generation
- Automatic JSON serialization
- Polly integration for retry policies

---

## Performance Optimizations

### Backend

1. **JSONB GIN Indexing**
   ```sql
   CREATE INDEX idx_ink_sessions_stroke_data ON ink_sessions USING GIN (stroke_data);
   ```

2. **Redis Caching (Future)**
   - Session metadata cached (5-minute TTL)
   - Assignment details cached (1-hour TTL)

3. **Blob Storage CDN (Future)**
   - Azure CDN in front of Blob Storage for PDF backgrounds
   - Reduces latency from 200ms → 20ms

### Client

1. **Stroke Batching**
   - Buffer strokes for 30 seconds or 100 strokes (whichever comes first)
   - Single HTTP request instead of per-stroke

2. **Lazy Loading**
   - Load only visible page's strokes
   - Virtualize multi-page documents

3. **Canvas Dirty Regions**
   - Redraw only changed canvas regions, not entire canvas

---

## Security Technologies

| Technology | Purpose |
|-----------|---------|
| **Azure Key Vault** | Secret storage (connection strings, API keys) |
| **Azure Managed Identity** | Service-to-service authentication |
| **SAS Tokens** | Time-limited Blob Storage URLs (15-min expiration) |
| **JWT** | User authentication (Identity Service) |

**SAS Token Generation:**
```csharp
public string GenerateSasUrl(string blobName, TimeSpan expiration)
{
    var blobClient = _blobContainerClient.GetBlobClient(blobName);
    var sasBuilder = new BlobSasBuilder
    {
        BlobContainerName = _containerName,
        BlobName = blobName,
        Resource = "b",
        ExpiresOn = DateTimeOffset.UtcNow.Add(expiration)
    };
    
    sasBuilder.SetPermissions(BlobSasPermissions.Read);
    
    var sasUri = blobClient.GenerateSasUri(sasBuilder);
    return sasUri.ToString();
}
```

---

## Testing Technologies

| Technology | Purpose | Layer |
|-----------|---------|-------|
| **xUnit** | Unit tests | Application, Domain |
| **FluentAssertions** | Readable assertions | All |
| **Moq** | Mocking dependencies | Unit tests |
| **Testcontainers** | PostgreSQL in integration tests | Infrastructure |
| **Reqnroll** | BDD feature tests | Scenarios |
| **Playwright** | UI automation | Client |
| **MassTransit Test Harness** | Event publishing tests | Integration |

---

## Development Tools

| Tool | Purpose |
|------|---------|
| **Visual Studio 2022** / **Rider** | IDE |
| **Docker Desktop** | Local PostgreSQL, Redis |
| **Azure Storage Explorer** | Blob Storage debugging |
| **pgAdmin** | PostgreSQL query tool |
| **Postman** / **Bruno** | API testing |

---

## Deployment Technologies

| Technology | Purpose |
|-----------|---------|
| **Azure App Service** | API hosting (Phase 4a-4c) |
| **Azure Kubernetes Service (AKS)** | Scalable hosting (Phase 4d+) |
| **Azure Container Registry** | Docker image storage |
| **Azure PostgreSQL Flexible Server** | Managed database |
| **Azure Storage Account** | Blob Storage, Archive tier |

---

## Related Documentation

- **Service Overview:** [../SERVICE_OVERVIEW.md](../SERVICE_OVERVIEW.md)
- **API Specification:** [./api-specification.md](./api-specification.md)
- **Domain Events:** [./domain-events.md](./domain-events.md)

---

**Last Updated:** November 20, 2025  
**Version:** 1.0
