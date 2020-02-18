using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Parser
{
    public abstract class InteractionMethod
    {
        protected InteractionMethod(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual void UpdateVariablesAfterMethod(InteractionMethodChainVariables variables)
        {
        }

        // Todo: Figure out the parameters for this.
        public abstract void Invoke();

        public abstract int ArgumentCount { get; }
    }
}
