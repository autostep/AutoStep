using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    public interface IAutoStepCompiler
    {
        Task<CompilerResult> CompileAsync(IContentSource source, CancellationToken cancelToken = default);

        TContext CompileEntryPoint<TContext>(
            string content,
            string? sourceName,
            Func<AutoStepParser, TContext> entryPoint,
            out ITokenStream tokenStream,
            out IEnumerable<CompilerMessage> parserErrors,
            int? customLexerStartMode = null)
            where TContext : ParserRuleContext;
    }
}
