using System;
using System.Collections.Generic;
using System.Reflection;
using AutoStep.Core.Elements;

namespace AutoStep.Core.Sources
{
    public interface IStepDefinitionSource
    {
        string SourceName { get; }

        DateTime GetLastModifyTime();

        IEnumerable<StepDefinition> GetStepDefinitions();
    }

    public class StepDefinition
    {
        private readonly List<StepMatchingPart> matchingParts = new List<StepMatchingPart>();

        public StepType Type { get; }

        public string Declaration { get; }

        public IReadOnlyList<StepMatchingPart> MatchingParts => matchingParts;

        protected StepDefinition(StepType type, string declaration)
        {
            Type = type;
            Declaration = declaration;
        }

        public void UpdateMatchingParts(IEnumerable<StepMatchingPart> newMatchingParts)
        {
            matchingParts.Clear();
            matchingParts.AddRange(newMatchingParts);
        }

        public void InvokeStep()
        {

        }
    }

#nullable enable

    public class BuiltStepDefinition : StepDefinition
    {
        public BuiltStepDefinition(MethodInfo method, StepDefinitionAttribute declaringAttribute)
            : base(declaringAttribute?.Type ?? throw new ArgumentNullException(nameof(declaringAttribute)), declaringAttribute.Declaration)
        {
        }
    }
}
