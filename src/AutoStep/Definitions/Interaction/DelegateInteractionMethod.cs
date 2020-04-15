using System;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Defines an interaction method backed by a delegate.
    /// </summary>
    public class DelegateInteractionMethod : DefinedInteractionMethod
    {
        private readonly object? target;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateInteractionMethod"/> class.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="delegate">The delegate providing the callback.</param>
        public DelegateInteractionMethod(string name, Delegate @delegate)
            : base(name, @delegate.ThrowIfNull(nameof(@delegate)).Method)
        {
            target = @delegate.ThrowIfNull(nameof(@delegate)).Target;
        }

        /// <inheritdoc/>
        public override string? GetDocumentation()
        {
            // TODO: For now, delegate interaction methods have no way to provide documentation,
            //       because they don't have attributes.
            return null;
        }

        /// <inheritdoc/>
        protected override object? GetMethodTarget(IServiceProvider scope)
        {
            return target;
        }
    }
}
