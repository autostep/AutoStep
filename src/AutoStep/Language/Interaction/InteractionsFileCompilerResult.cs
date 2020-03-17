using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Represents the result of an interactions file compilation.
    /// </summary>
    public class InteractionsFileCompilerResult : LanguageOperationResult<InteractionFileElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionsFileCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Success/failure.</param>
        /// <param name="messages">The set of messages generated during compilation.</param>
        /// <param name="output">The built output.</param>
        public InteractionsFileCompilerResult(bool success, IEnumerable<LanguageOperationMessage> messages, InteractionFileElement? output = null)
            : base(success, messages, output)
        {
        }
    }
}
