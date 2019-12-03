using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using AutoStep.Compiler.Parser;
using AutoStep.Core;

namespace AutoStep.Compiler
{
    internal enum CompilerState
    {
        None,
        PreFeature
    }

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
        public async Task<CompilerResult> Compile(IContentSource source, CancellationToken cancelToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var inputStream = new AntlrInputStream(await source.GetReaderAsync(cancelToken));

            var lexer = new AutoStepLexer(inputStream);

            var tokenStream = new CommonTokenStream(lexer);

            var parser = new AutoStepParser(tokenStream);
            parser.AddErrorListener(new DiagnosticErrorListener());

            var fileContext = parser.file();



            // Compile the file.
            return new CompilerResult();
        }
    }
}
