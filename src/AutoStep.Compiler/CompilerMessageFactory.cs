using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Core.Elements;

namespace AutoStep.Compiler
{
    internal static class CompilerMessageFactory
    {
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

        private static string GetMessageText(CompilerMessageCode code, object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, CompilerMessages.ResourceManager.GetString(code.ToString(), CultureInfo.CurrentCulture), args);
        }

        public static CompilerMessage Create(string? sourceName, CompilerMessageLevel level, CompilerMessageCode code, IToken start, IToken stop, params object[] args)
        {
            return Create(sourceName, level, code, start.Line, start.Column + 1, stop.Line, stop.Column + 1 + (stop.StopIndex - stop.StartIndex), args);
        }

        public static CompilerMessage Create(string? sourceName, ParserRuleContext context, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            return Create(sourceName, level, code, context.Start, context.Stop, args);
        }

        public static CompilerMessage Create(string? sourceName, PositionalElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            return Create(sourceName, level, code, element.SourceLine, element.SourceColumn, element.SourceLine, element.EndColumn, args);
        }

        public static CompilerMessage Create(string? sourceName, BuiltElement element, CompilerMessageLevel level, CompilerMessageCode code, params object[] args)
        {
            return Create(sourceName, level, code, element.SourceLine, element.SourceColumn, args);
        }
    }
}
