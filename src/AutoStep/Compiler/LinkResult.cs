using System.Collections.Generic;
using System.Linq;
using AutoStep.Definitions;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Defines a linking operation result.
    /// </summary>
    public class LinkResult : CompilerResult<BuiltFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkResult"/> class.
        /// </summary>
        /// <param name="success">Indicates a successful link.</param>
        /// <param name="messages">The set of messages resulting from a link.</param>
        /// <param name="output">The built output (if the link succeeded).</param>
        public LinkResult(bool success, IEnumerable<CompilerMessage> messages, IEnumerable<IStepDefinitionSource>? referencedSources = null, BuiltFile? output = null)
            : base(success, messages, output)
        {
            if (messages.Any(m => m.Level > CompilerMessageLevel.Info))
            {
                AnyIssues = true;
            }

            ReferencedSources = referencedSources ?? Enumerable.Empty<IStepDefinitionSource>();
        }

        public bool AnyIssues { get; internal set; }

        public IEnumerable<IStepDefinitionSource> ReferencedSources { get; }
    }
}
