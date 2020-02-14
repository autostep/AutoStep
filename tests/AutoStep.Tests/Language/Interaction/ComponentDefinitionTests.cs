using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction
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
                CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMissingExpectedName, 2, 17, 2, 26));
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
                    .Trait("editable", 9, 40)
                    .Trait("displayable", 10, 50)
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
                
                    based-on: button

                    locateNamed(name): select('label') -> withName(name)
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 2, 17, c => c
                    .BasedOn("button", 4, 31)
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
                
                    based-on: button

                    Step: Given I have clicked on {name}
                        select(""btn[text='<name>']"")
                        -> click()
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 2, 17, c => c
                    .BasedOn("button", 4, 31)
                    .StepDefinition(StepType.Given, "I have clicked on {name}", 6, 21, s => s
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
    }
}
