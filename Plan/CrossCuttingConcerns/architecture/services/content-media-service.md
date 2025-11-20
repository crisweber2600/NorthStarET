# Content & Media Service

## Overview

The Content & Media Service manages file uploads, document storage, video content, and Azure Blob Storage integration for the NorthStar LMS platform.

## Service Classification

- **Type**: Supporting Service
- **Phase**: Phase 4 (Weeks 25-30)
- **Implementation Path**: `NewDesign/NorthStarET.Lms/src/services/Media/`
- **Priority**: Low (non-critical supporting functionality)
- **LMS Role**: File and media management for attachments, videos, and documents

## Current State (Legacy)

**Location**: `NS4.WebAPI/Controllers/` (FileUploaderController, VideoController, AzureDownloadController)  
**Framework**: .NET Framework 4.6  
**Storage**: Local file system + Azure Blob Storage (partial)

**Key Components**:
- File upload to local server
- Azure Blob Storage integration (videos)
- Download management
- File metadata tracking

## Target State (.NET 8 Microservice)

### Architecture

**Clean Architecture Layers**:
```
Media.API/                      # UI Layer (upload/download endpoints)
Media.Application/              # Application Layer (upload orchestrator, streaming)
Media.Domain/                   # Domain Layer (File, Video, Folder aggregates)
Media.Infrastructure/           # Infrastructure Layer (Azure Blob, EF Core)
```

### Technology Stack

- **Framework**: .NET 8, ASP.NET Core
- **Storage**: Azure Blob Storage (primary), Azure CDN (videos)
- **Data Access**: EF Core with SQL Server (metadata only)
- **Messaging**: MassTransit + Azure Service Bus
- **Orchestration**: .NET Aspire hosting

### Owned Data

**Database**: `NorthStar_Media_DB` (metadata only, files in Blob Storage)

**Tables**:
- Files (Id, FileName, FileSize, ContentType, BlobUrl, UploadedBy, UploadDate, EntityType, EntityId)
- Videos (Id, Title, BlobUrl, CdnUrl, Duration, UploadedBy, UploadDate, IsPublished)
- Folders (Id, Name, ParentId, OwnerId, OwnerType, CreatedDate)

### Service Boundaries

**Owned Responsibilities**:
- File upload and download
- Azure Blob Storage integration
- Video streaming via CDN
- File metadata management
- Folder/directory structure
- File virus scanning (integration)

**Not Owned**:
- User authentication → Identity Service
- Entity associations (student files, assessment attachments) → Domain services

### Domain Events Published

- `FileUploadedEvent` - New file uploaded
- `VideoPublishedEvent` - Video ready for streaming
- `FileDeletedEvent` - File removed

### API Functional Intent

**File Management**:
- Upload files to Azure Blob Storage
- Download files
- Delete files
- List files by entity

**Video Management**:
- Upload videos
- Stream videos via CDN
- Video metadata management

**Folder Management**:
- Create folders
- Organize files

### Service Level Objectives (SLOs)

- **Availability**: 99% uptime
- **Upload**: < 30 seconds for 50MB files
- **Download**: CDN-backed, < 2 seconds to initiate
- **Consistency Model**: Eventually consistent

### Security & Compliance

**Authorization**:
- Upload restricted to authenticated users
- Download based on entity ownership
- Virus scanning before storage

**Data Protection**:
- Files encrypted at rest in Blob Storage
- Secure download URLs (SAS tokens)
- No PII in filenames

### Testing Requirements

**Reqnroll BDD Features**:
- `file-upload.feature`
- `video-streaming.feature`
- `file-security.feature`

### Migration Strategy

**Phase 4, Weeks 25-30**:
1. Azure Blob setup (Weeks 25-26)
2. File upload/download (Weeks 27-28)
3. Video streaming (Weeks 29-30)

### Dependencies

**Upstream**: Identity  
**Downstream**: Student, Assessment (file attachments)

### Implementation Checklist

- [ ] Azure Blob Storage configuration
- [ ] File upload API
- [ ] Download API with SAS tokens
- [ ] Video streaming with CDN
- [ ] Virus scanning integration
- [ ] Migrate legacy files to Blob Storage

### Open Questions / Risks

1. **Storage Costs**: Large file volumes can increase Azure costs. Need archival strategy.
2. **Virus Scanning**: Integration with third-party service required.

---

**Version**: 1.0  
**Last Updated**: 2025-11-13  
**Status**: Specification Complete - Ready for Implementation
