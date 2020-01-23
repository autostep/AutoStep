using System;
using AutoStep.Definitions;
using AutoStep.Elements;
using AutoStep.Elements.ReadOnly;

namespace AutoStep.Execution.Contexts
{
    public class FileDefinedStepContext : StepCollectionContext
    {
        public FileDefinedStepContext(IStepDefinitionInfo stepDef)
        {
            Definition = stepDef;
        }

        public IStepDefinitionInfo Definition { get; }
    }
}
