using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Parsing
{
    public class StepArgumentTests : CompilerTestBase
    {
        public StepArgumentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task StepCanHaveSingleArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'argument1' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'argument1' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "argument1", 41, 51)
            ))));
        }

        [Fact]
        public async Task StepCanHaveArgumentThatContainsGreaterThan()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'this > that' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'this > that' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "this > that", 41, 53)
            ))));
        }


        [Fact]
        public async Task StepCanHaveArgumentThatContainsLessThan()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'this < that' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'this < that' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "this < that", 41, 53)
            ))));
        }


        [Fact]
        public async Task StepCanHaveArgumentThatContainsLessThanFollowedByGreaterThan()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'this < that >' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'this < that >' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "this < that >", 41, 55)
            ))));
        }


        [Fact]
        public async Task ExamplesVariableInScenarioRaisesWarning()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'this <variable name>' to something

            ";

            await CompileAndAssertWarnings(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'this <variable name>' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "this <variable name>", 41, 62, arg => arg
                                .Section("this ", 42, 46)
                                .VariableInsertion("variable name", 47, 61)
                                .NullValue()
                            )
            ))),
            new CompilerMessage(null, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableInScenario,
                                "You have specified an Example variable to insert, 'variable name', but the step is in a Scenario; did you mean to use a Scenario Outline instead?",
                                6, 47, 6, 61)
            );
        }

        [Fact]
        public async Task ArgumentNotClosedError()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'argument1 to something

            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(
                    null,
                    CompilerMessageLevel.Error,
                    CompilerMessageCode.ArgumentHasNotBeenClosed,
                    "Quoted argument has not been closed.",
                    6, 41, 6, 63
                )
            );
        }


        [Fact]
        public async Task StepMultipleArguments()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'argument1' and 'argument2' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'argument1' and 'argument2' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "argument1", 41, 51)
                            .Argument(ArgumentType.Text, "argument2", 57, 67)
            ))));
        }

        [Fact]
        public async Task StepArgumentEmbeddedQuote()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'argument \' quoted' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'argument \\' quoted' to something", 6, 21, step => step
                            .Argument(ArgumentType.Text, "argument \\' quoted", 41, 60, arg => arg
                                .Escaped("argument ' quoted")
            )))));
        }

        [Fact]
        public async Task InterpolatedArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed ':Today at 10:00' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed ':Today at 10:00' to something", 6, 21, step => step
                            .Argument(ArgumentType.Interpolated, "Today at 10:00", 41, 57, arg => arg
                                .NullValue()
            )))));
        }

        [Fact]
        public async Task IntegerArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed '123' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed '123' to something", 6, 21, step => step
                            .Argument(ArgumentType.NumericInteger, "123", 41, 45, arg => arg
                                .Value(123)
            )))));
        }

        [Fact]
        public async Task FloatArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed '123.5' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed '123.5' to something", 6, 21, step => step
                            .Argument(ArgumentType.NumericDecimal, "123.5", 41, 47, arg => arg
                                .Value(123.5M)
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task CurrencyPoundArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed '£123.5' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed '£123.5' to something", 6, 21, step => step
                            .Argument(ArgumentType.NumericDecimal, "£123.5", 41, 48, arg => arg
                                .Value(123.5M)
                                .Symbol("£")
                            )
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task CurrencyDollarArgument()
        {
            const string TestFile =
            @"                
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed '$123.50' to something

            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed '$123.50' to something", 6, 21, step => step
                            .Argument(ArgumentType.NumericDecimal, "$123.50", 41, 49, arg => arg
                                .Value(123.50M)
                                .Symbol("$")
                            )
                        )
                    )
                )
            );
        }
    }
}
