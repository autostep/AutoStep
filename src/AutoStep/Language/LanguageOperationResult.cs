using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Language
{
    /// <summary>
    /// Base class for all compilation/link results.
    /// </summary>
    public abstract class LanguageOperationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageOperationResult"/> class.
        /// </summary>
        /// <param name="success">Indicates whether the operation was a success.</param>
        public LanguageOperationResult(bool success)
        {
            Success = success;
            Messages = Enumerable.Empty<LanguageOperationMessage>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageOperationResult"/> class.
        /// </summary>
        /// <param name="success">Indicates whether the operation was a success.</param>
        /// <param name="messages">The set of messages.</param>
        public LanguageOperationResult(bool success, IEnumerable<LanguageOperationMessage> messages)
        {
            Success = success;

            // Freeze the messages
            Messages = messages.ToList();
        }

        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the set of compilation messages.
        /// </summary>
        public IEnumerable<LanguageOperationMessage> Messages { get; }
    }
}
