using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.StepTokens
{
    internal abstract class StepToken : PositionalElement
    {
        protected StepToken(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        public int StartIndex { get; }

        public int Length { get; }
    }
}
