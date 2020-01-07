using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Represents the result from a compilation operation.
    /// </summary>
    public class CompilerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public CompilerResult(bool success, BuiltFile? output = null)
        {
            Success = success;
            Messages = Enumerable.Empty<CompilerMessage>();
            Output = output;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="messages">The set of messages resulting from a compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public CompilerResult(bool success, IEnumerable<CompilerMessage> messages, BuiltFile? output = null)
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
        public BuiltFile? Output { get; }

        /// <summary>
        /// Gets the set of compilation messages.
        /// </summary>
        public IEnumerable<CompilerMessage> Messages { get; }
    }
}
