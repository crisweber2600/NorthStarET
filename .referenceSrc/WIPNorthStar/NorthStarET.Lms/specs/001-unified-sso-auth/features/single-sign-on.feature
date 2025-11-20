Feature: Single Sign-On Access Across All Portals
  All personas authenticate through Microsoft Entra once and navigate between LMS portals without re-authentication prompts.

  Background:
    Given the Microsoft Entra tenant is reachable
  And the LMS Identity module is available

  Scenario: Unauthenticated user is redirected to Entra sign-in
    Given a user attempts to open the LMS dashboard without an active session
    When the user requests any protected LMS route
    Then the system redirects the user to Microsoft Entra sign-in

  Scenario: Successful Entra authentication establishes an LMS session
    Given a user completes the Microsoft Entra sign-in flow with valid credentials
    When the LMS exchanges the Entra token for an LMS access token
    Then the LMS issues a session with an active tenant context
    And the user is redirected to the original destination

  Scenario: Active session enables seamless navigation between portals
    Given the user has an active LMS session from the instruction portal
    When the user navigates to the admin portal within the same browser session
    Then the user gains access without an additional authentication prompt

  Scenario: User context is shown after authentication
    Given the user has an active LMS session
    When the user views any LMS portal interface
    Then the UI displays the user name, role, and active tenant context
