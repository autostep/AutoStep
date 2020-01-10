using System.Threading.Tasks;
using AutoStep.Compiler;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Compiler.Parsing
{
    public class AnnotationTests : CompilerTestBase
    {
        public AnnotationTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FeatureAnnotationMix()
        {
            const string TestFile =
            @"
              @tag1
              $opt1
              @tag2
              $opt2:setting2
              @tag3
              $opt3:setting3
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 8, 15, feat => feat
                    .Tag("tag1", 2, 15)
                    .Option("opt1", 3, 15)
                    .Tag("tag2", 4, 15)
                    .Option("opt2", "setting2", 5, 15)
                    .Tag("tag3", 6, 15)
                    .Option("opt3", "setting3", 7, 15)
                    .Scenario("My Scenario", 10, 17, scen => scen
                        .Given("I have", 12, 21)
            )));
        }

        [Fact]
        public async Task FeatureAnnotationComments()
        {
            const string TestFile =
            @"
              # tag comment
              @tag1 # tag comment
              $opt1 # tag comment
              @tag2# tag comment
              # tag comment
              $opt2:setting2#tag comment
              @tag3     # tag comment
              $opt3:setting3 #tag comment
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 10, 15, feat => feat
                    .Tag("tag1", 3, 15)
                    .Option("opt1", 4, 15)
                    .Tag("tag2", 5, 15)
                    .Option("opt2", "setting2", 7, 15)
                    .Tag("tag3", 8, 15)
                    .Option("opt3", "setting3", 9, 15)
                    .Scenario("My Scenario", 12, 17, scen => scen
                        .Given("I have", 14, 21)
            )));
        }

        [Fact]
        public async Task SyntaxErrorTagLeadingWhiteSpaceAfterMarker()
        {
            const string TestFile =
            @"
              @ tag1
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.BadTagFormat,
                "Bad tag format. Tag must have the format '@tagName'.",
                2, 15, 2, 15)
            );
        }

        [Fact]
        public async Task SyntaxErrorOptionLeadingWhiteSpaceAfterMarker()
        {
            const string TestFile =
            @"
              $ opt1
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.BadOptionFormat,
                "Bad option format. Option must the format '$optionName', " +
                "optionally with a value separated by ':', e.g. '$optionName:value'.",
                2, 15, 2, 15)
            );
        }


        [Fact]
        public async Task TagParseOkWithLeadingWhiteSpaceBeforeMarker()
        {
            const string TestFile =
            @"
                @tag1
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 3, 15, feat => feat
                    .Tag("tag1", 2, 17)
                    .Scenario("My Scenario", 5, 17, scen => scen
                        .Given("I have", 7, 21)
            )));
        }

        [Fact]
        public async Task OptParseOkWithLeadingWhiteSpaceBeforeMarker()
        {
            const string TestFile =
            @"
                $opt1
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 3, 15, feat => feat
                    .Option("opt1", 2, 17)
                    .Scenario("My Scenario", 5, 17, scen => scen
                        .Given("I have", 7, 21)
            )));
        }


        [Fact]
        public async Task OptionEmptySettingValueError()
        {
            const string TestFile =
            @"
              $opt1:
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.OptionWithNoSetting,
                "Provided Option 'opt1' has a setting value marker ':', but no value has been provided.",
                2, 15, 2, 20)
            );
        }


        [Fact]
        public async Task OptionWhiteSpaceSettingValueError()
        {
            const string TestFile =
            @"
              $opt1:    
              Feature: My Feature

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.OptionWithNoSetting,
                "Provided Option 'opt1' has a setting value marker ':', but no value has been provided.",
                2, 15, 2, 24)
            );
        }

        [Fact]
        public async Task ScenarioAnnotationMix()
        {
            const string TestFile =
            @"
              Feature: My Feature

                @tag1
                $opt1

                @tag2
                $opt2:setting2

                @tag3
                $opt3:setting3

                Scenario: My Scenario
                
                    Given I have
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 13, 17, scen => scen
                        .Tag("tag1", 4, 17)
                        .Option("opt1", 5, 17)
                        .Tag("tag2", 7, 17)
                        .Option("opt2", "setting2", 8, 17)
                        .Tag("tag3", 10, 17)
                        .Option("opt3", "setting3", 11, 17)
                        .Given("I have", 15, 21)
            )));
        }

    }
}
