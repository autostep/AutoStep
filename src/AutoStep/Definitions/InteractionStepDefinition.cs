using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoStep.Elements.Interaction;
using AutoStep.Execution;
using AutoStep.Execution.Contexts;
using AutoStep.Execution.Dependency;

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

        public override ValueTask ExecuteStepAsync(IServiceScope stepScope, StepContext context, VariableSet variables)
        {
            return default;
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
