using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Provides the autostep compiler as a service, and allows text content to be turned into a built structure of AutoStep content.
    /// </summary>
    public interface IAutoStepCompiler
    {
        /// <summary>
        /// Compile a source of AutoStep content (e.g. a file) and output the result.
        /// </summary>
        /// <param name="source">The source of the content to load.</param>
        /// <param name="cancelToken">A cancellation token to allow source loading or compilation to be cancelled.</param>
        /// <returns>A compilation result that indicates success or failure, and contains the built content.</returns>
        Task<CompilerResult> CompileAsync(IContentSource source, CancellationToken cancelToken = default);

        /// <summary>
        /// Compile a set of textual content into a resulting Antlr parse context, specifying the start point in the parse tree.
        /// </summary>
        /// <typeparam name="TContext">The type of context that is expected.</typeparam>
        /// <param name="content">The text content to parse.</param>
        /// <param name="sourceName">The name of the source (used for any errors).</param>
        /// <param name="entryPoint">A function that invokes the relevant Antlr parser context method.</param>
        /// <param name="tokenStream">The loaded token stream.</param>
        /// <param name="parserErrors">Any parser errors.</param>
        /// <param name="customLexerStartMode">An optional custom lexer mode to start parsing at.</param>
        /// <returns>The parsed context.</returns>
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
