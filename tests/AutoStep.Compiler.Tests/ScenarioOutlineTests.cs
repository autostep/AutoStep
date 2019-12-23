using AutoStep.Compiler.Tests.Builders;
using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests
{
    public class ScenarioOutlineTests : CompilerTestBase
    {
        public ScenarioOutlineTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ScenarioOutlineCanBeDefined()
        {
            const string TestFile = 
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I am in a scenario outline

                $opt1
                @tag1
                Examples:
                    | heading1   | heading2   |
                    | something1 | something2 | 
                
                $opt2
                @tag2
                Examples:
                    | heading1   | heading2   |
                    | new1       | new2       |

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I am in a scenario outline", 6, 21)
                        .Examples(10, 17, example => example 
                            .Option("opt1", 8, 17)
                            .Tag("tag1", 9, 17)
                            .Table(11, 21, tab => tab
                                .Headers(11, 21, 
                                    ("heading1", 23, 30),
                                    ("heading2", 36, 43)
                                )
                                .Row(12, 21, 
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 36, 45, null)
                                )                                
                            )
                        )
                        .Examples(16, 17, example => example
                            .Option("opt2", 14, 17)
                            .Tag("tag2", 15, 17)
                            .Table(17, 21, tab => tab
                                .Headers(17, 21,
                                    ("heading1", 23, 30),
                                    ("heading2", 36, 43)
                                )
                                .Row(18, 21,
                                    (ArgumentType.Text, "new1", 23, 32, null),
                                    (ArgumentType.Text, "new2", 36, 45, null)
                                )
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task ExamplesKeywordCasing()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I am in a scenario outline

                ExampLes:
                    | heading1   | heading2   |
                    | something1 | something2 |
            ";

            await CompileAndAssertErrors(TestFile, 
                new CompilerMessage(null, 
                CompilerMessageLevel.Error, CompilerMessageCode.InvalidExamplesKeyword,
                "The 'Examples' keyword is case-sensitive, so 'ExampLes:' should be 'Examples:'",
                8, 17, 8, 25
                )
            );
        }


        [Fact]
        public async Task ExamplesWithNoTableFollowedByEofProducesError()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I am in a scenario outline

                Examples:
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null,
                CompilerMessageLevel.Error, CompilerMessageCode.ExamplesBlockRequiresTable,
                "Examples blocks must contain a table.",
                8, 17, 8, 25
                )
            );
        }
        
        [Fact]
        public async Task ScenarioOutlineBlankTitle()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline:

            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.NoScenarioOutlineTitleProvided,
                                    "Scenario Outlines must have a title.",
                                    4, 17, 4, 33));
        }


        [Fact]
        public async Task ExamplesWithNoTableFollowedByScenarioProducesError()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I am in a scenario outline

                Examples:

                Scenario: My Scenario

                    Given a scenario
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null,
                CompilerMessageLevel.Error, CompilerMessageCode.ExamplesBlockRequiresTable,
                "Examples blocks must contain a table.",
                8, 17, 8, 25
                )
            );
        }
    }
}
