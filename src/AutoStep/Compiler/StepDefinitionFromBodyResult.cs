using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Defines the result of a step definition compilation, returned from
    /// <see cref="AutoStepCompiler.CompileStepDefinitionElementFromStatementBody(StepType, string)"/>.
    /// </summary>
    public class StepDefinitionFromBodyResult : CompilerResult<StepDefinitionElement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepDefinitionFromBodyResult"/> class.
        /// </summary>
        /// <param name="success">Did it succeed.</param>
        /// <param name="messages">Any messages.</param>
        /// <param name="element">The resulting element.</param>
        public StepDefinitionFromBodyResult(bool success, IEnumerable<CompilerMessage> messages, StepDefinitionElement? element)
            : base(success, messages, element)
        {
        }
    }
}
