using System;
using System.Reflection;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Test
{
    /// <summary>
    /// Represents a step definition backed by a delegate.
    /// </summary>
    public class DelegateBackedStepDefinition : MethodBackedStepDefinition
    {
        private readonly object target;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateBackedStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The step definition source.</param>
        /// <param name="target">The target object for the delegate.</param>
        /// <param name="method">The method info for the delegate.</param>
        /// <param name="type">The step type.</param>
        /// <param name="declaration">The text of the step declaration.</param>
        public DelegateBackedStepDefinition(IStepDefinitionSource source, object target, MethodInfo method, StepType type, string declaration)
            : base(source, method, type, declaration)
        {
            this.target = target;
        }

        /// <inheritdoc/>
        public override string? GetDocumentation()
        {
            // No documentation for delegate-defined steps right now.
            return null;
        }

        /// <inheritdoc/>
        public override bool IsSameDefinition(StepDefinition def)
        {
            if (def is DelegateBackedStepDefinition delDef)
            {
                return delDef.Method.MethodHandle == Method.MethodHandle && delDef.target == target;
            }

            return false;
        }

        /// <inheritdoc/>
        protected override object GetMethodTarget(IServiceProvider scope)
        {
            return target;
        }
    }
}
