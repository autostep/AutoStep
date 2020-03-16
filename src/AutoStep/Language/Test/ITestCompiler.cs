using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// Provides the autostep compiler as a service, and allows text content to be turned into a built structure of AutoStep content.
    /// </summary>
    public interface ITestCompiler
    {
        /// <summary>
        /// Compile a source of AutoStep content (e.g. a file) and output the result.
        /// </summary>
        /// <param name="source">The source of the content to load.</param>
        /// <param name="cancelToken">A cancellation token to allow source loading or compilation to be cancelled.</param>
        /// <returns>A compilation result that indicates success or failure, and contains the built content.</returns>
        ValueTask<FileCompilerResult> CompileAsync(IContentSource source, CancellationToken cancelToken = default);

        /// <summary>
        /// Compile a source of AutoStep content (e.g. a file) and output the result.
        /// </summary>
        /// <param name="source">The source of the content to load.</param>
        /// <param name="logFactory">A logger factory.</param>
        /// <param name="cancelToken">A cancellation token to allow source loading or compilation to be cancelled.</param>
        /// <returns>A compilation result that indicates success or failure, and contains the built content.</returns>
        ValueTask<FileCompilerResult> CompileAsync(IContentSource source, ILoggerFactory logFactory, CancellationToken cancelToken = default);

        /// <summary>
        /// Generates a step definition from a statement body/declaration.
        /// </summary>
        /// <param name="stepType">The type of step.</param>
        /// <param name="statementBody">The body of the step.</param>
        /// <returns>The step definition parsing result (which may contain errors).</returns>
        public StepDefinitionFromBodyResult CompileStepDefinitionElementFromStatementBody(StepType stepType, string statementBody);
    }
}
