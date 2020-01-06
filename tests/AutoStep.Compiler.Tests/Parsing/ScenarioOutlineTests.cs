using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Parsing
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
                                    (ArgumentType.Text, "new1", 23, 26, null),
                                    (ArgumentType.Text, "new2", 36, 39, null)
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<variable1>'", 6, 21, step => step
                            .Argument(ArgumentType.Text, "<variable1>", 46, 58, arg => arg
                                .VariableInsertion("variable1", 47, 57)
                                .NullValue()
                            )
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(10, 21,
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 37, 46, null)
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
                                .Row(8, 26, (ArgumentType.Text, "<variable1>", 28, 38, arg => arg
                                    .VariableInsertion("variable1", 28, 38)
                                    .NullValue()
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
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 37, 46, null)
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
                                .Row(8, 26, (ArgumentType.Text, "<variable1> and <variable2>", 28, 54, arg => arg
                                    .VariableInsertion("variable1", 28, 38)
                                    .Section(" and ", 39, 43)
                                    .VariableInsertion("variable2", 44, 54)
                                    .NullValue()
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
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 37, 46, null)
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
                                .Row(8, 26, (ArgumentType.Text, "<not a variable>", 28, 43, arg => arg
                                    .VariableInsertion("not a variable", 28, 43)
                                    .NullValue()
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
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 37, 46, null)
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<variable1> something <variable2>'", 6, 21, step => step
                            .Argument(ArgumentType.Text, "<variable1> something <variable2>", 46, 80, arg => arg
                                .VariableInsertion("variable1", 47, 57)
                                .Section(" something ", 58, 68)
                                .VariableInsertion("variable2", 69, 79)
                                .NullValue()
                            )
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(10, 21,
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 37, 46, null)
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .ScenarioOutline("My Scenario Outline", 4, 17, scen => scen
                        .Given("I pass an argument '<variable1> something <variable2>'", 6, 21, step => step
                            .Argument(ArgumentType.Text, "<variable1> something <variable2>", 46, 80, arg => arg
                                .VariableInsertion("variable1", 47, 57)
                                .Section(" something ", 58, 68)
                                .VariableInsertion("variable2", 69, 79)
                                .NullValue()
                            )
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31)
                                )
                                .Row(10, 21,
                                    (ArgumentType.Text, "something1", 23, 32, null)
                                )
                            )
                        )
                        .Examples(12, 17, example => example
                            .Table(13, 21, tab => tab
                                .Headers(13, 21,
                                    ("variable2", 23, 31)
                                )
                                .Row(14, 21,
                                    (ArgumentType.Text, "something2", 23, 32, null)
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
                        .Given("I pass an argument '<not a variable>'", 6, 21, step => step
                            .Argument(ArgumentType.Text, "<not a variable>", 46, 63, arg => arg
                                .VariableInsertion("not a variable", 47, 62)
                                .NullValue()
                            )
                        )
                        .Examples(8, 17, example => example
                            .Table(9, 21, tab => tab
                                .Headers(9, 21,
                                    ("variable1", 23, 31),
                                    ("variable2", 37, 45)
                                )
                                .Row(10, 21,
                                    (ArgumentType.Text, "something1", 23, 32, null),
                                    (ArgumentType.Text, "something2", 37, 46, null)
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
