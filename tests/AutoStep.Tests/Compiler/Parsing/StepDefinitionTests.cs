using AutoStep.Compiler;
using AutoStep.Tests.Utils;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Compiler.Parsing
{
    public class StepDefinitionTests : CompilerTestBase
    {
        public StepDefinitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task DefineStepNoArguments()
        {
            const string TestFile =
            @"                
              Step: Given I have done something
                
                Given I have done this
                Then this is true
            ";

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .StepDefinition(StepType.Given, "I have done something", 2, 21, step => step
                    .WordPart("I", 27)
                    .WordPart("have", 29)
                    .WordPart("done", 34)
                    .WordPart("something", 39)
                    .Given("I have done this", 4, 17, s => s
                        .Word("I", 23)
                        .Word("have", 25)
                        .Word("done", 30)
                        .Word("this", 35)
                    )
                    .Then("this is true", 5, 17, s => s
                        .Word("this", 22)
                        .Word("is", 27)
                        .Word("true", 30)
                    )
            ));
        }

        [Fact]
        public async Task DefineStepWithArguments()
        {
            const string TestFile =
            @"                
              @tag1
              $opt1
              Step: Given I have defined a step with {argument1} and {argument2}
                A description
                
                Given I have used '<argument1>'
                Then this is <argument2>
            ";

            await CompileAndAssertSuccessWithStatementParts(TestFile, file => file
                .StepDefinition(StepType.Given, "I have defined a step with {argument1} and {argument2}", 4, 21, step => step
                    .Tag("tag1", 2, 15)
                    .Option("opt1", 3, 15)
                    .Description("A description")
                    .WordPart("I", 27)
                    .WordPart("have", 29)
                    .WordPart("defined", 34)
                    .WordPart("a", 42)
                    .WordPart("step", 44)
                    .WordPart("with", 49)
                    .Argument("{argument1}", "argument1", 54)
                    .WordPart("and", 66)
                    .Argument("{argument2}", "argument2", 70)
                    .Given("I have used '<argument1>'", 7, 17, g => g
                        .Word("I", 23)
                        .Word("have", 25)
                        .Word("used", 30)
                        .Quote(35)
                        .Variable("argument1", 36)
                        .Quote(47)
                    )
                    .Then("this is <argument2>", 8, 17, t => t
                        .Word("this", 22)
                        .Word("is", 27)
                        .Variable("argument2", 30)
                    )
            ));
        }

        [Fact]
        public async Task DefineStepUsingAndGivesError()
        {
            const string TestFile =
            @"                
              Step: And I have done something
                
                Given I have done this
                Then this is true
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.InvalidStepDefineKeyword,
                                    "A custom step cannot be defined using 'And'. You must use Given, When or Then.",
                                    2, 21, 2, 23)
            );
        }

        [Fact]
        public async Task DefineStepUsingUndeclaredVariableGivesWarning()
        {
            const string TestFile =
            @"                
              Step: Given I have passed 'arg' to this
                
                Given I have referenced another '<not an arg>'
            ";

            await CompileAndAssertWarnings(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Warning, CompilerMessageCode.StepVariableNotDeclared,
                                    "You have specified a Step parameter variable to insert, 'not an arg', but you have not declared the variable in the step declaration. This value will always be blank when the test runs.",
                                    4, 50, 4, 61)
            );
        }

        [Fact]
        public async Task DefineStepUsingEmptyVariableGivesError()
        {
            const string TestFile =
            @"                
              Step: Given I have passed {}
                
                Given this is just a step
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.StepVariableNameRequired,
                                    "You cannot specify an Empty Parameter as a Step Parameter. Step Parameter variables must be literal names, e.g. {variable1} or {total}.",
                                    2, 41, 2, 42)
            );

        }
                
        [Fact(Skip = "This is future compiler functionality; the ability to give an error for putting variable insert statements inside definitions")]
        public async Task DefineStepUsingInsertionVariableGivesError()
        {
            const string TestFile =
            @"                
              Step: Given I have passed {normal} with <arg> to this
                
                Given I have referenced another '<normal>'
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition,
                                    "You cannot use '<arg>' as a Step Parameter. Step Parameter variables must be literal names, e.g. 'variable1' or 'total'. You cannot specify dynamic values.",
                                    2, 55, 2, 61)
            );

        }

        [Fact(Skip = "This is future compiler functionality; the ability to give an error for putting interpolation statements inside definitions")]
        public async Task DefineStepUsingInterpolationGivesError()
        {
            const string TestFile =
            @"                
              Step: Given I have passed {normal} with ':interpolated' to this
                
                Given I have referenced another '<normal>'
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition,
                                    "You cannot use ':interpolated' as a Step Parameter. Step Parameter variables must be literal names, e.g. 'variable1' or 'total'. You cannot specify dynamic values.",
                                    2, 55, 2, 69)
            );
        }

        [Fact]
        public async Task StepArgumentBadPaddingGivesError()
        {
            const string TestFile =
            @"                
              Step: Given I have passed {normal }
                
                Given I have referenced another '<normal>'
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.StepVariableInvalidWhitespace,
                                    "Extraneous whitespace detected in Step Parameter variable name. You cannot have extra whitespace at the start or end of a Step Parameter variable name.",
                                    2, 48, 2, 48)
            );
        }
    }
}
