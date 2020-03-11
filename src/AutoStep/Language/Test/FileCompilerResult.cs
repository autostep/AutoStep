using System.Collections.Generic;
using AutoStep.Elements.Test;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// Represents the result from a compilation operation.
    /// </summary>
    public class FileCompilerResult : LanguageOperationResult<FileElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public FileCompilerResult(bool success, FileElement? output = null)
            : base(success, output)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="messages">The set of messages resulting from a compilation.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public FileCompilerResult(bool success, IEnumerable<LanguageOperationMessage> messages, FileElement? output = null)
            : base(success, messages, output)
        {
        }
    }
}
