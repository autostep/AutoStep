using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Defines the interface for a service that can export results at the end of a test run.
    /// </summary>
    public interface IResultsExporter
    {
        /// <summary>
        /// Export the provided results.
        /// </summary>
        /// <param name="scope">The current service scope.</param>
        /// <param name="runContext">The context of the test run.</param>
        /// <param name="results">The set of all results from the run.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>An async completion task.</returns>
        ValueTask ExportAsync(ILifetimeScope scope, RunContext runContext, IRunResultSet results, CancellationToken cancelToken);
    }
}
