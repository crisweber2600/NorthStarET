Feature: Seamless Tenant Context Switching
  Multi-tenant administrators can switch between tenant contexts under 200ms with instant UI updates and cached tenant facts.

  Background:
    Given the LMS Identity module is available
    And the user has an active LMS session

  Scenario: Multi-tenant admin lists available tenants
    Given a user with memberships in multiple tenants
    When the user requests their available tenants
    Then the system returns all tenants where the user has membership
    And each tenant includes name, id, and user's role
    And the response completes in under 200 milliseconds

  Scenario: User switches to different tenant context
    Given a user with active tenant "Lincoln Elementary"
    And the user has membership in tenant "Washington Middle School"
    When the user switches to tenant "Washington Middle School"
    Then the session's active tenant is updated to "Washington Middle School"
    And the authorization cache is cleared for the previous tenant
    And a TenantSwitchedEvent is raised
    And the switch completes in under 200 milliseconds

  Scenario: Switched context reflects in authorization decisions
    Given a user switches from "Lincoln Elementary" to "Washington Middle School"
    When the user requests to perform an action
    Then authorization is evaluated in the context of "Washington Middle School"
    And previous tenant "Lincoln Elementary" permissions do not apply
    And cached authorization data reflects the new tenant

  Scenario: User cannot switch to tenant without membership
    Given a user with active tenant "Lincoln Elementary"
    And the user has no membership in tenant "Jefferson High School"
    When the user attempts to switch to tenant "Jefferson High School"
    Then the system denies the tenant switch
    And the active tenant remains "Lincoln Elementary"
    And an error message indicates insufficient permissions

  Scenario: Tenant switch invalidates stale cache entries
    Given a user has cached authorization for tenant "Lincoln Elementary"
    And the user switches to tenant "Washington Middle School"
    When the user performs an action requiring authorization
    Then the system queries fresh authorization data for "Washington Middle School"
    And cached data from "Lincoln Elementary" is not used
    And new authorization results are cached for "Washington Middle School"

  Scenario: Rapid tenant switching handles concurrent requests
    Given a user with memberships in three tenants
    When the user switches tenants twice in quick succession
    Then each switch updates the session atomically
    And the final active tenant reflects the last switch
    And no race conditions corrupt the session state
