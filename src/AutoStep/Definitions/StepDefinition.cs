using System;
using System.Threading;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Elements.Test;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents an abstract 'step definition'; i.e. something that can take a <see cref="StepReferenceElement"/> and execute it.
    /// </summary>
    public abstract class StepDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinition"/> class.
        /// </summary>
        /// <param name="source">Source of the step definition.</param>
        /// <param name="type">The step type.</param>
        /// <param name="declaration">The text declaration of the step.</param>
        protected StepDefinition(IStepDefinitionSource source, StepType type, string declaration)
        {
            Source = source ?? throw new System.ArgumentNullException(nameof(source));
            Type = type;
            Declaration = declaration;
        }

        /// <summary>
        /// Gets the source of the step definition.
        /// </summary>
        public IStepDefinitionSource Source { get; }

        /// <summary>
        /// Gets the step type.
        /// </summary>
        public StepType Type { get; }

        /// <summary>
        /// Gets the declaration body.
        /// </summary>
        public string Declaration { get; }

        /// <summary>
        /// Gets or sets the step definition element, which contains calculated metadata for the definition.
        /// </summary>
        public StepDefinitionElement? Definition { get; set; }

        /// <summary>
        /// Implementations should return a value indicating whether the provided step definition is the same as this one (but possibly a different version).
        /// </summary>
        /// <param name="def">The definition to check against.</param>
        /// <returns>True if the definition is semantically the same.</returns>
        public abstract bool IsSameDefinition(StepDefinition def);

        /// <summary>
        /// Gets the 'signature' of a step definition; i.e. a unique ID for the step definition that only this step would have.
        /// </summary>
        /// <returns>The signature.</returns>
        public virtual object GetSignature()
        {
            return (Type, Declaration);
        }

        /// <summary>
        /// This method is invoked when the step definition should be executed.
        /// </summary>
        /// <param name="stepScope">The current DI scope.</param>
        /// <param name="context">The step context (including all binding information).</param>
        /// <param name="variables">The set of variables currently in-scope and available to the step.</param>
        /// <param name="cancelToken">A cancellation token for the step execution.</param>
        /// <returns>A task that will complete when the step finishes executing.</returns>
        public abstract ValueTask ExecuteStepAsync(IServiceProvider stepScope, StepContext context, VariableSet variables, CancellationToken cancelToken);
    }
}
