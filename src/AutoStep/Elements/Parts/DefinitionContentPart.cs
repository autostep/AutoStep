using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    public abstract class DefinitionContentPart : ContentPart
    {
        public abstract bool IsDefinitionPartMatch(ContentPart part);

        public abstract StepReferenceMatchResult DoStepReferenceMatch(ContentPart referencePart);
    }
}
