Research Notes (Data Import)

Parser Libraries: CsvHelper chosen (robust quoting); EPPlus for Excel; SFTP via SSH.NET.
Duplicate Detection: Fuzzy matching using normalized name + DOB; consideration of phonetic algorithms (Soundex) deferred.
Rollback Approach: Maintain created-entity id list per job; eventual consistency cleanup.
Scheduling Reliability: Hangfire chosen over Quartz (simpler integration, dashboard visibility).
