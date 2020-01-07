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
using AutoStep.Tracing;

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
    public partial class AutoStepCompiler : IAutoStepCompiler
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

        /// <inheritdoc/>
        public TContext CompileEntryPoint<TContext>(
            string content,
            string? sourceName,
            Func<AutoStepParser, TContext> entryPoint,
            out ITokenStream tokenStream,
            out IEnumerable<CompilerMessage> parserErrors,
            int? customLexerStartMode = null)
            where TContext : ParserRuleContext
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (entryPoint is null)
            {
                throw new ArgumentNullException(nameof(entryPoint));
            }

            // Create the source stream, the lexer itself, and the resulting token stream.
            var inputStream = new AntlrInputStream(content);
            var lexer = new AutoStepLexer(inputStream);

            if (customLexerStartMode.HasValue)
            {
                lexer.PushMode(customLexerStartMode.Value);
            }

            var commonTokenStream = new CommonTokenStream(lexer);

            // Create a parser and register our error listener.
            var parser = new AutoStepParser(commonTokenStream);

            // First we will do the simpler/faster SLL strategy.
            parser.RemoveErrorListeners();

            parser.Interpreter.PredictionMode = PredictionMode.SLL;
            parser.ErrorHandler = new BailErrorStrategy();

            TContext context;

            var errorListener = new ParserErrorListener(sourceName, commonTokenStream);

            try
            {
                context = entryPoint(parser);
            }
            catch (ParseCanceledException)
            {
                commonTokenStream.Reset();
                parser.Reset();

                parser.AddErrorListener(errorListener);
                parser.ErrorHandler = new DefaultErrorStrategy();

                // Now we will do the full LL mode.
                parser.Interpreter.PredictionMode = PredictionMode.LL;

                context = entryPoint(parser);
            }

            // Write to the tracer if diagnostics are on.
            if (options.HasFlag(CompilerOptions.EnableDiagnostics) && tracer is object)
            {
                tracer.TraceInfo("Token Stream for source {sourceName}: \n{tokenStream}", new
                {
                    sourceName,
                    tokenStream = commonTokenStream.GetTokenDebugText(lexer.Vocabulary),
                });

                tracer.TraceInfo("Compiled Parse Tree for source {sourceName}: \n{parseTree}", new
                {
                    sourceName,
                    parseTree = context.GetParseTreeDebugText(parser),
                });
            }

            parserErrors = errorListener.ParserErrors;
            tokenStream = commonTokenStream;

            return context;
        }

        /// <inheritdoc/>
        public async Task<FileCompilerResult> CompileAsync(IContentSource source, CancellationToken cancelToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // Read from the content source.
            var sourceContent = await source.GetContentAsync(cancelToken);

            var fileContext = CompileEntryPoint(sourceContent, source.SourceName, p => p.file(), out var tokenStream, out var parserMessages);

            // Allow the op to be cancelled before we jump into the tree walker.
            if (cancelToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            // Inspect the errors.
            if (parserMessages.Any())
            {
                // Parser failed.
                return new FileCompilerResult(false, parserMessages);
            }

            // Once the parser has succeeded, we'll proceed to walk the parse tree and build the file.
            var compilerVisitor = new FileVisitor(source.SourceName, tokenStream);

            var builtFile = compilerVisitor.Visit(fileContext);

            // Compile the file.
            return new FileCompilerResult(compilerVisitor.Success, compilerVisitor.Messages, compilerVisitor.Success ? builtFile : null);
        }
    }
}
