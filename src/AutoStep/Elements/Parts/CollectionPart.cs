using System;
using System.Collections.Generic;
using AutoStep.Compiler.Matching;
using AutoStep.Definitions;

namespace AutoStep.Elements.Parts
{

    public class CollectionPart : ContentPart
    {
        private List<ContentPart> parts = new List<ContentPart>();

        public IReadOnlyList<ContentPart> Parts => parts;

        public void AddPart(ContentPart nestedPart)
        {
            if (nestedPart is null)
            {
                throw new ArgumentNullException(nameof(nestedPart));
            }

            parts.Add(nestedPart);
        }
    }

}
