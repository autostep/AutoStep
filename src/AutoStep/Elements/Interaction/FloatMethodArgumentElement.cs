namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a literal float method argument.
    /// </summary>
    public class FloatMethodArgumentElement : MethodArgumentElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatMethodArgumentElement"/> class.
        /// </summary>
        /// <param name="value">The argument value.</param>
        public FloatMethodArgumentElement(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the argument value.
        /// </summary>
        public double Value { get; }
    }
}
