using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Core.Matching
{
    public interface IMatchingTree
    {
        void AddDefinition(StepDefinition definition);
        System.Collections.Generic.LinkedList<(int confidence, StepDefinition def)> Match(StepReferenceElement stepReference, out int partsMatched);
    }
}