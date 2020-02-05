using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions;
using AutoStep.Elements;

namespace AutoStep.Language
{
    /// <summary>
    /// Defines a linking operation result.
    /// </summary>
    public class LinkResult : CompilerResult<FileElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful link.</param>
        /// <param name="messages">The set of messages resulting from a link.</param>
        /// <param name="referencedSources">The set of all referenced step definition sources found by the linker.</param>
        /// <param name="output">The built output (if the link succeeded).</param>
        public LinkResult(bool success, IEnumerable<CompilerMessage> messages, IEnumerable<IStepDefinitionSource>? referencedSources = null, FileElement? output = null)
            : base(success, messages, output)
        {
            if (messages.Any(m => m.Level > CompilerMessageLevel.Info))
            {
                AnyIssues = true;
            }

            ReferencedSources = referencedSources ?? Enumerable.Empty<IStepDefinitionSource>();
        }

        /// <summary>
        /// Gets a value indicating whether any Errors or Warnings were found.
        /// </summary>
        public bool AnyIssues { get; }

        /// <summary>
        /// Gets the set of all referenced step definition sources found by the linker.
        /// </summary>
        public IEnumerable<IStepDefinitionSource> ReferencedSources { get; }
    }
}
