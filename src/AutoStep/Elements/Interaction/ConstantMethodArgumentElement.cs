namespace AutoStep.Elements.Interaction
{
    /// <summary>
    /// Represents a constant argument to a method, e.g. TAB.
    /// </summary>
    public class ConstantMethodArgumentElement : MethodArgumentElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantMethodArgumentElement"/> class.
        /// </summary>
        /// <param name="name">The name of the constant.</param>
        public ConstantMethodArgumentElement(string name)
        {
            ConstantName = name;
        }

        /// <summary>
        /// Gets the name of the constant.
        /// </summary>
        public string ConstantName { get; }
    }
}
