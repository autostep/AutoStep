using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Test.Parsing
{
    public class BackgroundTests : CompilerTestBase
    {
        public BackgroundTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FeatureCanHaveBackground()
        {
            const string TestFile =
            @"                
              Feature: My Feature 
                Description               

                Background:
                    Given I have run this
                      And I have done that

                Scenario: Empty Scenario
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Description("Description")
                    .Background(5, 17, back => back
                        .Given("I have run this", 6, 21)
                        .And("I have done that", StepType.Given, 7, 23)
                    )
                    .Scenario("Empty Scenario", 9, 17)
                )
            );
        }
    }
}
