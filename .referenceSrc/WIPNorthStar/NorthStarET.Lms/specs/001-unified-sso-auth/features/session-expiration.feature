Feature: Graceful Session Expiration and Renewal
  Detect expired sessions, present a unified renewal prompt, and support transparent token refresh without cascading 401 errors.

  Background:
    Given the Microsoft Entra tenant is reachable
    And the LMS Identity module is available

  Scenario: Expired session triggers renewal prompt
    Given a user has an active LMS session
    And the session has exceeded its expiration time
    When the user attempts to access a protected resource
    Then the system detects the expired session
    And the user is prompted for session renewal

  Scenario: User renews session via Entra re-authentication
    Given a user receives a session expiration prompt
    When the user re-authenticates via Microsoft Entra
    Then the LMS refreshes the session tokens
    And the user is redirected to their original context
    And no cascading 401 errors are generated

  Scenario: Background token refresh extends session automatically
    Given a user has an active LMS session nearing expiration
    When the token refresh service runs in the background
    Then the session tokens are refreshed transparently
    And the user continues working without interruption
    And the session expiration time is extended

  Scenario: Session revocation removes all access
    Given a user has an active LMS session
    When an administrator revokes the user session
    Then the session is removed from Redis cache
    And the session record is marked as revoked
    And subsequent requests with the session token are denied

  Scenario: Multiple sessions for same user are managed independently
    Given a user has active sessions in two different browsers
    When one session expires
    Then only that session requires renewal
    And the other session remains active
    And both sessions can be renewed independently
