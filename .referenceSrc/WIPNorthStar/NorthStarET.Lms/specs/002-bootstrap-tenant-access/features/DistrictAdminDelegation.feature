Feature: District Admin Delegation
  As a System Admin
  I want to invite, manage, and revoke district admin assignments
  So that I can delegate administrative responsibilities within tenant boundaries

  Background:
    Given a system admin is authenticated
    And a district "Test District" with suffix "test.edu" exists

  # Invitation Flow
  Scenario: System Admin invites a new District Admin
    When the system admin invites a district admin with email "john.doe@test.edu"
    And provides first name "John" and last name "Doe"
    Then the district admin invitation should be created successfully
    And the admin status should be "Unverified"
    And an invitation email should be sent to "john.doe@test.edu"
    And the invitation should expire in 7 days
    And a DistrictAdminInvited event should be published

  Scenario: System Admin cannot invite admin with mismatched email suffix
    When the system admin invites a district admin with email "jane@wrongdomain.com"
    And provides first name "Jane" and last name "Smith"
    Then the invitation should fail with error "Email domain does not match district suffix"
    And no invitation email should be sent

  Scenario: System Admin cannot invite duplicate email to same district
    Given a district admin "alice@test.edu" already exists for the district
    When the system admin invites a district admin with email "alice@test.edu"
    Then the invitation should fail with error "Admin with this email already exists"

  Scenario: System Admin can invite same email to different districts
    Given another district "Other District" with suffix "other.edu" exists
    When the system admin invites a district admin with email "admin@test.edu" to "Test District"
    And the system admin invites a district admin with email "admin@other.edu" to "Other District"
    Then both invitations should succeed
    And each admin should be scoped to their respective district

  # Resend Invitation Flow
  Scenario: System Admin resends invitation to Unverified admin
    Given an unverified district admin "bob@test.edu" exists
    When the system admin resends the invitation
    Then the invitation timestamp should be updated
    And the expiration should be extended by 7 days
    And a new invitation email should be sent
    And a DistrictAdminInvited event should be published

  Scenario: System Admin cannot resend invitation to Verified admin
    Given a verified district admin "verified@test.edu" exists
    When the system admin attempts to resend the invitation
    Then the resend should fail with error "Cannot resend invitation for verified admins"

  Scenario: System Admin cannot resend invitation to Revoked admin
    Given a revoked district admin "revoked@test.edu" exists
    When the system admin attempts to resend the invitation
    Then the resend should fail with error "Cannot resend invitation for verified admins"

  Scenario: Resend invitation respects rate limiting
    Given an unverified district admin "ratelimit@test.edu" exists
    When the system admin resends the invitation 10 times within 1 minute
    Then the 10th request should succeed
    And the 11th request should be rate limited with HTTP 429

  # Revocation Flow
  Scenario: System Admin revokes an Unverified admin
    Given an unverified district admin "pending@test.edu" exists
    When the system admin revokes the admin with reason "No longer needed"
    Then the admin status should change to "Revoked"
    And the revoked timestamp should be recorded
    And a DistrictAdminRevoked event should be published
    And an audit record should be created

  Scenario: System Admin revokes a Verified admin
    Given a verified district admin "active@test.edu" exists
    When the system admin revokes the admin with reason "Policy violation"
    Then the admin status should change to "Revoked"
    And the admin should immediately lose access
    And a DistrictAdminRevoked event should be published

  Scenario: System Admin cannot revoke an already Revoked admin
    Given a revoked district admin "already.revoked@test.edu" exists
    When the system admin attempts to revoke the admin again
    Then the revocation should fail with error "Admin is already revoked"

  Scenario: Revocation cascades when district is deleted
    Given a district admin "cascade@test.edu" exists
    When the system admin deletes the district
    Then all associated admins should be revoked
    And each should have reason "District deleted"

  # List and Query Flow
  Scenario: System Admin views all admins for a district
    Given the district has 3 verified admins
    And the district has 2 unverified admins
    And the district has 1 revoked admin
    When the system admin views the admin list for the district
    Then 6 admins should be displayed
    And verified admins should show "Verified" badge
    And unverified admins should show "Unverified" badge
    And revoked admins should show "Revoked" badge

  Scenario: System Admin filters admins by status
    Given the district has admins in all statuses
    When the system admin filters by "Unverified" status
    Then only unverified admins should be displayed

  Scenario: Admin list respects tenant isolation
    Given "Test District" has 3 admins
    And "Other District" has 2 admins
    When the system admin views admins for "Test District"
    Then only the 3 admins from "Test District" should be displayed
    And admins from "Other District" should not be visible

  # Idempotency
  Scenario: Duplicate invite requests within 10 minutes are idempotent
    When the system admin invites "idempotent@test.edu" at T0
    And the system admin invites "idempotent@test.edu" again at T0+5min with identical payload
    Then only one admin record should exist
    And only one invitation email should be sent
    And both requests should return the same admin ID

  # Audit Trail
  Scenario: All admin lifecycle actions are audited
    Given a district admin lifecycle: invite → resend → verify → revoke
    When the system admin queries the audit log
    Then 4 audit records should exist
    And each record should capture actor, action, timestamp, and payload changes

  # Email Delivery
  Scenario: Email service retries failed deliveries with exponential backoff
    Given the email service is temporarily unavailable
    When the system admin invites "retry@test.edu"
    Then the system should retry sending the email 3 times
    And retry delays should be 1s, 2s, 4s (exponential backoff)
    And if all retries fail, the admin record should still be created
    And the failure should be logged to the dead-letter queue

  Scenario: Email delivery respects 7-day expiration
    Given an unverified admin "expired@test.edu" was invited 8 days ago
    When the admin attempts to verify their email
    Then verification should fail with error "Invitation has expired"
    And the admin should remain in "Unverified" status
    And the system admin should be able to resend the invitation

  # Integration with District CRUD
  Scenario: Admin count is reflected in District detail view
    Given a district has 2 verified admins and 1 unverified admin
    When the system admin views the district details
    Then the active admin count should be 2
    And the pending admin count should be 1

  Scenario: Deleting a district with active admins shows warning
    Given a district has 3 active admins
    When the system admin attempts to delete the district
    Then a warning should display "3 admin(s) will be archived"
    And confirmation should be required before proceeding
