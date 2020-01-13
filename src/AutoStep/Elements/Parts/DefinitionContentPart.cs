using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    public abstract class DefinitionContentPart : PositionalElement
    {
        public string Text { get; set; }

        public abstract bool IsDefinitionPartMatch(DefinitionContentPart part);

        public abstract StepReferenceMatchResult DoStepReferenceMatch(ContentPart referencePart);
    }
}
