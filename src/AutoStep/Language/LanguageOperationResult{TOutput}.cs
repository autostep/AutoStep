using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Language
{
    /// <summary>
    /// Base class for all compilation/link results.
    /// </summary>
    /// <typeparam name="TOutput">The output of the operation.</typeparam>
    public abstract class LanguageOperationResult<TOutput>
        where TOutput : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageOperationResult{TOutput}"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful operation.</param>
        /// <param name="output">The built output (if the operation succeeded).</param>
        public LanguageOperationResult(bool success, TOutput? output = null)
        {
            Success = success;
            Messages = Enumerable.Empty<LanguageOperationMessage>();
            Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageOperationResult{TOutput}"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful operation.</param>
        /// <param name="messages">The set of messages resulting from a operation.</param>
        /// <param name="output">The built output (if the operation succeeded).</param>
        public LanguageOperationResult(bool success, IEnumerable<LanguageOperationMessage> messages, TOutput? output = null)
        {
            Success = success;

            // Freeze the messages
            Messages = messages.ToArray();
            Output = output;
        }

        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the built file (or null if operation failed).
        /// </summary>
        public TOutput? Output { get; }

        /// <summary>
        /// Gets the set of compilation messages.
        /// </summary>
        public IEnumerable<LanguageOperationMessage> Messages { get; }
    }
}
