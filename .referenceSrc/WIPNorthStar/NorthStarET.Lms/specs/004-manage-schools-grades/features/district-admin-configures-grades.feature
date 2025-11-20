Feature: District Admin Configures School Grades
    As a district admin
    I want to configure which grades each school serves
    So that I can accurately define the scope of each school

    Background:
        Given a district "Northstar District" exists with ID "d1234567-89ab-cdef-0123-456789abcdef"
        And I am authenticated as a district admin for "Northstar District"

    @US2 @P1
    Scenario: District admin assigns individual grades via checklist
        Given a school "Lincoln Elementary" with code "LIN" exists in "Northstar District"
        When I click the edit icon for "Lincoln Elementary"
        And I navigate to the "Grades" tab
        And I select grades "K,Grade1,Grade2,Grade3,Grade4,Grade5"
        And I click "Save Grades" button
        Then I should see "Grades updated successfully" confirmation
        And the school should serve grades "K,Grade1,Grade2,Grade3,Grade4,Grade5"

    @US2 @P1
    Scenario: District admin selects a grade range
        Given a school "Washington High" with code "WASH" exists in "Northstar District"
        When I click the edit icon for "Washington High"
        And I navigate to the "Grades" tab
        And I select grade range from "Grade9" to "Grade12"
        And I click "Save Grades" button
        Then I should see "Grades updated successfully" confirmation
        And the school should serve grades "Grade9,Grade10,Grade11,Grade12"

    @US2 @P1
    Scenario: District admin uses select-all helper
        Given a school "Community School" with code "COMM" exists in "Northstar District"
        When I click the edit icon for "Community School"
        And I navigate to the "Grades" tab
        And I click "Select All Grades" button
        And I click "Save Grades" button
        Then I should see "Grades updated successfully" confirmation
        And the school should serve all available grades

    @US2 @P1
    Scenario: District admin modifies existing grade assignments
        Given a school "Jefferson Middle" with code "JEFF" exists in "Northstar District"
        And the school currently serves grades "Grade6,Grade7,Grade8"
        When I click the edit icon for "Jefferson Middle"
        And I navigate to the "Grades" tab
        And I deselect grade "Grade8"
        And I select grade "Grade5"
        And I click "Save Grades" button
        Then I should see "Grades updated successfully" confirmation
        And the school should serve grades "Grade5,Grade6,Grade7"

    @US2 @P1
    Scenario: System validates at least one grade is selected
        Given a school "Test School" exists in "Northstar District"
        When I click the edit icon for "Test School"
        And I navigate to the "Grades" tab
        And I deselect all grades
        And I click "Save Grades" button
        Then I should see an error message "At least one grade must be selected"
        And the grades should not be saved

    @US2 @P1
    Scenario: Grade changes reflect immediately on school detail
        Given a school "Roosevelt Elementary" with code "ROOS" exists in "Northstar District"
        And the school currently serves grades "PreK,K,Grade1,Grade2,Grade3"
        When I click the edit icon for "Roosevelt Elementary"
        And I navigate to the "Grades" tab
        And I select grade range from "K" to "Grade5"
        And I click "Save Grades" button
        And I navigate to the school detail view
        Then the school detail should display grades "K,Grade1,Grade2,Grade3,Grade4,Grade5"
        And the grade list should be ordered sequentially
