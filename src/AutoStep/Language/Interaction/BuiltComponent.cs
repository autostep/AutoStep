namespace AutoStep.Language.Interaction
{
    /// <summary>
    /// Represents the final state of a component as exposed in a <see cref="AutoStepInteractionSet"/>.
    /// Step definitions are registered elsewhere, so the only responsibility of this type is to hold the method table
    /// specific to the component.
    /// </summary>
    internal class BuiltComponent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltComponent"/> class.
        /// </summary>
        /// <param name="name">The name of the component (as it will be matched in steps).</param>
        /// <param name="methodTable">The method table.</param>
        public BuiltComponent(string name, MethodTable methodTable)
        {
            Name = name;
            MethodTable = methodTable;
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the component's method table.
        /// </summary>
        public MethodTable MethodTable { get; }
    }
}
