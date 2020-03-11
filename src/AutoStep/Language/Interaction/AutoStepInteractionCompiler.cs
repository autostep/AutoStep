using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Interaction.Parser;
using AutoStep.Language.Interaction.Visitors;
using AutoStep.Language.Test;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Provides the compilation of AutoStep interaction files.
    /// </summary>
    public class AutoStepInteractionCompiler : IAutoStepInteractionCompiler
    {
        private readonly InteractionsCompilerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepInteractionCompiler"/> class.
        /// </summary>
        /// <param name="options">Compiler options.</param>
        public AutoStepInteractionCompiler(InteractionsCompilerOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        public async ValueTask<InteractionsFileCompilerResult> CompileInteractionsAsync(IContentSource source, ILoggerFactory logFactory, CancellationToken cancelToken = default)
        {
            source = source.ThrowIfNull(nameof(source));

            // Read from the content source.
            var sourceContent = await source.GetContentAsync(cancelToken);

            var fileContext = CompileEntryPoint(sourceContent, source.SourceName, p => p.file(), logFactory, out var tokenStream, out var parserMessages);

            // Allow the op to be cancelled before we jump into the tree walker.
            if (cancelToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            var fileVisitor = new InteractionsFileVisitor(source.SourceName, tokenStream);

            var result = fileVisitor.Visit(fileContext);

            var success = parserMessages.All(x => x.Level != CompilerMessageLevel.Error) && !fileVisitor.MessageSet.AnyErrorMessages;
            var allMessages = parserMessages.Concat(fileVisitor.MessageSet.Messages);

            return new InteractionsFileCompilerResult(success, allMessages, result);
        }

        /// <summary>
        /// Compile a set of textual content into a resulting Antlr parse context, specifying the start point in the parse tree.
        /// </summary>
        /// <typeparam name="TContext">The type of context that is expected.</typeparam>
        /// <param name="content">The text content to parse.</param>
        /// <param name="sourceName">The name of the source (used for any errors).</param>
        /// <param name="entryPoint">A function that invokes the relevant Antlr parser context method.</param>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="tokenStream">The loaded token stream.</param>
        /// <param name="parserErrors">Any parser errors.</param>
        /// <param name="customLexerStartMode">An optional custom lexer mode to start parsing at.</param>
        /// <returns>The parsed context.</returns>
        private TContext CompileEntryPoint<TContext>(
            string content,
            string? sourceName,
            Func<AutoStepInteractionsParser, TContext> entryPoint,
            ILoggerFactory logFactory,
            out ITokenStream tokenStream,
            out IEnumerable<LanguageOperationMessage> parserErrors,
            int? customLexerStartMode = null)
            where TContext : ParserRuleContext
        {
            // Create the source stream, the lexer itself, and the resulting token stream.
            var inputStream = new AntlrInputStream(content);
            var lexer = new AutoStepInteractionsLexer(inputStream);

            var logger = logFactory.CreateLogger<AutoStepInteractionCompiler>();

            if (customLexerStartMode.HasValue)
            {
                lexer.PushMode(customLexerStartMode.Value);
            }

            var commonTokenStream = new CommonTokenStream(lexer);

            // Create a parser and register our error listener.
            var parser = new AutoStepInteractionsParser(commonTokenStream);

            // First we will do the simpler/faster SLL strategy.
            parser.RemoveErrorListeners();

            parser.Interpreter.PredictionMode = PredictionMode.SLL;
            parser.ErrorHandler = new BailErrorStrategy();

            TContext context;

            var errorListener = new InteractionsErrorListener(sourceName, commonTokenStream);

            try
            {
                context = entryPoint(parser);
            }
            catch (ParseCanceledException)
            {
                commonTokenStream.Reset();
                parser.Reset();

                parser.AddErrorListener(errorListener);

                // Use our custom error strategy for giving more meaningful errors.
                parser.ErrorHandler = new InteractionErrorStrategy();

                // Now we will do the full LL mode.
                parser.Interpreter.PredictionMode = PredictionMode.LL;

                context = entryPoint(parser);
            }

            // Write to the tracer if diagnostics are on.
            if (options.HasFlag(InteractionsCompilerOptions.EnableDiagnostics))
            {
                logger.LogDebug(
                    CompilerLogMessages.AutoStepInteractionCompiler_TokenStreamForSource,
                    sourceName,
                    commonTokenStream.GetTokenDebugText(lexer.Vocabulary));

                logger.LogDebug(
                    CompilerLogMessages.AutoStepInteractionCompiler_CompiledParseTreeForSource,
                    sourceName,
                    context.GetParseTreeDebugText(parser));
            }

            parserErrors = errorListener.ParserErrors;
            tokenStream = commonTokenStream;

            return context;
        }
    }
}
