using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{
    public abstract class ContentPart : PositionalElement
    {
        public string? Text { get; set; }
    }
}
