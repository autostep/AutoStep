using System.Collections.Generic;

namespace AutoStep.Language
{
    /// <summary>
    /// Base class for all compilation/link results.
    /// </summary>
    /// <typeparam name="TOutput">The output of the operation.</typeparam>
    public abstract class LanguageOperationResult<TOutput> : LanguageOperationResult
        where TOutput : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageOperationResult{TOutput}"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful operation.</param>
        /// <param name="output">The built output (if the operation succeeded).</param>
        public LanguageOperationResult(bool success, TOutput? output = null)
            : base(success)
        {
            Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageOperationResult{TOutput}"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful operation.</param>
        /// <param name="messages">The set of messages resulting from a operation.</param>
        /// <param name="output">The built output (if the operation succeeded).</param>
        public LanguageOperationResult(bool success, IEnumerable<LanguageOperationMessage> messages, TOutput? output = null)
            : base(success, messages)
        {
            Output = output;
        }

        /// <summary>
        /// Gets the built file (or null if operation failed).
        /// </summary>
        public TOutput? Output { get; }
    }
}
