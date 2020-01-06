using System;
using System.Collections.Generic;

namespace AutoStep.Core.Sources
{
    public interface IStepDefinitionSource
    {
        string Uid { get; }

        string Name { get; }

        DateTime GetLastModifyTime();

        IEnumerable<StepDefinition> GetStepDefinitions();
    }
}
