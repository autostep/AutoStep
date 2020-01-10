using System;
using AutoStep.Definitions;

namespace AutoStep.Tests.Utils
{
    public class UpdatableTestStepDefinitionSource : TestStepDefinitionSource, IUpdatableStepDefinitionSource
    {
        public UpdatableTestStepDefinitionSource(params StepDefinition[] defs) : base(defs)
        {
        }

        public UpdatableTestStepDefinitionSource(string uid, params StepDefinition[] defs) : base(uid, defs)
        {
        }

        public DateTime GetLastModifyTime()
        {
            return DateTime.MinValue;
        }
    }
}
