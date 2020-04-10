using System;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// Base implementation of an event handler, with the default implementations
    /// only invoking the next handler in the chain.
    /// </summary>
    public class BaseEventHandler : IEventHandler
    {
        /// <inheritdoc/>
        public virtual ValueTask OnExecute(IServiceProvider scope, RunContext ctxt, Func<IServiceProvider, RunContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnFeature(IServiceProvider scope, FeatureContext ctxt, Func<IServiceProvider, FeatureContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnScenario(IServiceProvider scope, ScenarioContext ctxt, Func<IServiceProvider, ScenarioContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnStep(IServiceProvider scope, StepContext ctxt, Func<IServiceProvider, StepContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnThread(IServiceProvider scope, ThreadContext ctxt, Func<IServiceProvider, ThreadContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }
    }
}
