using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    public class IntPart : NumericalPart<long>
    {
        public IntPart(int startIndex, int length) : base(startIndex, length)
        {
        }
    }

}
