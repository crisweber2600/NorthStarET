Feature: Authorization decisions under 50 milliseconds
  Personas need immediate allow or deny feedback when executing protected actions.

  Background:
    Given the LMS Identity module is available
    And authorization caching is warmed for previously granted actions

  Scenario: Allowed action is returned in under 50 milliseconds
    Given a district admin has an active LMS session for tenant "Riverbend District"
    When the user requests to "ManageDistrictSettings"
    Then the authorization decision should be allowed
    And the response time should be below 50 milliseconds
    And the authorization audit log records an allowed entry

  Scenario: Denied action is cached and surfaced with context
    Given a teacher has an active LMS session for tenant "Riverbend District"
    When the user requests to "ManageDistrictSettings"
    Then the authorization decision should be denied
    And the authorization audit log records a denied entry with reason
