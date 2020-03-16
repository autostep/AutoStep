using System.Collections.Generic;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Contains the result of a interaction set build.
    /// </summary>
    public class InteractionSetBuilderResult : LanguageOperationResult<IInteractionSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionSetBuilderResult"/> class.
        /// </summary>
        /// <param name="success">Success / failure.</param>
        /// <param name="messages">The set of messages.</param>
        /// <param name="output">The output of the set build.</param>
        public InteractionSetBuilderResult(bool success, IEnumerable<LanguageOperationMessage> messages, IInteractionSet? output = null)
            : base(success, messages, output)
        {
        }
    }
}
