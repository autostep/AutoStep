using System;

namespace AutoStep
{
    /// <summary>
    /// Indicates that a class contains steps.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InteractionMethodAttribute : Attribute
    {
        public InteractionMethodAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
