Feature: Audit Trail and Domain Event Emission
  As a System Admin or compliance officer
  I want all district and admin lifecycle actions captured in an immutable audit trail
  So that I can review historical changes and ensure accountability

  Background:
    Given I am authenticated as a System Admin
    And the audit system is configured
    And the Event Grid emulator is running

  @US4 @Audit
  Scenario: District creation is audited
    When I create a new district with name "Springfield School District" and suffix "springfield.edu"
    Then an audit record is created with action "Created"
    And the audit record captures the actor role as "SystemAdmin"
    And the audit record contains the district name in the after payload
    And the audit record has a correlation ID
    And a "DistrictCreated" domain event is emitted with the same correlation ID

  @US4 @Audit
  Scenario: District update is audited with before and after payloads
    Given a district exists with name "Springfield School District" and suffix "springfield.edu"
    When I update the district name to "Springfield Unified School District"
    Then an audit record is created with action "Updated"
    And the audit record captures the before payload with old name "Springfield School District"
    And the audit record captures the after payload with new name "Springfield Unified School District"
    And a "DistrictUpdated" domain event is emitted

  @US4 @Audit
  Scenario: District deletion is audited with soft delete indicator
    Given a district exists with name "Springfield School District" and suffix "springfield.edu"
    When I delete the district
    Then an audit record is created with action "Deleted"
    And the audit record captures the before payload with district details
    And the audit record after payload shows IsDeleted as true
    And a "DistrictDeleted" domain event is emitted

  @US4 @Audit
  Scenario: District admin invitation is audited
    Given a district exists with name "Springfield School District" and suffix "springfield.edu"
    When I invite a district admin with email "admin@springfield.edu", first name "John", and last name "Doe"
    Then an audit record is created with action "Invited"
    And the audit record captures the district ID
    And the audit record captures the admin email in the after payload
    And a "DistrictAdminInvited" domain event is emitted

  @US4 @Audit
  Scenario: District admin verification is audited
    Given a district exists with name "Springfield School District" and suffix "springfield.edu"
    And a district admin invitation exists for "admin@springfield.edu" with status "Unverified"
    When the district admin verifies their account
    Then an audit record is created with action "Verified"
    And the audit record captures status transition from "Unverified" to "Verified"
    And a "DistrictAdminVerified" domain event is emitted

  @US4 @Audit
  Scenario: District admin revocation is audited
    Given a district exists with name "Springfield School District" and suffix "springfield.edu"
    And a district admin exists for "admin@springfield.edu" with status "Verified"
    When I revoke the district admin access
    Then an audit record is created with action "Revoked"
    And the audit record captures status transition from "Verified" to "Revoked"
    And the audit record captures the revocation timestamp
    And a "DistrictAdminRevoked" domain event is emitted

  @US4 @Audit @Query
  Scenario: Query audit records for a specific district
    Given a district exists with name "Springfield School District" and suffix "springfield.edu"
    And multiple audit actions have occurred for the district
    When I query audit records filtered by the district ID
    Then I receive only audit records related to that district
    And the records are ordered by occurrence time descending
    And each record contains actor, action, entity type, and timestamp

  @US4 @Audit @Query
  Scenario: Query audit records with pagination
    Given multiple districts exist with audit history
    And there are more than 100 audit records in the system
    When I query audit records with page size 25 and page number 2
    Then I receive exactly 25 audit records
    And the records represent the second page of results
    And pagination metadata includes total count and page information

  @US4 @Audit @Query
  Scenario: System Admin can view audit records across all districts
    Given I am authenticated as a System Admin
    And multiple districts exist with audit history
    When I query audit records without district filter
    Then I receive audit records from all districts
    And each record clearly identifies the associated district

  @US4 @Audit @Query
  Scenario: District Admin can only view audit records for their district
    Given I am authenticated as a District Admin for "Springfield School District"
    And audit records exist for multiple districts
    When I query audit records
    Then I receive only audit records for "Springfield School District"
    And audit records for other districts are not returned

  @US4 @Events
  Scenario: Domain events are published to Event Grid
    Given the Event Grid emulator is configured
    When I create a new district with name "Springfield School District" and suffix "springfield.edu"
    Then a "DistrictCreated" event is published to Event Grid
    And the event payload contains the district ID and name
    And the event schema version is included
    And the event includes a correlation ID matching the audit record

  @US4 @Events @Outbox
  Scenario: Outbox processor publishes pending events
    Given domain events are stored in the outbox table
    And some events have not been published yet
    When the outbox processor runs
    Then unpublished events are published to Event Grid
    And the published timestamp is updated for each event
    And the publish attempts counter is incremented

  @US4 @Events @Outbox @Retry
  Scenario: Failed event publishing is retried with backoff
    Given a domain event in the outbox table has failed to publish
    And the publish attempts count is less than 5
    When the outbox processor runs
    Then the event publishing is retried
    And the publish attempts counter is incremented
    And the retry uses exponential backoff delay

  @US4 @Events @Outbox @Retry
  Scenario: Event publishing is abandoned after maximum retries
    Given a domain event in the outbox table has failed to publish
    And the publish attempts count is 5
    When the outbox processor runs
    Then the event is not retried
    And the event is marked as failed
    And an alert is logged for manual intervention

  @US4 @Audit @Immutability
  Scenario: Audit records cannot be modified after creation
    Given an audit record exists for a district creation
    When an attempt is made to update the audit record
    Then the update operation is rejected
    And an error indicates audit records are immutable

  @US4 @Audit @CorrelationTracking
  Scenario: Audit records and events share correlation IDs
    When I create a new district with name "Springfield School District" and suffix "springfield.edu"
    Then an audit record is created with a correlation ID
    And a "DistrictCreated" domain event is emitted
    And the domain event has the same correlation ID as the audit record
    And the correlation ID can be used to trace the action across both systems
