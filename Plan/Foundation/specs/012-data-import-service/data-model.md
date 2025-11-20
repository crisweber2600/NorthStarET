Data Model (Data Import Service)

Entities:
- ImportJob(Id, TenantId, FileName, FileFormat, TargetEntity, Status, TotalRows, SuccessCount, ErrorCount, StartedAt, CompletedAt, UploadedBy)
- ImportRow(Id, TenantId, JobId, RowNumber, RawData JSONB, MappedData JSONB, Status, ErrorMessage)
- ImportTemplate(Id, TenantId, TemplateName, TargetEntity, FieldMappings JSONB, ValidationRules JSONB, CreatedAt)
- ValidationRule(Id, TenantId, TargetEntity, FieldName, RuleType, Parameters JSONB)
- ImportError(Id, TenantId, JobId, RowNumber, ErrorMessage, FieldName, FieldValue)
- ImportAudit(Id, TenantId, JobId, Action, Details JSONB, Timestamp)

Indexes: job status progress, row by jobId, template lookup by name.
RLS across tables.
