using AutoStep.Language;
using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Test.Parsing
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
        public async Task FeatureWithNoTitleError()
        {
            const string TestFile =
            @"                
              Feature: 
            ";

            await CompileAndAssertWarnings(TestFile,
                new LanguageOperationMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.NoFeatureTitleProvided,
                    "Features must have a title.",
                    startLineNo: 2,
                    startColumn: 15,
                    endLineNo: 2,
                    endColumn: 22
                )
            );
        }

        [Fact]
        public async Task FeatureWithNoTitleImmediateEofError()
        {
            const string TestFile =
            @"                
              Feature:";

            await CompileAndAssertWarnings(TestFile,
                new LanguageOperationMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.NoFeatureTitleProvided,
                    "Features must have a title.",
                    startLineNo: 2,
                    startColumn: 15,
                    endLineNo: 2,
                    endColumn: 22
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
                new LanguageOperationMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.InvalidFeatureKeyword,
                    "The 'Feature' keyword is case-sensitive, so 'FeaturE:' should be 'Feature:'",
                    startLineNo: 2,
                    startColumn: 15,
                    endLineNo: 2,
                    endColumn: 22
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Description("Feature Description")
                    .Scenario("My Scenario", 5, 21, scen => scen
                        .Given("I have clicked on", 7, 25)
                        .And("I have gone to", StepType.Given, 8, 25)
                    )
                )
            );
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/37")]
        public async Task FeatureWithDuplicateScenarioNameGivesError()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario
                        
                        Given I have

                    Scenario: My Scenario
                        
                        # Check we can still get errors from a duplicate
                        And I click
            ";

            await CompileAndAssertErrors(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("I have", 6, 25)
                    )
                    .Scenario("My Scenario", 8, 21, scen => scen
                        .And("I click", null, 11, 25)
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.DuplicateScenarioNames, 8, 21, 8, 41, "My Scenario"),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.AndMustFollowNormalStep, 11, 25, 11, 35)
            );
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/37")]
        public async Task FeatureWithDuplicateScenarioNameDetectionCaseInsensitive()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario
                        
                        Given I have

                    Scenario: My scenario
                        
                        # Check we can still get errors from a duplicate
                        And I click
            ";

            await CompileAndAssertErrors(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("I have", 6, 25)
                    )
                    .Scenario("My scenario", 8, 21, scen => scen
                        .And("I click", null, 11, 25)
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.DuplicateScenarioNames, 8, 21, 8, 41, "My scenario"),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.AndMustFollowNormalStep, 11, 25, 11, 35)
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 3, 17, feat => feat
                   .Description("Feature Description")
                   .Scenario("My Scenario", 6, 21, scen => scen
                      .Description("Scenario Description")
                      .Given("I have clicked on", 9, 25)
                      .And("I have gone to", StepType.Given, 11, 25)
                   )
                )
            );
        }

        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/39")]
        public async Task OnlyOneFeatureAllowed()
        {
            const string TestFile =
            @"                
              Feature: Feature 1

                Scenario: My Scenario

              Feature: Feature 2

                Scenario: My Scenario
            ";

            await CompileAndAssertErrors(TestFile,
                LanguageMessageFactory.Create(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.OnlyOneFeatureAllowed,
                    6, 15, 6, 32
                )
            );
        }
        
        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/39")]
        public async Task OnlyOneFeatureAllowedErrorRaisedIfEarlierSyntaxError()
        {
            const string TestFile =
            @"                
              Scenario: Error

              Feature: Feature 1

                Scenario: My Scenario

              Feature: Feature 2

                Scenario: My Scenario
            ";

            await CompileAndAssertErrors(TestFile,
                LanguageMessageFactory.Create(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.SyntaxError,
                    2, 15, 2, 23, "no viable alternative at input '              Scenario:'"),
                LanguageMessageFactory.Create(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.OnlyOneFeatureAllowed,
                    8, 15, 8, 32
                )
            );
        }
        
        [Fact]
        [Issue("https://github.com/autostep/AutoStep/issues/38")]
        public async Task ScenarioOutlineNameWithStepPrefixes()
        {
            const string TestFile =
            @"                
              Feature: Test the Given, When, Then functionality

                Scenario: Scenario 1
            ";

            await CompileAndAssertSuccess(TestFile, cfg => cfg
                .Feature("Test the Given, When, Then functionality", 2, 15, feat => feat
                    .Scenario("Scenario 1", 4, 17)
                )
            );
        }
    }
}
