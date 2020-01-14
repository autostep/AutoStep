using System.Diagnostics;
using AutoStep.Compiler;

namespace AutoStep.Elements.Parts
{

    public class VariablePart : ContentPart
    {
        public VariablePart(int startIndex, int length) : base(startIndex, length)
        {
        }

        public string VariableName { get; set; }
    }

}
