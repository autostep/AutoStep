using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoStep.Compiler.Parser;
using AutoStep.Elements;
using AutoStep.Sources;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Defines a linking operation result.
    /// </summary>
    public class LinkResult : CompilerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful link.</param>
        /// <param name="output">The built output (if the link succeeded).</param>
        public LinkResult(bool success, BuiltFile? output = null)
            : base(success, output)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful link.</param>
        /// <param name="messages">The set of messages resulting from a link.</param>
        /// <param name="output">The built output (if the link succeeded).</param>
        public LinkResult(bool success, IEnumerable<CompilerMessage> messages, BuiltFile? output = null)
            : base(success, messages, output)
        {
        }
    }
}
