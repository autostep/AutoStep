﻿using System.Collections.Generic;

namespace AutoStep.Language.Interaction.Traits
{
    public class TraitOrderComparer : IComparer<TraitNode>
    {
        public int Compare(TraitNode x, TraitNode y)
        {
            return x.Ref.NumberOfReferencedTraits.CompareTo(y.Ref.NumberOfReferencedTraits);
        }
    }
}
