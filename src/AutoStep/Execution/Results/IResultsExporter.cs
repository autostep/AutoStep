using System;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Results
{
    public interface IResultsExporter
    {
        ValueTask Export(IServiceProvider scope, RunContext runContext, IRunResultSet results, CancellationToken cancelToken);
    }
}
