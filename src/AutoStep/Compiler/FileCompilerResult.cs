using System.Collections.Generic;
using System.Linq;

namespace AutoStep.Compiler
{
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

    /// <summary>
    /// Represents the result from a compilation operation.
    /// </summary>
    public class FileCompilerResult : CompilerResult<BuiltFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public FileCompilerResult(bool success, BuiltFile? output = null)
            : base(success, output)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="messages">The set of messages resulting from a compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public FileCompilerResult(bool success, IEnumerable<CompilerMessage> messages, BuiltFile? output = null)
            : base(success, messages, output)
        {
        }
    }
}
