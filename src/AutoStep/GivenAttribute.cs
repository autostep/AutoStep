using System;

namespace AutoStep
{
    /// <summary>
    /// Apply this to a method to indicate it can provide a step definition.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class GivenAttribute : StepDefinitionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenAttribute"/> class.
        /// </summary>
        /// <param name="declaration">The content of the step declaration (that would follow Given, e.g. "I have done something").</param>
        public GivenAttribute(string declaration)
            : base(StepType.Given, declaration)
        {
        }
    }
}
