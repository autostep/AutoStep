using System.Collections.Generic;
using AutoStep.Elements.Test;
using AutoStep.Language.Position;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful compilation.</param>
        /// <param name="messages">The set of messages resulting from a compilation.</param>
        /// <param name="positions">The position lookup.</param>
        /// <param name="output">The built output (if the compilation succeeded).</param>
        public FileCompilerResult(bool success, IEnumerable<LanguageOperationMessage> messages, IPositionIndex? positions, FileElement? output = null)
            : base(success, messages, output)
        {
            Positions = positions;
        }

        /// <summary>
        /// Gets the position reference, only generated if <see cref="TestCompilerOptions.CreatePositionIndex"/> is set.
        /// </summary>
        public IPositionIndex? Positions { get; }
    }
}
