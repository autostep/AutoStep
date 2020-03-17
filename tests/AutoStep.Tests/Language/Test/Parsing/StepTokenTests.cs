using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Language;

namespace AutoStep.Tests.Language.Test.Parsing
{
    public class StepTokenTests : CompilerTestBase
    {
        public StepTokenTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task StepCanHaveQuotedString()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed 'argument1' to something

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'argument1' to something", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Text("passed")
                            .Quote()
                            .Text("argument")
                            .Int("1")
                            .Quote()
                            .Text("to")
                            .Text("something")
            ))));
        }

        [Fact]
        public async Task StepCanContainGreaterThan()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given this > that

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("this > that", 6, 21, step => step
                            .Text("this")
                            .Text(">")
                            .Text("that")
            ))));
        }

        [Fact]
        public async Task StepCanContainLessThan()
        {
            const string TestFile =
                        @"
              Feature: My Feature

                Scenario: My Scenario

                    Given this < that

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("this < that", 6, 21, step => step
                            .Text("this")
                            .Text("<")
                            .Text("that")
            ))));
        }

        [Fact]
        public async Task StepCanContainLessThanFollowedByGreaterThan()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given this < that > those

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("this < that > those", 6, 21, step => step
                            .Text("this")
                            .Text("<")
                            .Text("that")
                            .Text(">")
                            .Text("those")
            ))));
        }

        [Fact]
        public async Task VariableInScenarioRaisesWarning()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have 'this <variable name>' to something

            ";

            await CompileAndAssertWarningsWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have 'this <variable name>' to something", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Quote()
                            .Text("this")
                            .Variable("variable name")
                            .Quote()
                            .Text("to")
                            .Text("something")
            ))),
            new LanguageOperationMessage(null, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableInScenario,
                                "You have specified an Example variable to insert, 'variable name', but the step is in a Scenario; did you mean to use a Scenario Outline instead?",
                                6, 40, 6, 54)
            );
        }

        [Fact]
        public async Task QuoteAsAnApostrophe()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I don't do something

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                 .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I don't do something", 6, 21, step => step
                            .Text("I")
                            .Text("don")
                            .Quote()
                            .Text("t")
                            .Text("do")
                            .Text("something")
            ))));
        }

        [Fact]
        public async Task StepEscapedQuote()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed \'argument
            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed \\'argument", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Text("passed")
                            .EscapeChar("\\'", "'")
                            .Text("argument")
            ))));
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

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed ':Today at 10:00' to something", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Text("passed")
                            .Quote()
                                .InterpolateStart()
                                .Text("Today")
                                .Text("at")
                                .Int("10")
                                .Colon()
                                .Int("00")
                            .Quote()
                            .Text("to")
                            .Text("something")
            ))));
        }

        [Fact]
        public async Task IntegerPart()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have 123 now

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have 123 now", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Int("123")
                            .Text("now")
            ))));
        }

        [Fact]
        public async Task IntegerPartNoSpaces()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have123 now

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have123 now", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Int("123")
                            .Text("now")
            ))));
        }

        [Fact]
        public async Task FloatArgument()
        {
            const string TestFile =
            @"
              Feature: My Feature

                Scenario: My Scenario

                    Given I have passed '123.5'

            ";

            await CompileAndAssertSuccessWithStatementTokens(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed '123.5'", 6, 21, step => step
                            .Text("I")
                            .Text("have")
                            .Text("passed")
                            .Quote()
                            .Float("123.5")
                            .Quote()
                        )
                    )
                )
            );
        }
    }
}
