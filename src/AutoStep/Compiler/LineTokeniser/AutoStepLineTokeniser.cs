using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using AutoStep.Compiler;
using AutoStep.Compiler.Parser;
using static AutoStep.Compiler.Parser.AutoStepParser;

namespace AutoStep
{
    internal class AutoStepLineTokeniser
    {
        private readonly IAutoStepLinker linker;

        public AutoStepLineTokeniser(IAutoStepLinker linker)
        {
            this.linker = linker;
        }

        public LineTokeniseResult Tokenise(string text, LineTokeniserState lastState)
        {
            var parseTree = CompileLine(text, out var tokenStream);

            if (parseTree is object)
            {
                return VisitParseTree(parseTree, tokenStream, lastState);
            }
            else
            {
                // null means that the parser really had no idea, so yield an empty token set.
                return new LineTokeniseResult(0, Enumerable.Empty<LineToken>());
            }
        }

        private LineTokeniseResult VisitParseTree(OnlyLineContext ctxt, CommonTokenStream tokenStream, LineTokeniserState lastState)
        {
            var visitor = new AutoStepLineVisitor(linker, ctxt, tokenStream, lastState);

            return visitor.BuildLineResult();
        }

        private OnlyLineContext CompileLine(string line, out CommonTokenStream tokenStream)
        {
            var inputStream = new AntlrInputStream(line);
            var lexer = new AutoStepLexer(inputStream);

            tokenStream = new CommonTokenStream(lexer);

            // Create a parser and register our error listener.
            var parser = new AutoStepParser(tokenStream);

            return parser.onlyLine();
        }
    }
}
