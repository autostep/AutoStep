using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Language.Position
{
    /// <summary>
    /// Represents a single token.
    /// </summary>
    public class PositionLineToken : PositionEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionLineToken"/> class.
        /// </summary>
        /// <param name="startColumn">The first column of the token.</param>
        /// <param name="endColumn">The last column of the token.</param>
        /// <param name="scopes">The scopes applied to the token.</param>
        /// <param name="attachedElement">An optional syntax element attached to the token.</param>
        /// <param name="category">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public PositionLineToken(int startColumn, int endColumn, IReadOnlyList<BuiltElement> scopes, BuiltElement? attachedElement, LineTokenCategory category, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
            : base(scopes)
        {
            StartColumn = startColumn;
            EndColumn = endColumn;
            AttachedElement = attachedElement;
            Category = category;
            SubCategory = subCategory;
        }

        /// <summary>
        /// Gets the first column of the token.
        /// </summary>
        public int StartColumn { get; }

        /// <summary>
        /// Gets the last column of the token.
        /// </summary>
        public int EndColumn { get; }

        /// <summary>
        /// Gets an optional syntax element attached to the token.
        /// </summary>
        public BuiltElement? AttachedElement { get; }

        /// <summary>
        /// Gets the token category.
        /// </summary>
        public LineTokenCategory Category { get; }

        /// <summary>
        /// Gets the token sub-category.
        /// </summary>
        public LineTokenSubCategory SubCategory { get; }
    }
}
