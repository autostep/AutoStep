using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Definitions;

namespace AutoStep.Projects
{
    public interface IProjectCompiler
    {
        void AddStaticStepDefinitionSource(IStepDefinitionSource source);
        void AddUpdatableStepDefinitionSource(IUpdatableStepDefinitionSource source);
        Task<ProjectCompilerResult> Compile(CancellationToken cancelToken = default);
        IEnumerable<IStepDefinitionSource> EnumerateStepDefinitionSources();
        ProjectCompilerResult Link(CancellationToken cancelToken = default);
    }
}