using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStep
{
    /// <summary>
    /// Defines the base attribute for step definitions bound to methods.
    /// </summary>
    public abstract class StepDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionAttribute"/> class.
        /// </summary>
        /// <param name="type">The step type.</param>
        /// <param name="declaration">The step body.</param>
        protected StepDefinitionAttribute(StepType type, string declaration)
        {
            Type = type;
            Declaration = declaration;
        }

        /// <summary>
        /// Gets the declaration body for the step definition.
        /// </summary>
        public string Declaration { get; }

        /// <summary>
        /// Gets the type of step (Given, When, Then).
        /// </summary>
        public StepType Type { get; }
    }
}
