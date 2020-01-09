using System.Collections.Generic;

namespace AutoStep.Compiler
{
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
