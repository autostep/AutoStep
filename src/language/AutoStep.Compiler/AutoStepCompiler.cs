using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
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
        public enum CompilerOptions
        {
            Default,
            EnableDiagnostics
        }

        private CompilerOptions options;
        private ITracer? tracer;

        public AutoStepCompiler()
        {
            options = CompilerOptions.Default;
        }

        public AutoStepCompiler(CompilerOptions options)
        {
            this.options = options;
        }

        public AutoStepCompiler(CompilerOptions options, ITracer tracer)
            : this(options)
        {
            this.tracer = tracer;
        }

        public async Task<CompilerResult> Compile(IContentSource source, CancellationToken cancelToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var sourceContent = await source.GetContentAsync(cancelToken);

            var inputStream = new AntlrInputStream(sourceContent);

            var lexer = new AutoStepLexer(inputStream);

            var tokenStream = new CommonTokenStream(lexer);

            var parser = new AutoStepParser(tokenStream);
            parser.RemoveErrorListeners();

            var errorListener = new ParserErrorListener(source.SourceName, tokenStream);
            parser.AddErrorListener(errorListener);

            var fileContext = parser.file();

            if (options.HasFlag(CompilerOptions.EnableDiagnostics) && tracer is object)
            {
                tracer.Info("Token Stream for source {sourceName}: \n{tokenStream}", new
                {
                    sourceName = source.SourceName,
                    tokenStream = tokenStream.GetTokenDebugText(lexer.Vocabulary),
                });

                tracer.Info("Compiled Parse Tree for source {sourceName}: \n{parseTree}", new
                {
                    sourceName = source.SourceName,
                    parseTree = fileContext.GetParseTreeDebugText(parser),
                });
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
