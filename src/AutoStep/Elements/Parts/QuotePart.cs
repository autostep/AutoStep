using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    public class QuotePart : ContentPart
    {
        public QuotePart(int startIndex) : base(startIndex, 1)
        {
        }

        public bool IsDoubleQuote { get; set; }
    }
}
