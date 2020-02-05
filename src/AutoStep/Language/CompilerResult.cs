using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Language
{
    /// <summary>
    /// Base class for all compilation/link results.
    /// </summary>
    /// <typeparam name="TOutput">The output of the operation.</typeparam>
    public abstract class CompilerResult<TOutput>
        where TOutput : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerResult{TOutput}"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public CompilerResult(bool success, TOutput? output = null)
        {
            Success = success;
            Messages = Enumerable.Empty<CompilerMessage>();
            Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerResult{TOutput}"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="messages">The set of messages resulting from a compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public CompilerResult(bool success, IEnumerable<CompilerMessage> messages, TOutput? output = null)
        {
            Success = success;

            // Freeze the messages
            Messages = messages.ToArray();
            Output = output;
        }

        /// <summary>
        /// Gets a value indicating whether the compilation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the built file (or null if compilation failed).
        /// </summary>
        public TOutput? Output { get; }

        /// <summary>
        /// Gets the set of compilation messages.
        /// </summary>
        public IEnumerable<CompilerMessage> Messages { get; }
    }
}
