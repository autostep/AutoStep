using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions.Interaction
{
    /// <summary>
    /// Defines an interaction step definition (which executes a method call chain when invoked).
    /// </summary>
    public class InteractionStepDefinition : StepDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionStepDefinition"/> class.
        /// </summary>
        /// <param name="source">The step source.</param>
        /// <param name="stepDef">The step definition element from the interaction file.</param>
        public InteractionStepDefinition(IStepDefinitionSource source, InteractionStepDefinitionElement stepDef)
            : base(
                source,
                stepDef.ThrowIfNull(nameof(stepDef)).Type,
                stepDef.ThrowIfNull(nameof(stepDef)).Declaration!)
        {
            Definition = stepDef;
        }

        /// <inheritdoc/>
        public override async ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
        {
            stepScope = stepScope.ThrowIfNull(nameof(stepScope));
            context = context.ThrowIfNull(nameof(context));
            variables = variables.ThrowIfNull(nameof(context));

            var stepDef = (InteractionStepDefinitionElement)Definition!;

            string componentName;

            if (stepDef.FixedComponentName is string)
            {
                // The component name is known.
                componentName = stepDef.FixedComponentName;
            }
            else
            {
                // The component name comes from the placeholders matched during step binding.
                var placeholders = context.Step.Binding?.Placeholders;

                if (placeholders is object && placeholders.TryGetValue(StepPlaceholders.Component, out var placeHolderName))
                {
                    componentName = placeHolderName;
                }
                else
                {
                    // Not possible to get here if the parser has done its job.
                    throw new LanguageEngineAssertException();
                }
            }

            // Resolve the interaction set.
            var interactionSet = stepScope.Resolve<IInteractionSet>();

            if (interactionSet.Components.TryGetValue(componentName, out var component))
            {
                var initialContext = new MethodContext();

                // Populate the initial set of variables from the step definition.
                for (var argIdx = 0; argIdx < Definition!.Arguments.Count; argIdx++)
                {
                    var argValue = context.Step.Binding!.Arguments[argIdx];

                    var argText = argValue.GetFullText(stepScope, context.Step.Text, variables);

                    initialContext.Variables.Set(Definition.Arguments[argIdx].Name, argText);
                }

                await stepDef.InvokeChainAsync(stepScope, initialContext, component.MethodTable);
            }
            else
            {
                // Step should not have bound if there is no component for it.
                throw new LanguageEngineAssertException();
            }
        }

        /// <inheritdoc/>
        public override bool IsSameDefinition(StepDefinition def)
        {
            // A definition is the same if the IDs are the same and it matches all the same components.
            if (def is InteractionStepDefinition)
            {
                if (Definition is InteractionStepDefinitionElement myElement &&
                    def.Definition is InteractionStepDefinitionElement otherElement)
                {
                    return Source.Uid == def.Source.Uid && Type == def.Type && Declaration == def.Declaration && myElement.MatchesSameComponentsAs(otherElement);
                }

                return false;
            }

            return false;
        }
    }
}
