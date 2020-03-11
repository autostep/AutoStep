using System.Collections.Generic;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Contains the result of a interaction set build.
    /// </summary>
    internal class AutoStepInteractionSetBuilderResult : LanguageOperationResult<AutoStepInteractionSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoStepInteractionSetBuilderResult"/> class.
        /// </summary>
        /// <param name="success">Success / failure.</param>
        /// <param name="messages">The set of messages.</param>
        /// <param name="output">The output of the set build.</param>
        public AutoStepInteractionSetBuilderResult(bool success, IEnumerable<LanguageOperationMessage> messages, AutoStepInteractionSet? output = null)
            : base(success, messages, output)
        {
        }
    }
}
