using System;
using System.Threading.Tasks;

namespace AutoStep.Execution
{
    /// <summary>
    /// This is a granular version for hooking into events
    /// during the lifecycle of test execution, rather than just results.
    /// </summary>
    public interface IEventHandler
    {
        ValueTask BeginExecute(RunContext runContext);
        ValueTask BeginThread(ThreadContext threadContext);
        ValueTask EndExecute(RunContext runContext);
        ValueTask EndThread(ThreadContext threadContext);
        ValueTask BeginFeature(FeatureContext ctxt);
        ValueTask EndFeature(FeatureContext ctxt);
        ValueTask BeginScenario(ScenarioContext ctxt);
        ValueTask EndScenario(ScenarioContext ctxt);
        ValueTask BeginStep(StepContext ctxt);
        ValueTask EndStep(StepContext ctxt);
        ValueTask ScenarioPassed(ScenarioContext ctxt);
        ValueTask ScenarioFailed(ScenarioContext ctxt);
    }
}
