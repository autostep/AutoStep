using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Defines the result of a step definition compilation, returned from
    /// <see cref="AutoStepLinker.GetStepDefinitionElementFromStatementBody(StepType, string)"/>.
    /// </summary>
    public class StepDefinitionFromBodyResult : CompilerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionFromBodyResult"/> class.
        /// </summary>
        /// <param name="success">Did it succeed.</param>
        /// <param name="messages">Any messages.</param>
        /// <param name="element">The resulting element.</param>
        public StepDefinitionFromBodyResult(bool success, IEnumerable<CompilerMessage> messages, StepDefinitionElement? element)
            : base(success, messages)
        {
            StepDefinition = element;
        }

        /// <summary>
        /// Gets the generated <see cref="StepDefinitionElement"/>.
        /// </summary>
        public StepDefinitionElement? StepDefinition { get; }
    }
}
