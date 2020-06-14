using System.Collections.Generic;
using AutoStep.Language;

namespace AutoStep.Projects
{
    /// <summary>
    /// Represents the outcome of a project build, by <see cref="IProjectBuilder.CompileAsync(System.Threading.CancellationToken)"/>
    /// or <see cref="IProjectBuilder.Link(System.Threading.CancellationToken)"/>.
    /// </summary>
    public class ProjectBuilderResult : LanguageOperationResult<Project>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderResult"/> class.
        /// </summary>
        /// <param name="success">Did the compilation succeed.</param>
        /// <param name="messages">The aggregate set of messages across the whole project.</param>
        /// <param name="output">The compiled project.</param>
        public ProjectBuilderResult(bool success, IEnumerable<LanguageOperationMessage> messages, Project? output = null)
            : base(success, messages, output)
        {
        }
    }
}
