using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Antlr4.Runtime;

namespace AutoStep.Language
{
    /// <summary>
    /// Listener attached to the AutoStep parser that generates <see cref="LanguageOperationMessage"/> items from any raised syntax errors.
    /// </summary>
    /// <typeparam name="TParser">The underlying ANTLR Parser implementation that this listener attaches to.</typeparam>
    internal abstract class ParserErrorListener<TParser> : BaseErrorListener
        where TParser : Parser
    {
        private readonly string? sourceName;
        private readonly List<LanguageOperationMessage> messages;
        private bool swallowEndOfFileErrors = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserErrorListener{TParser}"/> class.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="tokenStream">The token stream.</param>
        public ParserErrorListener(string? sourceName, ITokenStream tokenStream)
        {
            this.sourceName = sourceName;
            this.messages = new List<LanguageOperationMessage>();
            this.TokenStream = tokenStream;
        }

        /// <summary>
        /// Gets the token stream.
        /// </summary>
        protected ITokenStream TokenStream { get; }

        /// <summary>
        /// Gets the set of compiler messages.
        /// </summary>
        public IReadOnlyList<LanguageOperationMessage> ParserErrors => messages;

        /// <summary>
        /// A derived implementation should create a new appopriate error handling context when this method is called.
        /// </summary>
        /// <param name="recognizer">The recognizer.</param>
        /// <param name="offendingSymbol">The offending symbol.</param>
        /// <param name="e">The recognition exception.</param>
        /// <returns>An error context.</returns>
        protected abstract BaseAutoStepErrorHandlingContext<TParser> CreateErrorContext(IRecognizer recognizer, IToken offendingSymbol, RecognitionException e);

        /// <inheritdoc/>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var ctxt = CreateErrorContext(recognizer, offendingSymbol, e);

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

            var compileMsg = new LanguageOperationMessage(
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
