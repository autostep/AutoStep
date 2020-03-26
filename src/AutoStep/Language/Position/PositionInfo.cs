using System;
using System.Collections.Generic;
using AutoStep.Elements;

namespace AutoStep.Language.Position
{
    /// <summary>
    /// Represents the result of a look-up in a <see cref="IPositionIndex"/>.
    /// </summary>
    public struct PositionInfo : IEquatable<PositionInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PositionInfo"/> struct.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        /// <param name="scopes">The set of scopes that apply at this position.</param>
        /// <param name="lineTokens">The set of tokens that apply at this position.</param>
        /// <param name="cursorTokenIndex">The index of the token, in the set of tokens, that the cursor is at.</param>
        /// <param name="closedPrecedingTokenIndex">The index of the token, in the set of tokens, that immediately precedes the cursor.</param>
        public PositionInfo(int line, int column, IReadOnlyList<BuiltElement> scopes, IReadOnlyList<PositionLineToken> lineTokens, int? cursorTokenIndex, int? closedPrecedingTokenIndex)
        {
            Line = line;
            Column = column;
            Scopes = scopes;
            LineTokens = lineTokens;
            CursorTokenIndex = cursorTokenIndex;
            ClosestPrecedingTokenIndex = closedPrecedingTokenIndex;
        }

        /// <summary>
        /// Gets the line number of the position.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the column number of the position.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the set of scopes that apply at this position. First element is current scope, subsequent elements are the parent scopes.
        /// </summary>
        public IReadOnlyList<BuiltElement> Scopes { get; }

        /// <summary>
        /// Gets the set of tokens that apply at this position.
        /// </summary>
        public IReadOnlyList<PositionLineToken> LineTokens { get; }

        /// <summary>
        /// Gets the index of the token, in the set of tokens, that the cursor is at. Will be null if the cursor is not on a token position.
        /// </summary>
        public int? CursorTokenIndex { get; }

        /// <summary>
        /// Gets the index of the token, in the set of tokens, that immediately precedes the cursor. Will be null if the line contains no tokens.
        /// </summary>
        public int? ClosestPrecedingTokenIndex { get; }

        /// <summary>
        /// Gets the token of the current cursor position.
        /// </summary>
        public PositionLineToken? Token
        {
            get
            {
                if (CursorTokenIndex.HasValue)
                {
                    return LineTokens[CursorTokenIndex.Value];
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the current scope.
        /// </summary>
        public BuiltElement? CurrentScope
        {
            get
            {
                if (Scopes.Count == 0)
                {
                    return null;
                }

                return Scopes[0];
            }
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is PositionInfo pos)
            {
                return Equals(pos);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Equals(PositionInfo other)
        {
            return Line == other.Line && Column == other.Column;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Line, Column);
        }

        /// <summary>
        /// Equals operator.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Equal.</returns>
        public static bool operator ==(PositionInfo left, PositionInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Not Equals operator.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Not Equal.</returns>
        public static bool operator !=(PositionInfo left, PositionInfo right)
        {
            return !(left == right);
        }
    }
}
