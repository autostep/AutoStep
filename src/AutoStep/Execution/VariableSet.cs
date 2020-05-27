using System;
using System.Collections.Generic;
using AutoStep.Elements.Metadata;

namespace AutoStep.Execution
{
    /// <summary>
    /// This class holds a set of variables used for resolving variable references in steps.
    /// </summary>
    public class VariableSet : VariableSetBase<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableSet"/> class.
        /// </summary>
        /// <param name="isReadOnly">Set to true to prevent modification of this set after it has been created.</param>
        public VariableSet(bool isReadOnly = false)
            : base(isReadOnly)
        {
        }

        /// <summary>
        /// Gets a fixed empty variable set.
        /// </summary>
        public static VariableSet Blank { get; } = new VariableSet(true);

        /// <inheritdoc/>
        protected override string? GetDefault()
        {
            return string.Empty;
        }
    }
}
