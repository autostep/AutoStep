using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Language;
using AutoStep.Tests.Builders;
using AutoStep.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace AutoStep.Tests.Language.Interaction.Parser
{
    /// <summary>
    /// Method Definition Parsing behaviour is the same for traits and components,
    /// so these tests should cover declaring them in both places.
    /// </summary>
    public class MethodDefinitionTests : InteractionsCompilerTestBase
    {
        public MethodDefinitionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task DeclareMethod()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, name2): call('label')
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, m => m
                        .Argument("name1", 4, 28)
                        .Argument("name2", 4, 35)
                        .Call("call", 4, 43, 4, 55, c => c
                            .String("label", 48)
                        )
                    )
                ));
        }

        [Fact]
        public async Task UnterminatedDeclaration()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, name2: call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 39, m => m
                        .Argument("name1", 4, 28)
                        .Argument("name2", 4, 35)
                        .Call("call", 4, 43, 4, 55, c => c
                            .String("label", 48)
                        )
                    )
                ), 
                CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionUnterminatedMethod, 4, 21, 4, 39));
        }
    }
}
