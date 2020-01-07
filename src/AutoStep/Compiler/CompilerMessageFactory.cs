using System.Globalization;
using Antlr4.Runtime;
using AutoStep.Elements;

namespace AutoStep.Compiler
{
    /// <summary>
    /// Factory for creating compiler messages.
    /// </summary>
    internal static class CompilerMessageFactory
    {
        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="lineStart">The line on which the message starts.</param>
        /// <param name="colStart">The column position at which the message starts.</param>
        /// <param name="lineEnd">The line on which the message ends.</param>
        /// <param name="colEnd">The column position at which the message ends.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public static CompilerMessage Create(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, int lineEnd, int colEnd, params object[] args)
        {
            var message = new CompilerMessage(
                sourceName,
                level,
                code,
                GetMessageText(code, args),
                lineStart,
                colStart,
                lineEnd,
                colEnd);

            return message;
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="lineStart">The line on which the message starts.</param>
        /// <param name="colStart">The column position at which the message starts.</param>
        /// <param name="args">Message arguments.</param>
        /// <returns>The created message.</returns>
        public static CompilerMessage Create(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, int lineStart, int colStart, params object[] args)
        {
            var message = new CompilerMessage(
                sourceName,
                level,
                code,
                GetMessageText(code, args),
                lineStart,
                colStart);

            return message;
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="start">The Antlr token at which the message starts.</param>
        /// <param name="stop">The Antlr token at which the message stops.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public static CompilerMessage Create(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
        {
            return Create(sourceName, level, code, start.Line, start.Column + 1, stop.Line, stop.Column + 1 + (stop.StopIndex - stop.StartIndex), args);
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="context">The Antlr parser context that is covered by this message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public static CompilerMessage Create(string? sourceName, ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            return Create(sourceName, level, code, context.Start, context.Stop, args);
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="element">A built positional element that is covered by the message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public static CompilerMessage Create(string? sourceName, PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            return Create(sourceName, level, code, element.SourceLine, element.SourceColumn, element.SourceLine, element.EndColumn, args);
        }

        /// <summary>
        /// Create a compiler message.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="element">A built element that is covered by the message.</param>
        /// <param name="level">Message level.</param>
        /// <param name="code">Message code.</param>
        /// <param name="args">Any arguments used to prepare the message string.</param>
        /// <returns>The created message.</returns>
        public static CompilerMessage Create(string? sourceName, BuiltElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            return Create(sourceName, level, code, element.SourceLine, element.SourceColumn, args);
        }

        private static string GetMessageText(CompilerMessageCode code, object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, CompilerMessages.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture), args);
        }
    }
}
