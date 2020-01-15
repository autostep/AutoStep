using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AutoStep.Compiler;
using AutoStep.Elements.Parts;

namespace AutoStep.Tests.Compiler.Parsing
{
    public class StepPartTests : CompilerTestBase
    {
        public StepPartTests(ITestOutputHelper output) : base(output)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed 'argument1' to something", 6, 21, step => step
                            .Text("I", 27)
                            .Text("have", 29)
                            .Text("passed", 34)
                            .Quote(41)
                            .Text("argument", 42)
                            .Int("1", 50)
                            .Quote(51)
                            .Text("to", 53)
                            .Text("something", 56)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("this > that", 6, 21, step => step
                            .Text("this", 27)
                            .Text(">", 32)
                            .Text("that", 34)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("this < that", 6, 21, step => step
                            .Text("this", 27)
                            .Text("<", 32)
                            .Text("that", 34)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("this < that > those", 6, 21, step => step
                            .Text("this", 27)
                            .Text("<", 32)
                            .Text("that", 34)
                            .Text(">", 39)
                            .Text("those", 41)
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
                            .Text("I", 27)
                            .Text("have", 29)
                            .Quote(34)
                            .Text("this", 35)
                            .Variable("variable name", 40)
                            .Quote(55)
                            .Text("to", 57)
                            .Text("something", 60)
            ))),
            new CompilerMessage(null, CompilerMessageLevel.Warning, CompilerMessageCode.ExampleVariableInScenario,
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                 .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I don't do something", 6, 21, step => step
                            .Text("I", 27)
                            .Text("don", 29)
                            .Quote(32)
                            .Text("t", 33)
                            .Text("do", 35)
                            .Text("something", 38)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed \\'argument", 6, 21, step => step
                            .Text("I", 27)
                            .Text("have", 29)
                            .Text("passed", 34)
                            .EscapeChar("\\'", "'", 41)
                            .Text("argument", 43)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed ':Today at 10:00' to something", 6, 21, step => step
                            .Text("I", 27)
                            .Text("have", 29)
                            .Text("passed", 34)
                            .Quote(41)
                                .InterpolateStart(":", 42)
                                .Text("Today", 43)
                                .Text("at", 49)
                                .Int("10", 52)
                                .Colon(54)
                                .Int("00", 55)
                            .Quote(57)
                            .Text("to", 59)
                            .Text("something", 62)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have 123 now", 6, 21, step => step
                            .Text("I", 27)
                            .Text("have", 29)
                            .Int("123", 34)
                            .Text("now", 38)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have123 now", 6, 21, step => step
                            .Text("I", 27)
                            .Text("have", 29)
                            .Int("123", 33)
                            .Text("now", 37)
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

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .Feature("My Feature", 2, 15, feat => feat
                    .Scenario("My Scenario", 4, 17, scen => scen
                        .Given("I have passed '123.5'", 6, 21, step => step
                            .Text("I", 27)
                            .Text("have", 29)
                            .Text("passed", 34)
                            .Quote(41)
                            .Float("123.5", 42)
                            .Quote(47)
                        )
                    )
                )
            );
        }      
    }
}
