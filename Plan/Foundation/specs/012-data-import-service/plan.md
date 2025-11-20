Technical Plan (Data Import Service)

Pipeline:
1. Upload endpoint stores file (Azure Blob) + creates ImportJob row (Status=Uploaded).
2. StartImportCommand enqueues Hangfire ImportProcessorJob.
3. FileParser strategy (CsvParser, ExcelParser, SftpFetcher) yields rows.
4. ValidationService: structural → business rules hierarchy; collects errors.
5. MappingService applies field transformations; logs mapping decisions.
6. Row dispatch: calls target service client (Student/Staff/Assessment) with Create* command; handle partial failures.
7. Progress broadcaster: updates ImportJob every N rows; cancellation token check.
8. Error logging: ImportErrors table; failed record export builder.
9. Rollback path: RollbackImportCommand enumerates created entity IDs + publishes ImportRollbackEvent.
10. Scheduling: Hangfire recurring jobs with cron expressions stored.

Performance: Parallel row batches (bounded concurrency); backpressure if target service latency > threshold.
Security: Malware scan hook pre-processing (future); SAS tokens short-lived.
Testing: Parser edge cases, duplicate detection, rollback integrity.
