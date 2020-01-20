using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution;
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

        public abstract Task ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables);
    }
}
