# Research: Staff Management Service
Layer: Foundation
Version: 0.1.0

## Multi-School Assignment Patterns
- Represent FTE as decimal (0.00–1.00); ensure precision by decimal(3,2).
- Consider effective date ranges for future scheduling changes.

## Certification Tracking
- Store expiration date; scheduled job queries soon-expiring.
- Potential future integration: external credential verification API.

## Schedule Representation
- MVP: discrete availability rows (DayOfWeek, Start, End, ActivityType).
- Future: ICS export / calendar integration.

## Import Considerations
- Bulk operations may move to COPY for large volume; MVP uses EF batch.

## Open Questions
1. Should FTE enforcement block creation or warn? – Block above 1.0.
2. Handling partial year assignments? – Represent with StartDate/EndDate.
3. Support for department-level teams vs PLC distinction? – Attribute on Team entity.

---
Manual research artifact.