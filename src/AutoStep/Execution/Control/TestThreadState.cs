using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep.Execution.Control
{
    public enum TestThreadState
    {
        Starting,
        StartingFeature,
        StartingStep,
        StartingScenario,
        StartingScenarioStep,
        StartingStepCollection
    }
}
