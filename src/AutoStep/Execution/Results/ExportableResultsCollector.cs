using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Provides a results collector that invokes each of the registered <see cref="IResultsExporter"/> implementations
    /// when the results are available.
    /// </summary>
    public class ExportableResultsCollector : ResultsCollector
    {
        private const string ExportScopeTag = "export";

        /// <inheritdoc/>
        protected override async ValueTask OnResultsReady(IServiceProvider scope, RunContext ctxt, WorkingResultSet results, CancellationToken cancelToken)
        {
            var logger = scope.GetRequiredService<ILogger<ExportableResultsCollector>>();
            var allTasks = new List<Task>();

            var exporters = scope.GetRequiredService<IEnumerable<IResultsExporter>>();

            logger.LogDebug(ExportableResultsCollectorMessages.StartingResultExport);

            // Invoke our exporters.
            foreach (var exporter in exporters)
            {
                allTasks.Add(InvokeExporter(exporter, scope, ctxt, results, cancelToken));
            }

            if (allTasks.Count == 0)
            {
                logger.LogWarning(ExportableResultsCollectorMessages.NoResultExporters);
            }
            else
            {
                await Task.WhenAll(allTasks).ConfigureAwait(false);

                logger.LogDebug(ExportableResultsCollectorMessages.EndResultExport);
            }
        }

        /// <summary>
        /// Called to invoke an individual exporter.
        /// </summary>
        /// <param name="exporter">The exporter.</param>
        /// <param name="scope">The execution service scope.</param>
        /// <param name="runContext">The run context.</param>
        /// <param name="results">The set of results.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>A completion task.</returns>
        [SuppressMessage(
            "Design",
            "CA1031:Do not catch general exception types",
            Justification = "Need to handle any error an exporter might raise.")]
        protected virtual async Task InvokeExporter(IResultsExporter exporter, IServiceProvider scope, RunContext runContext, IRunResultSet results, CancellationToken cancelToken)
        {
            if (exporter is null)
            {
                throw new ArgumentNullException(nameof(exporter));
            }

            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (runContext is null)
            {
                throw new ArgumentNullException(nameof(runContext));
            }

            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            var asScope = (IAutoStepServiceScope)scope;

            // Give each exporter its own service scope; that way they are somewhat isolated from each other when we run them in parallel.
            using (var exportScope = asScope.BeginNewScope(ExportScopeTag))
            {
                var logger = scope.GetRequiredService<ILogger<ExportableResultsCollector>>();

                var exporterName = exporter.ToString();

                try
                {
                    logger.LogDebug(ExportableResultsCollectorMessages.ExportingResults, exporterName);

                    await exporter.Export(scope, runContext, results, cancelToken);

                    logger.LogDebug(ExportableResultsCollectorMessages.ExportComplete, exporterName);
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning(ExportableResultsCollectorMessages.ExportCancelled, exporterName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ExportableResultsCollectorMessages.ExportException, exporterName, ex.Message);
                }
            }
        }
    }
}
