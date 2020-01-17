using System.Collections.Generic;
using AutoStep.Compiler;

namespace AutoStep.Projects
{
    /// <summary>
    /// Represents the outcome of a project compilation, by <see cref="ProjectCompiler.Compile(System.Threading.CancellationToken)"/>.
    /// </summary>
    public class ProjectCompilerResult : CompilerResult<Project>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectCompilerResult"/> class.
        /// </summary>
        /// <param name="success">Did the compilation succeed.</param>
        /// <param name="messages">The aggregate set of messages across the whole project.</param>
        /// <param name="output">The compiled project.</param>
        public ProjectCompilerResult(bool success, IEnumerable<CompilerMessage> messages, Project? output = null)
            : base(success, messages, output)
        {
        }
    }
}
