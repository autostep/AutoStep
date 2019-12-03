$Playback: Central\Layout\USCentralCashCentre
$Playback: Central\Currency\USCentralCurrency
$Playback: Central\Currency\USCentralMediaNotes
$Playback: Central\Orders\USCentralOrderRules

$OutputPlayback: Central\Clients\USCentralClients

# comments above line
@CashCentreType:Central # comment end of line
@SetupArea:Clients
# comments above feature
Feature: US Central Clients
# comments in feature description
  Given this is a description only and won't match
  partly this is part of the description # but this is a comment
	
@scenariotag
$scenarioinstruction
Scenario: This is a scenario
  This is a scenario description

Scenario Outline: Setup
	Given I have logged in to ISA as 'INSTALL', password '123' # scenario has no description, this is a comment
      And I have turned on the Central Bank system config flag

      #Time travel to June 2019 for Order Rule start date
      And the date/time is ':Tomorrow at 13:00'
  
  Given I have selected 'Client Location Management' -> 'Client Location' in the menu
  Then the 'Client Location Management - Client Location' page should be displayed

  When I press 'Add'
  Then the 'Client Location Management - Client Location - Add' page should be displayed

  Given I have entered '<Name>' into 'Name'
    And I have entered '<Code>' into 'Code'
    And I have selected '<Type>' in the 'Client Location Type' dropdown

	Given I have 'checked' the checkbox for the '<Cash Centre>' row in the 'Depositing Cash Centres' entry grid
		And I have 'checked' the checkbox for the '<Cash Centre>' row in the 'Order Entry Cash Centres' entry grid
		And I have 'checked' the checkbox for the '<Cash Centre>' row in the 'Order Delivery Cash Centres' entry grid

  Given I have selected '<Order Rule 1>' in the 'Order Rule 1' dropdown
    And I have selected 'Main Issuing Centre' in the 'Order Filling Cash Centre 1' dropdown

  Given I have 'checked' the checkbox for the '<Cash Centre>' row in the 'Order Entry Cash Centres' entry grid
		And I have 'checked' the checkbox for the '<Cash Centre>' row in the 'Order Delivery Cash Centres' entry grid

  When I press 'Save'

  Then ISA should have a client location '<Name>'
    And it should have a code of '<Code>'
    And it should have an 'Order Filling Cash Centre 1' of '<Cash Centre>'
		And it should have '<Cash Centre>' in the 'Order Delivery Cash Centre' list

Examples: 

| Name                   | Code   | Type     | Cash Centre         | Order Rule 1 |
| Wells Fargo - Miami    | WF_Mi  | Standard | Main Issuing Centre | Central USD  |
| JPMorgan - Miami       | JMP_Mi | Standard | Main Issuing Centre | Central USD  |
| Wells Fargo - Internal | WF_Int | Internal | Main Issuing Centre | Central USD  |
