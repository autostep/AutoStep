using System;
using System.Threading.Tasks;

namespace AutoStep.Execution
{
    /// <summary>
    /// This is a granular version for hooking into events.
    /// Event handlers are built like pipelines, so errors in later event handlers
    /// can be caught by earlier ones.
    /// </summary>
    public interface IEventHandler
    {
        Task Execute(RunContext runContext, Func<RunContext, Task> next);

        Task Thread(ThreadContext threadContext, Func<ThreadContext, Task> next);

        Task Feature(FeatureContext featureContext, Func<FeatureContext, Task> next);

        Task Scenario(ScenarioContext scenarioContext, Func<ScenarioContext, Task> next);

        Task Step(StepContext stepContext, Func<StepContext, Task> next);
    }
}
