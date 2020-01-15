using System;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements.Parts
{

    internal abstract class DefinitionPart : PositionalElement
    {
        public string Text { get; set; }

        public abstract bool IsDefinitionPartMatch(DefinitionPart part);

        public abstract StepReferenceMatchResult DoStepReferenceMatch(string referenceText, ReadOnlySpan<StepToken> currentPartSpan);
    }
}
