using System.Diagnostics;

namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents a numerical token within a step reference.
    /// </summary>
    /// <typeparam name="TNumberType">The underlying numeric type.</typeparam>
    internal abstract class NumericalToken<TNumberType> : StepToken
        where TNumberType : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericalToken{TNumberType}"/> class.
        /// </summary>
        /// <param name="startIndex">The starting 0-based index within the text.</param>
        /// <param name="length">The character length of the token.</param>
        protected NumericalToken(int startIndex, int length)
            : base(startIndex, length)
        {
        }
    }
}
