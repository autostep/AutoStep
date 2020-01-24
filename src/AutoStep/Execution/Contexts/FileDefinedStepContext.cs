using AutoStep.Elements.Metadata;

namespace AutoStep.Execution.Contexts
{
    /// <summary>
    /// Defines the context for a file-backed step definition.
    /// </summary>
    public class FileDefinedStepContext : StepCollectionContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileDefinedStepContext"/> class.
        /// </summary>
        /// <param name="stepDef">The step definition info.</param>
        public FileDefinedStepContext(IStepDefinitionInfo stepDef)
        {
            Definition = stepDef;
        }

        /// <summary>
        /// Gets the step definition.
        /// </summary>
        public IStepDefinitionInfo Definition { get; }
    }
}
