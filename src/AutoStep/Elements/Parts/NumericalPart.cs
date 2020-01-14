using System.Diagnostics;

namespace AutoStep.Elements.Parts
{
    public abstract class NumericalPart<TNumberType> : ContentPart
        where TNumberType : struct
    {
        protected NumericalPart(int startIndex, int length) : base(startIndex, length)
        {
        }
    }

}
