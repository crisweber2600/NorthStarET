Data Model Summary (Assessment Service)

Entities:
- Assessment(Id, TenantId, Name, Subject, GradeLevels[], Type, ScoringMethod, MaxScore, CreatedAt, CreatedBy, IsTemplate)
- AssessmentField(Id, TenantId, AssessmentId, FieldName, FieldType, MaxValue, Weight, SortOrder)
- AssessmentAssignment(Id, TenantId, AssessmentId, StudentId, AssignedDate, DueDate, Status, AssignedBy)
- AssessmentResult(Id, TenantId, AssignmentId, StudentId, AssessmentId, TotalScore, BenchmarkLevel, CompletedDate, RecordedBy, FieldScores JSONB)
- Benchmark(Id, TenantId, AssessmentId?, GradeLevel, Subject, BenchmarkName, MinScore, MaxScore)
- AssessmentTemplate(Id, TenantId, TemplateName, Description, PrebuiltFields JSONB)

Indexes:
- idx_assessments_tenant (tenant_id)
- idx_results_student (tenant_id, student_id)
- idx_results_assessment (tenant_id, assessment_id)
- Composite search index: (tenant_id, subject, assessment_type, created_at DESC)

Derived Views:
- assessment_result_field_projection (resultId, fieldId, fieldName, value, maxValue)

RLS Policies: tenant_isolation on all tables using current_setting('app.current_tenant').
