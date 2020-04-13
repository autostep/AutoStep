using System.Threading.Tasks;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction.Parser
{
    public class InteractionStepDefinitionTests : InteractionsCompilerTestBase
    {
        public InteractionStepDefinitionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task StepCanHaveADescription()
        {
            const string Test = @"
                Trait: clickable + named
                    
                    ## Clicks on the specified component.
                    Step: Given I have clicked the {arg} $component$
                        locateNamed(arg)
                        -> click()
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + named", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("named", 36)
                     .StepDefinition(StepType.Given, "I have clicked the {arg} $component$", 5, 27, step => step
                        .Description("Clicks on the specified component.")
                        .WordPart("I", 33)
                        .WordPart("have", 35)
                        .WordPart("clicked", 40)
                        .WordPart("the", 48)
                        .Argument("{arg}", "arg", 52)
                        .ComponentMatch(58)
                        .Expression(e => e
                            .Call("locateNamed", 6, 25, 6, 40, c => c
                                .Variable("arg", 37)
                            )
                            .Call("click", 7, 28, 7, 34)
                        )
                     )
                 )
             );
        }
    }
}
