using System;
using System.Collections.Generic;
using AutoStep.Elements.StepTokens;

namespace AutoStep.Elements
{
    /// <summary>
    /// Represents a table cell. A cell's value is treated as a statement argument.
    /// </summary>
    public class TableCellElement : PositionalElement
    {
        private List<StepToken> parts = new List<StepToken>();

        public string Text { get; set; }

        internal IReadOnlyList<StepToken> Parts => parts;

        internal void AddPart(StepToken nestedPart)
        {
            if (nestedPart is null)
            {
                throw new ArgumentNullException(nameof(nestedPart));
            }

            parts.Add(nestedPart);
        }
    }
}
