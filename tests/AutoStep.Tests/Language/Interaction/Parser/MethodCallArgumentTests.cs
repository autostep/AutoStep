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
    /// Method Call Argument Compilation.
    /// </summary>
    public class MethodCallArgumentTests : InteractionsCompilerTestBase
    {
        public MethodCallArgumentTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task AllTypes()
        {
            const string Test = @"
                Component: button

                    method(): call('string1', 123, 123.5, TAB, variable, varArr['something'], varArr[avariable])
            ";

            await CompileAndAssertSuccess(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 112, c => c
                                .String("string1", 36)
                                .Int(123, 47)
                                .Float(123.5, 52)
                                .Constant("TAB", 59)
                                .Variable("variable", 64)
                                .VariableArray("varArr", 74, 92, r => r
                                    .String("something", 81)
                                )
                                .VariableArray("varArr", 95, 111, r => r
                                    .Variable("avariable", 102)
                                )
                            )
                        )
                    ));
        }

        [Fact]
        public async Task MethodCallMissingSeparatorCharacter()
        {
            const string Test = @"
                Component: button

                    method(): call('string1' 123 TAB)
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 53, c => c
                                .String("string1", 36)
                            )
                        )
                    ),
                    LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodCallMissingParameterSeparator, 4, 46, 4, 48));
        }

        [Fact]
        public async Task MethodCallUnterminatedString()
        {
            const string Test = @"
                Component: button

                    method(): call('string1

                    method2(): call()
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                    .Component("button", 2, 17, comp => comp
                        .Method("method", 4, 21, m => m
                            .Call("call", 4, 31, 4, 43, c => c
                                .String("string1", 4, 36, 43)
                            )
                        )
                        .Method("method2", 6, 21, m => m
                            .Call("call", 6, 32, 6, 37)
                        )
                    ),
                    LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionUnterminatedString, 4, 37, 4, 43));
        }
    }
}
