using System.Diagnostics;
using AutoStep.Elements.Interaction;

namespace AutoStep.Language.Interaction.Traits
{
    /// <summary>
    /// Represents a single node in the ordered set of trait nodes.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    internal struct TraitNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraitNode"/> struct.
        /// </summary>
        /// <param name="reference">The trait reference.</param>
        /// <param name="trait">The trait element.</param>
        public TraitNode(TraitRef reference, TraitDefinitionElement trait)
        {
            Ref = reference;
            Trait = trait;
        }

        /// <summary>
        /// Gets the reference for the node (i.e. the unique combination of trait names).
        /// </summary>
        public TraitRef Ref { get; }

        /// <summary>
        /// Gets the trait element.
        /// </summary>
        public TraitDefinitionElement Trait { get; }

        /// <summary>
        /// Gets the number of reference traits.
        /// </summary>
        public int NumberOfReferencedTraits => Ref.NumberOfReferencedTraits;

        /// <summary>
        /// Gets debugger output.
        /// </summary>
        /// <returns>Text content useful when debugging.</returns>
        public string DebuggerToString()
        {
            return Ref.DebuggerToString();
        }

        /// <summary>
        /// Test if the traits for this node are entirely contained in
        /// the specified trait set.
        /// </summary>
        /// <param name="traits">The trait matching set.</param>
        /// <returns>True if the trait is entirely contained in the provided set; false otherwise.</returns>
        public bool EntirelyContainedIn(TraitNameMatchingSet traits)
        {
            if (Ref.TopLevelName is object)
            {
                return traits.Contains(Ref.TopLevelName);
            }

            return traits.Contains(Ref.ReferencedTraits);
        }
    }
}
