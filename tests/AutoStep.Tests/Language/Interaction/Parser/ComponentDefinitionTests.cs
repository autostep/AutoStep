using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction.Parser
{
    public class ComponentDefinitionTests : InteractionsCompilerTestBase
    {
        public ComponentDefinitionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task CanDefineComponent()
        {
            const string Test = @"
                Component: button

                    traits: clickable, editable, displayable

                    locateNamed(name): select('label') -> withName(name)
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 2, 17, c => c
                    .Trait("clickable", 4, 29)
                    .Trait("editable", 4, 40)
                    .Trait("displayable", 4, 50)
                    .Method("locateNamed", 6, 21, m => m
                        .Argument("name", 6, 33)
                        .Call("select", 6, 40, 6, 54, a => a.String("label", 47))
                        .Call("withName", 6, 59, 6, 72, a => a.Variable("name", 68))
                    )
                )
            );
        }

        [Fact]
        public async Task NoComponentNameError()
        {
            const string Test = @"
                Component:
            ";

            await CompileAndAssertErrors(Test,
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMissingExpectedName, 2, 17, 2, 26));
        }

        [Fact]
        public async Task CanDefineComponentWithComments()
        {
            const string Test = @"
                # comment
                Component: button
                    ## doc comment
                    ## doc comment 2

                    traits: clickable,
                            # comment
                            editable,
                            displayable # comment

                    locateNamed(name): select('label')
                        # comment
                        -> withName(name)
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 3, 17, c => c
                    .Trait("clickable", 7, 29)
                    .Trait("editable", 9, 29)
                    .Trait("displayable", 10, 29)
                    .Method("locateNamed", 12, 21, m => m
                        .Argument("name", 12, 33)
                        .Call("select", 12, 40, 12, 54, a => a.String("label", 47))
                        .Call("withName", 14, 28, 14, 41, a => a.Variable("name", 37))
                    )
                )
            );
        }

        [Fact]
        public async Task ComponentCanBeBasedOnAnother()
        {
            const string Test = @"
                Component: button

                    inherits: button

                    locateNamed(name): select('label') -> withName(name)
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 2, 17, c => c
                    .Inherits("button", 4, 31)
                    .Method("locateNamed", 6, 21, m => m
                        .Argument("name", 6, 33)
                        .Call("select", 6, 40, 6, 54, a => a.String("label", 47))
                        .Call("withName", 6, 59, 6, 72, a => a.Variable("name", 68))
                    )
                )
            );
        }

        [Fact]
        public async Task ComponentCanDefineSteps()
        {
            const string Test = @"
                Component: button

                    inherits: button

                    Step: Given I have clicked on {name}
                        select(""btn[text='<name>']"")
                        -> click()
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 2, 17, c => c
                    .Inherits("button", 4, 31)
                    .StepDefinition(StepType.Given, "I have clicked on {name}", 6, 27, s => s
                        .WordPart("I", 33)
                        .WordPart("have", 35)
                        .WordPart("clicked", 40)
                        .WordPart("on", 48)
                        .Argument("{name}", "name", 51)
                        .Expression(e => e
                            .Call("select", 7, 25, 7, 52, a => a
                                .String("btn[text='<name>']", 32, s => s
                                    .Text("btn[text='")
                                    .Variable("name")
                                    .Text("']")
                                )
                            )
                            .Call("click", 8, 28, 8, 34)
                        )
                    )
                )
            );
        }

        [Fact]
        public async Task ComponentBlankInheritsError()
        {
            const string Test = @"
                Component: button

                    inherits:

                    Step: Given I have clicked on {name}
                        select(""btn[text='<name>']"")
                        -> click()
            ";

            await CompileAndAssertErrors(Test,
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMissingExpectedName, 4, 21, 4, 29));
        }

        [Fact]
        public async Task ComponentBlankStepError()
        {
            const string Test = @"
                Component: button

                    Step:
                        select(""btn[text='<name>']"")
                        -> click()
            ";

            await CompileAndAssertErrors(Test,
                file => file.Component("button", 2, 17),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMissingStepDeclaration, 4, 21, 4, 25));
        }

        [Fact]
        public async Task ComponentMissingMethodParensError()
        {
            const string Test = @"
                Component: button

                    method
            ";

            await CompileAndAssertErrors(Test,
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNeedsParentheses, 4, 21, 4, 26));
        }

        [Fact]
        public async Task ComponentMultipleRandomNamesError()
        {
            const string Test = @"
                Component: button

                    not a valid set
            ";

            // The compiler will think we're trying to declare methods and associated calls.
            await CompileAndAssertErrors(Test,
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNeedsParentheses, 4, 21, 4, 23),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNeedsParentheses, 4, 25, 4, 25),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNeedsParentheses, 4, 27, 4, 31),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodNeedsParentheses, 4, 33, 4, 35));
        }

        [Fact]
        public async Task ComponentJunkContentError()
        {
            const string Test = @"
                Component: button

                    name: 'abc'

                    <<!!3214FFF-_ @@@!

                    Step: Given I
                        method()

                Component: other
            ";

            // The compiler will think we're trying to declare methods and associated calls.
            await CompileAndAssertErrors(Test, file => file
                .Component("button", 2, 17, cfg => cfg
                    .Name("abc")
                )
                .Component("other", 11, 17),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.SyntaxError, 6, 21, 6, 21,
                                             "extraneous input '<' expecting {<EOF>, 'App:', 'Trait:', 'Component:'}"));
        }

        [Fact]
        public async Task DuplicateMethodDefinitionRaisesError()
        {
            const string Test = @"
                Component: button

                    method(name1, name2): call('label')

                    method(): needs-defining
            ";

            await CompileAndAssertErrors(Test,
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionDuplicateMethodDefinition, 6, 21, 6, 44, "method"));
        }

        [Fact]
        public async Task ComponentCanHaveDocumentation()
        {
            const string Test = @"
                ## Button Component
                Component: button
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 3, 17, c => c
                    .Documentation("Button Component")
                )
            );
        }

    }
}
