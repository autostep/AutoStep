using System;
using System.Collections.Generic;
using System.Text;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction
{
    internal class TraitSet
    {
        private Dictionary<string, TraitDefinitionElement> singularTraitElements;

        private Dictionary<string, TraitDefinitionElement> combinationTraitElements;

        //public IEnumerable<TraitDefinitionElement> GetMatchingTraits()
    }

    internal class InteractionRegistry
    {
        public Dictionary<string, TraitDefinitionElement> DefinedTraits { get; } = new Dictionary<string, TraitDefinitionElement>();

       // public Dictionary<string, C>
    }
}
