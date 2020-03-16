using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Provides interaction file compilation.
    /// </summary>
    public interface IInteractionCompiler
    {
        /// <summary>
        /// Compile an interaction file, giving an asynchronous compilation result. This method is responsible for generating the file elements, and checking for
        /// language consistency. It does not validate method bindings or component inheritance.
        /// </summary>
        /// <param name="source">The content source to load from.</param>
        /// <param name="logFactory">A log factory to create a compilation logger from.</param>
        /// <param name="cancelToken">A cancellation token for the compile operation.</param>
        /// <returns>A task-wrapped compilation result.</returns>
        ValueTask<InteractionsFileCompilerResult> CompileInteractionsAsync(IContentSource source, ILoggerFactory logFactory, CancellationToken cancelToken = default);
    }
}
