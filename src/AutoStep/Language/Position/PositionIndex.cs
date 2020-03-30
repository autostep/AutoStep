using System;
using System.Collections;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Elements;

namespace AutoStep.Language.Position
{
    /// <summary>
    /// Contains a built index of file positions (line, column) mapped back to syntax elements and the active
    /// scopes at that position.
    /// </summary>
    internal class PositionIndex : IPositionIndex
    {
        private static readonly IReadOnlyList<PositionLineToken> EmptyLineTokens = Array.Empty<PositionLineToken>();
        private static readonly ImmutableElementStack Empty = new ImmutableElementStack();

        private readonly LineEntry[] lines;

        private long lastKnownLine = 0;
        private ImmutableElementStack currentStack = Empty;
        private bool isSealed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionIndex"/> class.
        /// </summary>
        /// <param name="fileLines">The number of lines in the file.</param>
        public PositionIndex(long fileLines)
        {
            if (fileLines < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fileLines));
            }

            lines = new LineEntry[fileLines];
        }

        /// <inheritdoc/>
        public PositionInfo Lookup(long line, long column)
        {
            if (!isSealed)
            {
                throw new InvalidOperationException(PositionMessages.CannotGetPositionInfoFromUnsealed);
            }

            if (line < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), line, PositionMessages.LineNumberMinimum);
            }

            if (column < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), column, PositionMessages.ColumnNumberMinimum);
            }

            var lineIndex = line - 1;

            // Get the line.
            // If we don't it, return false.
            if (lineIndex >= lines.Length)
            {
                return new PositionInfo(line, column, Empty, EmptyLineTokens, null, null);
            }

            // Get the line.
            var lineEntry = lines[lineIndex];

            if (lineEntry.Tokens is object)
            {
                // Determine the token.
                // TODO: If need be, we can switch this to a more efficient search algorithm,
                //       but we'll leave as linear for now.
                var lastToken = 0;
                int? matched = null;

                for (var currentTokenIdx = 0; currentTokenIdx < lineEntry.Tokens.Count; currentTokenIdx++)
                {
                    var token = lineEntry.Tokens[currentTokenIdx];

                    if (column >= token.StartColumn)
                    {
                        if (column <= token.EndColumn)
                        {
                            matched = currentTokenIdx;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                    lastToken = currentTokenIdx;
                }

                var relevantToken = matched ?? lastToken;

                var tokenForScopes = lineEntry.Tokens[relevantToken];

                return new PositionInfo(line, column, tokenForScopes.Scopes, lineEntry.Tokens, matched, lastToken);
            }

            return new PositionInfo(line, column, lineEntry.Scopes, EmptyLineTokens, null, null);
        }

        /// <summary>
        /// Push a new scope, based on the provided language element.
        /// </summary>
        /// <param name="associatedElement">The element to use as the scope.</param>
        /// <param name="lineNo">A custom line number to start the scope from.</param>
        public void PushScope(BuiltElement associatedElement, long lineNo)
        {
            AssertUnsealed();
            AssertLineNumberRange(lineNo, nameof(lineNo));

            PopulateBlankLines(lineNo - 1);

            // Our 'current stack' is immutable.
            currentStack = currentStack.Push(associatedElement);
        }

        /// <summary>
        /// Push a new scope, using the provided parse context for the start position.
        /// </summary>
        /// <param name="element">The element to use as the scope.</param>
        /// <param name="context">The related parse context.</param>
        public void PushScope(BuiltElement element, ParserRuleContext context)
        {
            PushScope(element, context.Start.Line);
        }

        /// <summary>
        /// Pops the current scope.
        /// </summary>
        /// <param name="endLineNo">The line number at which the scope ends.</param>
        public void PopScope(long endLineNo)
        {
            AssertUnsealed();
            AssertLineNumberRange(endLineNo, nameof(endLineNo));

            if (currentStack.Count == 0)
            {
                throw new InvalidOperationException(PositionMessages.EmptyStackCannotPop);
            }

            PopulateBlankLines(endLineNo - 1);

            currentStack = currentStack.Pop();
        }

        /// <summary>
        /// Pops the current scope.
        /// </summary>
        /// <param name="context">A parse context defining the boundaries of the scope.</param>
        public void PopScope(ParserRuleContext context)
        {
            PopScope(context.Stop.Line);
        }

        /// <summary>
        /// Adds a line token to the index.
        /// </summary>
        /// <param name="line">The line number of the token.</param>
        /// <param name="startColumn">The start column of the token.</param>
        /// <param name="endColumn">The end column of the token.</param>
        /// <param name="element">An optional related element for the token.</param>
        /// <param name="tokenCategory">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public void AddLineToken(long line, int startColumn, int endColumn, BuiltElement? element, LineTokenCategory tokenCategory, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            AssertUnsealed();
            AssertLineNumberRange(line);

            var lineIndex = line - 1;

            PopulateBlankLines(lineIndex);

            var lineInfo = lines[lineIndex];

            if (lineInfo is null)
            {
                lines[lineIndex] = lineInfo = new LineEntry(currentStack);
                lastKnownLine = lineIndex;
            }

            lineInfo.AddToken(new PositionLineToken(startColumn, endColumn, currentStack, element, tokenCategory, subCategory));
        }

        /// <summary>
        /// Adds a line token to the index.
        /// </summary>
        /// <param name="line">The line number of the token.</param>
        /// <param name="startColumn">The start column of the token.</param>
        /// <param name="endColumn">The end column of the token.</param>
        /// <param name="tokenCategory">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public void AddLineToken(long line, int startColumn, int endColumn, LineTokenCategory tokenCategory, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            AddLineToken(line, startColumn, endColumn, null, tokenCategory, subCategory);
        }

        /// <summary>
        /// Adds a line token to the index.
        /// </summary>
        /// <param name="element">The element the token maps to.</param>
        /// <param name="tokenCategory">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public void AddLineToken(PositionalElement element, LineTokenCategory tokenCategory, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            AddLineToken(element.SourceLine, element.StartColumn, element.EndColumn, element, tokenCategory, subCategory);
        }

        /// <summary>
        /// Adds a line token to the index.
        /// </summary>
        /// <param name="parseContext">A parse context to use for the position.</param>
        /// <param name="element">The element the token maps to.</param>
        /// <param name="tokenCategory">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public void AddLineToken(ParserRuleContext parseContext, BuiltElement element, LineTokenCategory tokenCategory, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            AddLineToken(parseContext.Start.Line, parseContext.Start.Column, GetEndColumnForToken(parseContext.Stop), element, tokenCategory, subCategory);
        }

        /// <summary>
        /// Adds a line token to the index.
        /// </summary>
        /// <param name="parseContext">A parse context to use for the position.</param>
        /// <param name="tokenCategory">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public void AddLineToken(ParserRuleContext parseContext, LineTokenCategory tokenCategory, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
            AddLineToken(parseContext.Start.Line, parseContext.Start.Column, GetEndColumnForToken(parseContext.Stop), tokenCategory, subCategory);
        }

        /// <summary>
        /// Adds a line token to the index.
        /// </summary>
        /// <param name="tokenNode">A terminal node to use for the position.</param>
        /// <param name="tokenCategory">The token category.</param>
        /// <param name="subCategory">The token sub-category.</param>
        public void AddLineToken(ITerminalNode tokenNode, LineTokenCategory tokenCategory, LineTokenSubCategory subCategory = LineTokenSubCategory.None)
        {
             var symbol = tokenNode.Symbol;
             AddLineToken(symbol.Line, symbol.Column, GetEndColumnForToken(symbol), null, tokenCategory, subCategory);
        }

        /// <summary>
        /// Seals the index, populating remaining blank lines.
        /// </summary>
        public void Seal()
        {
            AssertUnsealed();

            PopulateBlankLines(lines.Length);
            isSealed = true;
        }

        private void AssertLineNumberRange(long line, string? paramName = null)
        {
            if (line < 1)
            {
                throw new ArgumentOutOfRangeException(paramName ?? nameof(line), line, PositionMessages.LineNumberMinimum);
            }

            if (line > lines.Length)
            {
                throw new ArgumentOutOfRangeException(paramName ?? nameof(line), line, PositionMessages.LineNumberBeyondEndOfFile);
            }
        }

        private void AssertUnsealed()
        {
            if (isSealed)
            {
                throw new InvalidOperationException(PositionMessages.CannotModifySealedIndex);
            }
        }

        private void PopulateBlankLines(long nextKnownLine)
        {
            // From the last known line to the line before the new one, apply the current scope stack.
            for (var pos = lastKnownLine; pos < nextKnownLine; pos++)
            {
                if (lines[pos] is null)
                {
                    lines[pos] = new LineEntry(currentStack);
                }
            }

            lastKnownLine = nextKnownLine;
        }

        private static int GetEndColumnForToken(IToken token)
        {
            return token.Column + (token.StopIndex - token.StartIndex) + 1;
        }

        /// <summary>
        /// Represents an immutable stack of elements that is copied whenever it is modified.
        /// Ensures that order of elements is maintained when copying, and exposes the available elements
        /// as an indexable list.
        /// </summary>
        private class ImmutableElementStack : IReadOnlyList<BuiltElement>
        {
            private readonly BuiltElement[] elements;

            public ImmutableElementStack()
            {
                elements = Array.Empty<BuiltElement>();
            }

            private ImmutableElementStack(BuiltElement[] array)
            {
                elements = array;
            }

            public int Count => elements.Length;

            public BuiltElement this[int index] => elements[index];

            public ImmutableElementStack Push(BuiltElement element)
            {
                var newArr = new BuiltElement[elements.Length + 1];
                elements.CopyTo(newArr, 1);
                newArr[0] = element;

                return new ImmutableElementStack(newArr);
            }

            public ImmutableElementStack Pop()
            {
                var newArr = new BuiltElement[elements.Length - 1];
                Array.Copy(elements, 1, newArr, 0, newArr.Length);

                return new ImmutableElementStack(newArr);
            }

            public IEnumerator<BuiltElement> GetEnumerator()
            {
                return ((IEnumerable<BuiltElement>)elements).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return elements.GetEnumerator();
            }
        }
    }
}
