using System;

namespace AutoStep
{
    /// <summary>
    /// Defines a line token as output from line tokenisation.
    /// </summary>
    public struct LineToken : IEquatable<LineToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineToken"/> struct.
        /// </summary>
        /// <param name="startPosition">The start column of the token (0-based).</param>
        /// <param name="category">The category of the token.</param>
        /// <param name="subCategory">The sub-category of the token.</param>
        public LineToken(int startPosition, LineTokenCategory category, LineTokenSubCategory subCategory = default)
        {
            StartPosition = startPosition;
            Category = category;
            SubCategory = subCategory;
        }

        /// <summary>
        /// Gets the token start column (0-based).
        /// </summary>
        public int StartPosition { get; }

        /// <summary>
        /// Gets the token category.
        /// </summary>
        public LineTokenCategory Category { get; }

        /// <summary>
        /// Gets the token sub-category.
        /// </summary>
        public LineTokenSubCategory SubCategory { get; }

        /// <summary>
        /// Checks equality for 2 line tokens.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>True if equal.</returns>
        public static bool operator ==(LineToken left, LineToken right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks in-equality for 2 line tokens.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>True if not equal.</returns>
        public static bool operator !=(LineToken left, LineToken right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is LineToken token && Equals(token);
        }

        /// <inheritdoc/>
        public bool Equals(LineToken other)
        {
            return StartPosition == other.StartPosition &&
                   Category == other.Category &&
                   SubCategory == other.SubCategory;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(StartPosition, Category, SubCategory);
        }
    }
}
