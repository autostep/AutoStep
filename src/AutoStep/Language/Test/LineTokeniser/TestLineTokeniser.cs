using System.Linq;
using Antlr4.Runtime;
using AutoStep.Language.Test.Parser;
using static AutoStep.Language.Test.Parser.AutoStepParser;

namespace AutoStep.Language.Test.LineTokeniser
{
    /// <summary>
    /// Provides line tokenisation. Outputs a set of line tokens, optimised for syntax highlighting rather than full compilation.
    /// The line tokeniser is much more forgiving of errors, and generally strives to extract as much info as it can.
    /// </summary>
    internal class TestLineTokeniser : ILineTokeniser<LineTokeniserState>
    {
        private readonly ILinker linker;
        private readonly bool enableDiagnosticException;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLineTokeniser"/> class.
        /// </summary>
        /// <param name="linker">The linker to use for step reference binding.</param>
        /// <param name="enableDiagnosticException">Set to true to enable the throwing of a diagnostic exception if tokenisation cannot parse the input.</param>
        public TestLineTokeniser(ILinker linker, bool enableDiagnosticException = false)
        {
            this.linker = linker;
            this.enableDiagnosticException = enableDiagnosticException;
        }

        /// <summary>
        /// Tokenises a given line of text.
        /// </summary>
        /// <param name="text">The text to tokenise (no line terminators expected).</param>
        /// <param name="lastState">The state of the tokeniser as returned from this method for the previous line in a file.</param>
        /// <returns>The result of tokenisation.</returns>
        public LineTokeniseResult<LineTokeniserState> Tokenise(string text, LineTokeniserState lastState)
        {
            var parseTree = CompileLine(text, out var tokenStream);

            if (parseTree is object)
            {
                return VisitParseTree(parseTree, tokenStream, lastState);
            }

            // null means that the parser really had no idea, so yield an empty token set, still in the previous state.
            return new LineTokeniseResult<LineTokeniserState>(lastState, Enumerable.Empty<LineToken>());
        }

        private LineTokeniseResult<LineTokeniserState> VisitParseTree(OnlyLineContext ctxt, CommonTokenStream tokenStream, LineTokeniserState lastState)
        {
            var visitor = new TestLineVisitor(linker, ctxt, tokenStream, lastState);

            return visitor.BuildLineResult();
        }

        private OnlyLineContext CompileLine(string line, out CommonTokenStream tokenStream)
        {
            var inputStream = new AntlrInputStream(line);
            var lexer = new AutoStepLexer(inputStream);

            tokenStream = new CommonTokenStream(lexer);

            // Create a parser and register our error listener.
            var parser = new AutoStepParser(tokenStream);

            // We don't want to do anything with errors
            // (yes, I know that sounds odd, but tokenisation should just quietly return empty contexts, rather than generating useful error messages).
            parser.RemoveErrorListeners();

            TestParserErrorListener? diagnosticParser = null;

            if (enableDiagnosticException)
            {
                // Ok, so if we are in tests, we want to be able to collect those errors.
                diagnosticParser = new TestParserErrorListener(null, tokenStream);
                parser.AddErrorListener(diagnosticParser);
            }

            var parseTree = parser.onlyLine();

            if (diagnosticParser is object && diagnosticParser.ParserErrors.Any())
            {
                throw new CompilerDiagnosticException(diagnosticParser.ParserErrors, tokenStream.GetTokenDebugText(parser.Vocabulary));
            }

            return parseTree;
        }
    }
}
