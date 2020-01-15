namespace AutoStep.Elements.StepTokens
{
    /// <summary>
    /// Represents an interpolation start marker.
    /// </summary>
    internal class InterpolateStartToken : StepToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterpolateStartToken"/> class.
        /// </summary>
        /// <param name="startIndex">The position of the interpolation start token.</param>
        public InterpolateStartToken(int startIndex)
            : base(startIndex, 1)
        {
        }
    }
}
