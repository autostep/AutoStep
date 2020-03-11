using System.Threading.Tasks;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Language;

namespace AutoStep.Tests.Language.Test.Parsing
{
    public class TableTests : CompilerTestBase
    {
        public TableTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task StepCanHaveATable()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1 | heading2 |
                            | r1.c1    | r1.c2    |
                            | r2.c1    | r2.c2    |

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38),
                                    ("heading2", 42, 49)
                                )
                                .Row(8, 29,
                                    ("r1.c1", 31, 35, c => c.Text("r").Float("1.").Text("c").Int("1")),
                                    ("r1.c2", 42, 46, c => c.Text("r").Float("1.").Text("c").Int("2"))
                                )
                                .Row(9, 29,
                                    ("r2.c1", 31, 35, c => c.Text("r").Float("2.").Text("c").Int("1")),
                                    ("r2.c2", 42, 46, c => c.Text("r").Float("2.").Text("c").Int("2"))
                                )
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task TableCanHaveCollapsedWhitespace()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            |heading1|heading2|
                            |r1.c1|r1.c2|
                            |r2.c1|r2.c2|

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 30, 37),
                                    ("heading2", 39, 46)
                                )
                                .Row(8, 29,
                                    ("r1.c1", 30, 34, c => c.Text("r").Float("1.").Text("c").Int("1")),
                                    ("r1.c2", 36, 40, c => c.Text("r").Float("1.").Text("c").Int("2"))
                                )
                                .Row(9, 29,
                                    ("r2.c1", 30, 34, c => c.Text("r").Float("2.").Text("c").Int("1")),
                                    ("r2.c2", 36, 40, c => c.Text("r").Float("2.").Text("c").Int("2"))
                                )
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task TableCanHaveBlankValues()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1 | heading2 |
                            |          |          |
                            | r2.c1    | r2.c2    |

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38),
                                    ("heading2", 42, 49)
                                )
                                .Row(8, 29,
                                    (null, 30, 39, null),
                                    (null, 41, 50, null)
                                )
                                .Row(9, 29,
                                    ("r2.c1", 31, 35, c => c.Text("r").Float("2.").Text("c").Int("1")),
                                    ("r2.c2", 42, 46, c => c.Text("r").Float("2.").Text("c").Int("2"))
                                )
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task TableCanHaveEachArgType()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1         | 
                            | 123              |
                            | 123.50           |
                            | £123.50          |
                            | :interpolated    |
                            | text             |

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Text("this")
                            .Text("step")
                            .Text("has")
                            .Text("a")
                            .Text("table")
                            .Colon()
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(8, 29,
                                    ("123", 31, 33, c => c.Int("123"))
                                )
                                .Row(9, 29,
                                    ("123.50", 31, 36, c => c.Float("123.50"))
                                )
                                .Row(10, 29,
                                    ("£123.50", 31, 37, c => c.Text("£").Float("123.50"))
                                )
                                .Row(11, 29,
                                    (":interpolated", 31, 43, c => c.InterpolateStart().Text("interpolated"))
                                )
                                .Row(12, 29,
                                    ("text", 31, 34, null)
                                )
                            )
                        )
                    )
                )
            );
        }


        [Fact]
        public async Task UnterminatedTableError()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1         | 
                            | 123              |
                            | 123.50           
                            | £123.50          |
                            | :interpolated    |
                            | text             |

            ";

            await CompileAndAssertErrors(TestFile, new LanguageOperationMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.TableRowHasNotBeenTerminated,
                "Table cell has not been terminated. Expecting a table delimiter character '|'.",
                9, 29, 9, 36));
        }


        [Fact]
        public async Task EmptyHeadingAllowed()
        {
            const string TestFile =
                        @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1 |          |
                            | r1.c1    | r1.c2    |
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38),
                                    (null, 41, 50)
                                )
                                .Row(8, 29,
                                    ("r1.c1", 31, 35, c => c.Text("r").Float("1.").Text("c").Int("1")),
                                    ("r1.c2", 42, 46, c => c.Text("r").Float("1.").Text("c").Int("2"))
                                )
                            )
                        )
                    )
                )
            );
        }


        [Fact]
        public async Task MultipleUnterminatedRows()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1         | 
                            | 123              |
                            | 123.50           
                            | £123.50          
                            | :interpolated    |
                            | text             |

            ";

            await CompileAndAssertErrors(TestFile, new LanguageOperationMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.TableRowHasNotBeenTerminated,
                "Table cell has not been terminated. Expecting a table delimiter character '|'.",
                9, 29, 9, 36),

                new LanguageOperationMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.TableRowHasNotBeenTerminated,
                "Table cell has not been terminated. Expecting a table delimiter character '|'.",
                10, 29, 10, 37));
        }


        [Fact]
        public async Task TableWithMismatchedCellCounts()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1         | 
                            | 123              | another value |   
                            | 123.50           |

            ";

            await CompileAndAssertErrors(TestFile, new LanguageOperationMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.TableColumnsMismatch,
                "The row contains 2 cell(s), but we are expecting 1, because of the number of headers.",
                8, 29, 8, 64)
            );
        }


        [Fact]
        public async Task TableBlankRowGap()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1         | 
                            | 123              |
                            
                            | 123.50           |

                        Then another thing happens
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(8, 29,
                                    ("123", 31, 33, null)
                                )
                                .Row(10, 29,
                                    ("123.50", 31, 36, null)
                                )
                            )
                        )
                        .Then("another thing happens", 12, 25)
                    )
                )
            );
        }

        [Fact]
        public async Task TableComments()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            # comment before table
                            | heading1         | 
                            | 123              | # comment after table
                            # comment inside blank line
                            | 123.50           |

                        Then another thing happens
            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Text("this")
                            .Text("step")
                            .Text("has")
                            .Text("a")
                            .Text("table")
                            .Colon()
                            .Table(8, 29, table => table
                                .Headers(8, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(9, 29,
                                    ("123", 31, 33, c => c.Int("123"))
                                )
                                .Row(11, 29,
                                    ("123.50", 31, 36, c => c.Float("123.50"))
                                )
                            )
                        )
                        .Then("another thing happens", 13, 25, step => step
                            .Text("another")
                            .Text("thing")
                            .Text("happens")
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task TableCellCanHaveColonSeparatorNotInterpolated()
        {
            const string TestFile =
            @"
                Feature: My Feature

                    Scenario: My Scenario

                        Given this step has a table:
                            | heading1 | 
                            | this: 1  |

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Text("this")
                            .Text("step")
                            .Text("has")
                            .Text("a")
                            .Text("table")
                            .Colon()
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(8, 29,
                                    ("this: 1", 31, 37, c => c.Text("this").Colon().Int("1"))
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}
