using System;
using System.Collections.Generic;
using System.Reflection;
using AutoStep.Core.Elements;

namespace AutoStep.Core.Sources
{
    public interface IStepDefinitionSource
    {
        string Uid { get; }

        string Name { get; }

        DateTime GetLastModifyTime();

        IEnumerable<StepDefinition> GetStepDefinitions();
    }

    public abstract class StepDefinition
    {
        public StepType Type { get; }

        public string Declaration { get; }

        protected StepDefinition(StepType type, string declaration)
        {
            Type = type;
            Declaration = declaration;
        }

        public StepDefinitionElement Definition { get; set; }

        public void InvokeStep()
        {

        }
    }

    public class BuiltStepDefinition : StepDefinition
    {
        private readonly MethodInfo method;

        public BuiltStepDefinition(MethodInfo method, StepDefinitionAttribute declaringAttribute)
            : base(declaringAttribute?.Type ?? throw new ArgumentNullException(nameof(declaringAttribute)), declaringAttribute.Declaration)
        {
            this.method = method;
        }
    }
}
