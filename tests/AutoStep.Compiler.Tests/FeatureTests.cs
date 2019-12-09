using AutoStep.Compiler.Tests.Utils;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests
{
    public class FeatureTests : CompilerTestBase
    {
        public FeatureTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FeatureWithNoScenariosProducesWarning()
        {
            const string TestFile = 
            @"                
              Feature: My Feature
                Description words

            ";

            await CompileAndAssertWarnings(TestFile,
                new CompilerMessage (
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
        public async Task BadFeatureTokenSyntaxError()
        {
            const string TestFile =
            @"                
              FeaturE: My Feature
                Description words

            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.SyntaxError,
                    "Syntax Error: missing 'Feature:' at 'FeaturE:'",
                    startLineNo: 2,
                    startColumn: 15,
                    endLineNo: 2,
                    endColumn: 22
                )
            );
        }

        [Fact]
        public async Task FeatureWithSingleScenarioPasses()
        {
            const string TestFile =
            @"
                Feature: My Feature
                    Feature Description

                    Scenario: My Scenario

                        Given I have clicked on
                        And I have gone to

            ";

            await CompileAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Description("Feature Description")
                    .Scenario("My Scenario", 5, 21, scen => scen
                        .Given("I have clicked on", 7, 25)
                        .And("I have gone to", 8, 25)
                    )
                )
            );
        }


        [Fact]
        public async Task CommentedFeatureWithScenarioPasses()
        {
            const string TestFile =
            @"
                # Comment 1
                Feature: My Feature # Comment 2
                    Feature Description # Description Comment
                    # Line by itself
                    Scenario: My Scenario # scenario comment
                        Scenario Description
                        # Comment
                        Given I have clicked on # comment
                        # Splitting comment
                        And I have gone to #statement comment

            ";

            await CompileAssertSuccess(TestFile, file => file
                .Feature("My Feature", 3, 17, feat => feat
                   .Description("Feature Description")
                   .Scenario("My Scenario", 6, 21,  scen => scen
                       .Description("Scenario Description")
                       .Given("I have clicked on", 9, 25)
                       .And("I have gone to", 11, 25)
                   )
                )
            );
        }
    }
}
