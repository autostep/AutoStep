using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using AutoStep.Language.Test.Parser;
using AutoStep.Language.Test.Visitors;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Test
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
    public class AutoStepCompiler : IAutoStepCompiler
    {
        private readonly TestCompilerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepCompiler"/> class.
        /// </summary>
        public AutoStepCompiler()
            : this(TestCompilerOptions.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepCompiler"/> class.
        /// </summary>
        /// <param name="options">Compiler options.</param>
        public AutoStepCompiler(TestCompilerOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Generates a step definition from a statement body/declaration.
        /// </summary>
        /// <param name="stepType">The type of step.</param>
        /// <param name="statementBody">The body of the step.</param>
        /// <returns>The step definition parsing result (which may contain errors).</returns>
        public StepDefinitionFromBodyResult CompileStepDefinitionElementFromStatementBody(StepType stepType, string statementBody)
        {
            using var nullLogger = new LoggerFactory();

            return CompileStepDefinitionElementFromStatementBody(nullLogger, stepType, statementBody);
        }

        /// <summary>
        /// Generates a step definition from a statement body/declaration.
        /// </summary>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="stepType">The type of step.</param>
        /// <param name="statementBody">The body of the step.</param>
        /// <returns>The step definition parsing result (which may contain errors).</returns>
        public StepDefinitionFromBodyResult CompileStepDefinitionElementFromStatementBody(ILoggerFactory logFactory, StepType stepType, string statementBody)
        {
            statementBody = statementBody.ThrowIfNull(nameof(statementBody));

            // Trim first, we don't want to worry about the whitespace at the end.
            statementBody = statementBody.Trim();

            var errors = new List<LanguageOperationMessage>();
            var success = true;

            // Compile the text, specifying a starting lexical mode of 'statement'.
            var parseContext = CompileEntryPoint(statementBody, null, p => p.stepDeclarationBody(), logFactory, out var tokenStream, out var parserErrors, AutoStepLexer.definition);

            if (parserErrors.Any(x => x.Level == CompilerMessageLevel.Error))
            {
                errors.AddRange(parserErrors);
                success = false;
            }

            // Now we need a visitor.
            var stepReferenceVisitor = new StepDefinitionVisitor(null, tokenStream);

            // Construct a 'reference' step.
            var stepDefinition = stepReferenceVisitor.BuildStepDefinition(stepType, parseContext);

            if (!stepReferenceVisitor.Success)
            {
                success = false;
            }

            errors.AddRange(stepReferenceVisitor.MessageSet.Messages);

            if (stepDefinition.Arguments is object)
            {
                // At this point, we'll validate the provided 'arguments' to the step. All the arguments should just be variable names.
                foreach (var declaredArgument in stepDefinition.Arguments)
                {
                    // TODO: Validate argument type hints.
                }
            }

            return new StepDefinitionFromBodyResult(success, errors, stepDefinition);
        }

        /// <inheritdoc/>
        public async ValueTask<FileCompilerResult> CompileAsync(IContentSource source, CancellationToken cancelToken = default)
        {
            using var logFactory = new LoggerFactory();

            return await CompileAsync(source, logFactory, cancelToken);
        }

        /// <inheritdoc/>
        public async ValueTask<FileCompilerResult> CompileAsync(IContentSource source, ILoggerFactory logFactory, CancellationToken cancelToken = default)
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

            // Once the parser has completed, we'll proceed to walk the parse tree and build the file.
            var compilerVisitor = new FileVisitor(source.SourceName, tokenStream);

            var builtFile = compilerVisitor.Visit(fileContext);

            var success = parserMessages.All(x => x.Level != CompilerMessageLevel.Error) && !compilerVisitor.MessageSet.AnyErrorMessages;
            var allMessages = parserMessages.Concat(compilerVisitor.MessageSet.Messages);

            // Compile the file.
            return new FileCompilerResult(success, allMessages, builtFile);
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
            Func<AutoStepParser, TContext> entryPoint,
            ILoggerFactory logFactory,
            out ITokenStream tokenStream,
            out IEnumerable<LanguageOperationMessage> parserErrors,
            int? customLexerStartMode = null)
            where TContext : ParserRuleContext
        {
            // Create the source stream, the lexer itself, and the resulting token stream.
            var inputStream = new AntlrInputStream(content);
            var lexer = new AutoStepLexer(inputStream);
            var logger = logFactory.CreateLogger<AutoStepCompiler>();

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

            var errorListener = new AutoStepParserErrorListener(sourceName, commonTokenStream);

            try
            {
                context = entryPoint(parser);
            }
            catch (ParseCanceledException)
            {
                commonTokenStream.Reset();
                parser.Reset();

                parser.AddErrorListener(errorListener);
                parser.ErrorHandler = new TestErrorStrategy();

                // Now we will do the full LL mode.
                parser.Interpreter.PredictionMode = PredictionMode.LL;

                context = entryPoint(parser);
            }

            // Write to the tracer if diagnostics are on.
            if (options.HasFlag(TestCompilerOptions.EnableDiagnostics))
            {
                logger.LogDebug(
                    CompilerLogMessages.AutoStepCompiler_TokenStreamForSource,
                    sourceName,
                    commonTokenStream.GetTokenDebugText(lexer.Vocabulary));

                logger.LogDebug(
                    CompilerLogMessages.AutoStepCompiler_CompiledParseTreeForSource,
                    sourceName,
                    context.GetParseTreeDebugText(parser));
            }

            parserErrors = errorListener.ParserErrors;
            tokenStream = commonTokenStream;

            return context;
        }
    }
}
