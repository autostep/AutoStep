using System.Threading.Tasks;
using AutoStep.Tests.Compiler.FullFiles;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Compiler.Parsing
{
    public class FullFileTests : CompilerTestBase
    {
        public FullFileTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task SingleScenarioSimple()
        {
            await CompileAndAssertSuccess(Files.SingleScenarioSimple, file => file
                    .Feature("My Feature", 9, 1, feat => feat
                        .Option("Option1", 1, 1)
                        .Option("Option2", "Setting 1", 2, 1)
                        .Option("Option1", "Setting 2", 3, 1)
                        .Tag("Tag1", 6, 1)
                        .Tag("Tag2", 7, 1)
                        .Description($"This is a description only{NewLine}partly this is part of the description")
                        .Scenario("Setup", 16, 3, scen => scen
                            .Tag("scenariotag", 14, 3)
                            .Option("scenarioinstruction", 15, 3)
                            .Given("I have logged in to my app as 'USER', password 'PWD'", 17, 5)
                                .And("I have turned on the global system config flag", StepType.Given, 18, 9)
                                .And("the date/time is ':Tomorrow at 13:00'", StepType.Given, 21, 9)
                            .Given("I have selected 'Client Management' -> 'Client Location' in the menu", 23, 5)
                            .Then("the 'Client Management - Client Location' page should be displayed", 24, 5)
                            .When("I press 'Add'", 26, 5)
                            .Then("the 'Client Management - Client Location - Add' page should be displayed", 27, 5)
                            .Given("I have entered 'My Name' into 'Name'", 29, 5)
                                .And("I have entered 'My Code' into 'Code'", StepType.Given, 30, 7)
                                .And("I have selected 'A Type' in the 'Client Type' dropdown", StepType.Given, 31, 7)
                    )));
        }
    }
}
