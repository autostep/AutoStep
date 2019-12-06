using AutoStep.Compiler.Tests.Utils;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests
{
    public class FeatureCompileTests : CompilerTestBase
    {
        public FeatureCompileTests(ITestOutputHelper output) : base(output)
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

            await CompileAndAssertMessages(TestFile, 
                new CompilerMessage (
                    null,
                    CompilerMessageLevel.Warning,
                    CompilerMessageCode.NoScenarios,
                    "Feature 'My Feature' contains no scenarios.",
                    startLineNo: 2,
                    startColumn: 15,
                    endLineNo: 2,
                    endColumn: 15
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

            await CompileAndAssertMessages(TestFile,
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

            await CompileSuccessNoWarnings(TestFile, file => file
                .Feature("My Feature",  feat => feat
                    .Description("Feature Description")
                    .Scenario("My Scenario", scen => scen
                        .Given("I have clicked on")
                        .And("I have gone to")
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

            await CompileSuccessNoWarnings(TestFile, file => file
                .Feature("My Feature", feat => feat
                   .Description("Feature Description")
                   .Scenario("My Scenario", scen => scen
                       .Given("I have clicked on")
                       .And("I have gone to")
                   )
                )
            );
        }
    }
}
