using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{
    public abstract class ContentPart : PositionalElement
    {
        protected ContentPart(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        public int StartIndex { get; }

        public int Length { get; }
    }
}
