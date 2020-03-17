using System;
using System.Linq;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    internal static class InteractionEntityBuilderExtensions
    {
        public static TEntityBuilder StepDefinition<TEntityBuilder>(this TEntityBuilder entityBuilder, StepType type, string declaration, int line, int column, Action<InteractionStepDefinitionBuilder>? cfg = null)
            where TEntityBuilder : IInteractionEntityBuilder<InteractionDefinitionElement>
        {
            var stepDefinitionBuilder = new InteractionStepDefinitionBuilder(type, declaration, line, column);

            if (cfg is object)
            {
                cfg(stepDefinitionBuilder);
            }

            if (entityBuilder is InteractionComponentBuilder)
            {
                // Mark the fixed name.
                stepDefinitionBuilder.Built.FixedComponentName = entityBuilder.Built.Name;
            }

            entityBuilder.Built.Steps.Add(stepDefinitionBuilder.Built);

            return entityBuilder;
        }

        public static TEntityBuilder Method<TEntityBuilder>(this TEntityBuilder entityBuilder, string name, int line, int column, Action<MethodDefinitionBuilder> callBuilder)
            where TEntityBuilder : IInteractionEntityBuilder<InteractionDefinitionElement>
        {
            var methodDef = new MethodDefinitionElement(name);
            methodDef.SourceLine = line;
            methodDef.StartColumn = column;

            var methodDefBuilder = new MethodDefinitionBuilder(methodDef);

            // Do the call builder.
            callBuilder(methodDefBuilder);

            // Update the end position.
            // Last argument end position.
            var lastArgument = methodDef.Arguments.LastOrDefault();

            if(lastArgument is object)
            {
                methodDef.EndLine = lastArgument.EndLine;
                methodDef.EndColumn = lastArgument.EndColumn + 1;
            }
            else
            {
                methodDef.EndLine = methodDef.SourceLine;
                methodDef.EndColumn = column + name.Length + 1;
            }

            entityBuilder.Built.Methods.Add(methodDef);

            return entityBuilder;
        }

        public static TEntityBuilder Method<TEntityBuilder>(this TEntityBuilder entityBuilder, string name, int startLine, int startColumn, int endLine, int endColumn, Action<MethodDefinitionBuilder> callBuilder)
            where TEntityBuilder : IInteractionEntityBuilder<InteractionDefinitionElement>
        {
            var methodDef = new MethodDefinitionElement(name);
            methodDef.SourceLine = startLine;
            methodDef.StartColumn = startColumn;
            methodDef.EndLine = endLine;
            methodDef.EndColumn = endColumn;

            var methodDefBuilder = new MethodDefinitionBuilder(methodDef);

            // Do the call builder.
            callBuilder(methodDefBuilder);

            entityBuilder.Built.Methods.Add(methodDef);

            return entityBuilder;
        }
    }
}
