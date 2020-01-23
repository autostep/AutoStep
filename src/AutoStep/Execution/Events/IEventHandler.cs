using System;
using System.Threading.Tasks;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Execution.Events
{
    /// <summary>
    /// This is a granular version for hooking into events.
    /// Event handlers are built like pipelines, so errors in later event handlers
    /// can be caught by earlier ones.
    /// </summary>
    public interface IEventHandler
    {
        void ConfigureServices(IServicesBuilder builder, RunConfiguration configuration);

        ValueTask Execute(IServiceScope scope, RunContext ctxt, Func<IServiceScope, RunContext, ValueTask> next);

        ValueTask Thread(IServiceScope scope, ThreadContext ctxt, Func<IServiceScope, ThreadContext, ValueTask> next);

        ValueTask Feature(IServiceScope scope, FeatureContext ctxt, Func<IServiceScope, FeatureContext, ValueTask> next);

        ValueTask Scenario(IServiceScope scope, ScenarioContext ctxt, Func<IServiceScope, ScenarioContext, ValueTask> next);

        ValueTask Step(IServiceScope scope, StepContext ctxt, Func<IServiceScope, StepContext, ValueTask> next);
    }
}
