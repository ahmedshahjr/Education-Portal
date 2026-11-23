@MasterPortal
Feature: Master portal

Scenario: The master administrator reviews the plans and price books
    Given a signed-in master administrator
    When the user opens the "/pricing" page
    Then the master page shows the heading "Plans and prices"
    And the master page shows the heading "Plans"
    And the master page shows the heading "Price books"

Scenario: The master administrator reviews the module catalogue
    Given a signed-in master administrator
    When the user opens the "/modules" page
    Then the master page shows the heading "Modules"

Scenario: A visitor who is not signed in is sent to sign in before a master page
    Given a logged out user
    When the user opens the "/pricing" page
    Then the sign-in form is shown
