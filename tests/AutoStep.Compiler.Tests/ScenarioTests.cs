using AutoStep.Compiler.Tests.Builders;
using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests
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
                new CompilerMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.InvalidScenarioKeyword,
                    "The 'Scenario' keyword is case-sensitive, so 'ScenariO:' should be 'Scenario:'",
                    startLineNo: 5,
                    startColumn: 17,
                    endLineNo: 5,
                    endColumn: 25
                ),
                new CompilerMessage(
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
    }
}
