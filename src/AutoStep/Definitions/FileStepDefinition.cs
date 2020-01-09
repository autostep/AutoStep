using System;
using AutoStep.Elements;

namespace AutoStep.Definitions
{
    internal class FileStepDefinition : StepDefinition
    {
        public FileStepDefinition(IStepDefinitionSource source, StepDefinitionElement element)
            : base(
                  source,
                  element?.Type ?? throw new ArgumentNullException(nameof(element)),
                  element.Declaration!) // The declaration is validated by FileStepDefinitionSource before instantiating.
        {
            Definition = element;
        }

        public override bool IsSameDefinition(StepDefinition def)
        {
            return Type == def.Type && def.Declaration == def.Declaration;
        }
    }
}
