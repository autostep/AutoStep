using System.Threading.Tasks;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Compiler;

namespace AutoStep.Tests.Compiler.Parsing
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
                                    ("r1.c1", 31, 35, null),
                                    ("r1.c2", 42, 46, null)
                                )
                                .Row(9, 29,
                                    ("r2.c1", 31, 35, null),
                                    ("r2.c2", 42, 46, null)
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
                                    ("r1.c1", 30, 34, null),
                                    ("r1.c2", 36, 40, null)
                                )
                                .Row(9, 29,
                                    ("r2.c1", 30, 34, null),
                                    ("r2.c2", 36, 40, null)
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
                                    ("r2.c1", 31, 35, null),
                                    ("r2.c2", 42, 46, null)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Word("this", 31)
                            .Word("step", 36)
                            .Word("has", 41)
                            .Word("a", 45)
                            .Word("table", 47)
                            .Colon(52)
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(8, 29,
                                    ("123", 31, 33, c => c.Int("123", 31))
                                )
                                .Row(9, 29,
                                    ("123.50", 31, 36, c => c.Float("123.50", 31))
                                )
                                .Row(10, 29,
                                    ("£123.50", 31, 37, c => c.Word("£", 31).Float("123.50", 32))
                                )
                                .Row(11, 29,
                                    (":interpolated", 31, 43, c => c.InterpolateStart(":", 31).Word("interpolated", 32))
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

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
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
                                    ("r1.c1", 31, 35, null),
                                    ("r1.c2", 42, 46, null)
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

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
                null,
                CompilerMessageLevel.Error,
                CompilerMessageCode.TableRowHasNotBeenTerminated,
                "Table cell has not been terminated. Expecting a table delimiter character '|'.",
                9, 29, 9, 36),

                new CompilerMessage(
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

            await CompileAndAssertErrors(TestFile, new CompilerMessage(
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Word("this", 31)
                            .Word("step", 36)
                            .Word("has", 41)
                            .Word("a", 45)
                            .Word("table", 47)
                            .Colon(52)
                            .Table(8, 29, table => table
                                .Headers(8, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(9, 29,
                                    ("123", 31, 33, c => c.Int("123", 31))
                                )
                                .Row(11, 29,
                                    ("123.50", 31, 36, c => c.Float("123.50", 31))
                                )
                            )
                        )
                        .Then("another thing happens", 13, 25, step => step
                            .Word("another", 30)
                            .Word("thing", 38)
                            .Word("happens", 44)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Word("this", 31)
                            .Word("step", 36)
                            .Word("has", 41)
                            .Word("a", 45)
                            .Word("table", 47)
                            .Colon(52)
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(8, 29,
                                    ("this: 1", 31, 37, c => c.Word("this", 31).Colon(35).Int("1", 37))
                                )
                            )
                        )
                    )
                )
            );
        }
    }
}
