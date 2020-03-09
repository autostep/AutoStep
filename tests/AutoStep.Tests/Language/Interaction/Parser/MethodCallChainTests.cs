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
    /// Method Definition Call behaviour is the same for traits and components,
    /// so these tests should cover declaring them in both places.
    /// </summary>
    public class MethodCallChainTests : InteractionsCompilerTestBase
    {
        public MethodCallChainTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task SingleCallTargetNoArgs()
        {
            const string Test = @"                
                Component: button
                
                    method(): call()
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                        )
                    ));
        }
        
        [Fact]
        public async Task SingleUnterminatedCallNoArgs()
        {
            const string Test = @"                
                Component: button
                
                    method(): call(
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 35)
                        )
                    ),
                    CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallUnterminated, 4, 31, 4, 35));
        }

        [Fact]
        public async Task MultipleCallNoArgs()
        {
            const string Test = @"                
                Component: button
                
                    method(): call() -> call2() -> call3()
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                            .Call("call2", 4, 41, 4, 47)
                            .Call("call3", 4, 52, 4, 58)
                        )
                    ));
        }

        [Fact]
        public async Task MultipleCallSingleMissingSeparatorError()
        {
            const string Test = @"                
                Component: button
                
                    method(): call() call2() -> call3()
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                            .Call("call3", 4, 49, 4, 55)
                        )
                    ),
                    CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallMissingSeparator, 4, 38, 4, 42));
        }

        [Fact]
        public async Task CallChainMultiLineWithComments()
        {
            const string Test = @"                
                Component: button
                
                    method(): call() # comment
                           # comment
                           -> call2() 
                           # comment
                           -> call3()
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                            .Call("call2", 6, 31, 6, 37)
                            .Call("call3", 8, 31, 8, 37)
                        )
                    ));
        }

        [Fact]
        public async Task MultipleDefinitionSingleMissingSeparatorParseContinuesWithError()
        {
            const string Test = @"                
                Component: button
                
                    method(): call() -> call2() call3()

                    method2(): call()
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                            .Call("call2", 4, 41, 4, 47)
                        )
                        .Method("method2", 6, 21, 6, 29, m => m
                            .Call("call", 6, 32, 6, 37)
                        )
                    ),
                    CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallMissingSeparator, 4, 49, 4, 53));
        }

        [Fact]
        public async Task MultipleDefinitionSingleCall()
        {
            const string Test = @"                
                Component: button
                
                    method(): call()

                    method2(): call2()
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                        )
                        .Method("method2", 6, 21, m => m
                            .Call("call2", 6, 32, 6, 38)
                    )));
        }

        [Fact]
        public async Task MultipleComponentDefinitionSeparateCallWithLaterError()
        {
            const string Test = @"                
                Component: button
                
                    method(): call()
                
                Component: field

                    method2(): call2(
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                        )
                    )
                    .Component("field", 6, 17, comp => comp
                        .Method("method2", 8, 21, m => m
                            .Call("call2", 8, 32, 8, 37)
                    )),
                    CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallUnterminated, 8, 32, 8, 37));
        }

        [Fact]
        public async Task MultipleCallMissingSeparatorsError()
        {
            const string Test = @"                
                Component: button
                
                    method(): call() call2() call3()
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 36)
                        )
                    ),
                    CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallMissingSeparator, 4, 38, 4, 42),
                    CompilerMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallMissingSeparator, 4, 46, 4, 50));
        }
    }
}
