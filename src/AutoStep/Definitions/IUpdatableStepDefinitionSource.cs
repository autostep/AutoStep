using System;

namespace AutoStep.Definitions
{
    public interface IUpdatableStepDefinitionSource : IStepDefinitionSource
    {
        DateTime GetLastModifyTime();
    }
}
