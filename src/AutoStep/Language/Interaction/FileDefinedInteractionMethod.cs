using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Parser
{

    public class FileDefinedInteractionMethod : InteractionMethod
    {
        public FileDefinedInteractionMethod(string name) 
            : base(name)
        {
        }

        public bool NeedsDefining { get; set; }

        public MethodDefinitionElement MethodDefinition { get; set; }

        public override int ArgumentCount => MethodDefinition.Arguments.Count;

        public override void Invoke()
        {
            throw new NotImplementedException();
        }
    }
}
