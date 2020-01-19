using System;
using System.Threading.Tasks;
using AutoStep.Elements;
using AutoStep.Execution;

namespace AutoStep.Definitions
{
    /// <summary>
    /// Represents a step definined inside an autostep file.
    /// </summary>
    internal class FileStepDefinition : StepDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The source from which this file was created.</param>
        /// <param name="element">The element that defines the step.</param>
        public FileStepDefinition(IStepDefinitionSource source, StepDefinitionElement element)
            : base(
                  source,
                  element?.Type ?? throw new ArgumentNullException(nameof(element)),
                  element.Declaration!) // The declaration is validated by FileStepDefinitionSource before instantiating.
        {
            Definition = element;
        }

        /// <summary>
        /// Compares two step definitions within the same source and decides if they are the same actual definition
        /// (i.e. one can be replaced with the other).
        /// </summary>
        /// <param name="def">The other definition.</param>
        /// <returns>True if the same, false otherwise.</returns>
        public override bool IsSameDefinition(StepDefinition def)
        {
            return Type == def.Type && def.Declaration == def.Declaration;
        }

        public override Task ExecuteStepAsync(StepExecutionArgs executionArguments)
        {
            // Extract the arguments, and invoke the collection executor.
            var variables = new VariableSet();

            // TODO: Populate the variables from the binding arguments.

            // Execute the referenced steps.
            return executionArguments.CollectionExecutionStrategy.Execute(
                executionArguments.Scope,
                executionArguments.Context,
                Definition,
                variables,
                executionArguments.Events,
                executionArguments.ExecutionManager);
        }
    }
}
