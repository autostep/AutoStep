using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Language.Position
{
    /// <summary>
    /// Base class for all position entries (that possess scopes).
    /// </summary>
    public abstract class PositionEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionEntry"/> class.
        /// </summary>
        /// <param name="scopes">The set of scopes that apply.</param>
        protected PositionEntry(IReadOnlyList<BuiltElement> scopes)
        {
            Scopes = scopes;
        }

        /// <summary>
        /// Gets the set of scopes applicable at the position.
        /// </summary>
        public IReadOnlyList<BuiltElement> Scopes { get; }
    }
}
