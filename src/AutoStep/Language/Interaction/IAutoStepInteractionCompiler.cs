using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AutoStep.Language.Interaction
{
    public interface IAutoStepInteractionCompiler
    {
        ValueTask<InteractionsFileCompilerResult> CompileInteractionsAsync(IContentSource source, ILoggerFactory logFactory, CancellationToken cancelToken = default);
    }
}