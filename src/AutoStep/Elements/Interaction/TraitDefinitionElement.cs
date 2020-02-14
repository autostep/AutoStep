using System;
using AutoStep.Language.Interaction.Traits;

namespace AutoStep.Elements.Interaction
{
    public class TraitDefinitionElement : InteractionDefinitionElement
    {
        public void SetNameParts(params NameRefElement[] nameParts)
        {
            if (nameParts.Length == 0)
            {
                throw new ArgumentException();
            }

            // Sort the components so each identical trait (i.e. same combination of parent traits) has exactly the same
            // value.
            Array.Sort(nameParts);
            NameParts = nameParts;
        }

        internal TraitRef GetRef()
        {
            return new TraitRef(NameParts);
        }

        internal void ExtendWith(TraitDefinitionElement newTrait)
        {
            foreach (var item in Methods)
            {
                // Override the method (or add it if it doesn't exist).
                Methods.Add(item);
            }

            Steps.AddRange(newTrait.Steps);
        }

        public int NumberOfReferencedTraits => NameParts.Length > 1 ? NameParts.Length : 0;

        public NameRefElement[] NameParts { get; private set; } = Array.Empty<NameRefElement>();
    }
}
