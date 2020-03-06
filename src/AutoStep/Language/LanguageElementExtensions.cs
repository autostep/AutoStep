using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AutoStep.Elements;

namespace AutoStep.Language
{
    internal static class LanguageElementExtensions
    {
        /// <summary>
        /// Add positional line information to the specified element, using the given parser context for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr parser context.</param>
        /// <returns>The same input element, after update.</returns>
        public static TElement AddPositionalLineInfo<TElement>(this TElement element, ParserRuleContext ctxt)
            where TElement : PositionalElement
        {
            return AddPositionalLineInfo(element, ctxt.Start, ctxt.Stop);
        }

        /// <summary>
        /// Add positional line information to the specified element, using the given parser context for positions. If the rule context ends with the specified error token,
        /// the preceding token will be used instead.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr parser context.</param>
        /// <param name="stream">The token stream.</param>
        /// <param name="errorToken">The error token type to check for.</param>
        /// <returns>The same input element, after update.</returns>
        public static TElement AddPositionalLineInfoExcludingErrorStopToken<TElement>(this TElement element, ParserRuleContext ctxt, ITokenStream stream, int errorToken)
            where TElement : PositionalElement
        {
            var defaultEnd = ctxt.Stop;

            if (errorToken == ctxt.Stop.Type)
            {
                // Don't use this one.
                defaultEnd = stream.Get(ctxt.Stop.TokenIndex - 1);
            }

            return AddPositionalLineInfo(element, ctxt.Start, defaultEnd);
        }

        /// <summary>
        /// Add positional line information to the specified element, using the given terminal node for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr terminal node.</param>
        /// <returns>The same input element, after update.</returns>
        public static TElement AddPositionalLineInfo<TElement>(this TElement element, ITerminalNode ctxt)
            where TElement : PositionalElement
        {
            return AddPositionalLineInfo(element, ctxt.Symbol, ctxt.Symbol);
        }

        /// <summary>
        /// Add positional line information to the specified element, using the given tokens for start and stop positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="start">The start position.</param>
        /// <param name="stop">The stop position.</param>
        /// <returns>The same input element, after update.</returns>
        public static TElement AddPositionalLineInfo<TElement>(this TElement element, IToken start, IToken stop)
            where TElement : PositionalElement
        {
            element.SourceLine = start.Line;
            element.StartColumn = start.Column + 1;
            element.EndColumn = stop.Column + (stop.StopIndex - stop.StartIndex) + 1;
            element.EndLine = stop.Line;

            return element;
        }

        /// <summary>
        /// Add line information to the specified element, using the given parser context for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr parser context.</param>
        /// <returns>The same input element, after update.</returns>
        public static TElement AddLineInfo<TElement>(this TElement element, ParserRuleContext ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Start.Line;
            element.StartColumn = ctxt.Start.Column + 1;

            return element;
        }

        /// <summary>
        /// Add line information to the specified element, using the given terminal node for positions.
        /// </summary>
        /// <typeparam name="TElement">The element type.</typeparam>
        /// <param name="element">The element to update.</param>
        /// <param name="ctxt">The Antlr terminal node.</param>
        /// <returns>The same input element, after update.</returns>
        public static TElement AddLineInfo<TElement>(this TElement element, ITerminalNode ctxt)
            where TElement : BuiltElement
        {
            element.SourceLine = ctxt.Symbol.Line;
            element.StartColumn = ctxt.Symbol.Column + 1;

            return element;
        }
    }
}
