using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using AutoStep.Compiler.Parser;
using AutoStep.Core.Tracing;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Compiles individual AutoStep files and outputs built files.
    /// </summary>
    /// <remarks>
    /// The AutoStep Compiler goes through the given file and parses it, outputting an in-memory definition
    /// of the file, including features, scenarios, steps etc.
    ///
    /// The AutoStepLinker will go through the built output and bind it against a given project's available steps.
    /// </remarks>
    public class AutoStepCompiler
    {
        private readonly CompilerOptions options;
        private readonly ITracer? tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepCompiler"/> class.
        /// </summary>
        public AutoStepCompiler()
            : this(CompilerOptions.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepCompiler"/> class.
        /// </summary>
        /// <param name="options">Compiler options.</param>
        public AutoStepCompiler(CompilerOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepCompiler"/> class.
        /// </summary>
        /// <param name="options">Compiler options.</param>
        /// <param name="tracer">A tracer instance that will receive internal messages and diagnostics from the compiler.</param>
        public AutoStepCompiler(CompilerOptions options, ITracer tracer)
            : this(options)
        {
            this.tracer = tracer;
        }

        /// <summary>
        /// The available Compiler Options.
        /// </summary>
        [Flags]
        public enum CompilerOptions
        {
            /// <summary>
            /// Default compiler behaviour.
            /// </summary>
            Default,

            /// <summary>
            /// Enable diagnostics, which causes full lexer and parser data to be written to the tracer.
            /// </summary>
            EnableDiagnostics,
        }

        /// <summary>
        /// Compile a content source, and return a result.
        /// </summary>
        /// <param name="source">A source (e.g. <see cref="StringContentSource"/>) containing an AutoStep file.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>A task, the result of which is the outcome of compilation.</returns>
        public async Task<CompilerResult> CompileAsync(IContentSource source, CancellationToken cancelToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Read from the content source.
            var sourceContent = await source.GetContentAsync(cancelToken);

            // Create the soruce stream, the lexer itself, and the resulting token stream.
            var inputStream = new AntlrInputStream(sourceContent);
            var lexer = new AutoStepLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);

            // Create a parser and register our error listener.
            var parser = new AutoStepParser(tokenStream);

            // First we will do the simpler/faster SLL strategy.
            parser.RemoveErrorListeners();

            parser.Interpreter.PredictionMode = PredictionMode.SLL;
            parser.ErrorHandler = new BailErrorStrategy();

            AutoStepParser.FileContext fileContext;

            var errorListener = new ParserErrorListener(source.SourceName, tokenStream);

            try
            {
                fileContext = parser.file();
            }
            catch (ParseCanceledException)
            {
                tokenStream.Reset();
                parser.Reset();

                parser.AddErrorListener(errorListener);
                parser.ErrorHandler = new DefaultErrorStrategy();

                // Now we will do the full LL mode.
                parser.Interpreter.PredictionMode = PredictionMode.LL;

                fileContext = parser.file();
            }

            // Write to the tracer if diagnostics are on.
            if (options.HasFlag(CompilerOptions.EnableDiagnostics) && tracer is object)
            {
                tracer.TraceInfo("Token Stream for source {sourceName}: \n{tokenStream}", new
                {
                    sourceName = source.SourceName,
                    tokenStream = tokenStream.GetTokenDebugText(lexer.Vocabulary),
                });

                tracer.TraceInfo("Compiled Parse Tree for source {sourceName}: \n{parseTree}", new
                {
                    sourceName = source.SourceName,
                    parseTree = fileContext.GetParseTreeDebugText(parser),
                });
            }

            // Allow the op to be cancelled before we jump into the tree walker.
            if (cancelToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            // Inspect the errors.
            if (errorListener.ParserErrors.Any())
            {
                // Parser failed.
                return new CompilerResult(false, errorListener.ParserErrors);
            }

            // Once the parser has succeeded, we'll proceed to walk the parse tree and build the file.
            var compilerVisitor = new CompilerTreeWalker(source.SourceName, tokenStream);

            var builtFile = compilerVisitor.Visit(fileContext);

            // Compile the file.
            return new CompilerResult(compilerVisitor.Success, compilerVisitor.Messages, compilerVisitor.Success ? builtFile : null);
        }
    }
}
