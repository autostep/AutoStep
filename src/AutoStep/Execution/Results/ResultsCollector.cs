using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Elements.Metadata;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Events;
using Microsoft.Extensions.DependencyInjection;

namespace AutoStep.Execution.Results
{
    /// <summary>
    /// Provides basic result collection behaviour that will invoke <see cref="OnResultsReady"/> with the complete set of results
    /// when execution has completed.
    /// </summary>
    public abstract class ResultsCollector : BaseEventHandler
    {
        private readonly WorkingResultSet resultData = new WorkingResultSet();

        /// <inheritdoc/>
        public override async ValueTask OnExecuteAsync(IServiceProvider scope, RunContext ctxt, Func<IServiceProvider, RunContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (ctxt is null)
            {
                throw new ArgumentNullException(nameof(ctxt));
            }

            if (nextHandler is null)
            {
                throw new ArgumentNullException(nameof(nextHandler));
            }

            try
            {
                resultData.StartTimeUtc = DateTime.UtcNow;

                await nextHandler(scope, ctxt, cancelToken);
            }
            finally
            {
                resultData.EndTimeUtc = DateTime.UtcNow;

                await OnResultsReady(scope, ctxt, resultData, cancelToken);
            }
        }

        /// <summary>
        /// Invoked at the end of test execution, when the complete set of results are available.
        /// </summary>
        /// <param name="scope">The service scope for the test run.</param>
        /// <param name="ctxt">The run context.</param>
        /// <param name="results">The complete set of results.</param>
        /// <param name="cancelToken">A cancellation token.</param>
        /// <returns>An asynchronous completion task.</returns>
        protected abstract ValueTask OnResultsReady(IServiceProvider scope, RunContext ctxt, WorkingResultSet results, CancellationToken cancelToken);

        /// <inheritdoc/>
        public override async ValueTask OnFeatureAsync(IServiceProvider scope, FeatureContext ctxt, Func<IServiceProvider, FeatureContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (ctxt is null)
            {
                throw new ArgumentNullException(nameof(ctxt));
            }

            if (nextHandler is null)
            {
                throw new ArgumentNullException(nameof(nextHandler));
            }

            var featureData = resultData.AddFeature(ctxt.Feature, DateTime.UtcNow);

            try
            {
                await nextHandler(scope, ctxt, cancelToken);
            }
            catch (Exception ex)
            {
                featureData.FeatureFailureException = ex;
                throw;
            }
            finally
            {
                featureData.EndTimeUtc = DateTime.UtcNow;
            }
        }

        /// <inheritdoc/>
        public override async ValueTask OnScenarioAsync(IServiceProvider scope, ScenarioContext ctxt, Func<IServiceProvider, ScenarioContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (ctxt is null)
            {
                throw new ArgumentNullException(nameof(ctxt));
            }

            if (nextHandler is null)
            {
                throw new ArgumentNullException(nameof(nextHandler));
            }

            var featureContext = scope.GetRequiredService<FeatureContext>();

            ScenarioInvocationResultData invokeData;

            if (ctxt.Variables is TableVariableSet tableVariables)
            {
                var invokeName = DetermineInvocationName((IScenarioOutlineInfo)ctxt.Scenario, tableVariables);
                invokeData = resultData.AddScenarioInvocation(featureContext.Feature, ctxt.Scenario, DateTime.UtcNow, invokeName, tableVariables);
            }
            else
            {
                invokeData = resultData.AddScenarioInvocation(featureContext.Feature, ctxt.Scenario, DateTime.UtcNow);
            }

            try
            {
                await nextHandler(scope, ctxt, cancelToken);
            }
            finally
            {
                invokeData.UpdateOutcome(DateTime.UtcNow, ctxt.Elapsed, ctxt.FailException, ctxt.FailingStep);
            }
        }

        /// <summary>
        /// Determines the name of an individual invocation from the scenario info and the table-provided variables
        /// passed into the scenario.
        /// </summary>
        /// <param name="info">The scenario outline information.</param>
        /// <param name="variables">The set of variables being used.</param>
        /// <returns>An optional name for the individual invocation.</returns>
        protected virtual string? DetermineInvocationName(IScenarioOutlineInfo info, TableVariableSet variables)
        {
            if (variables is null)
            {
                throw new ArgumentNullException(nameof(variables));
            }

            // Get the first column of the table.
            var firstColumn = variables.ColumnNames.FirstOrDefault();

            if (firstColumn is object)
            {
                return variables.Get(firstColumn);
            }

            return null;
        }
    }
}
