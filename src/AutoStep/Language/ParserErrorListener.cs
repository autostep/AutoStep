using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Antlr4.Runtime;

namespace AutoStep.Language
{
    /// <summary>
    /// Listener attached to the AutoStep parser that generates <see cref="CompilerMessage"/> items from any raised syntax errors.
    /// </summary>
    internal abstract class ParserErrorListener<TParser> : BaseErrorListener
        where TParser : Antlr4.Runtime.Parser
    {
        private readonly string? sourceName;
        private readonly List<CompilerMessage> messages;
        private bool swallowEndOfFileErrors = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserErrorListener{TParser}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public ParserErrorListener(string? sourceName, ITokenStream tokenStream)
        {
            this.sourceName = sourceName;
            this.messages = new List<CompilerMessage>();

            this.TokenStream = tokenStream;
        }

        protected ITokenStream TokenStream { get; }

        /// <summary>
        /// Gets the set of compiler messages.
        /// </summary>
        public IReadOnlyList<CompilerMessage> ParserErrors => messages;

        protected abstract BaseAutoStepErrorHandlingContext<TParser> GetErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e);

        /// <inheritdoc/>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var ctxt = GetErrorContext(recognizer, offendingSymbol, e);

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

                if (ctxt.MessageArguments?.Length > 0)
                {
                    msg = string.Format(CultureInfo.CurrentCulture, msg, ctxt.MessageArguments);
                }
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
