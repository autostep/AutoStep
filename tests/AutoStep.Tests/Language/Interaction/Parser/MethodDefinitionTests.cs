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
                    .Method("method", 4, 21, m => m
                        .Argument("name1", 4, 28)
                        .Argument("name2", 4, 35)
                        .Call("call", 4, 42, 4, 54, c => c
                            .String("label", 47)
                        )
                    )
                ), 
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclUnterminated, 4, 40, 4, 40));
        }

        [Fact]
        public async Task MissingExtraArg()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, ): call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 35, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 4, 38, 4, 50, c => c
                            .String("label", 43)
                        )
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclMissingParameter, 4, 33, 4, 35));
        }
        
        [Fact]
        public async Task MethodDefinitionMissingSeparator()
        {
            const string Test = @"                
                Component: button
                
                    method(name1 name2): call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 39, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 4, 42, 4, 54, c => c
                            .String("label", 47)
                        )
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclMissingParameterSeparator, 4, 34, 4, 38));
        }

        [Fact]
        public async Task MethodDefinitionUnexpectedString()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 'something'): call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 46, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 4, 49, 4, 61, c => c
                            .String("label", 54)
                        )
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclUnexpectedContent, 4, 35, 4, 45));
        }

        [Fact]
        public async Task MethodDefinitionUnexpectedFloat()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 123.5): call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 40, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 4, 43, 4, 55, c => c
                            .String("label", 48)
                        )
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclUnexpectedContent, 4, 35, 4, 39));
        }

        [Fact]
        public async Task MethodDefinitionUnexpectedInt()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 123): call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 38, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 4, 41, 4, 53, c => c
                            .String("label", 46)
                        )
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclUnexpectedContent, 4, 35, 4, 37));
        }

        [Fact]
        public async Task MethodDefinitionUnexpectedConstant()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, TAB): call('label')
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 38, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 4, 41, 4, 53, c => c
                            .String("label", 46)
                        )
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionMethodDeclUnexpectedContent, 4, 35, 4, 37));
        }


        [Fact]
        public async Task MethodDefinitionUnterminatedString()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 'something): call()
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 53, m => m
                        .Argument("name1", 4, 28)
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionUnterminatedString, 4, 35, 4, 53));
        }

        [Fact]
        public async Task MethodDefinitionUnterminatedStringCanContinueParseWithPartialDefinitionMatch()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 'something): call()
                        -> call2()

                    method2(): needs-defining
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 5, 26, m => m
                        .Argument("name1", 4, 28)
                        .Call("call2", 5, 28, 5, 34)
                    )
                    .Method("method2", 7, 21, m => m
                        .NeedsDefining()
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionUnterminatedString, 4, 35, 4, 53));
        }
        
        [Fact]
        public async Task MethodDefinitionUnterminatedStringCanContinueParseWithCallOnNextLine()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 'something):
                        -> call()

                    method2(): needs-defining
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 5, 26, m => m
                        .Argument("name1", 4, 28)
                        .Call("call", 5, 28, 5, 33)
                    )
                    .Method("method2", 7, 21, m => m
                        .NeedsDefining()
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionUnterminatedString, 4, 35, 4, 46));
        }

        [Fact]
        public async Task MethodDefinitionUnterminatedStringCanContinueParse()
        {
            const string Test = @"                
                Component: button
                
                    method(name1, 'something): call()

                    method2(): needs-defining
            ";

            await CompileAndAssertErrors(Test, cfg => cfg
                .Component("button", 2, 17, comp => comp
                    .Method("method", 4, 21, 4, 53, m => m
                        .Argument("name1", 4, 28)
                    )
                    .Method("method2", 6, 21, m => m
                        .NeedsDefining()
                    )
                ),
                LanguageMessageFactory.Create(null, CompilerMessageLevel.Error, CompilerMessageCode.InteractionUnterminatedString, 4, 35, 4, 53));
        }
    }
}
