using System;
using System.Diagnostics;

namespace AutoStep.Language.Interaction.Traits
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public class Trait
    {
        public Trait(params string[] nameComponents)
        {
            if (nameComponents.Length == 0)
            {
                throw new ArgumentException();
            }

            // Sort the components so each identical trait (i.e. same combination of parent traits) has exactly the same
            // value.
            Array.Sort(nameComponents);
            NameParts = nameComponents;
        }

        public TraitRef GetRef()
        {
            return new TraitRef(NameParts);
        }

        internal void ExtendWith(Trait newTrait)
        {
            //throw new NotImplementedException();
        }

        public string DebuggerToString()
        {
            return string.Join(" + ", NameParts);
        }

        public int NumberOfReferencedTraits => NameParts.Length;

        public string[] NameParts { get; set; }
    }
}
