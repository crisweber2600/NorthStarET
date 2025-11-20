Feature: District Admin Manages Schools
    As a district admin
    I want to manage my district's school catalog
    So that I can maintain an accurate organizational structure

    Background:
        Given a district "Northstar District" exists with ID "d1234567-89ab-cdef-0123-456789abcdef"
        And I am authenticated as a district admin for "Northstar District"

    @US1 @P1
    Scenario: District admin views school list scoped to their district
        Given the following schools exist in "Northstar District":
            | Name              | Code  | Status |
            | Lincoln Elementary| LIN   | Active |
            | Washington High   | WASH  | Active |
            | Jefferson Middle  | JEFF  | Active |
        When I navigate to "School Management"
        Then I should see 3 schools in the list
        And I should see "Lincoln Elementary" in the school list
        And I should see "Washington High" in the school list
        And I should see "Jefferson Middle" in the school list
        And I should see search and sort controls

    @US1 @P1
    Scenario: District admin creates a new school
        When I click "Create School" button
        And I fill in school name with "Roosevelt Elementary"
        And I fill in school code with "ROOS"
        And I select grades "PreK,K,Grade1,Grade2,Grade3,Grade4,Grade5"
        And I click "Create School" button
        Then I should see "School created successfully" confirmation
        And "Roosevelt Elementary" should appear in the school list within 2 seconds
        And the school should have code "ROOS"

    @US1 @P1
    Scenario: District admin updates an existing school
        Given a school "Lincoln Elementary" with code "LIN" exists in "Northstar District"
        When I click the edit icon for "Lincoln Elementary"
        And I change the school name to "Abraham Lincoln Elementary"
        And I change the code to "ALIN"
        And I click "Update School" button
        Then I should see "School updated successfully" confirmation
        And the school list should show "Abraham Lincoln Elementary" within 2 seconds
        And the school should have code "ALIN"

    @US1 @P1
    Scenario: District admin deletes a school with confirmation
        Given a school "Old School" exists in "Northstar District"
        When I click the delete icon for "Old School"
        Then I should see a delete confirmation modal
        When I click "Delete School" button in the modal
        Then I should see "School deleted successfully" confirmation
        And "Old School" should disappear from the school list within 2 seconds

    @US1 @P1
    Scenario: District admin cancels school deletion
        Given a school "Lincoln Elementary" exists in "Northstar District"
        When I click the delete icon for "Lincoln Elementary"
        And I should see a delete confirmation modal
        When I click "Cancel" button in the modal
        Then the modal should close
        And "Lincoln Elementary" should remain in the school list

    @US1 @P1
    Scenario: School name must be unique within district
        Given a school "Lincoln Elementary" exists in "Northstar District"
        When I click "Create School" button
        And I fill in school name with "Lincoln Elementary"
        And I click "Create School" button
        Then I should see an error "A school with this name already exists in your district"
        And the school should not be created

    @US1 @P1
    Scenario: School code must be unique within district when provided
        Given a school "Lincoln Elementary" with code "LIN" exists in "Northstar District"
        When I click "Create School" button
        And I fill in school name with "New School"
        And I fill in school code with "LIN"
        And I click "Create School" button
        Then I should see an error "A school with this code already exists in your district"
        And the school should not be created

    @US1 @P1
    Scenario: District admin searches schools by name
        Given the following schools exist in "Northstar District":
            | Name              | Code  |
            | Lincoln Elementary| LIN   |
            | Washington High   | WASH  |
            | Jefferson Middle  | JEFF  |
        When I enter "Lincoln" in the search box
        Then I should see 1 school in the filtered list
        And I should see "Lincoln Elementary" in the school list
        And I should not see "Washington High" in the school list

    @US1 @P1
    Scenario: District admin searches schools by code
        Given the following schools exist in "Northstar District":
            | Name              | Code  |
            | Lincoln Elementary| LIN   |
            | Washington High   | WASH  |
            | Jefferson Middle  | JEFF  |
        When I enter "WASH" in the search box
        Then I should see 1 school in the filtered list
        And I should see "Washington High" in the school list
        And I should not see "Lincoln Elementary" in the school list

    @US1 @P1
    Scenario: District admin cannot see schools from other districts
        Given a district "Other District" exists with ID "d9876543-21fe-dcba-9876-543210fedcba"
        And the following schools exist in "Other District":
            | Name              |
            | Other School      |
        When I navigate to "School Management"
        Then I should not see "Other School" in the school list
        And I should only see schools from "Northstar District"
