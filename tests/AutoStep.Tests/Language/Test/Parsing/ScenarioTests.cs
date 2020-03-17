﻿using AutoStep.Language;
using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Test.Parsing
{
    public class ScenarioTests : CompilerTestBase
    {
        public ScenarioTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ScenarioCanHaveNoSteps()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17)
                )
            );
        }

        [Fact]
        public async Task ScenarioBlankTitle()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario:

            ";

            await CompileAndAssertErrors(TestFile,
                new LanguageOperationMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.NoScenarioTitleProvided,
                                    "Scenarios must have a title.",
                                    4, 17, 4, 25));
        }

        [Fact]
        public async Task ScenarioEachStepType()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have
                    Then I have
                    When I have
                    And I have

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have", 6, 21)
                        .Then("I have", 7, 21)
                        .When("I have", 8, 21)
                        .And("I have", StepType.When, 9, 21)
                )
            ));
        }

        [Fact]
        public async Task BadScenarioTokenSyntaxError()
        {
            const string TestFile =
            @"
              Feature: My Feature
                Description words

                ScenariO: My Scenario

            ";

            await CompileAndAssertErrors(TestFile,
                new LanguageOperationMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.InvalidScenarioKeyword,
                    "The 'Scenario' keyword is case-sensitive, so 'ScenariO:' should be 'Scenario:'",
                    startLineNo: 5,
                    startColumn: 17,
                    endLineNo: 5,
                    endColumn: 25
                ),
                new LanguageOperationMessage(
                    null,
                    CompilerMessageLevel.Warning,
                    CompilerMessageCode.NoScenarios,
                    "Your Feature 'My Feature' has no Scenarios, so will not run any tests.",
                    startLineNo: 2,
                    startColumn: 15,
                    endLineNo: 2,
                    endColumn: 33
                )
            );
        }

        [Fact]
        public async Task UnexpectedExampleGivesError()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have done something

                Examples:
                    | header1 |
                    | value1  |
            ";

            await CompileAndAssertErrors(TestFile,
                new LanguageOperationMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.NotExpectingExample,
                    "Not expecting an Examples block here; did you mean to define 'My Scenario' as a Scenario Outline rather than a Scenario?",
                    8, 17, 8, 25
                )
            );
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/38")]
        public async Task ScenarioNameWithStepPrefixes()
        {
            const string TestFile =
            @"
              Feature: Test Feature

                Scenario: Test the Given, When, Then functionality
            ";

            await CompileAndAssertSuccess(TestFile, cfg => cfg
                .Feature("Test Feature", 2, 15, feat => feat
                    .Scenario("Test the Given, When, Then functionality", 4, 17)
                )
            );
        }
    }
}
