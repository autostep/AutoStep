using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests
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
                                    (ArgumentType.Text, "r1.c1", 31, 35, null),
                                    (ArgumentType.Text, "r1.c2", 42, 46, null)
                                )
                                .Row(9, 29,
                                    (ArgumentType.Text, "r2.c1", 31, 35, null),
                                    (ArgumentType.Text, "r2.c2", 42, 46, null)
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
                                    (ArgumentType.Text, "r1.c1", 30, 34, null),
                                    (ArgumentType.Text, "r1.c2", 36, 40, null)
                                )
                                .Row(9, 29,
                                    (ArgumentType.Text, "r2.c1", 30, 34, null),
                                    (ArgumentType.Text, "r2.c2", 36, 40, null)
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
                                    (ArgumentType.Empty, null, 30, 39, arg => arg.NullValue()),
                                    (ArgumentType.Empty, null, 41, 50, arg => arg.NullValue())
                                )
                                .Row(9, 29,
                                    (ArgumentType.Text, "r2.c1", 31, 35, null),
                                    (ArgumentType.Text, "r2.c2", 42, 46, null)
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(7, 29, table => table
                                .Headers(7, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(8, 29,
                                    (ArgumentType.NumericInteger, "123", 31, 33, arg => arg.Value(123))
                                )
                                .Row(9, 29,
                                    (ArgumentType.NumericDecimal, "123.50", 31, 36, arg => arg.Value(123.50M))
                                )
                                .Row(10, 29,
                                    (ArgumentType.NumericDecimal, "£123.50", 31, 37, arg => arg.Value(123.50M).Symbol("£"))
                                )
                                .Row(11, 29,
                                    (ArgumentType.Interpolated, "interpolated", 31, 43, arg => arg.NullValue())
                                )
                                .Row(12, 29,
                                    (ArgumentType.Text, "text", 31, 34, null)
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

            await CompileAndAssertErrors(TestFile, new CompilerMessage (
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
                                    (ArgumentType.Text, "r1.c1", 31, 35, null),
                                    (ArgumentType.Text, "r1.c2", 42, 46, null)
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
                                    (ArgumentType.NumericInteger, "123", 31, 33, arg => arg.Value(123))
                                )
                                .Row(10, 29,
                                    (ArgumentType.NumericDecimal, "123.50", 31, 36, arg => arg.Value(123.50M))
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

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 17, feat => feat
                    .Scenario("My Scenario", 4, 21, scen => scen
                        .Given("this step has a table:", 6, 25, step => step
                            .Table(8, 29, table => table
                                .Headers(8, 29,
                                    ("heading1", 31, 38)
                                )
                                .Row(9, 29,
                                    (ArgumentType.NumericInteger, "123", 31, 33, arg => arg.Value(123))
                                )
                                .Row(11, 29,
                                    (ArgumentType.NumericDecimal, "123.50", 31, 36, arg => arg.Value(123.50M))
                                )
                            )
                        )
                        .Then("another thing happens", 13, 25)
                    )
                )
            );
        }
    }
}
