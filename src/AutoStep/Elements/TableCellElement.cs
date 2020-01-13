using System;
using System.Collections.Generic;
using AutoStep.Elements.Parts;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a table cell. A cell's value is treated as a statement argument.
    /// </summary>
    public class TableCellElement : PositionalElement
    {
        private List<ContentPart> parts = new List<ContentPart>();

        public string Text { get; set; }

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
