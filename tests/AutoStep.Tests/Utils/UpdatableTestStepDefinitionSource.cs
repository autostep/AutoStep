using System;
using AutoStep.Definitions;
using AutoStep.Elements;

namespace AutoStep.Tests.Utils
{
    public class UpdatableTestStepDefinitionSource : TestStepDefinitionSource, IUpdatableStepDefinitionSource
    {
        private DateTime lastModifyTime = DateTime.MinValue;

        public UpdatableTestStepDefinitionSource(params StepDefinition[] defs) : base(defs)
        {
        }

        public UpdatableTestStepDefinitionSource(string uid, params StepDefinition[] defs) : base(uid, defs)
        {
        }

        public DateTime GetLastModifyTime()
        {
            return lastModifyTime;
        }

        public override void AddStepDefinition(StepType type, string declaration)
        {
            base.AddStepDefinition(type, declaration);
            lastModifyTime = DateTime.UtcNow;
        }

        public override void ReplaceStepDefinitions(params StepDefinition[] newDefs)
        {
            base.ReplaceStepDefinitions(newDefs);
            lastModifyTime = DateTime.UtcNow;
        }

        public override void RemoveStepDefinition(StepDefinition def)
        {
            base.RemoveStepDefinition(def);
            lastModifyTime = DateTime.UtcNow;
        }
    }
}
