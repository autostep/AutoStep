using System;

namespace AutoStep
{
    /// <summary>
    /// Indicates that a class method provides an interaction method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InteractionMethodAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionMethodAttribute"/> class.
        /// </summary>
        /// <param name="name">The method name when invoked by the interaction system.</param>
        public InteractionMethodAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string Name { get; }
    }
}
