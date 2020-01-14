using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    public class FloatPart : NumericalPart<decimal>
    {
        public FloatPart(int startIndex, int length) : base(startIndex, length)
        {
        }
    }

}
