using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Compiler;

namespace AutoStep.Tests.Compiler.Parsing
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
                                    ("something1", 23, 32, null),
                                    ("something2", 36, 45, null)
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
                                    ("new1", 23, 26, null),
                                    ("new2", 36, 39, null)
                                )
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task ScenarioOutlineCanInsertExamplesVariable()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument '<variable1>'

                Examples:
                    | variable1   | variable2   |
                    | something1  | something2  | 
                
            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<variable1>'", 6, 21, step => step
                            .Text("I")
                            .Text("pass")
                            .Text("an")
                            .Text("argument")
                            .Quote()
                            .Variable("variable1")
                            .Quote()
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(10, 21,
                                    ("something1", 23, 32, c => c.Word("something", 23).Int("1", 32)),
                                    ("something2", 37, 46, c => c.Word("something", 37).Int("2", 46))
                                )
                            )
                        )
                    )
                )
            );
        }


        [Fact]
        public async Task ScenarioOutlineCanInsertExamplesVariableIntoStepTable()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument for this table:
                         | heading1    |
                         | <variable1> |

                Examples:
                    | variable1   | variable2   |
                    | something1  | something2  | 
                
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument for this table:", 6, 21, step => step
                            .Table(7, 26, tab => tab
                                .Headers(7, 26, ("heading1", 28, 35))
                                .Row(8, 26, ("<variable1>", 28, 38, arg => arg
                                    .Variable("variable1", 28)
                                ))
                            )
                        )
                        .Examples(10, 17, example => example
                            .Table(11, 21, tab => tab
                                .Headers(11, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(12, 21,
                                    ("something1", 23, 32, null),
                                    ("something2", 37, 46, null)
                                )
                            )
                        )
                    )
                )
            );
        }


        [Fact]
        public async Task ScenarioOutlineCanInsertMultipleExamplesVariablesIntoStepTable()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument for this table:
                         | heading1                    |
                         | <variable1> and <variable2> |

                Examples:
                    | variable1   | variable2   |
                    | something1  | something2  | 
                
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument for this table:", 6, 21, step => step
                            .Table(7, 26, tab => tab
                                .Headers(7, 26, ("heading1", 28, 35))
                                .Row(8, 26, ("<variable1> and <variable2>", 28, 54, c => c
                                    .Variable("variable1", 28)
                                    .Word("and", 40)
                                    .Variable("variable2", 44)
                                ))
                            )
                        )
                        .Examples(10, 17, example => example
                            .Table(11, 21, tab => tab
                                .Headers(11, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(12, 21,
                                    ("something1", 23, 32, null),
                                    ("something2", 37, 46, null)
                                )
                            )
                        )
                    )
                )
            );
        }


        [Fact]
        public async Task ScenarioOutlineUndeclaredExamplesVariableInStepTable()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument for this table:
                         | heading1                    |
                         | <not a variable>            |

                Examples:
                    | variable1   | variable2   |
                    | something1  | something2  | 
                
            ";

            await CompileAndAssertWarnings(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument for this table:", 6, 21, step => step
                            .Table(7, 26, tab => tab
                                .Headers(7, 26, ("heading1", 28, 35))
                                .Row(8, 26, ("<not a variable>", 28, 43, arg => arg
                                    .Variable("not a variable", 28)
                                ))
                            )
                        )
                        .Examples(10, 17, example => example
                            .Table(11, 21, tab => tab
                                .Headers(11, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(12, 21,
                                    ("something1", 23, 32, null),
                                    ("something2", 37, 46, null)
                                )
                            )
                        )
                    )
                ),
                new CompilerMessage(null, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableNotDeclared,
                                    "You have specified an Example variable to insert, 'not a variable', but you have not declared the variable in any of your Examples. This value will always be blank when the test runs.",
                                    8, 28, 8, 43)
            );
        }

        [Fact]
        public async Task ScenarioOutlineCanInsertMultipleVariablesInOneArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument '<variable1> something <variable2>'

                Examples:
                    | variable1   | variable2   |
                    | something1  | something2  | 
                
            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<variable1> something <variable2>'", 6, 21, step => step
                            .Text("I")
                            .Text("pass")
                            .Text("an")
                            .Text("argument")
                            .Quote()
                            .Variable("variable1")
                            .Text("something")
                            .Variable("variable2")
                            .Quote()
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(10, 21,
                                    ("something1", 23, 32, c => c.Word("something", 23).Int("1", 32)),
                                    ("something2", 37, 46, c => c.Word("something", 37).Int("2", 46))
                                )
                            )
                        )
                    )
                )
            );
        }


        [Fact]
        public async Task ScenarioOutlineCanInsertVariablesFromMultipleExamples()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument '<variable1> something <variable2>'

                Examples:
                    | variable1   | 
                    | something1  | 

                Examples:
                    | variable2   |
                    | something2  |
                
            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<variable1> something <variable2>'", 6, 21, step => step
                            .Text("I")
                            .Text("pass")
                            .Text("an")
                            .Text("argument")
                            .Quote()
                            .Variable("variable1")
                            .Text("something")
                            .Variable("variable2")             
                            .Quote()
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31)
                                )
                                .Row(10, 21,
                                    ("something1", 23, 32, c => c.Word("something", 23).Int("1", 32))
                                )
                            )
                        )
                        .Examples(12, 17, example => example
                            .Table(13, 21, tab => tab
                                .Headers(13, 21,
                                    ("variable2", 23, 31)
                                )
                                .Row(14, 21,
                                    ("something2", 23, 32, c => c.Word("something", 23).Int("2", 32))
                                )
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task ScenarioOutlineUndeclaredExamplesVariableProducesWarning()
        {
            const string TestFile =
            @"                
              Feature: My Feature
                
                Scenario Outline: My Scenario Outline

                    Given I pass an argument '<not a variable>'

                Examples:
                    | variable1   | variable2   |
                    | something1  | something2  | 
                
            ";

            await CompileAndAssertWarnings(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<not a variable>'", 6, 21
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(10, 21,
                                    ("something1", 23, 32, null),
                                    ("something2", 37, 46, null)
                                )
                            )
                        )
                    )
                ),
                new CompilerMessage(null, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableNotDeclared,
                                    "You have specified an Example variable to insert, 'not a variable', but you have not declared the variable in any of your Examples. This value will always be blank when the test runs.", 6, 47, 6, 62)
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
