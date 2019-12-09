using AutoStep.Compiler.Tests.Utils;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests
{
    public class DescriptionTests : CompilerTestBase
    {
        public DescriptionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task FeatureSingleLineDescription()
        {
            const string TestFile = 
            @"                
              Feature: My Feature
                Description words
            ";

            await CompileAndAssert(TestFile, file => file
                    .Feature("My Feature", 2, 15, feat => feat
                        .Description("Description words")
            ));
        }

        [Fact]
        public async Task FeatureMultiLineDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                Description words
                Line 2
                Line 3
            ";

            await CompileAndAssert(TestFile, file => file
                    .Feature("My Feature", 2, 15, feat => feat
                        .Description("Description words\r\nLine 2\r\nLine 3")
            ));
        }

        [Fact]
        public async Task FeatureIndentedDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                Line 1
                    Line 2
                        Line 3
                    Line 4
                Line 5
            ";

            await CompileAndAssert(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Description("Line 1\r\n    Line 2\r\n        Line 3\r\n    Line 4\r\nLine 5")
            ));
        }

        [Fact]
        public async Task FeatureSplitLinesDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                Line 1

                    Line 2

                        Line 3

                    Line 4

                Line 5
            ";

            await CompileAndAssert(TestFile, file => file
                    .Feature("My Feature", 2, 15, feat => feat
                        .Description("Line 1\r\n\r\n    Line 2\r\n\r\n        Line 3\r\n\r\n    Line 4\r\n\r\nLine 5")
            ));
        }

        [Fact]
        public async Task FeatureDescriptionEmbeddedComments()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                Line 1 # Comment 1
                    # Comment 2
                    Line 2 # Comment 3
                        # Comment
                        Line 3 # #
                            
                    Line 4

                Line 5
                # Comment
            ";

            await CompileAndAssert(TestFile, file => file
                    .Feature("My Feature", 2, 15, feat => feat
                        .Description("Line 1\r\n\r\n    Line 2\r\n\r\n        Line 3\r\n\r\n    Line 4\r\n\r\nLine 5")
            ));
        }

        [Fact]
        public async Task FeatureDescriptionIgnoresTrailingBlankLines()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                Line 1
                    Line 2





            ";

            await CompileAndAssert(TestFile, file => file
                    .Feature("My Feature", 2, 15, feat => feat
                        .Description("Line 1\r\n    Line 2")
            ));
        }


        [Fact]
        public async Task FeatureDescriptionIgnoresLeadingBlankLines()
        {
            const string TestFile =
            @"                
              Feature: My Feature




                Line 1
                    Line 2


            ";

            await CompileAndAssert(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Description("Line 1\r\n    Line 2")
            ));
        }

        [Fact]
        public async Task ScenarioSingleLineDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario
                    Line 1

                    Given I have
            ";

            await CompileAndAssert(TestFile, file => file
                    .Feature("My Feature", 2, 15, feat => feat
                        .Scenario("My Scenario", 4, 17, scen => scen
                        .Description("Line 1")
                        .Given("I have", 7, 21)                        
            )));
        }

        [Fact]
        public async Task ScenarioMultiLineDescriptionNoSteps()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario
                    Line 1
                    Line 2
                    Line 3
            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Description("Line 1\r\nLine 2\r\nLine 3")
            )));
        }

        [Fact]
        public async Task ScenarioMultiLineDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario
                    Line 1
                    Line 2
                    Line 3

                    Given I have
            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Description("Line 1\r\nLine 2\r\nLine 3")
                        .Given("I have", 9, 21)
            )));
        }

        [Fact]
        public async Task ScenarioIndentedDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario
                Line 1
                    Line 2
                        Line 3
                    Line 4
                Line 5

                    Given I have
            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Description("Line 1\r\n    Line 2\r\n        Line 3\r\n    Line 4\r\nLine 5")
                        .Given("I have", 11, 21)
            )));
        }

        [Fact]
        public async Task ScenarioSplitLinesDescription()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario
                Line 1

                    Line 2

                        Line 3

                    Line 4

                Line 5
            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Description("Line 1\r\n\r\n    Line 2\r\n\r\n        Line 3\r\n\r\n    Line 4\r\n\r\nLine 5")
            )));
        }

        [Fact]
        public async Task ScenarioDescriptionEmbeddedComments()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario: My Scenario
                Line 1 # Comment 1
                    # Comment 2
                    Line 2 # Comment 3
                        # Comment
                        Line 3 # #
                            
                    Line 4

                Line 5
                # Comment

                Given I have
            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Description("Line 1\r\n\r\n    Line 2\r\n\r\n        Line 3\r\n\r\n    Line 4\r\n\r\nLine 5")
                        .Given("I have", 16, 17)
            )));
        }

        [Fact]
        public async Task ScenarioDescriptionIgnoresTrailingBlankLines()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Line 1
                        Line 2

                



            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                        .Scenario("My Scenario", 4, 17, scen => scen
                            .Description("Line 1\r\n    Line 2")
            )));
        }


        [Fact]
        public async Task ScenarioDescriptionIgnoresLeadingBlankLines()
        {
            const string TestFile =
            @"                
              Feature: My Feature


                Scenario: My Scenario



                Line 1
                    Line 2

            ";

            await CompileAndAssert(TestFile, file =>
                file.Feature("My Feature", 2, 15, feat => feat
                        .Scenario("My Scenario", 5, 17, scen => scen
                            .Description("Line 1\r\n    Line 2")
            )));
        }
    }
}
