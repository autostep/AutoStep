using System.Diagnostics;

namespace AutoStep.Elements.Parts
{
    public class NumericalPart<TNumberType> : ContentPart
        where TNumberType : struct
    {
        public TNumberType Value { get; set; }

        public string? Symbol { get; internal set; }
    }

}
