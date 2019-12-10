using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Compiler.Parser;

namespace AutoStep.Compiler
{
    internal class ParserErrorListener : BaseErrorListener
    {
        string? sourceName;

        public List<CompilerMessage> ParserErrors { get; } = new List<CompilerMessage>();

        public ParserErrorListener(string? sourceName)
        {
            this.sourceName = sourceName;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var endIndex = offendingSymbol.StopIndex - offendingSymbol.StartIndex;

            if (endIndex < 0)
            {
                endIndex = 0;
            }

            string finalMsg = offendingSymbol.Type switch
            {
                AutoStepParser.Eof => "Unexpected end of file",
                AutoStepParser.WORD => offendingSymbol.Text switch
                {
                    "@" => "Bad tag format. Tag must have the format '@tagName'",
                    "$" => "Bad option format. Option must the format '$optionName', " +
                           "optionally with a value separated by ':', e.g. '$optionName:value'",
                    _ => msg
                },
                _ => msg
            };

            // Work out the 'end line'.
            var compileMsg = new CompilerMessage(
                sourceName,
                CompilerMessageLevel.Error,
                CompilerMessageCode.SyntaxError,
                string.Format(CultureInfo.CurrentCulture, CompilerMessages.SyntaxError, finalMsg),
                line,
                charPositionInLine + 1, // Antlr uses 0 offset, but 1 offset for message display feels more natural,
                line,
                charPositionInLine + 1 + endIndex);

            ParserErrors.Add(compileMsg);
        }
    }
}
