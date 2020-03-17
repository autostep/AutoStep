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
        public virtual void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration)
        {
        }

        /// <inheritdoc/>
        public virtual ValueTask OnExecute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnFeature(IServiceScope scope, FeatureContext ctxt, Func<IServiceScope, FeatureContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnScenario(IServiceScope scope, ScenarioContext ctxt, Func<IServiceScope, ScenarioContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnStep(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }

        /// <inheritdoc/>
        public virtual ValueTask OnThread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> nextHandler)
        {
            nextHandler = nextHandler.ThrowIfNull(nameof(nextHandler));

            return nextHandler(scope, ctxt);
        }
    }
}
