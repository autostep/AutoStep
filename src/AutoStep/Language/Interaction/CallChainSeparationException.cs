using Antlr4.Runtime;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Exception thrown internally from the language system when the custom error strategy <see cref="InteractionErrorStrategy"/>
    /// detects a call chain separation issue.
    /// </summary>
    internal class CallChainSeparationException : InputMismatchException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallChainSeparationException"/> class.
        /// </summary>
        /// <param name="recognizer">The parser.</param>
        public CallChainSeparationException(Antlr4.Runtime.Parser recognizer)
            : base(recognizer)
        {
        }
    }
}
