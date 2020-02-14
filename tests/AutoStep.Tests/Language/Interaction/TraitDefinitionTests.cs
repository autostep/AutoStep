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
    public class TraitDefinitionTests : InteractionsCompilerTestBase
    {
        public TraitDefinitionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task CanDefineEmptyTrait()
        {
            const string Test = @"
                Trait: clickable
            ";

           await CompileAndAssertSuccess(Test, file => file
                .Trait("clickable", 2, 17, t => t
                    .NamePart("clickable", 24)
                )
            );
        }

        [Fact]
        public async Task CanDefineTraitWithMultipleParts()
        {
            const string Test = @"
                Trait: clickable + editable
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + editable", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("editable", 36)
                 )
             );
        }

        [Fact]
        public async Task CanDefineMultipleTraits()
        {
            const string Test = @"
                Trait: clickable + editable
                
                Trait: clickable + editable + visible
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + editable", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("editable", 36)
                 )
                 .Trait("clickable + editable + visible", 4, 17, t => t
                    .NamePart("clickable", 24)
                    .NamePart("editable", 36)
                    .NamePart("visible", 47)
                )
             );
        }
        
        [Fact]
        public async Task CanDefineTraitWithStep()
        {
            const string Test = @"
                Trait: clickable + named

                    Step: Given I have clicked the {arg} $component$
                        locateNamed(arg)
                        -> click()
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + named", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("named", 36)
                     .StepDefinition(StepType.Given, "I have clicked the {arg} $component$", 4, 21, step => step
                        .WordPart("I", 33)
                        .WordPart("have", 35)
                        .WordPart("clicked", 40)
                        .WordPart("the", 48)
                        .Argument("{arg}", "arg", 52)
                        .ComponentMatch(58)
                        .Expression(e => e
                            .Call("locateNamed", 5, 25, 5, 40, c => c
                                .Variable("arg", 37)
                            )
                            .Call("click", 6, 28, 6, 34)
                        )
                     )
                     
                 )
             );
        }

        [Fact]
        public async Task CanDefineTraitWithStepAndMethods()
        {
            const string Test = @"
                Trait: clickable + named

                    Step: Given I have clicked the {arg} $component$
                        locateNamed(arg)
                        -> click()
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + named", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("named", 36)
                     .StepDefinition(StepType.Given, "I have clicked the {arg} $component$", 4, 21, step => step
                        .WordPart("I", 33)
                        .WordPart("have", 35)
                        .WordPart("clicked", 40)
                        .WordPart("the", 48)
                        .Argument("{arg}", "arg", 52)
                        .ComponentMatch(58)
                        .Expression(e => e
                            .Call("locateNamed", 5, 25, 5, 40, c => c
                                .Variable("arg", 37)
                            )
                            .Call("click", 6, 28, 6, 34)
                        )
                     )
                 )
             );
        }


        [Fact]
        public async Task CanDefineTraitWithMethods()
        {
            const string Test = @"
                Trait: clickable + editable

                    locateNamed(name): select('label') -> withName(name)

                    locateFirst(): select('label') -> first()
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + editable", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("editable", 36)
                     .Method("locateNamed", 4, 21, def => def
                        .Argument("name", 4, 33)
                        .Call("select", 4, 40, 4, 54, m => m.String("label", 47))
                        .Call("withName", 4, 59, 4, 72, m => m.Variable("name", 68))
                     )
                     .Method("locateFirst", 6, 21, def => def
                        .Call("select", 6, 36, 6, 50, m => m.String("label", 43))
                        .Call("first", 6, 55, 6, 61)
                    )
                 )               
             );
        }

        [Fact]
        public async Task CanDefineTraitWithMethodLineBreaks()
        {
            const string Test = @"
                Trait: clickable + editable

                    locateNamed(name): select('label') 
                                       -> withName(name)

                    locateFirst(): select('label') 
                                   -> first()
            ";

            await CompileAndAssertSuccess(Test, file => file
                 .Trait("clickable + editable", 2, 17, t => t
                     .NamePart("clickable", 24)
                     .NamePart("editable", 36)
                     .Method("locateNamed", 4, 21, def => def
                        .Argument("name", 4, 33)
                        .Call("select", 4, 40, 4, 54, m => m.String("label", 47))
                        .Call("withName", 5, 43, 5, 56, m => m.Variable("name", 52))
                     )
                     .Method("locateFirst", 7, 21, def => def
                        .Call("select", 7, 36, 7, 50, m => m.String("label", 43))
                        .Call("first", 8, 39, 8, 45)
                    )
                 )
             );
        }
    }
}
