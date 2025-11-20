# Feature: District Management
# As a System Admin
# I want to manage districts with unique suffixes
# So that I can organize tenants and delegate admin access

Feature: District Management
  System Admins can create, view, update, and delete districts with tenant isolation

  Background:
    Given I am authenticated as a System Admin
    And the system has no existing districts

  @P1 @MVP
  Scenario: System Admin creates a new district with valid data
    When I create a district with:
      | Field  | Value      |
      | Name   | Demo District |
      | Suffix | demo       |
    Then the district should be created successfully
    And the district should have ID assigned
    And the district "Name" should be "Demo District"
    And the district "Suffix" should be "demo"
    And the district "CreatedAtUtc" should be set to current timestamp
    And a "DistrictCreated" domain event should be emitted
    And an audit record should be created with action "CreateDistrict"

  @P1 @MVP
  Scenario: System Admin cannot create district with duplicate suffix
    Given a district exists with suffix "demo"
    When I attempt to create a district with:
      | Field  | Value           |
      | Name   | Another District |
      | Suffix | demo             |
    Then the operation should fail with error "Suffix 'demo' is already in use"
    And no district should be created
    And no audit record should be created

  @P1 @MVP
  Scenario: System Admin creates district with invalid name (too short)
    When I attempt to create a district with:
      | Field  | Value |
      | Name   | AB    |
      | Suffix | test  |
    Then the operation should fail with validation error "Name must be between 3 and 100 characters"

  @P1 @MVP
  Scenario: System Admin creates district with invalid suffix (special characters)
    When I attempt to create a district with:
      | Field  | Value          |
      | Name   | Test District  |
      | Suffix | test_district! |
    Then the operation should fail with validation error "Suffix must contain only lowercase letters, numbers, dots, and hyphens"

  @P1 @MVP
  Scenario: System Admin views all active districts
    Given districts exist:
      | Name          | Suffix  |
      | District One  | one     |
      | District Two  | two     |
      | District Three| three   |
    When I request the list of active districts
    Then I should see 3 districts
    And the districts should be ordered by name
    And each district should include:
      | Field         |
      | Id            |
      | Name          |
      | Suffix        |
      | CreatedAtUtc  |

  @P1 @MVP
  Scenario: System Admin updates district name and suffix
    Given a district exists with:
      | Field  | Value         |
      | Name   | Old District  |
      | Suffix | old           |
    When I update the district with:
      | Field  | Value         |
      | Name   | New District  |
      | Suffix | new           |
    Then the district should be updated successfully
    And the district "Name" should be "New District"
    And the district "Suffix" should be "new"
    And the district "UpdatedAtUtc" should be set to current timestamp
    And a "DistrictUpdated" domain event should be emitted
    And an audit record should be created with:
      | Field       | Value           |
      | Action      | UpdateDistrict  |
      | BeforePayload | Contains old values |
      | AfterPayload  | Contains new values |

  @P1 @MVP
  Scenario: System Admin cannot update district suffix to duplicate value
    Given districts exist:
      | Name          | Suffix  |
      | District One  | one     |
      | District Two  | two     |
    When I attempt to update "District One" with suffix "two"
    Then the operation should fail with error "Suffix 'two' is already in use"
    And the district "Suffix" should remain "one"

  @P1 @MVP
  Scenario: System Admin soft-deletes a district
    Given a district exists with:
      | Field  | Value         |
      | Name   | Test District |
      | Suffix | test          |
    When I delete the district
    Then the district should be soft-deleted
    And the district "DeletedAt" should be set to current timestamp
    And a "DistrictDeleted" domain event should be emitted
    And an audit record should be created with action "DeleteDistrict"
    And the district should not appear in active district list

  @P1 @MVP
  Scenario: System Admin deletes district with active admins (cascade revoke)
    Given a district exists with suffix "test"
    And the district has 2 active district admins
    When I delete the district
    Then the district should be soft-deleted
    And all 2 district admins should be revoked
    And "DistrictAdminRevoked" events should be emitted for each admin
    And audit records should be created for admin revocations

  @P1 @MVP
  Scenario: Idempotency prevents duplicate district creation
    Given I create a district with:
      | Field  | Value      |
      | Name   | Test District |
      | Suffix | test       |
    When I attempt to create the same district again within 10 minutes
    Then the operation should return the existing district ID
    And no new district should be created
    And no additional audit record should be created

  @P1 @MVP
  Scenario: District creation after idempotency window expires
    Given I created a district with suffix "test" 11 minutes ago
    When I create a new district with the same data
    Then the operation should fail with error "Suffix 'test' is already in use"
    And no new district should be created

  @P1 @MVP
  Scenario: System Admin retrieves district by ID
    Given a district exists with:
      | Field  | Value         |
      | Name   | Test District |
      | Suffix | test          |
    When I request the district by ID
    Then I should receive the district details
    And the response should include:
      | Field           |
      | Id              |
      | Name            |
      | Suffix          |
      | CreatedAtUtc    |
      | UpdatedAtUtc    |
      | IsDeleted       |
      | ActiveAdminCount    |
      | PendingAdminCount   |
      | RevokedAdminCount   |

  @P1 @MVP
  Scenario: Soft-deleted districts are excluded from active list
    Given districts exist:
      | Name          | Suffix  | DeletedAt      |
      | Active One    | one     |                |
      | Deleted Two   | two     | 2025-10-20     |
      | Active Three  | three   |                |
    When I request the list of active districts
    Then I should see 2 districts
    And "Deleted Two" should not be in the list

  @P2
  Scenario: District suffix is case-insensitive
    Given a district exists with suffix "demo"
    When I attempt to create a district with suffix "DEMO"
    Then the operation should fail with error "Suffix 'DEMO' is already in use"

  @P2
  Scenario: District name is trimmed before validation
    When I create a district with:
      | Field  | Value              |
      | Name   |   Test District    |
      | Suffix | test               |
    Then the district "Name" should be "Test District"
    And the name should not have leading or trailing spaces

  @P3
  Scenario: Pagination for large district lists
    Given 100 districts exist
    When I request districts with page size 20 and page 2
    Then I should see 20 districts
    And the response should include pagination metadata:
      | Field      | Value |
      | Page       | 2     |
      | PageSize   | 20    |
      | TotalCount | 100   |
      | TotalPages | 5     |
