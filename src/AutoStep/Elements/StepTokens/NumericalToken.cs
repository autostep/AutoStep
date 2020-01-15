using System.Diagnostics;

namespace AutoStep.Elements.StepTokens
{
    internal abstract class NumericalToken<TNumberType> : StepToken
        where TNumberType : struct
    {
        protected NumericalToken(int startIndex, int length) : base(startIndex, length)
        {
        }
    }

}
