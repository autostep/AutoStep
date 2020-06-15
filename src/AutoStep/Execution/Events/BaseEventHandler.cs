using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
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
        public virtual ValueTask OnExecuteAsync(ILifetimeScope scope, RunContext ctxt, Func<ILifetimeScope, RunContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnFeatureAsync(ILifetimeScope scope, FeatureContext ctxt, Func<ILifetimeScope, FeatureContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnScenarioAsync(ILifetimeScope scope, ScenarioContext ctxt, Func<ILifetimeScope, ScenarioContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnStepAsync(ILifetimeScope scope, StepContext ctxt, Func<ILifetimeScope, StepContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnThreadAsync(ILifetimeScope scope, ThreadContext ctxt, Func<ILifetimeScope, ThreadContext, CancellationToken, ValueTask> nextHandler, CancellationToken cancelToken)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt, cancelToken);
        }
    }
}
