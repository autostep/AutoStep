using AutoStep.Core.Elements;

namespace AutoStep.Core.Sources
{
    public abstract class StepDefinition
    {
        public IStepDefinitionSource Source { get; }

        public StepType Type { get; }

        public string Declaration { get; }

        protected StepDefinition(IStepDefinitionSource source, StepType type, string declaration)
        {
            Source = source ?? throw new System.ArgumentNullException(nameof(source));
            Type = type;
            Declaration = declaration;
        }

        public StepDefinitionElement Definition { get; set; }

        public abstract bool IsSameDefinition(StepDefinition def);

        public void InvokeStep()
        {

        }
    }
}
