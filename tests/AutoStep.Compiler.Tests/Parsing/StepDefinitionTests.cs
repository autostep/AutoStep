using AutoStep.Compiler.Tests.Utils;
using AutoStep.Core;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Compiler.Tests.Parsing
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

            await CompileAndAssertSuccess(TestFile, file => file
                .StepDefinition(StepType.Given, "I have done something", 2, 15, step => step
                    .Given("I have done this", 4, 17)
                    .Then("this is true", 5, 17)
            ));
        }

        [Fact]
        public async Task DefineStepWithArguments()
        {
            const string TestFile =
            @"                
              @tag1
              $opt1
              Step: Given I have defined a step with 'argument1' and 'argument2'
                A description
                
                Given I have used '<argument1>'
                Then this is '<argument2>'
            ";

            await CompileAndAssertSuccess(TestFile, file => file
                .StepDefinition(StepType.Given, "I have defined a step with 'argument1' and 'argument2'", 4, 15, step => step
                    .Tag("tag1", 2, 15)
                    .Option("opt1", 3, 15)
                    .Description("A description")
                    .Argument(ArgumentType.Text, "argument1", 54, 64)
                    .Argument(ArgumentType.Text, "argument2", 70, 80)
                    .Given("I have used '<argument1>'", 7, 17, g => g
                        .Argument(ArgumentType.Text, "<argument1>", 35, 47, arg => arg
                            .VariableInsertion("argument1", 36, 46)
                            .NullValue()
                        )
                    )
                    .Then("this is '<argument2>'", 8, 17, t => t
                        .Argument(ArgumentType.Text, "<argument2>", 30, 42, arg => arg
                            .VariableInsertion("argument2", 31, 41)
                            .NullValue()
                        )
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
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotDefineAStepWithAnd, 
                                    "A custom step cannot be defined using the 'And' keyword. You must use Given, When or Then.", 
                                    2, 21, 2, 45)
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
        public async Task DefineStepUsingInsertionVariableGivesError()
        {
            const string TestFile =
            @"                
              Step: Given I have passed 'normal' with '<arg>' to this
                
                Given I have referenced another '<normal>'
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition,
                                    "You cannot use '<arg>' as a Step Parameter. Step Parameter variables must be literal names, e.g. 'variable1' or 'total'. You cannot specify dynamic values.",
                                    2, 55, 2, 61)
            );

        }

        [Fact]
        public async Task DefineStepUsingInterpolationGivesError()
        {
            const string TestFile =
            @"                
              Step: Given I have passed 'normal' with ':interpolated' to this
                
                Given I have referenced another '<normal>'
            ";

            await CompileAndAssertErrors(TestFile,
                new CompilerMessage(null, CompilerMessageLevel.Error, CompilerMessageCode.CannotSpecifyDynamicValueInStepDefinition,
                                    "You cannot use ':interpolated' as a Step Parameter. Step Parameter variables must be literal names, e.g. 'variable1' or 'total'. You cannot specify dynamic values.",
                                    2, 55, 2, 69)
            );
        }
    }
}
