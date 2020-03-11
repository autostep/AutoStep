using System;

namespace AutoStep
{
    /// <summary>
    /// Apply this to a method to indicate it can provide a step definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ThenAttribute : StepDefinitionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThenAttribute"/> class.
        /// </summary>
        /// <param name="declaration">The content of the step declaration (that would follow Then, e.g. "this should be true").</param>
        public ThenAttribute(string declaration)
            : base(StepType.Then, declaration)
        {
        }
    }
}
