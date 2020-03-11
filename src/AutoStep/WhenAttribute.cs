using System;

namespace AutoStep
{
    /// <summary>
    /// Apply this to a method to indicate it can provide a step definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class WhenAttribute : StepDefinitionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhenAttribute"/> class.
        /// </summary>
        /// <param name="declaration">The content of the step declaration (that would follow When, e.g. "I have done something").</param>
        public WhenAttribute(string declaration)
            : base(StepType.When, declaration)
        {
        }
    }
}
