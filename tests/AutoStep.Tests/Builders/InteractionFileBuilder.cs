using System;
using System.Text;
using AutoStep.Elements.Interaction;

namespace AutoStep.Tests.Builders
{
    public class InteractionFileBuilder : BaseBuilder<InteractionFileElement>
    {
        public InteractionFileBuilder()
        {
            Built = new InteractionFileElement();
        }

        public InteractionFileBuilder Trait(string fullName, int line, int column, Action<InteractionTraitBuilder> traitCfg = null)
        {
            var traitBuilder = new InteractionTraitBuilder(fullName, line, column);

            if(traitCfg is object)
            {
                traitCfg(traitBuilder);
            }

            Built.TraitGraph.AddOrExtendTrait(traitBuilder.Built);

            return this;
        }

        public InteractionFileBuilder Component(string name, int line, int column, Action<InteractionComponentBuilder> componentCfg = null)
        {
            var componentBuilder = new InteractionComponentBuilder(name, line, column);

            if(componentCfg is object)
            {
                componentCfg(componentBuilder);
            }

            Built.Components.Add(componentBuilder.Built);

            return this;
        }
    }
}
