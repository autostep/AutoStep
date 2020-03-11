using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Language.Test
{
    /// <summary>
    /// Defines the result of a step definition compilation, returned from
    /// <see cref="AutoStepCompiler.CompileStepDefinitionElementFromStatementBody(StepType, string)"/>.
    /// </summary>
    public class StepDefinitionFromBodyResult : LanguageOperationResult<StepDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionFromBodyResult"/> class.
        /// </summary>
        /// <param name="success">Did it succeed.</param>
        /// <param name="messages">Any messages.</param>
        /// <param name="element">The resulting element.</param>
        public StepDefinitionFromBodyResult(bool success, IEnumerable<LanguageOperationMessage> messages, StepDefinitionElement? element)
            : base(success, messages, element)
        {
        }
    }
}
