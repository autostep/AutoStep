using System;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Base implementation of an event handler, with the default implementations
    /// only invoking the next handler in the chain.
    /// </summary>
    public class BaseEventHandler : IEventHandler
    {
        /// <inheritdoc/>
        public virtual ValueTask OnExecuteAsync(IServiceProvider scope, RunContext ctxt, Func<IServiceProvider, RunContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
    {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnFeatureAsync(IServiceProvider scope, FeatureContext ctxt, Func<IServiceProvider, FeatureContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
    {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnScenarioAsync(IServiceProvider scope, ScenarioContext ctxt, Func<IServiceProvider, ScenarioContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
    {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnStepAsync(IServiceProvider scope, StepContext ctxt, Func<IServiceProvider, StepContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
    {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnThreadAsync(IServiceProvider scope, ThreadContext ctxt, Func<IServiceProvider, ThreadContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
    {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }
    }
}
