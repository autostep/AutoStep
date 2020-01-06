using System.Collections.Generic;
using AutoStep.Core.Elements;
using AutoStep.Core.Sources;

namespace AutoStep.Core.Matching
{
    public struct MatchResult
    {
        public MatchResult(bool isExact, int confidence, StepDefinition definition)
        {
            IsExact = isExact;
            Confidence = confidence;
            Definition = definition;
        }

        public int Confidence { get; }

        public StepDefinition Definition { get; }

        public bool IsExact { get; }
    }

    public interface IMatchingTree
    {
        void AddDefinition(StepDefinition definition);

        LinkedList<MatchResult> Match(StepReferenceElement stepReference, bool exactOnly, out int partsMatched);
    }
}
