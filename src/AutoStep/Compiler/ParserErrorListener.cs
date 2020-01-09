using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Listener attached to the AutoStep parser that generates <see cref="CompilerMessage"/> items from any raised syntax errors.
    /// </summary>
    internal partial class ParserErrorListener : BaseErrorListener
    {
        private readonly string? sourceName;
        private readonly ITokenStream tokenStream;
        private readonly List<CompilerMessage> messages;
        private bool swallowEndOfFileErrors = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserErrorListener"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public ParserErrorListener(string? sourceName, ITokenStream tokenStream)
        {
            this.sourceName = sourceName;
            this.tokenStream = tokenStream;
            this.messages = new List<CompilerMessage>();
        }

        /// <summary>
        /// Gets the set of compiler messages.
        /// </summary>
        public IReadOnlyList<CompilerMessage> ParserErrors => messages;

        /// <inheritdoc/>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var ctxt = new DefaultAutoStepErrorHandling(tokenStream, recognizer, offendingSymbol, e);

            ctxt.DoErrorMatching();

            if (ctxt.Code == CompilerMessageCode.UnexpectedEndOfFile && swallowEndOfFileErrors)
            {
                // Do not raise the end of file message if we've been told to ignore it by a precediing error.
                return;
            }

            var errorLength = ctxt.EndingSymbol.StopIndex - ctxt.EndingSymbol.StartIndex;

            if (errorLength < 0)
            {
                errorLength = 0;
            }

            if (ctxt.Code == CompilerMessageCode.SyntaxError)
            {
                // Default form, build the complete message.
                msg = string.Format(CultureInfo.CurrentCulture, CompilerMessageCodeText.SyntaxError, msg);
            }
            else
            {
                msg = CompilerMessageCodeText.ResourceManager.GetString(ctxt.Code.ToString(), CultureInfo.CurrentCulture);
            }

            if (ctxt.IgnoreFollowingEndOfFileMessages)
            {
                swallowEndOfFileErrors = true;
            }

            var compileMsg = new CompilerMessage(
                sourceName,
                CompilerMessageLevel.Error,
                ctxt.Code,
                msg,
                ctxt.StartingSymbol.Line,
                ctxt.StartingSymbol.Column + 1, // Antlr uses 0 offset, but 1 offset for message display feels more natural,
                ctxt.EndingSymbol.Line,
                ctxt.EndingSymbol.Column + 1 + errorLength);

            messages.Add(compileMsg);
        }
    }
}
