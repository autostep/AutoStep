using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;
using AutoStep.Execution.Interaction;
using AutoStep.Language;
using AutoStep.Language.Interaction;

namespace AutoStep.Definitions
{
    public class InteractionStepDefinition : StepDefinition
    {
        public InteractionStepDefinition(IStepDefinitionSource source, InteractionStepDefinitionElement stepDef)
            : base(
                source,
                stepDef.ThrowIfNull(nameof(stepDef)).Type,
                stepDef.ThrowIfNull(nameof(stepDef)).Declaration!)
        {
            Definition = stepDef;
        }

        public override async ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
        {
            var stepDef = (InteractionStepDefinitionElement) Definition!;

            string componentName;

            if (stepDef.FixedComponentName is string)
            {
                componentName = stepDef.FixedComponentName;
            }
            else
            {
                var placeholders = context.Step.Binding?.Placeholders;

                if (placeholders is object && placeholders.TryGetValue(InteractionPlaceholders.Component, out var placeHolderName))
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
            var interactionSet = stepScope.Resolve<AutoStepInteractionSet>();

            if (interactionSet.Components.TryGetValue(componentName, out var component))
            {
                var initialContext = new MethodContext();

                // Populate the initial set of variables from the step definition.
                for (var argIdx = 0; argIdx < Definition.Arguments.Count; argIdx++)
                {
                    var argValue = context.Step.Binding.Arguments[argIdx];

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

        public override object GetSignature()
        {
            return base.GetSignature();
        }

        public override bool IsSameDefinition(StepDefinition def)
        {
            if (def is InteractionStepDefinition)
            {
                var myElement = Definition as InteractionStepDefinitionElement;
                var otherElement = def.Definition as InteractionStepDefinitionElement;

                if (myElement is object && otherElement is object)
                {
                    return Source.Uid == def.Source.Uid && Type == def.Type && Declaration == def.Declaration && myElement.MatchesSameComponentsAs(otherElement);
                }

                return false;
            }

            return false;
        }
    }
}
