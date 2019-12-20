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
    }
}
