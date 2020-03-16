using System.Collections.Generic;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Result of a set build result for a specific file.
    /// </summary>
    public class InteractionsFileSetBuildResult : LanguageOperationResult<InteractionFileElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionsFileSetBuildResult"/> class.
        /// </summary>
        /// <param name="success">Whether the set build was successful.</param>
        /// <param name="messages">The set of messages.</param>
        public InteractionsFileSetBuildResult(bool success, IEnumerable<LanguageOperationMessage> messages)
            : base(success, messages)
        {
        }
    }
}
